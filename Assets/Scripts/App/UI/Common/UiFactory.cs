using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SubMonitor.App.UI.Common
{
    public sealed class ScrollViewParts
    {
        public RectTransform Root;
        public RectTransform Viewport;
        public RectTransform Content;
        public ScrollRect ScrollRect;
    }

    public static class UiFactory
    {
        private static Sprite _whiteSprite;

        public static void EnsureEventSystem()
        {
            EventSystem existing = Object.FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetParent(null);
        }

        public static Canvas CreateCanvas(string name, Transform parent)
        {
            var canvasObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Stretch(canvasObject.GetComponent<RectTransform>());
            return canvas;
        }

        public static RectTransform CreatePanel(
            string name,
            Transform parent,
            Color color,
            Sprite sprite = null,
            Image.Type imageType = Image.Type.Sliced)
        {
            var panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.GetComponent<Image>();
            image.sprite = sprite ?? WhiteSprite;
            image.color = color;
            image.type = imageType;

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        public static Button CreateButton(
            string name,
            Transform parent,
            string title,
            Color backgroundColor,
            UnityAction onClick,
            TMP_FontAsset font,
            float fontSize = 30f,
            Color? textColor = null,
            Sprite backgroundSprite = null)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = backgroundSprite ?? WhiteSprite;
            image.color = backgroundColor;
            image.type = Image.Type.Sliced;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            button.colors = colors;

            TMP_Text label = CreateText(
                "Label",
                buttonObject.transform,
                title,
                font,
                fontSize,
                textColor ?? UiTheme.White,
                FontStyles.Bold,
                TextAlignmentOptions.Center);
            Stretch(label.rectTransform);

            return button;
        }

        public static TMP_Text CreateText(
            string name,
            Transform parent,
            string value,
            TMP_FontAsset font,
            float fontSize,
            Color color,
            FontStyles fontStyle,
            TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = font;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return text;
        }

        public static TMP_InputField CreateInputField(
            string name,
            Transform parent,
            TMP_FontAsset font,
            string placeholderText,
            bool isPassword = false,
            bool multiLine = false)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            root.transform.SetParent(parent, false);

            Image background = root.GetComponent<Image>();
            background.color = UiTheme.SurfaceMuted;
            background.sprite = WhiteSprite;
            background.type = Image.Type.Sliced;

            TMP_InputField inputField = root.GetComponent<TMP_InputField>();
            inputField.contentType = isPassword ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            inputField.lineType = multiLine ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            inputField.caretWidth = 2;
            inputField.selectionColor = new Color(0.2f, 0.5f, 0.95f, 0.3f);

            RectTransform textArea = CreatePanel("TextArea", root.transform, Color.clear, WhiteSprite, Image.Type.Simple);
            textArea.anchorMin = Vector2.zero;
            textArea.anchorMax = Vector2.one;
            textArea.offsetMin = new Vector2(28f, 18f);
            textArea.offsetMax = new Vector2(-28f, -18f);
            textArea.gameObject.GetComponent<Image>().raycastTarget = false;
            textArea.gameObject.AddComponent<RectMask2D>();

            TMP_Text text = CreateText("Text", textArea, string.Empty, font, 28f, UiTheme.TextPrimary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Stretch(text.rectTransform);

            TMP_Text placeholder = CreateText("Placeholder", textArea, placeholderText, font, 28f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Stretch(placeholder.rectTransform);

            inputField.textViewport = textArea;
            inputField.textComponent = text as TextMeshProUGUI;
            inputField.placeholder = placeholder;

            return inputField;
        }

        public static ScrollViewParts CreateScrollView(string name, Transform parent, float spacing = 16f)
        {
            RectTransform root = CreatePanel(name, parent, Color.clear, WhiteSprite, Image.Type.Simple);
            root.GetComponent<Image>().raycastTarget = false;

            ScrollRect scrollRect = root.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 40f;

            RectTransform viewport = CreatePanel("Viewport", root, new Color(1f, 1f, 1f, 0.01f), WhiteSprite, Image.Type.Simple);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = CreatePanel("Content", viewport, Color.clear, WhiteSprite, Image.Type.Simple);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            content.GetComponent<Image>().raycastTarget = false;

            VerticalLayoutGroup verticalLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.spacing = spacing;
            verticalLayout.childControlHeight = true;
            verticalLayout.childControlWidth = true;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.padding = new RectOffset(0, 0, 0, 24);

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;

            return new ScrollViewParts
            {
                Root = root,
                Viewport = viewport,
                Content = content,
                ScrollRect = scrollRect
            };
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void ApplySafeArea(RectTransform rect)
        {
            if (rect == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 min = safeArea.position;
            Vector2 max = safeArea.position + safeArea.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void DestroyChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Object child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Object.Destroy(child);
                }
                else
                {
                    Object.DestroyImmediate(child);
                }
            }
        }

        public static Sprite WhiteSprite
        {
            get
            {
                return UiTheme.CreateWhiteSprite(ref _whiteSprite, "Strelka_UI_White");
            }
        }
    }
}
