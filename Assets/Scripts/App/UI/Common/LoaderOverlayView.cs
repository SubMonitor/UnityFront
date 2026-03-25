using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SubMonitor.App.UI.Common
{
    public sealed class LoaderOverlayView : MonoBehaviour
    {
        private RectTransform _root;
        private TMP_Text _label;

        public static LoaderOverlayView Create(Transform parent, TMP_FontAsset font)
        {
            RectTransform overlay = UiFactory.CreatePanel("LoaderOverlay", parent, UiTheme.Overlay, UiFactory.WhiteSprite, Image.Type.Simple);
            UiFactory.Stretch(overlay);
            overlay.gameObject.SetActive(false);

            RectTransform card = UiFactory.CreatePanel("LoaderCard", overlay, UiTheme.Surface, UiFactory.WhiteSprite, Image.Type.Sliced);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(360f, 180f);

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 28, 28);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text title = UiFactory.CreateText("Title", card, "Загрузка", font, 34f, UiTheme.TextPrimary, FontStyles.Bold, TextAlignmentOptions.Center);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;

            TMP_Text label = UiFactory.CreateText("Label", card, "Подождите, выполняем запрос к серверу...", font, 26f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.Center);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = 72f;

            LoaderOverlayView view = overlay.gameObject.AddComponent<LoaderOverlayView>();
            view._root = overlay;
            view._label = label;
            return view;
        }

        public void Show(string message = null)
        {
            EnsureBound();
            if (_root == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(message) && _label != null)
            {
                _label.text = message;
            }

            _root.gameObject.SetActive(true);
        }

        public void Hide()
        {
            EnsureBound();
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private void EnsureBound()
        {
            if (_root == null)
            {
                _root = transform as RectTransform;
            }

            if (_label != null)
            {
                return;
            }

            Transform labelTransform = transform.Find("LoaderCard/Label");
            if (labelTransform != null)
            {
                _label = labelTransform.GetComponent<TMP_Text>();
            }

            if (_label == null)
            {
                TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text text in texts)
                {
                    if (text.name == "Label")
                    {
                        _label = text;
                        break;
                    }
                }
            }
        }
    }
}
