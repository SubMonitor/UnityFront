using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SubMonitor.SubscriptionsUI
{
    public class SubscriptionCardUI : MonoBehaviour
    {
        private TMP_Text _titleText;
        private TMP_Text _dateText;
        private TMP_Text _arrowText;
        private TMP_Text _detailsText;
        private RectTransform _detailsContainer;
        private Button _toggleButton;

        private SubscriptionItem _item;
        private Action<SubscriptionItem, bool> _stateChangedCallback;

        public void BindViews(
            TMP_Text titleText,
            TMP_Text dateText,
            TMP_Text arrowText,
            TMP_Text detailsText,
            RectTransform detailsContainer,
            Button toggleButton)
        {
            _titleText = titleText;
            _dateText = dateText;
            _arrowText = arrowText;
            _detailsText = detailsText;
            _detailsContainer = detailsContainer;
            _toggleButton = toggleButton;

            if (_toggleButton != null)
            {
                _toggleButton.onClick.RemoveAllListeners();
                _toggleButton.onClick.AddListener(ToggleExpanded);
            }
        }

        public void Setup(SubscriptionItem item, Action<SubscriptionItem, bool> stateChangedCallback = null)
        {
            _item = item;
            _stateChangedCallback = stateChangedCallback;
            Refresh();
        }

        public void ToggleExpanded()
        {
            if (_item == null)
            {
                return;
            }

            _item.IsExpanded = !_item.IsExpanded;
            Refresh();
            _stateChangedCallback?.Invoke(_item, _item.IsExpanded);
        }

        private void Refresh()
        {
            if (_item == null)
            {
                return;
            }

            if (_titleText != null)
            {
                _titleText.text = _item.Title;
            }

            if (_dateText != null)
            {
                _dateText.text = _item.ExpireDate;
            }

            if (_arrowText != null)
            {
                _arrowText.text = _item.IsExpanded ? "▲" : "▼";
            }

            if (_detailsContainer != null)
            {
                _detailsContainer.gameObject.SetActive(_item.IsExpanded);
            }

            if (_detailsText != null)
            {
                _detailsText.text = _item.IsExpanded
                    ? "Автопродление включено. Здесь можно добавить действия по подписке."
                    : string.Empty;
            }
        }
    }
}
