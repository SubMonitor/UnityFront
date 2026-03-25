using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SubMonitor.App.DTO;

namespace SubMonitor.App.UI.Common
{
    public sealed class SubscriptionCardView : MonoBehaviour
    {
        private TMP_Text _title;
        private TMP_Text _meta;
        private TMP_Text _comment;
        private TMP_Text _usage;
        private Button _markUsedButton;
        private Button _actionPlanButton;
        private Button _toggleButton;
        private Button _deleteButton;
        private SubscriptionDto _subscription;

        public static SubscriptionCardView Create(Transform parent, TMP_FontAsset font, Sprite roundedSprite)
        {
            RectTransform root = UiFactory.CreatePanel("SubscriptionCard", parent, UiTheme.Surface, roundedSprite, Image.Type.Sliced);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 396f;

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            SubscriptionCardView view = root.gameObject.AddComponent<SubscriptionCardView>();
            view._title = UiFactory.CreateText("Title", root, string.Empty, font, 32f, UiTheme.TextPrimary, FontStyles.Bold, TextAlignmentOptions.Left);
            view._meta = UiFactory.CreateText("Meta", root, string.Empty, font, 24f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left);
            view._comment = UiFactory.CreateText("Comment", root, string.Empty, font, 24f, UiTheme.TextPrimary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            view._comment.gameObject.AddComponent<LayoutElement>().preferredHeight = 56f;
            view._usage = UiFactory.CreateText("Usage", root, string.Empty, font, 22f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            view._usage.gameObject.AddComponent<LayoutElement>().preferredHeight = 88f;

            RectTransform primaryRow = CreateButtonRow(root);
            RectTransform secondaryRow = CreateButtonRow(root);

            view._markUsedButton = UiFactory.CreateButton(
                "MarkUsedButton",
                primaryRow,
                "Отметить использование",
                new Color32(231, 246, 236, 255),
                null,
                font,
                22f,
                UiTheme.Success,
                roundedSprite);
            view._actionPlanButton = UiFactory.CreateButton(
                "ActionPlanButton",
                primaryRow,
                "Шаблон паузы",
                UiTheme.SurfaceMuted,
                null,
                font,
                22f,
                UiTheme.TextPrimary,
                roundedSprite);
            view._toggleButton = UiFactory.CreateButton(
                "ToggleButton",
                secondaryRow,
                "Деактивировать",
                UiTheme.SurfaceMuted,
                null,
                font,
                22f,
                UiTheme.TextPrimary,
                roundedSprite);
            view._deleteButton = UiFactory.CreateButton(
                "DeleteButton",
                secondaryRow,
                "Удалить",
                new Color32(249, 232, 232, 255),
                null,
                font,
                22f,
                UiTheme.Error,
                roundedSprite);
            return view;
        }

        public void Bind(
            SubscriptionDto subscription,
            SubscriptionUsageStatusDto usageStatus,
            Action<SubscriptionDto> onMarkUsed,
            Action<SubscriptionDto> onCopyActionPlan,
            Action<SubscriptionDto> onToggle,
            Action<SubscriptionDto> onDelete)
        {
            _subscription = subscription;
            _title.text = subscription.name + " • " + subscription.cost.ToString("0.00");
            _meta.text = "Категория: " + subscription.category + "\nСледующее списание: " + FormatDate(subscription.next_payment_date) + " • " + subscription.billing_cycle;
            _comment.text = string.IsNullOrWhiteSpace(subscription.comment) ? "Комментарий не добавлен." : subscription.comment;
            _usage.text = BuildUsageText(usageStatus);
            SetToggleButtonState(subscription.is_active);

            _markUsedButton.onClick.RemoveAllListeners();
            _markUsedButton.onClick.AddListener(() => onMarkUsed?.Invoke(_subscription));

            _actionPlanButton.onClick.RemoveAllListeners();
            _actionPlanButton.onClick.AddListener(() => onCopyActionPlan?.Invoke(_subscription));

            _toggleButton.onClick.RemoveAllListeners();
            _toggleButton.onClick.AddListener(() => onToggle?.Invoke(_subscription));

            _deleteButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.AddListener(() => onDelete?.Invoke(_subscription));
        }

        private void SetToggleButtonState(bool isActive)
        {
            Image image = _toggleButton.GetComponent<Image>();
            TMP_Text label = _toggleButton.GetComponentInChildren<TextMeshProUGUI>();
            label.text = isActive ? "Деактивировать" : "Активировать";
            image.color = isActive ? UiTheme.SurfaceMuted : (Color)new Color32(231, 246, 236, 255);
            label.color = isActive ? UiTheme.TextPrimary : UiTheme.Success;
        }

        private static RectTransform CreateButtonRow(Transform parent)
        {
            RectTransform row = UiFactory.CreatePanel("ButtonsRow", parent, Color.clear, UiFactory.WhiteSprite, Image.Type.Simple);
            row.GetComponent<Image>().raycastTarget = false;
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;

            HorizontalLayoutGroup buttonsLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 12f;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            return row;
        }

        private static string BuildUsageText(SubscriptionUsageStatusDto usageStatus)
        {
            if (usageStatus == null)
            {
                return "Использование: данных пока нет.";
            }

            string lastRecord = string.IsNullOrWhiteSpace(usageStatus.last_recorded_at)
                ? "нет отметок"
                : FormatDate(usageStatus.last_recorded_at);
            return "Использование: " + usageStatus.status_label +
                   " • score " + usageStatus.usage_score +
                   "\nПоследняя отметка: " + lastRecord +
                   "\n" + usageStatus.recommended_action;
        }

        private static string FormatDate(string value)
        {
            if (DateTime.TryParse(value, out DateTime date))
            {
                return date.ToLocalTime().ToString("dd.MM.yyyy");
            }

            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
