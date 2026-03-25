using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SubMonitor.App.Core;
using SubMonitor.App.DTO;
using SubMonitor.App.Services;
using SubMonitor.App.UI.Common;

namespace SubMonitor.SubscriptionsUI
{
    [DisallowMultipleComponent]
    public class SubscriptionsScreenController : MonoBehaviour
    {
        private enum MainScreen
        {
            Dashboard,
            Emails,
            AddEmail,
            ProviderSelect,
            SmtpInfo,
            EmailSuccess,
            EditEmail,
            MailList,
            MailPreview,
            Subscriptions,
            ManualAddSubscription
        }

        private sealed class DashboardView
        {
            public RectTransform Root;
            public TMP_Text GreetingText;
            public TMP_Text SummaryText;
            public RectTransform ChargesContent;
            public Button GoToEmailsButton;
            public Button GoToAddSubscriptionButton;
        }

        private sealed class EmailListView
        {
            public RectTransform Root;
            public TMP_Text SummaryText;
            public RectTransform Content;
            public TMP_Text EmptyText;
            public Button AddButton;
            public Button RefreshButton;
        }

        private sealed class EmailFormView
        {
            public RectTransform Root;
            public TMP_Text TitleText;
            public TMP_Text ProviderText;
            public TMP_Text NoteText;
            public TMP_InputField EmailInput;
            public TMP_InputField PasswordInput;
            public TMP_InputField CustomHostInput;
            public TMP_InputField CustomPortInput;
            public Button ProviderButton;
            public Button InstructionButton;
            public Button ConnectButton;
            public Button BackButton;
        }

        private sealed class MailListView
        {
            public RectTransform Root;
            public TMP_Text TitleText;
            public TMP_Text SummaryText;
            public TMP_InputField KeywordsInput;
            public TMP_InputField DaysBackInput;
            public TMP_InputField FoldersInput;
            public RectTransform ResultsContent;
            public TMP_Text EmptyText;
            public Button SearchButton;
            public Button BackButton;
        }

        private sealed class MailPreviewView
        {
            public RectTransform Root;
            public TMP_Text TitleText;
            public TMP_Text MetaText;
            public TMP_Text BodyText;
            public RectTransform CandidatePanel;
            public TMP_Text CandidateText;
            public Button ParseButton;
            public Button ConfirmButton;
            public Button BackButton;
        }

        private sealed class SubscriptionListView
        {
            public RectTransform Root;
            public TMP_Text SummaryText;
            public RectTransform Content;
            public TMP_Text EmptyText;
            public Button AddButton;
            public Button RefreshButton;
        }

        private sealed class ManualSubscriptionView
        {
            public RectTransform Root;
            public TMP_InputField NameInput;
            public TMP_InputField CategoryInput;
            public TMP_InputField CostInput;
            public TMP_InputField BillingCycleInput;
            public TMP_InputField PaymentDateInput;
            public TMP_InputField CommentInput;
            public Button SaveButton;
            public Button BackButton;
        }

        [Header("Visuals")]
        [SerializeField] private Sprite _roundedSprite;

        [Header("Navigation")]
        [SerializeField] private string authSceneName = "Auth";

        [Header("Core Scene References")]
        [SerializeField] private RectTransform headerRoot;
        [SerializeField] private NotificationBannerView notificationBanner;
        [SerializeField] private LoaderOverlayView loaderOverlay;
        [SerializeField] private Button dashboardNavButton;
        [SerializeField] private Button emailsNavButton;
        [SerializeField] private Button subscriptionsNavButton;
        [SerializeField] private Button logoutButton;

        private static readonly string[] DefaultKeywords =
        {
            "подписка", "subscription", "payment", "renewal", "premium", "plan", "списание", "invoice"
        };

        private AppServices _services;
        private TMP_FontAsset _fontAsset;

        private DashboardView _dashboardView;
        private EmailListView _emailListView;
        private EmailFormView _addEmailView;
        private EmailFormView _editEmailView;
        private RectTransform _providerSelectScreen;
        private RectTransform _providerSelectContent;
        private TMP_Text _providerSelectSummaryText;
        private Button _providerSelectBackButton;
        private RectTransform _smtpInfoScreen;
        private TMP_Text _smtpInfoTitle;
        private TMP_Text _smtpInfoBody;
        private TMP_Text _smtpInfoLink;
        private Button _smtpContinueButton;
        private Button _smtpBackButton;
        private RectTransform _emailSuccessScreen;
        private TMP_Text _emailSuccessText;
        private Button _emailSuccessOpenEmailsButton;
        private MailListView _mailListView;
        private MailPreviewView _mailPreviewView;
        private SubscriptionListView _subscriptionListView;
        private ManualSubscriptionView _manualSubscriptionView;

        private EmailServerDto[] _servers = Array.Empty<EmailServerDto>();
        private EmailAccountDto[] _accounts = Array.Empty<EmailAccountDto>();
        private SubscriptionDto[] _subscriptions = Array.Empty<SubscriptionDto>();
        private SubscriptionInsightsDto _subscriptionInsights;
        private EmailPreviewDto[] _mailPreviews = Array.Empty<EmailPreviewDto>();

        private EmailServerDto _selectedServer;
        private EmailAccountDto _selectedAccount;
        private EmailAccountDto _editingAccount;
        private EmailPreviewDto _selectedPreview;
        private EmailDetailDto _selectedEmailDetail;
        private SubscriptionRequestDto _parsedSubscriptionDraft;
        private bool _providerSelectionTargetsEditFlow;

