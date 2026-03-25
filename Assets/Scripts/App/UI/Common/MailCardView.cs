using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SubMonitor.App.DTO;

namespace SubMonitor.App.UI.Common
{
    public sealed class MailCardView : MonoBehaviour
    {
        private TMP_Text _subject;
        private TMP_Text _meta;
        private TMP_Text _preview;
        private Button _openButton;
        private EmailPreviewDto _previewDto;

        public static MailCardView Create(Transform parent, TMP_FontAsset font, Sprite roundedSprite)
        {
            RectTransform root = UiFactory.CreatePanel("MailCard", parent, UiTheme.Surface, roundedSprite, Image.Type.Sliced);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 240f;

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            MailCardView view = root.gameObject.AddComponent<MailCardView>();
            view._subject = UiFactory.CreateText("Subject", root, string.Empty, font, 30f, UiTheme.TextPrimary, FontStyles.Bold, TextAlignmentOptions.Left);
            view._meta = UiFactory.CreateText("Meta", root, string.Empty, font, 24f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left);
            view._preview = UiFactory.CreateText("Preview", root, string.Empty, font, 24f, UiTheme.TextPrimary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            view._preview.gameObject.AddComponent<LayoutElement>().preferredHeight = 76f;
            view._openButton = UiFactory.CreateButton("OpenButton", root, "Открыть письмо", UiTheme.Accent, null, font, 24f, UiTheme.White, roundedSprite);
            view._openButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 58f;
            return view;
        }

        public void Bind(EmailPreviewDto previewDto, Action<EmailPreviewDto> onOpen)
        {
            _previewDto = previewDto;
            _subject.text = string.IsNullOrWhiteSpace(previewDto.subject) ? "(без темы)" : previewDto.subject;
            string keywords = previewDto.matched_keywords == null || previewDto.matched_keywords.Length == 0
                ? "без ключевых слов"
                : string.Join(", ", previewDto.matched_keywords.Where(keyword => !string.IsNullOrWhiteSpace(keyword)).ToArray());
            _meta.text = previewDto.from + "\n" + previewDto.date_str + " • " + keywords;
            _preview.text = string.IsNullOrWhiteSpace(previewDto.text_preview) ? "Нет текстового предпросмотра." : previewDto.text_preview;
            _openButton.onClick.RemoveAllListeners();
            _openButton.onClick.AddListener(() => onOpen?.Invoke(_previewDto));
        }
    }
}
