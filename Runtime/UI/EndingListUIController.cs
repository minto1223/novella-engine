using System.Collections.Generic;
using Novella.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class EndingListUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _listContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_FontAsset _font;

        [Header("Ending Definitions")]
        [Tooltip("全エンディング名を定義（未到達は???で表示）")]
        [SerializeField] private List<string> _allEndings = new List<string>();

        public bool IsOpen => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Open()
        {
            BuildList();
            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void BuildList()
        {
            if (_listContainer == null) return;

            foreach (Transform child in _listContainer)
                Destroy(child.gameObject);

            var unlocked = EndingManager.GetUnlockedEndings();

            // allEndingsが定義されていればそれを使う、なければ解放済みのみ表示
            if (_allEndings.Count > 0)
            {
                int unlockedCount = 0;
                foreach (var ending in _allEndings)
                {
                    bool isUnlocked = EndingManager.IsUnlocked(ending);
                    if (isUnlocked) unlockedCount++;
                    CreateEndingEntry(ending, isUnlocked);
                }
                // ヘッダーに進捗表示
                CreateProgressLabel(unlockedCount, _allEndings.Count);
            }
            else if (unlocked.Count > 0)
            {
                foreach (var ending in unlocked)
                    CreateEndingEntry(ending, true);
            }
            else
            {
                CreateLabel("(No endings reached)");
            }
        }

        private void CreateEndingEntry(string endingName, bool unlocked)
        {
            var go = new GameObject($"Ending_{endingName}");
            go.transform.SetParent(_listContainer, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 70);

            var img = go.AddComponent<Image>();
            img.color = unlocked
                ? new Color(0.15f, 0.2f, 0.15f, 0.9f)
                : new Color(0.15f, 0.15f, 0.15f, 0.7f);

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16, 4);
            labelRect.offsetMax = new Vector2(-16, -4);

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = unlocked ? endingName : "???";
            tmp.fontSize = 30;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            if (_font != null) tmp.font = _font;
        }

        private void CreateProgressLabel(int unlocked, int total)
        {
            var go = new GameObject("Progress");
            go.transform.SetParent(_listContainer, false);
            go.transform.SetAsFirstSibling();

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 40);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = $"Endings: {unlocked} / {total}";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.8f, 0.8f, 0.5f);
            if (_font != null) tmp.font = _font;
        }

        private void CreateLabel(string text)
        {
            var go = new GameObject("EmptyLabel");
            go.transform.SetParent(_listContainer, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.6f, 0.6f, 0.6f);
            if (_font != null) tmp.font = _font;
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 60);
        }
    }
}
