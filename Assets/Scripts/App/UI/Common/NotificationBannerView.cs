using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SubMonitor.App.UI.Common
{
    public sealed class NotificationBannerView : MonoBehaviour
    {
        private const float DefaultAutoHideDelaySeconds = 5f;

        private RectTransform _root;
        private TMP_Text _messageText;
        private Coroutine _autoHideCoroutine;

        public static NotificationBannerView Create(Transform parent, TMP_FontAsset font)
        {
            RectTransform root = UiFactory.CreatePanel("NotificationBanner", parent, UiTheme.Surface, UiFactory.WhiteSprite, Image.Type.Sliced);
            root.gameObject.SetActive(false);

            HorizontalLayoutGroup layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 18, 18);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            LayoutElement layoutElement = root.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 84f;

            NotificationBannerView banner = root.gameObject.AddComponent<NotificationBannerView>();
            banner._root = root;
            banner._messageText = UiFactory.CreateText(
                "Message",
                root,
                string.Empty,
                font,
                28f,
                UiTheme.TextPrimary,
                FontStyles.Normal,
                TextAlignmentOptions.Left);

            return banner;
        }

        public void ShowInfo(string message)
        {
            EnsureBound();
            Show(message, UiTheme.SurfaceMuted, UiTheme.TextPrimary);
        }

        public void ShowSuccess(string message)
        {
            EnsureBound();
            Show(message, new Color32(226, 247, 234, 255), UiTheme.Success);
        }

        public void ShowError(string message)
        {
            EnsureBound();
            Show(message, new Color32(251, 236, 236, 255), UiTheme.Error);
        }

        public void Hide()
        {
            EnsureBound();
            CancelAutoHide();
            HideImmediate();
        }

        private void Show(string message, Color background, Color textColor)
        {
            EnsureBound();
            if (_root == null || _messageText == null)
            {
                return;
            }

            _root.gameObject.SetActive(true);
            _root.GetComponent<Image>().color = background;
            _messageText.text = message;
            _messageText.color = textColor;
            RestartAutoHide();
        }

        private void EnsureBound()
        {
            if (_root == null)
            {
                _root = transform as RectTransform;
            }

            if (_messageText != null)
            {
                return;
            }

            Transform messageTransform = transform.Find("Message");
            if (messageTransform != null)
            {
                _messageText = messageTransform.GetComponent<TMP_Text>();
            }

            if (_messageText == null)
            {
                _messageText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void RestartAutoHide()
        {
            CancelAutoHide();
            if (!isActiveAndEnabled)
            {
                return;
            }

            _autoHideCoroutine = StartCoroutine(HideAfterDelay());
        }

        private void CancelAutoHide()
        {
            if (_autoHideCoroutine == null)
            {
                return;
            }

            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(DefaultAutoHideDelaySeconds);
            _autoHideCoroutine = null;
            HideImmediate();
        }

        private void HideImmediate()
        {
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _autoHideCoroutine = null;
        }
    }
}
