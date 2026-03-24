using System.Collections.Generic;
using Novella.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// CGギャラリー画面。閲覧済みのCG(背景画像)をグリッド表示し、
    /// タップで拡大表示する。
    /// </summary>
    public class CGGalleryUIController : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _gridContainer;
        [SerializeField] private Button _closeButton;

        [Header("Full View")]
        [SerializeField] private GameObject _fullViewPanel;
        [SerializeField] private Image _fullViewImage;
        [SerializeField] private Button _fullViewCloseButton;

        [Header("Settings")]
        [SerializeField] private Vector2 _thumbnailSize = new Vector2(240, 135);

        private System.Action _onClose;

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
            if (_fullViewPanel != null) _fullViewPanel.SetActive(false);
        }

        public void Open(System.Action onClose = null)
        {
            _onClose = onClose;
            BuildGallery();
            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }

            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);
            if (_fullViewCloseButton != null)
                _fullViewCloseButton.onClick.AddListener(CloseFullView);
        }

        public bool IsOpen => _panel != null && _panel.activeSelf;

        public void Close()
        {
            if (_fullViewPanel != null) _fullViewPanel.SetActive(false);
            if (_panel != null) _panel.SetActive(false);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            if (_fullViewCloseButton != null) _fullViewCloseButton.onClick.RemoveListener(CloseFullView);
            var cb = _onClose;
            _onClose = null;
            cb?.Invoke();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_fullViewPanel != null && _fullViewPanel.activeSelf)
                    CloseFullView();
                else if (IsOpen)
                    Close();
            }
        }

        private void BuildGallery()
        {
            if (_gridContainer == null) return;

            // 既存のサムネを削除
            foreach (Transform child in _gridContainer)
                Destroy(child.gameObject);

            List<string> cgs = CGManager.GetViewedCGs();

            foreach (string cgName in cgs)
            {
                var sprite = Resources.Load<Sprite>($"Backgrounds/{cgName}");
                if (sprite == null) continue;

                var thumbGO = new GameObject(cgName);
                thumbGO.transform.SetParent(_gridContainer, false);

                var rect = thumbGO.AddComponent<RectTransform>();
                rect.sizeDelta = _thumbnailSize;

                var img = thumbGO.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;

                var btn = thumbGO.AddComponent<Button>();
                string capturedName = cgName;
                btn.onClick.AddListener(() => ShowFullView(capturedName));
            }
        }

        private void ShowFullView(string cgName)
        {
            if (_fullViewPanel == null || _fullViewImage == null) return;

            var sprite = Resources.Load<Sprite>($"Backgrounds/{cgName}");
            if (sprite == null) return;

            _fullViewImage.sprite = sprite;
            _fullViewImage.preserveAspect = true;
            _fullViewPanel.SetActive(true);
            _fullViewPanel.transform.SetAsLastSibling();
        }

        private void CloseFullView()
        {
            if (_fullViewPanel != null) _fullViewPanel.SetActive(false);
        }
    }
}
