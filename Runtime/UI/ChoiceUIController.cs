using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    public class ChoiceUIController : MonoBehaviour, Novella.Core.IChoiceUI
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _buttonPrefab;

        private readonly List<GameObject> _activeButtons = new List<GameObject>();

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        public void Show(List<Core.ChoiceOption> choices, Action<Core.ChoiceOption> onSelected)
        {
            ClearButtons();
            _panel.SetActive(true);

            foreach (var choice in choices)
            {
                var go = Instantiate(_buttonPrefab, _buttonContainer);
                _activeButtons.Add(go);

                var label = go.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = choice.Text;

                var btn = go.GetComponent<Button>();
                var captured = choice;
                btn.onClick.AddListener(() =>
                {
                    Hide();
                    onSelected?.Invoke(captured);
                });
            }
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            ClearButtons();
        }

        private void ClearButtons()
        {
            foreach (var go in _activeButtons)
                Destroy(go);
            _activeButtons.Clear();
        }
    }
}