        private void Awake()
        {
            _services = ServiceRegistry.Current;
            _fontAsset = UiTheme.ResolveFontAsset();
            EnsureSceneBindings();
            BindButtons();
            SetScreen(MainScreen.Dashboard);

            if (Application.isPlaying)
            {
                _ = InitializeAsync();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureSceneBindings();
        }

        [ContextMenu("Auto Bind Scene UI")]
        private void AutoBindSceneUi()
        {
            EnsureSceneBindings();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public void RebuildInEditMode()
        {
            EnsureSceneBindings();
        }

        public async void OnOpenDashboard()
        {
            await LoadDashboardAsync();
        }

        public async void OnOpenEmails()
        {
            await LoadEmailsAsync();
        }

        public async void OnOpenSubscriptions()
        {
            await LoadSubscriptionsAsync();
        }

        public void OnLogout()
        {
            _services.Auth.Logout();
            SceneManager.LoadScene(authSceneName);
        }

        public void OnOpenAddEmail()
        {
            ResetEmailFlow();
            PopulateEmailForm(_addEmailView, null, false);
            SetScreen(MainScreen.AddEmail);
        }

        public void OnOpenManualAddSubscription()
        {
            ResetManualSubscriptionForm();
            SetScreen(MainScreen.ManualAddSubscription);
        }

        public async void OnOpenProviderSelectionFromAdd()
        {
            _providerSelectionTargetsEditFlow = false;
            await OpenProviderSelectionAsync();
        }

        public async void OnOpenProviderSelectionFromEdit()
        {
            _providerSelectionTargetsEditFlow = true;
            await OpenProviderSelectionAsync();
        }

        public void OnBackToAddEmail()
        {
            SetScreen(_providerSelectionTargetsEditFlow ? MainScreen.EditEmail : MainScreen.AddEmail);
        }

        public async void OnConnectAddEmail()
        {
            await ConnectEmailAsync(_addEmailView, false);
        }

        public async void OnConnectEditEmail()
        {
            await ConnectEmailAsync(_editEmailView, true);
        }

        public async void OnSearchMails()
        {
            await RunMailSearchAsync();
        }

        public async void OnRefreshSubscriptions()
        {
            await LoadSubscriptionsAsync();
        }

        public async void OnSaveManualSubscription()
        {
            await SaveManualSubscriptionAsync();
        }

        private async Task InitializeAsync()
        {
            if (!_services.SessionStore.HasToken())
            {
                SceneManager.LoadScene(authSceneName);
                return;
            }

            await LoadDashboardAsync();
        }

        private void EnsureSceneBindings()
        {
            notificationBanner = notificationBanner != null ? notificationBanner : FindComponent<NotificationBannerView>("NotificationBanner");
            loaderOverlay = loaderOverlay != null ? loaderOverlay : FindComponent<LoaderOverlayView>("LoaderOverlay");
            headerRoot = headerRoot != null ? headerRoot : FindRect("Header");

            dashboardNavButton = dashboardNavButton != null ? dashboardNavButton : FindButtonByLabel(headerRoot, "Dashboard");
            emailsNavButton = emailsNavButton != null ? emailsNavButton : FindButtonByLabel(headerRoot, "Почта");
            subscriptionsNavButton = subscriptionsNavButton != null ? subscriptionsNavButton : FindButtonByLabel(headerRoot, "Подписки");
            logoutButton = logoutButton != null ? logoutButton : FindButtonInScope(headerRoot, "LogoutButton");

            _dashboardView = BindDashboardView(_dashboardView);
            _emailListView = BindEmailListView(_emailListView, "EmailsScreen");
            _addEmailView = BindEmailFormView(_addEmailView, "AddEmailScreen");
            _editEmailView = BindEmailFormView(_editEmailView, "EditEmailScreen");
            BindProviderSelectView();
            BindSmtpInfoView();
            BindEmailSuccessView();
            _mailListView = BindMailListView(_mailListView);
            _mailPreviewView = BindMailPreviewView(_mailPreviewView);
            _subscriptionListView = BindSubscriptionListView(_subscriptionListView);
            _manualSubscriptionView = BindManualSubscriptionView(_manualSubscriptionView);

            if (Application.isPlaying)
            {
                ConfigureResponsiveLayout();
            }
        }

        private void ConfigureResponsiveLayout()
        {
            RectTransform safeArea = FindRect("SafeArea");
            EnsureComponent<ResponsiveSafeArea>(safeArea);

            RectTransform body = FindRectInScope(safeArea, "Body");
            ResponsiveWidthLimiter bodyLimiter = EnsureComponent<ResponsiveWidthLimiter>(body);
            if (bodyLimiter != null)
            {
                bodyLimiter.Configure(1440f);
            }

            ResponsiveWidthLimiter headerLimiter = EnsureComponent<ResponsiveWidthLimiter>(headerRoot);
            if (headerLimiter != null)
            {
                headerLimiter.Configure(1440f);
            }
        }

        private DashboardView BindDashboardView(DashboardView view)
        {
            view = view ?? new DashboardView();
            view.Root = view.Root != null ? view.Root : FindRect("DashboardScreen");

            Transform content = GetScrollContent(view.Root);
            view.GreetingText = view.GreetingText != null ? view.GreetingText : FindDirectText(content, "SectionTitle", 0);
            view.SummaryText = view.SummaryText != null ? view.SummaryText : FindDirectText(content, "SectionBody", 0);
            view.ChargesContent = view.ChargesContent != null ? view.ChargesContent : FindRectInScope(view.Root, "ChargesContent");
            view.GoToEmailsButton = view.GoToEmailsButton != null ? view.GoToEmailsButton : FindButtonInScope(view.Root, "GoToEmails");
            view.GoToAddSubscriptionButton = view.GoToAddSubscriptionButton != null ? view.GoToAddSubscriptionButton : FindButtonInScope(view.Root, "GoToAddSub");
            return view;
        }

        private EmailListView BindEmailListView(EmailListView view, string screenName)
        {
            view = view ?? new EmailListView();
            view.Root = view.Root != null ? view.Root : FindRect(screenName);

            Transform content = GetScrollContent(view.Root);
            view.SummaryText = view.SummaryText != null ? view.SummaryText : FindDirectText(content, "SectionBody", 0);
            view.EmptyText = view.EmptyText != null ? view.EmptyText : FindDirectText(content, "SectionBody", 1);
            view.Content = view.Content != null ? view.Content : FindRectInScope(view.Root, "EmailCardsContent");
            view.AddButton = view.AddButton != null ? view.AddButton : FindButtonInScope(view.Root, "AddEmail");
            view.RefreshButton = view.RefreshButton != null ? view.RefreshButton : FindButtonInScope(view.Root, "RefreshEmails");
            return view;
        }

        private EmailFormView BindEmailFormView(EmailFormView view, string screenName)
        {
            view = view ?? new EmailFormView();
            view.Root = view.Root != null ? view.Root : FindRect(screenName);

            Transform content = GetScrollContent(view.Root);
            view.TitleText = view.TitleText != null ? view.TitleText : FindDirectText(content, "SectionTitle", 0);
            view.ProviderText = view.ProviderText != null ? view.ProviderText : FindTextInScope(view.Root, "ProviderValue");
            view.NoteText = view.NoteText != null ? view.NoteText : FindTextInScope(FindRectInScope(view.Root, "ProviderCard"), "SectionBody");
            view.EmailInput = view.EmailInput != null ? view.EmailInput : FindInputInScope(view.Root, "EmailInput");
            view.PasswordInput = view.PasswordInput != null ? view.PasswordInput : FindInputInScope(view.Root, "PasswordInput");
            view.CustomHostInput = view.CustomHostInput != null ? view.CustomHostInput : FindInputInScope(view.Root, "CustomHostInput");
            view.CustomPortInput = view.CustomPortInput != null ? view.CustomPortInput : FindInputInScope(view.Root, "CustomPortInput");
            view.ProviderButton = view.ProviderButton != null ? view.ProviderButton : FindButtonInScope(view.Root, "ProviderButton");
            view.InstructionButton = view.InstructionButton != null ? view.InstructionButton : FindButtonInScope(view.Root, "InstructionButton");
            view.ConnectButton = view.ConnectButton != null ? view.ConnectButton : FindButtonInScope(view.Root, "ConnectButton");
            view.BackButton = view.BackButton != null ? view.BackButton : FindButtonInScope(view.Root, "BackButton");
            return view;
        }

        private void BindProviderSelectView()
        {
            _providerSelectScreen = _providerSelectScreen != null ? _providerSelectScreen : FindRect("ProviderSelectScreen");
            Transform content = GetScrollContent(_providerSelectScreen);
            _providerSelectSummaryText = _providerSelectSummaryText != null ? _providerSelectSummaryText : FindDirectText(content, "SectionBody", 0);
            _providerSelectContent = _providerSelectContent != null ? _providerSelectContent : FindRectInScope(_providerSelectScreen, "ProviderCardsContent");
            _providerSelectBackButton = _providerSelectBackButton != null ? _providerSelectBackButton : FindButtonInScope(_providerSelectScreen, "BackFromProviders");
        }

        private void BindSmtpInfoView()
        {
            _smtpInfoScreen = _smtpInfoScreen != null ? _smtpInfoScreen : FindRect("SmtpInfoScreen");
            Transform content = GetScrollContent(_smtpInfoScreen);
            _smtpInfoTitle = _smtpInfoTitle != null ? _smtpInfoTitle : FindDirectText(content, "SectionTitle", 0);
            _smtpInfoBody = _smtpInfoBody != null ? _smtpInfoBody : FindDirectText(content, "SectionBody", 0);
            _smtpInfoLink = _smtpInfoLink != null ? _smtpInfoLink : FindDirectText(content, "SectionBody", 1);
            _smtpContinueButton = _smtpContinueButton != null ? _smtpContinueButton : FindButtonInScope(_smtpInfoScreen, "ContinueFromInstruction");
            _smtpBackButton = _smtpBackButton != null ? _smtpBackButton : FindButtonInScope(_smtpInfoScreen, "BackToProviders");
        }

        private void BindEmailSuccessView()
        {
            _emailSuccessScreen = _emailSuccessScreen != null ? _emailSuccessScreen : FindRect("EmailSuccessScreen");
            Transform content = GetScrollContent(_emailSuccessScreen);
            _emailSuccessText = _emailSuccessText != null ? _emailSuccessText : FindDirectText(content, "SectionBody", 0);
            _emailSuccessOpenEmailsButton = _emailSuccessOpenEmailsButton != null ? _emailSuccessOpenEmailsButton : FindButtonInScope(_emailSuccessScreen, "OpenEmailsButton");
        }

        private MailListView BindMailListView(MailListView view)
        {
            view = view ?? new MailListView();
            view.Root = view.Root != null ? view.Root : FindRect("MailListScreen");

            Transform content = GetScrollContent(view.Root);
            view.TitleText = view.TitleText != null ? view.TitleText : FindDirectText(content, "SectionTitle", 0);
            view.SummaryText = view.SummaryText != null ? view.SummaryText : FindDirectText(content, "SectionBody", 0);
            view.EmptyText = view.EmptyText != null ? view.EmptyText : FindDirectText(content, "SectionBody", 1);
            view.KeywordsInput = view.KeywordsInput != null ? view.KeywordsInput : FindInputInScope(view.Root, "KeywordsInput");
            view.DaysBackInput = view.DaysBackInput != null ? view.DaysBackInput : FindInputInScope(view.Root, "DaysBackInput");
            view.FoldersInput = view.FoldersInput != null ? view.FoldersInput : FindInputInScope(view.Root, "FoldersInput");
            view.ResultsContent = view.ResultsContent != null ? view.ResultsContent : FindRectInScope(view.Root, "MailCardsContent");
            view.SearchButton = view.SearchButton != null ? view.SearchButton : FindButtonInScope(view.Root, "SearchButton");
            view.BackButton = view.BackButton != null ? view.BackButton : FindButtonInScope(view.Root, "BackToEmails");
            return view;
        }

        private MailPreviewView BindMailPreviewView(MailPreviewView view)
        {
            view = view ?? new MailPreviewView();
            view.Root = view.Root != null ? view.Root : FindRect("MailPreviewScreen");

            Transform content = GetScrollContent(view.Root);
            view.TitleText = view.TitleText != null ? view.TitleText : FindDirectText(content, "SectionTitle", 0);
            view.MetaText = view.MetaText != null ? view.MetaText : FindDirectText(content, "SectionBody", 0);
            view.BodyText = view.BodyText != null ? view.BodyText : FindDirectText(content, "SectionBody", 1);
            view.CandidatePanel = view.CandidatePanel != null ? view.CandidatePanel : FindRectInScope(view.Root, "CandidatePanel");
            view.CandidateText = view.CandidateText != null ? view.CandidateText : FindTextInScope(view.CandidatePanel, "SectionBody");
            view.ParseButton = view.ParseButton != null ? view.ParseButton : FindButtonInScope(view.Root, "ParseButton");
            view.ConfirmButton = view.ConfirmButton != null ? view.ConfirmButton : FindButtonInScope(view.Root, "ConfirmImportButton");
            view.BackButton = view.BackButton != null ? view.BackButton : FindButtonInScope(view.Root, "BackToMailList");
            return view;
        }

        private SubscriptionListView BindSubscriptionListView(SubscriptionListView view)
        {
            view = view ?? new SubscriptionListView();
            view.Root = view.Root != null ? view.Root : FindRect("SubscriptionsScreen");

            Transform content = GetScrollContent(view.Root);
            view.SummaryText = view.SummaryText != null ? view.SummaryText : FindDirectText(content, "SectionBody", 0);
            view.EmptyText = view.EmptyText != null ? view.EmptyText : FindDirectText(content, "SectionBody", 1);
            view.Content = view.Content != null ? view.Content : FindRectInScope(view.Root, "SubscriptionCardsContent");
            view.AddButton = view.AddButton != null ? view.AddButton : FindButtonInScope(view.Root, "AddSubscription");
            view.RefreshButton = view.RefreshButton != null ? view.RefreshButton : FindButtonInScope(view.Root, "RefreshSubscription");
            return view;
        }

        private ManualSubscriptionView BindManualSubscriptionView(ManualSubscriptionView view)
        {
            view = view ?? new ManualSubscriptionView();
            view.Root = view.Root != null ? view.Root : FindRect("ManualSubscriptionScreen");
            view.NameInput = view.NameInput != null ? view.NameInput : FindInputInScope(view.Root, "NameInput");
            view.CategoryInput = view.CategoryInput != null ? view.CategoryInput : FindInputInScope(view.Root, "CategoryInput");
            view.CostInput = view.CostInput != null ? view.CostInput : FindInputInScope(view.Root, "CostInput");
            view.BillingCycleInput = view.BillingCycleInput != null ? view.BillingCycleInput : FindInputInScope(view.Root, "BillingCycleInput");
            view.PaymentDateInput = view.PaymentDateInput != null ? view.PaymentDateInput : FindInputInScope(view.Root, "PaymentDateInput");
            view.CommentInput = view.CommentInput != null ? view.CommentInput : FindInputInScope(view.Root, "CommentInput");
            view.SaveButton = view.SaveButton != null ? view.SaveButton : FindButtonInScope(view.Root, "SaveSubscription");
            view.BackButton = view.BackButton != null ? view.BackButton : FindButtonInScope(view.Root, "BackToSubscriptions");
            return view;
        }

        private void BindButtons()
        {
            BindButton(dashboardNavButton, OnOpenDashboard);
            BindButton(emailsNavButton, OnOpenEmails);
            BindButton(subscriptionsNavButton, OnOpenSubscriptions);
            BindButton(logoutButton, OnLogout);

            BindButton(_dashboardView.GoToEmailsButton, OnOpenEmails);
            BindButton(_dashboardView.GoToAddSubscriptionButton, OnOpenManualAddSubscription);

            BindButton(_emailListView.AddButton, OnOpenAddEmail);
            BindButton(_emailListView.RefreshButton, OnOpenEmails);

            BindButton(_addEmailView.ProviderButton, OnOpenProviderSelectionFromAdd);
            BindButton(_addEmailView.InstructionButton, () => OpenInstructionForCurrentServer(false));
            BindButton(_addEmailView.ConnectButton, OnConnectAddEmail);
            BindButton(_addEmailView.BackButton, OnOpenEmails);

            BindButton(_editEmailView.ProviderButton, OnOpenProviderSelectionFromEdit);
            BindButton(_editEmailView.InstructionButton, () => OpenInstructionForCurrentServer(true));
            BindButton(_editEmailView.ConnectButton, OnConnectEditEmail);
            BindButton(_editEmailView.BackButton, OnOpenEmails);

            BindButton(_providerSelectBackButton, OnBackToAddEmail);
            BindButton(_smtpContinueButton, OnBackToAddEmail);
            BindButton(_smtpBackButton, () => SetScreen(MainScreen.ProviderSelect));
            BindButton(_emailSuccessOpenEmailsButton, OnOpenEmails);

            BindButton(_mailListView.SearchButton, OnSearchMails);
            BindButton(_mailListView.BackButton, OnOpenEmails);

            BindButton(_mailPreviewView.ParseButton, OnParseSubscriptionClicked);
            BindButton(_mailPreviewView.ConfirmButton, OnConfirmImportedSubscriptionClicked);
            BindButton(_mailPreviewView.BackButton, () => SetScreen(MainScreen.MailList));

            BindButton(_subscriptionListView.AddButton, OnOpenManualAddSubscription);
            BindButton(_subscriptionListView.RefreshButton, OnRefreshSubscriptions);

            BindButton(_manualSubscriptionView.SaveButton, OnSaveManualSubscription);
            BindButton(_manualSubscriptionView.BackButton, OnOpenSubscriptions);
        }

        private async Task LoadDashboardAsync()
        {
            SetBusy(true, "Загружаем dashboard...");

            ApiResult<EmailAccountDto[]> emailResult = await _services.Emails.GetAccountsAsync();
            if (HandleFailure(emailResult))
            {
                SetBusy(false);
                return;
            }

            ApiResult<SubscriptionDto[]> subscriptionResult = await _services.Subscriptions.GetAllAsync();
            if (HandleFailure(subscriptionResult))
            {
                SetBusy(false);
                return;
            }

            ApiResult<SubscriptionInsightsDto> insightsResult = await _services.Subscriptions.GetInsightsAsync();
            if (HandleFailure(insightsResult))
            {
                SetBusy(false);
                return;
            }

            _accounts = emailResult.Data ?? Array.Empty<EmailAccountDto>();
            _subscriptions = subscriptionResult.Data ?? Array.Empty<SubscriptionDto>();
            _subscriptionInsights = insightsResult.Data;

            UpdateDashboardUi();
            SetBusy(false);
            SetScreen(MainScreen.Dashboard);
        }

        private async Task LoadEmailsAsync()
        {
            SetBusy(true, "Загружаем email...");

            ApiResult<EmailAccountDto[]> result = await _services.Emails.GetAccountsAsync();
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            _accounts = result.Data ?? Array.Empty<EmailAccountDto>();
            RefreshEmailListUi();
            SetScreen(MainScreen.Emails);
        }

        private async Task LoadSubscriptionsAsync()
        {
            SetBusy(true, "Загружаем подписки...");

            ApiResult<SubscriptionDto[]> result = await _services.Subscriptions.GetAllAsync();

            if (HandleFailure(result))
            {
                SetBusy(false);
                return;
            }

            ApiResult<SubscriptionInsightsDto> insightsResult = await _services.Subscriptions.GetInsightsAsync();
            SetBusy(false);

            if (HandleFailure(insightsResult))
            {
                return;
            }

            _subscriptions = result.Data ?? Array.Empty<SubscriptionDto>();
            _subscriptionInsights = insightsResult.Data;
            RefreshSubscriptionsUi();
            SetScreen(MainScreen.Subscriptions);
        }

        private async Task OpenProviderSelectionAsync()
        {
            if (!await EnsureServersLoadedAsync())
            {
                return;
            }

            PopulateProviderSelectionUi();
            SetScreen(MainScreen.ProviderSelect);
        }

        private async Task ConnectEmailAsync(EmailFormView view, bool isEditMode)
        {
            if (_selectedServer == null)
            {
                notificationBanner.ShowError("Сначала выберите тип почты.");
                return;
            }

            if (view == null || view.EmailInput == null || view.PasswordInput == null)
            {
                notificationBanner.ShowError("На сцене не найдена форма подключения email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(view.EmailInput.text) || string.IsNullOrWhiteSpace(view.PasswordInput.text))
            {
                notificationBanner.ShowError("Введите email и пароль приложения.");
                return;
            }

            int customPort = 993;
            if (!string.IsNullOrWhiteSpace(view.CustomPortInput.text) && !int.TryParse(view.CustomPortInput.text, out customPort))
            {
                notificationBanner.ShowError("Порт должен быть числом.");
                return;
            }

            SetBusy(true, isEditMode ? "Сохраняем обновленный email..." : "Подключаем email...");

            ApiResult<EmailConnectResponseDto> result = await _services.Emails.ConnectAsync(new EmailConnectRequestDto
            {
                email = view.EmailInput.text.Trim(),
                password = view.PasswordInput.text,
                server_key = _selectedServer.key,
                custom_host = string.IsNullOrWhiteSpace(view.CustomHostInput.text) ? null : view.CustomHostInput.text.Trim(),
                custom_port = customPort
            });

            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            if (_emailSuccessText != null)
            {
                _emailSuccessText.text = result.Data.message + "\nEmail: " + view.EmailInput.text.Trim();
            }

            notificationBanner.ShowSuccess(result.Data.message);
            SetScreen(MainScreen.EmailSuccess);
        }

        private async Task RunMailSearchAsync()
        {
            if (_selectedAccount == null)
            {
                notificationBanner.ShowError("Сначала выберите email для импорта.");
                return;
            }

            string[] keywords = SplitInput(_mailListView.KeywordsInput != null ? _mailListView.KeywordsInput.text : string.Empty);
            string[] folders = SplitInput(_mailListView.FoldersInput != null ? _mailListView.FoldersInput.text : string.Empty);

            int daysBack;
            if (_mailListView.DaysBackInput == null || !int.TryParse(_mailListView.DaysBackInput.text, out daysBack))
            {
                notificationBanner.ShowError("Количество дней должно быть числом.");
                return;
            }

            SetBusy(true, "Ищем письма по ключевым словам...");

            ApiResult<EmailSearchResponseDto> result = await _services.Emails.SearchAsync(_selectedAccount.id, new EmailSearchRequestDto
            {
                keywords = keywords.Length == 0 ? DefaultKeywords : keywords,
                days_back = Mathf.Clamp(daysBack, 1, 30),
                folders = folders.Length == 0 ? new[] { "INBOX" } : folders,
                max_emails = 50
            });

            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            _mailPreviews = result.Data.emails ?? Array.Empty<EmailPreviewDto>();
            RefreshMailSearchUi(result.Data.message);
            SetScreen(MainScreen.MailList);
        }

        private async Task SaveManualSubscriptionAsync()
        {
            float cost;
            if (!TryParseFloat(_manualSubscriptionView.CostInput != null ? _manualSubscriptionView.CostInput.text : null, out cost))
            {
                notificationBanner.ShowError("Стоимость должна быть числом.");
                return;
            }

            string isoDate;
            if (!TryParseInputDate(_manualSubscriptionView.PaymentDateInput != null ? _manualSubscriptionView.PaymentDateInput.text : null, out isoDate))
            {
                notificationBanner.ShowError("Не удалось распознать дату. Используйте dd.MM.yyyy или yyyy-MM-dd.");
                return;
            }

            if (_manualSubscriptionView.NameInput == null ||
                _manualSubscriptionView.CategoryInput == null ||
                _manualSubscriptionView.BillingCycleInput == null ||
                string.IsNullOrWhiteSpace(_manualSubscriptionView.NameInput.text) ||
                string.IsNullOrWhiteSpace(_manualSubscriptionView.CategoryInput.text) ||
                string.IsNullOrWhiteSpace(_manualSubscriptionView.BillingCycleInput.text))
            {
                notificationBanner.ShowError("Заполните название, категорию и периодичность.");
                return;
            }

            SetBusy(true, "Сохраняем подписку...");
            ApiResult<SubscriptionDto> result = await _services.Subscriptions.AddAsync(new SubscriptionRequestDto
            {
                name = _manualSubscriptionView.NameInput.text.Trim(),
                category = _manualSubscriptionView.CategoryInput.text.Trim(),
                comment = _manualSubscriptionView.CommentInput != null ? _manualSubscriptionView.CommentInput.text.Trim() : string.Empty,
                cost = cost,
                billing_cycle = _manualSubscriptionView.BillingCycleInput.text.Trim().ToLowerInvariant(),
                payment_date = isoDate,
                is_next_date = true
            });
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            notificationBanner.ShowSuccess("Подписка добавлена.");
            await LoadSubscriptionsAsync();
        }

        private void PopulateEmailForm(EmailFormView view, EmailAccountDto account, bool isEditMode)
        {
            if (view == null)
            {
                return;
            }

            if (view.TitleText != null)
            {
                view.TitleText.text = isEditMode ? "Редактирование email" : "Подключение email";
            }

            if (view.EmailInput != null)
            {
                view.EmailInput.text = account != null ? account.email : string.Empty;
                view.EmailInput.interactable = !isEditMode;
            }

            if (view.PasswordInput != null)
            {
                view.PasswordInput.text = string.Empty;
            }

            if (view.CustomHostInput != null)
            {
                view.CustomHostInput.text = string.Empty;
            }

            if (view.CustomPortInput != null)
            {
                view.CustomPortInput.text = "993";
            }

            if (view.NoteText != null)
            {
                view.NoteText.text = isEditMode
                    ? "Адрес email заблокирован на форме, потому что backend обновляет параметры только по тому же адресу. Для смены email используйте удаление и новое подключение."
                    : "Для Gmail, Яндекс и Mail.ru обычно нужен пароль приложения.";
            }

            if (account != null && _servers.Length > 0)
            {
                _selectedServer = _servers.FirstOrDefault(server => string.Equals(server.key, account.server_key, StringComparison.OrdinalIgnoreCase));
            }

            UpdateEmailFormServerState(view);
        }

        private void ResetEmailFlow()
        {
            _selectedServer = null;
            _editingAccount = null;

            if (_addEmailView != null)
            {
                PopulateEmailForm(_addEmailView, null, false);
            }
        }

        private void ResetManualSubscriptionForm()
        {
            if (_manualSubscriptionView == null)
            {
                return;
            }

            if (_manualSubscriptionView.NameInput != null)
            {
                _manualSubscriptionView.NameInput.text = string.Empty;
            }

            if (_manualSubscriptionView.CategoryInput != null)
            {
                _manualSubscriptionView.CategoryInput.text = string.Empty;
            }

            if (_manualSubscriptionView.CostInput != null)
            {
                _manualSubscriptionView.CostInput.text = string.Empty;
            }

            if (_manualSubscriptionView.BillingCycleInput != null)
            {
                _manualSubscriptionView.BillingCycleInput.text = "month";
            }

            if (_manualSubscriptionView.PaymentDateInput != null)
            {
                _manualSubscriptionView.PaymentDateInput.text = DateTime.Today.AddDays(30).ToString("dd.MM.yyyy");
            }

            if (_manualSubscriptionView.CommentInput != null)
            {
                _manualSubscriptionView.CommentInput.text = string.Empty;
            }
        }

        private void SetScreen(MainScreen screen)
        {
            ToggleScreen(_dashboardView != null ? _dashboardView.Root : null, screen == MainScreen.Dashboard);
            ToggleScreen(_emailListView != null ? _emailListView.Root : null, screen == MainScreen.Emails);
            ToggleScreen(_addEmailView != null ? _addEmailView.Root : null, screen == MainScreen.AddEmail);
            ToggleScreen(_providerSelectScreen, screen == MainScreen.ProviderSelect);
            ToggleScreen(_smtpInfoScreen, screen == MainScreen.SmtpInfo);
            ToggleScreen(_emailSuccessScreen, screen == MainScreen.EmailSuccess);
            ToggleScreen(_editEmailView != null ? _editEmailView.Root : null, screen == MainScreen.EditEmail);
            ToggleScreen(_mailListView != null ? _mailListView.Root : null, screen == MainScreen.MailList);
            ToggleScreen(_mailPreviewView != null ? _mailPreviewView.Root : null, screen == MainScreen.MailPreview);
            ToggleScreen(_subscriptionListView != null ? _subscriptionListView.Root : null, screen == MainScreen.Subscriptions);
            ToggleScreen(_manualSubscriptionView != null ? _manualSubscriptionView.Root : null, screen == MainScreen.ManualAddSubscription);

            ApplyNavigationState(screen);
        }

        private void UpdateDashboardUi()
        {
            if (_dashboardView == null)
            {
                return;
            }

            if (_dashboardView.GreetingText != null)
            {
                string email = _services.SessionStore.GetUserEmail();
                _dashboardView.GreetingText.text = "Здравствуйте, " + (!string.IsNullOrWhiteSpace(email) ? email : "пользователь");
            }

            if (_dashboardView.SummaryText != null)
            {
                _dashboardView.SummaryText.text =
                    "Подключено email: " + _accounts.Length +
                    "\nПодписок в системе: " + _subscriptions.Length +
                    "\nАктивных подписок: " + _subscriptions.Count(subscription => subscription.is_active);
            }

            if (_dashboardView.SummaryText != null && _subscriptionInsights != null && _subscriptionInsights.summary != null)
            {
                _dashboardView.SummaryText.text =
                    "Подключено email: " + _accounts.Length +
                    "\nПодписок в системе: " + _subscriptions.Length +
                    "\nАктивных подписок: " + _subscriptions.Count(subscription => subscription.is_active) +
                    "\nРасход в месяц: " + _subscriptionInsights.summary.monthly_total.ToString("0.00") +
                    "\nПотенциал экономии за год: " + _subscriptionInsights.summary.savings_opportunity_total.ToString("0.00");
            }

            if (_dashboardView.ChargesContent == null)
            {
                return;
            }

            UiFactory.DestroyChildren(_dashboardView.ChargesContent);

            if (_subscriptionInsights != null)
            {
                bool hasCards = false;

                if (_subscriptionInsights.upcoming_charges != null)
                {
                    foreach (SubscriptionUpcomingChargeDto charge in _subscriptionInsights.upcoming_charges.Take(3))
                    {
                        CreateInfoCard(
                            _dashboardView.ChargesContent,
                            "Ближайшее списание: " + charge.service_name +
                            " • " + charge.cost.ToString("0.00") +
                            " • через " + charge.days_left + " дн.");
                        hasCards = true;
                    }
                }

                if (_subscriptionInsights.alerts != null)
                {
                    foreach (SubscriptionAlertDto alert in _subscriptionInsights.alerts.Take(2))
                    {
                        CreateInfoCard(_dashboardView.ChargesContent, "Алерт: " + alert.title + "\n" + alert.message);
                        hasCards = true;
                    }
                }

                if (_subscriptionInsights.recommendations != null)
                {
                    foreach (SubscriptionRecommendationDto recommendation in _subscriptionInsights.recommendations.Take(2))
                    {
                        string alternatives = recommendation.alternative_services != null && recommendation.alternative_services.Length > 0
                            ? string.Join(", ", recommendation.alternative_services)
                            : "нет данных";
                        CreateInfoCard(
                            _dashboardView.ChargesContent,
                            "Рекомендация: " + recommendation.service_name +
                            "\n" + recommendation.reason +
                            "\nАльтернативы: " + alternatives);
                        hasCards = true;
                    }
                }

                if (!hasCards)
                {
                    CreateInfoCard(_dashboardView.ChargesContent, "Пока нет insight-данных по подпискам.");
                }

                return;
            }

            SubscriptionDto[] nearest = _subscriptions
                .Where(subscription => !string.IsNullOrWhiteSpace(subscription.next_payment_date))
                .OrderBy(subscription => ParseDateTimeSafe(subscription.next_payment_date))
                .Take(5)
                .ToArray();

            if (nearest.Length == 0)
            {
                CreateInfoCard(_dashboardView.ChargesContent, "Пока нет ближайших списаний.");
                return;
            }

            foreach (SubscriptionDto subscription in nearest)
            {
                string line = subscription.name + " • " + subscription.cost.ToString("0.00") + " • " + FormatDate(subscription.next_payment_date);
                CreateInfoCard(_dashboardView.ChargesContent, line);
            }
        }

        private void RefreshEmailListUi()
        {
            if (_emailListView == null)
            {
                return;
            }

            if (_emailListView.SummaryText != null)
            {
                _emailListView.SummaryText.text = "Подключений: " + _accounts.Length;
            }

            if (_emailListView.Content != null)
            {
                UiFactory.DestroyChildren(_emailListView.Content);
            }

            if (_emailListView.EmptyText != null)
            {
                _emailListView.EmptyText.gameObject.SetActive(_accounts.Length == 0);
            }

            if (_emailListView.Content == null)
            {
                return;
            }

            foreach (EmailAccountDto account in _accounts)
            {
                EmailCardView card = EmailCardView.Create(_emailListView.Content, _fontAsset, RoundedSprite);
                card.Bind(account, OpenMailImportFlow, OpenEditEmail, DeleteEmail);
            }
        }

        private void RefreshSubscriptionsUi()
        {
            if (_subscriptionListView == null)
            {
                return;
            }

            if (_subscriptionListView.SummaryText != null)
            {
                _subscriptionListView.SummaryText.text = "Всего подписок: " + _subscriptions.Length;
            }

            if (_subscriptionListView.SummaryText != null && _subscriptionInsights != null && _subscriptionInsights.summary != null)
            {
                _subscriptionListView.SummaryText.text = "Всего подписок: " + _subscriptions.Length +
                                                         "\nНуждаются в проверке: " + _subscriptionInsights.summary.needs_attention_count;
            }

            if (_subscriptionListView.Content != null)
            {
                UiFactory.DestroyChildren(_subscriptionListView.Content);
            }

            if (_subscriptionListView.EmptyText != null)
            {
                _subscriptionListView.EmptyText.gameObject.SetActive(_subscriptions.Length == 0);
            }

            if (_subscriptionListView.Content == null)
            {
                return;
            }

            foreach (SubscriptionDto subscription in _subscriptions.OrderBy(item => ParseDateTimeSafe(item.next_payment_date)))
            {
                SubscriptionCardView card = SubscriptionCardView.Create(_subscriptionListView.Content, _fontAsset, RoundedSprite);
                card.Bind(
                    subscription,
                    FindUsageStatus(subscription.id),
                    RecordSubscriptionUsage,
                    CopySubscriptionActionPlan,
                    ToggleSubscriptionActive,
                    DeleteSubscription);
            }
        }

        private async Task<bool> EnsureServersLoadedAsync()
        {
            if (_servers.Length > 0)
            {
                return true;
            }

            SetBusy(true, "Загружаем почтовые сервисы...");
            ApiResult<EmailServerDto[]> result = await _services.Emails.GetServersAsync();
            SetBusy(false);

            if (HandleFailure(result))
            {
                return false;
            }

            _servers = result.Data ?? Array.Empty<EmailServerDto>();
            return true;
        }

        private void PopulateProviderSelectionUi()
        {
            if (_providerSelectSummaryText != null)
            {
                _providerSelectSummaryText.text = "Доступных сервисов: " + _servers.Length;
            }

            if (_providerSelectContent == null)
            {
                return;
            }

            UiFactory.DestroyChildren(_providerSelectContent);

            foreach (EmailServerDto server in _servers)
            {
                RectTransform card = UiFactory.CreatePanel("ProviderCard_" + server.key, _providerSelectContent, UiTheme.Surface, RoundedSprite, Image.Type.Sliced);
                card.gameObject.AddComponent<LayoutElement>().preferredHeight = 250f;

                VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(22, 22, 22, 22);
                layout.spacing = 10f;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;

                TMP_Text title = UiFactory.CreateText("Title", card, server.name, _fontAsset, 30f, UiTheme.TextPrimary, FontStyles.Bold, TextAlignmentOptions.Left);
                title.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;

                string description = string.IsNullOrWhiteSpace(server.help_url)
                    ? "Инструкция по паролю приложения пока недоступна."
                    : "Ссылка на инструкцию: " + server.help_url;
                TMP_Text text = UiFactory.CreateText("Description", card, description, _fontAsset, 24f, UiTheme.TextSecondary, FontStyles.Normal, TextAlignmentOptions.TopLeft);
                text.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
                ConfigureExternalLinkText(text, server.help_url);

                Button selectButton = UiFactory.CreateButton("SelectButton", card, "Выбрать", UiTheme.Accent, () => OnSelectServer(server), _fontAsset, 24f, UiTheme.White, RoundedSprite);
                selectButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 80f;
            }
        }

        private void OnSelectServer(EmailServerDto server)
        {
            _selectedServer = server;
            UpdateEmailFormServerState(_addEmailView);
            UpdateEmailFormServerState(_editEmailView);
            UpdateInstructionScreen(server);
            SetScreen(MainScreen.SmtpInfo);
        }

        private void UpdateInstructionScreen(EmailServerDto server)
        {
            if (_smtpInfoTitle != null)
            {
                _smtpInfoTitle.text = "SMTP-пароль: " + server.name;
            }

            if (_smtpInfoBody != null)
            {
                _smtpInfoBody.text = server.requires_custom_host
                    ? "Для custom-сервера дополнительно заполните host и port на предыдущем экране."
                    : "Сначала создайте пароль приложения у провайдера, затем вернитесь и вставьте его в форму подключения.";
            }

            if (_smtpInfoLink != null)
            {
                _smtpInfoLink.text = string.IsNullOrWhiteSpace(server.help_url)
                    ? "TODO: backend не вернул help_url для этого провайдера."
                    : server.help_url;
                ConfigureExternalLinkText(_smtpInfoLink, server.help_url);
            }
        }

        private void OpenInstructionForCurrentServer(bool isEditMode)
        {
            if (_selectedServer == null)
            {
                notificationBanner.ShowError("Сначала выберите тип почты.");
                return;
            }

            _providerSelectionTargetsEditFlow = isEditMode;
            UpdateInstructionScreen(_selectedServer);
            SetScreen(MainScreen.SmtpInfo);
        }

        private void UpdateEmailFormServerState(EmailFormView view)
        {
            if (view == null || view.ProviderText == null)
            {
                return;
            }

            view.ProviderText.text = _selectedServer == null ? "Не выбран" : _selectedServer.name + " (" + _selectedServer.key + ")";
        }

        private async void OpenEditEmail(EmailAccountDto account)
        {
            _editingAccount = account;
            if (!await EnsureServersLoadedAsync())
            {
                return;
            }

            _selectedServer = _servers.FirstOrDefault(server => string.Equals(server.key, account.server_key, StringComparison.OrdinalIgnoreCase));
            PopulateEmailForm(_editEmailView, account, true);
            SetScreen(MainScreen.EditEmail);
        }

        private async void DeleteEmail(EmailAccountDto account)
        {
            SetBusy(true, "Удаляем email...");
            ApiResult result = await _services.Emails.DeleteAsync(account.id);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            notificationBanner.ShowSuccess("Email удален.");
            await LoadEmailsAsync();
        }

        private async void OpenMailImportFlow(EmailAccountDto account)
        {
            _selectedAccount = account;

            if (_mailListView.TitleText != null)
            {
                _mailListView.TitleText.text = "Письма из " + account.email;
            }

            if (_mailListView.KeywordsInput != null)
            {
                _mailListView.KeywordsInput.text = string.Join(", ", DefaultKeywords);
            }

            if (_mailListView.DaysBackInput != null)
            {
                _mailListView.DaysBackInput.text = "30";
            }

            if (_mailListView.FoldersInput != null)
            {
                _mailListView.FoldersInput.text = "INBOX";
            }

            if (_mailListView.SummaryText != null)
            {
                _mailListView.SummaryText.text = "Получаем список папок и выполняем поиск писем...";
            }

            if (_mailListView.EmptyText != null)
            {
                _mailListView.EmptyText.text = "Идет первый поиск...";
            }

            if (_mailListView.ResultsContent != null)
            {
                UiFactory.DestroyChildren(_mailListView.ResultsContent);
            }

            SetScreen(MainScreen.MailList);

            await TryPreloadFoldersAsync();
            await RunMailSearchAsync();
        }

        private async Task TryPreloadFoldersAsync()
        {
            if (_selectedAccount == null)
            {
                return;
            }

            ApiResult<string[]> foldersResult = await _services.Emails.GetFoldersAsync(_selectedAccount.id);
            if (!foldersResult.IsSuccess || foldersResult.Data == null || foldersResult.Data.Length == 0 || _mailListView.FoldersInput == null)
            {
                return;
            }

            _mailListView.FoldersInput.text = string.Join(", ", foldersResult.Data.Take(3).ToArray());
        }

        private void RefreshMailSearchUi(string summaryMessage)
        {
            if (_mailListView == null)
            {
                return;
            }

            if (_mailListView.SummaryText != null)
            {
                _mailListView.SummaryText.text = summaryMessage;
            }

            if (_mailListView.EmptyText != null)
            {
                _mailListView.EmptyText.gameObject.SetActive(_mailPreviews.Length == 0);
                _mailListView.EmptyText.text = _mailPreviews.Length == 0 ? "Письма не найдены. Попробуйте изменить ключевые слова." : string.Empty;
            }

            if (_mailListView.ResultsContent != null)
            {
                UiFactory.DestroyChildren(_mailListView.ResultsContent);
            }

            if (_mailListView.ResultsContent == null)
            {
                return;
            }

            foreach (EmailPreviewDto preview in _mailPreviews)
            {
                MailCardView card = MailCardView.Create(_mailListView.ResultsContent, _fontAsset, RoundedSprite);
                card.Bind(preview, OpenMailPreview);
            }
        }

        private async void OpenMailPreview(EmailPreviewDto preview)
        {
            _selectedPreview = preview;
            _parsedSubscriptionDraft = null;

            if (_mailPreviewView.CandidatePanel != null)
            {
                _mailPreviewView.CandidatePanel.gameObject.SetActive(false);
            }

            SetBusy(true, "Загружаем письмо...");

            ApiResult<EmailDetailEnvelopeDto> result = await _services.Emails.GetEmailDetailAsync(_selectedAccount.id, preview.uid, preview.folder);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            _selectedEmailDetail = result.Data.email;

            if (_mailPreviewView.TitleText != null)
            {
                _mailPreviewView.TitleText.text = string.IsNullOrWhiteSpace(_selectedEmailDetail.subject) ? "(без темы)" : _selectedEmailDetail.subject;
            }

            if (_mailPreviewView.MetaText != null)
            {
                _mailPreviewView.MetaText.text = _selectedEmailDetail.from + "\n" + _selectedEmailDetail.date_str + "\nFolder: " + preview.folder;
            }

            if (_mailPreviewView.BodyText != null)
            {
                _mailPreviewView.BodyText.text = !string.IsNullOrWhiteSpace(_selectedEmailDetail.text)
                    ? _selectedEmailDetail.text
                    : (!string.IsNullOrWhiteSpace(_selectedEmailDetail.html) ? _selectedEmailDetail.html : "Содержимое письма отсутствует.");
            }

            SetScreen(MainScreen.MailPreview);
        }

        private async void OnParseSubscriptionClicked()
        {
            await ParseSubscriptionAsync();
        }

        private async void OnConfirmImportedSubscriptionClicked()
        {
            await ConfirmImportedSubscriptionAsync();
        }

        private async Task ParseSubscriptionAsync()
        {
            if (_selectedPreview == null || _selectedAccount == null)
            {
                notificationBanner.ShowError("Сначала откройте письмо.");
                return;
            }

            SetBusy(true, "Распознаем подписку из письма...");
            ApiResult<SubscriptionRequestDto> result = await _services.Emails.ParseSubscriptionAsync(_selectedAccount.id, _selectedPreview.uid, _selectedPreview.folder);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            _parsedSubscriptionDraft = result.Data;
            if (_mailPreviewView.CandidateText != null)
            {
                _mailPreviewView.CandidateText.text = BuildSubscriptionDraftText(_parsedSubscriptionDraft);
            }

            if (_mailPreviewView.CandidatePanel != null)
            {
                _mailPreviewView.CandidatePanel.gameObject.SetActive(true);
            }
        }

        private async Task ConfirmImportedSubscriptionAsync()
        {
            if (_parsedSubscriptionDraft == null)
            {
                notificationBanner.ShowError("Сначала распознайте подписку.");
                return;
            }

            SetBusy(true, "Создаем подписку...");
            ApiResult<SubscriptionDto> result = await _services.Subscriptions.AddAsync(_parsedSubscriptionDraft);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            notificationBanner.ShowSuccess("Подписка создана из письма.");
            await LoadSubscriptionsAsync();
        }

        private async void RecordSubscriptionUsage(SubscriptionDto subscription)
        {
            SetBusy(true, "Сохраняем usage-отметку...");
            ApiResult<SubscriptionUsageStatusDto> result = await _services.Subscriptions.RecordUsageAsync(new SubscriptionUsageLogRequestDto
            {
                subscription_id = subscription.id,
                signal = "used",
                note = "Marked from Unity client"
            });
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            notificationBanner.ShowSuccess("Использование отмечено.");
            await LoadSubscriptionsAsync();
        }

        private async void CopySubscriptionActionPlan(SubscriptionDto subscription)
        {
            string action = subscription.is_active ? "pause" : "cancel";

            SetBusy(true, "Готовим шаблон обращения...");
            ApiResult<SubscriptionActionPlanDto> result = await _services.Subscriptions.GetActionPlanAsync(subscription.id, action);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            GUIUtility.systemCopyBuffer = result.Data.copy_text;
            notificationBanner.ShowSuccess("Шаблон обращения скопирован в буфер обмена.");
        }

        private async void ToggleSubscriptionActive(SubscriptionDto subscription)
        {
            SetBusy(true, "Обновляем статус подписки...");
            ApiResult<bool> result = await _services.Subscriptions.SetActiveAsync(subscription.id, !subscription.is_active);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            await LoadSubscriptionsAsync();
        }

        private async void DeleteSubscription(SubscriptionDto subscription)
        {
            SetBusy(true, "Удаляем подписку...");
            ApiResult result = await _services.Subscriptions.DeleteAsync(subscription.id);
            SetBusy(false);

            if (HandleFailure(result))
            {
                return;
            }

            notificationBanner.ShowSuccess("Подписка удалена.");
            await LoadSubscriptionsAsync();
        }

        private SubscriptionUsageStatusDto FindUsageStatus(int subscriptionId)
        {
            if (_subscriptionInsights == null || _subscriptionInsights.usage_reviews == null)
            {
                return null;
            }

            return _subscriptionInsights.usage_reviews.FirstOrDefault(item => item.subscription_id == subscriptionId);
        }

        private void ConfigureExternalLinkText(TMP_Text text, string url)
        {
            if (text == null)
            {
                return;
            }

            bool hasUrl = !string.IsNullOrWhiteSpace(url);
            text.raycastTarget = hasUrl;
            text.color = hasUrl ? UiTheme.Accent : UiTheme.TextSecondary;
            text.fontStyle = hasUrl ? FontStyles.Underline : FontStyles.Normal;

            Button button = text.GetComponent<Button>();
            if (button == null)
            {
                button = text.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = text as Graphic;
            button.transition = Selectable.Transition.None;
            button.interactable = hasUrl;
            button.onClick.RemoveAllListeners();

            if (hasUrl)
            {
                button.onClick.AddListener(() => Application.OpenURL(url));
            }
        }

        private void ToggleScreen(RectTransform screen, bool isActive)
        {
            if (screen != null)
            {
                screen.gameObject.SetActive(isActive);
            }
        }

        private void ApplyNavigationState(MainScreen screen)
        {
            SetNavButtonState(dashboardNavButton, screen == MainScreen.Dashboard);
            SetNavButtonState(emailsNavButton, screen == MainScreen.Emails || screen == MainScreen.AddEmail || screen == MainScreen.ProviderSelect || screen == MainScreen.SmtpInfo || screen == MainScreen.EmailSuccess || screen == MainScreen.EditEmail || screen == MainScreen.MailList || screen == MainScreen.MailPreview);
            SetNavButtonState(subscriptionsNavButton, screen == MainScreen.Subscriptions || screen == MainScreen.ManualAddSubscription);
        }

        private void SetNavButtonState(Button button, bool isActive)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            TMP_Text text = button.GetComponentInChildren<TextMeshProUGUI>(true);

            if (image != null)
            {
                image.color = isActive ? UiTheme.Accent : UiTheme.SurfaceMuted;
            }

            if (text != null)
            {
                text.color = isActive ? UiTheme.White : UiTheme.TextPrimary;
            }
        }

        private bool HandleFailure(ApiResult result)
        {
            if (result.IsSuccess)
            {
                return false;
            }

            if (result.FailureKind == ApiFailureKind.Unauthorized || result.FailureKind == ApiFailureKind.Forbidden)
            {
                _services.Auth.Logout();
                SceneManager.LoadScene(authSceneName);
                return true;
            }

            notificationBanner.ShowError(result.ErrorMessage);
            return true;
        }

        private bool HandleFailure<T>(ApiResult<T> result)
        {
            if (result.IsSuccess)
            {
                return false;
            }

            if (result.FailureKind == ApiFailureKind.Unauthorized || result.FailureKind == ApiFailureKind.Forbidden)
            {
                _services.Auth.Logout();
                SceneManager.LoadScene(authSceneName);
                return true;
            }

            notificationBanner.ShowError(result.ErrorMessage);
            return true;
        }

        private void SetBusy(bool isBusy, string message = null)
        {
            if (loaderOverlay == null)
            {
                return;
            }

            SetAllButtonsInteractable(!isBusy);

            if (isBusy)
            {
                loaderOverlay.Show(message);
            }
            else
            {
                loaderOverlay.Hide();
            }
        }

        private void SetAllButtonsInteractable(bool isInteractable)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                button.interactable = isInteractable;
            }
        }

        private void CreateInfoCard(Transform parent, string text)
        {
            RectTransform card = UiFactory.CreatePanel("InfoCard", parent, UiTheme.SurfaceMuted, RoundedSprite, Image.Type.Sliced);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 132f;

            HorizontalLayoutGroup layout = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            UiFactory.CreateText("Text", card, text, _fontAsset, 24f, UiTheme.TextPrimary, FontStyles.Normal, TextAlignmentOptions.Left);
        }

        private static string[] SplitInput(string input)
        {
            return (input ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToArray();
        }

        private static bool TryParseFloat(string value, out float result)
        {
            string normalized = (value ?? string.Empty).Trim().Replace(',', '.');
            return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryParseInputDate(string value, out string isoDate)
        {
            isoDate = null;
            string[] formats =
            {
                "dd.MM.yyyy",
                "dd.MM.yyyy HH:mm",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-ddTHH:mm:ss"
            };

            DateTime parsed;
            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed) ||
                DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
            {
                if (parsed.TimeOfDay == TimeSpan.Zero)
                {
                    parsed = parsed.Date.AddHours(12);
                }

                isoDate = parsed.ToString("yyyy-MM-ddTHH:mm:ss");
                return true;
            }

            return false;
        }

        private static DateTime ParseDateTimeSafe(string value)
        {
            DateTime parsed;
            if (DateTime.TryParse(value, out parsed))
            {
                return parsed;
            }

            return DateTime.MaxValue;
        }

        private static string FormatDate(string value)
        {
            DateTime parsed;
            if (DateTime.TryParse(value, out parsed))
            {
                return parsed.ToLocalTime().ToString("dd.MM.yyyy");
            }

            return string.IsNullOrWhiteSpace(value) ? "—" : value;
        }

        private static string BuildSubscriptionDraftText(SubscriptionRequestDto draft)
        {
            return "Сервис: " + draft.name +
                   "\nКатегория: " + draft.category +
                   "\nСтоимость: " + draft.cost.ToString("0.00") +
                   "\nПериод: " + draft.billing_cycle +
                   "\nДата списания: " + draft.payment_date +
                   "\nКомментарий: " + draft.comment;
        }

        private void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private T FindComponent<T>(string name) where T : Component
        {
            foreach (T candidate in GetComponentsInChildren<T>(true))
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private RectTransform FindRect(string name)
        {
            return FindComponent<RectTransform>(name);
        }

        private RectTransform FindRectInScope(Transform scope, string name)
        {
            return FindComponentInScope<RectTransform>(scope, name);
        }

        private TMP_InputField FindInputInScope(Transform scope, string name)
        {
            return FindComponentInScope<TMP_InputField>(scope, name);
        }

        private TMP_Text FindTextInScope(Transform scope, string name)
        {
            return FindComponentInScope<TMP_Text>(scope, name);
        }

        private Button FindButtonInScope(Transform scope, string name)
        {
            return FindComponentInScope<Button>(scope, name);
        }

        private T FindComponentInScope<T>(Transform scope, string name) where T : Component
        {
            if (scope == null)
            {
                return null;
            }

            foreach (T candidate in scope.GetComponentsInChildren<T>(true))
            {
                if (candidate.name == name)
                {
                    return candidate;
                }
            }

            return null;
        }

        private Button FindButtonByLabel(Transform scope, string label)
        {
            if (scope == null)
            {
                return null;
            }

            foreach (Button button in scope.GetComponentsInChildren<Button>(true))
            {
                TMP_Text text = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text != null && text.text == label)
                {
                    return button;
                }
            }

            return null;
        }

        private Transform GetScrollContent(RectTransform root)
        {
            if (root == null)
            {
                return null;
            }

            ScrollRect scrollRect = root.GetComponentInChildren<ScrollRect>(true);
            return scrollRect != null ? scrollRect.content : root;
        }

        private TMP_Text FindDirectText(Transform parent, string name, int index)
        {
            if (parent == null)
            {
                return null;
            }

            int currentIndex = 0;
            for (int i = 0; i < parent.childCount; i++)
            {
                TMP_Text text = parent.GetChild(i).GetComponent<TMP_Text>();
                if (text == null || text.name != name)
                {
                    continue;
                }

                if (currentIndex == index)
                {
                    return text;
                }

                currentIndex++;
            }

            return null;
        }

        private Sprite RoundedSprite
        {
            get
            {
                return _roundedSprite != null ? _roundedSprite : UiFactory.WhiteSprite;
            }
        }

        private static T EnsureComponent<T>(Component target) where T : Component
        {
            if (target == null)
            {
                return null;
            }

            T component = target.GetComponent<T>();
            return component != null ? component : target.gameObject.AddComponent<T>();
        }
    }
}
