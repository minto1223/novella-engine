using System.Collections.Generic;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public enum SavePanelMode { Save, Load }

    public class SaveUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private SavePanelMode _mode = SavePanelMode.Save;
        [SerializeField] private RectTransform _slotContainer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Button _closeButton;

        [Header("Paging")]
        [SerializeField] private Button _prevPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private TMP_Text _pageLabel;
        [SerializeField, Min(1)] private int _slotsPerPage = 3;
        [SerializeField, Min(1)] private int _totalPages = 5;

        private NovellaEngine _engine;
        private SaveManager _saveManager;
        private System.Action _onClose;
        private int _currentPage = 0;

        private readonly List<Button> _buttons = new List<Button>();
        private readonly List<SaveSlotView> _slotViews = new List<SaveSlotView>();

        public void Init(NovellaEngine engine)
        {
            _engine = engine;
            _saveManager = new SaveManager();
            _slotsPerPage = Mathf.Max(1, engine.SaveSlotCount);

            ResolveSlotContainer();
            BuildSlots(_slotsPerPage);

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            if (_prevPageButton != null)
                _prevPageButton.onClick.AddListener(PrevPage);
            if (_nextPageButton != null)
                _nextPageButton.onClick.AddListener(NextPage);

            if (_panel != null) _panel.SetActive(false);
        }

        private void ResolveSlotContainer()
        {
            if (_slotContainer != null) return;

            var root = _panel != null ? _panel.transform : transform;
            var glg = root.GetComponentInChildren<GridLayoutGroup>(true);
            if (glg != null)
            {
                _slotContainer = glg.GetComponent<RectTransform>();
                return;
            }
            var csf = root.GetComponentInChildren<ContentSizeFitter>(true);
            if (csf != null)
                _slotContainer = csf.GetComponent<RectTransform>();

            if (_slotContainer == null)
                Debug.LogError("[Novella] SaveUIController: SlotContainer が見つかりません。Novella > Rebuild Save Panels を実行してください。");
        }

        private void BuildSlots(int count)
        {
            if (_slotContainer != null)
                foreach (Transform child in _slotContainer)
                    Destroy(child.gameObject);

            _buttons.Clear();
            _slotViews.Clear();

            if (_slotContainer == null || _slotPrefab == null)
            {
                if (_slotPrefab == null)
                    Debug.LogError("[Novella] SaveUIController: _slotPrefab が未設定です。Novella > Rebuild Save Panels を実行してください。");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var go = Instantiate(_slotPrefab, _slotContainer);
                go.name = $"Slot{i}";

                var btn = go.GetComponent<Button>();
                var view = go.GetComponent<SaveSlotView>();

                _buttons.Add(btn);
                _slotViews.Add(view);

                int localIndex = i;
                btn?.onClick.AddListener(() =>
                {
                    int globalSlot = _currentPage * _slotsPerPage + localIndex;
                    if (_mode == SavePanelMode.Save) OnSave(globalSlot);
                    else OnLoad(globalSlot);
                });
            }
        }

        public void Open(System.Action onClose = null)
        {
            _onClose = onClose;
            _currentPage = 0;
            RefreshPage();
            if (_panel != null)
            {
                _panel.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_panel.GetComponent<RectTransform>());
                Canvas.ForceUpdateCanvases();
            }
        }

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
                Close();
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            var cb = _onClose;
            _onClose = null;
            cb?.Invoke();
        }

        private void PrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                RefreshPage();
            }
        }

        private void NextPage()
        {
            if (_currentPage < _totalPages - 1)
            {
                _currentPage++;
                RefreshPage();
            }
        }

        private void RefreshPage()
        {
            RefreshSlots();
            if (_mode == SavePanelMode.Load)
                UpdateLoadButtonInteractivity();
            UpdatePageUI();
        }

        private void UpdatePageUI()
        {
            if (_pageLabel != null)
                _pageLabel.text = $"{_currentPage + 1} / {_totalPages}";
            if (_prevPageButton != null)
                _prevPageButton.interactable = _currentPage > 0;
            if (_nextPageButton != null)
                _nextPageButton.interactable = _currentPage < _totalPages - 1;
        }

        private void OnSave(int slot)
        {
            _saveManager.Save(slot, _engine);
            RefreshSlots();
        }

        private void OnLoad(int slot)
        {
            if (_saveManager.GetInfo(slot) == null) return;
            Close();
            _saveManager.Load(slot, _engine);
        }

        private void RefreshSlots()
        {
            for (int i = 0; i < _slotViews.Count; i++)
                RefreshSlot(i);
        }

        private void RefreshSlot(int localIndex)
        {
            if (localIndex >= _slotViews.Count) return;
            var view = _slotViews[localIndex];
            if (view == null) return;

            int globalSlot = _currentPage * _slotsPerPage + localIndex;

            if (view.SlotNumberText != null)
                view.SlotNumberText.text = $"SLOT {globalSlot + 1:00}";

            var info = _saveManager.GetInfo(globalSlot);
            bool hasData = info != null;

            if (view.NoDataOverlay != null)
                view.NoDataOverlay.SetActive(!hasData);

            if (view.DateText != null)
                view.DateText.text = hasData ? info.SavedAt : "";

            if (view.TitleText != null)
                view.TitleText.text = hasData ? (info.Title ?? "") : "";

            if (view.DialogueText != null)
                view.DialogueText.text = hasData ? (info.LastDialogue ?? "") : "";

            if (view.ThumbnailImage != null)
            {
                var sprite = hasData ? SaveManager.LoadThumbnail(info.ThumbnailFile) : null;
                view.ThumbnailImage.sprite = sprite;
                view.ThumbnailImage.color = sprite != null ? Color.white : new Color(0.1f, 0.1f, 0.15f);
            }
        }

        private void UpdateLoadButtonInteractivity()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                int globalSlot = _currentPage * _slotsPerPage + i;
                if (_buttons[i] != null)
                    _buttons[i].interactable = _saveManager.GetInfo(globalSlot) != null;
            }
        }
    }
}
