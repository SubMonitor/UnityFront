using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SubMonitor.App.DTO;

namespace SubMonitor.App.UI.Common
{
    public sealed class EmailCardView : MonoBehaviour
    {
        private TMP_Text _title;
        private TMP_Text _subtitle;
        private TMP_Text _status;
        private Button _importButton;
        private Button _editButton;
        private Button _deleteButton;
        private EmailAccountDto _account;

        public static EmailCardView Create(Transform parent, TMP_FontAsset font, Sprite roundedSprite)
        {
            RectTransform root = UiFactory.CreatePanel("EmailCard", parent, UiTheme.Surface, roundedSprite, Image.Type.Sliced);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 238f;

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            EmailCardView view = root.gameObject.AddComponent<EmailCardView>();
            view._title = UiFactory.CreateText("Title", root, string.Empty, font, 32f, UiTheme.TextPrimary, FontStyles.Bold, TextAlignmentOptions.Left);
            view._title.gameObject.AddComponent<LayoutElement>().preferredHeight = 42f;

            view._subtitle = UiFactory.CreateText("Subtitle", root, string.Empty, font, 24f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left);
            view._subtitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;

            view._status = UiFactory.CreateText("Status", root, string.Empty, font, 24f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left);
            view._status.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;

            RectTransform buttonsRow = UiFactory.CreatePanel("Buttons", root, Color.clear, UiFactory.WhiteSprite, Image.Type.Simple);
            buttonsRow.GetComponent<Image>().raycastTarget = false;
            buttonsRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 64f;

            HorizontalLayoutGroup buttonsLayout = buttonsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 12f;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            buttonsLayout.childForceExpandWidth = true;

            view._importButton = UiFactory.CreateButton("ImportButton", buttonsRow, "Импорт", UiTheme.Accent, null, font, 24f, UiTheme.White, roundedSprite);
            view._editButton = UiFactory.CreateButton("EditButton", buttonsRow, "Редактировать", UiTheme.SurfaceMuted, null, font, 24f, UiTheme.TextPrimary, roundedSprite);
            view._deleteButton = UiFactory.CreateButton("DeleteButton", buttonsRow, "Удалить", new Color32(249, 232, 232, 255), null, font, 24f, UiTheme.Error, roundedSprite);

            return view;
        }

        public void Bind(
            EmailAccountDto account,
            Action<EmailAccountDto> onImport,
            Action<EmailAccountDto> onEdit,
            Action<EmailAccountDto> onDelete)
        {
            _account = account;
            _title.text = account.email;
            _subtitle.text = "Провайдер: " + account.server_key + "\nПодключен: " + FormatDate(account.created_at);
            _status.text = BuildStatusText(account);
            _status.color = string.IsNullOrWhiteSpace(account.last_error) ? UiTheme.Success : UiTheme.Warning;

            RebindButton(_importButton, () => onImport?.Invoke(_account));
            RebindButton(_editButton, () => onEdit?.Invoke(_account));
            RebindButton(_deleteButton, () => onDelete?.Invoke(_account));
        }

        private static void RebindButton(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(() => action());
            }
        }

        private static string BuildStatusText(EmailAccountDto account)
        {
            if (!string.IsNullOrWhiteSpace(account.last_error))
            {
                return "Последняя ошибка: " + account.last_error;
            }

            if (!string.IsNullOrWhiteSpace(account.last_checked_at))
            {
                return "Проверено: " + FormatDate(account.last_checked_at);
            }

            return account.is_active ? "Активно" : "Неактивно";
        }

        private static string FormatDate(string value)
        {
            if (DateTime.TryParse(value, out DateTime date))
            {
                return date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
            }

            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
