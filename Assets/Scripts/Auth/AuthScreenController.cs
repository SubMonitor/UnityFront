using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SubMonitor.App.Core;
using SubMonitor.App.DTO;
using SubMonitor.App.Services;
using SubMonitor.App.UI.Common;

namespace SubMonitor.Auth
{
    [DisallowMultipleComponent]
    public class AuthScreenController : MonoBehaviour
    {
        private enum AuthScreen
        {
            Start,
            Login,
            Register
        }

        [Header("Navigation")]
        [SerializeField] private string mainSceneName = "Main";

        [Header("Scene References")]
        [SerializeField] private RectTransform startScreen;
        [SerializeField] private RectTransform loginScreen;
        [SerializeField] private RectTransform registerScreen;
        [SerializeField] private NotificationBannerView notificationBanner;
        [SerializeField] private LoaderOverlayView loaderOverlay;
        [SerializeField] private TMP_InputField loginEmailInput;
        [SerializeField] private TMP_InputField loginPasswordInput;
        [SerializeField] private TMP_InputField registerLastNameInput;
        [SerializeField] private TMP_InputField registerFirstNameInput;
        [SerializeField] private TMP_InputField registerPatronymicInput;
        [SerializeField] private TMP_InputField registerEmailInput;
        [SerializeField] private TMP_InputField registerPasswordInput;
        [SerializeField] private TMP_InputField registerConfirmPasswordInput;
        [SerializeField] private Button startLoginButton;
        [SerializeField] private Button startRegisterButton;
        [SerializeField] private Button loginSubmitButton;
        [SerializeField] private Button loginToRegisterButton;
        [SerializeField] private Button registerSubmitButton;
        [SerializeField] private Button registerBackButton;

        private AppServices _services;

        private void Awake()
        {
            _services = ServiceRegistry.Current;
            EnsureSceneBindings();
            BindButtons();

            if (Application.isPlaying)
            {
                SetScreen(AuthScreen.Start);
                _ = RestoreSessionAsync();
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

        public void ShowStartScreen()
        {
            SetScreen(AuthScreen.Start);
        }

        public void ShowLoginScreen()
        {
            SetScreen(AuthScreen.Login);
        }

        public void ShowRegisterScreen()
        {
            SetScreen(AuthScreen.Register);
        }

        public async void OnLoginClicked()
        {
            if (!ValidateLoginForm())
            {
                return;
            }

            SetBusy(true, "Выполняем вход...");

            ApiResult<TokenResponseDto> result = await _services.Auth.LoginAsync(new LoginRequestDto
            {
                email = loginEmailInput.text.Trim(),
                password = loginPasswordInput.text
            });

            SetBusy(false);

            if (!result.IsSuccess)
            {
                notificationBanner.ShowError(result.ErrorMessage);
                return;
            }

            notificationBanner.ShowSuccess("Вход выполнен. Переходим в приложение...");
            SceneManager.LoadScene(mainSceneName);
        }

        public async void OnRegisterClicked()
        {
            if (!ValidateRegisterForm())
            {
                return;
            }

            SetBusy(true, "Создаем аккаунт...");

            ApiResult<RegisterResponseDto> result = await _services.Auth.RegisterAsync(new RegisterRequestDto
            {
                email = registerEmailInput.text.Trim(),
                password = registerPasswordInput.text,
                first_name = registerFirstNameInput.text.Trim(),
                last_name = registerLastNameInput.text.Trim(),
                patronymic = registerPatronymicInput.text.Trim()
            });

            SetBusy(false);

            if (!result.IsSuccess)
            {
                notificationBanner.ShowError(result.ErrorMessage);
                return;
            }

            ClearRegisterInputs();
            notificationBanner.ShowSuccess("Регистрация прошла успешно. Теперь можно войти.");
            SetScreen(AuthScreen.Login);
        }

        private async Task RestoreSessionAsync()
        {
            if (!_services.SessionStore.HasToken())
            {
                return;
            }

            SetBusy(true, "Проверяем сохраненную сессию...");
            ApiResult<UserProfileDto> result = await _services.Auth.GetProfileAsync();
            SetBusy(false);

            if (result.IsSuccess)
            {
                SceneManager.LoadScene(mainSceneName);
                return;
            }

            _services.Auth.Logout();
            notificationBanner.ShowInfo("Сохраненная сессия истекла. Войдите снова.");
            SetScreen(AuthScreen.Start);
        }

        private void EnsureSceneBindings()
        {
            startScreen = startScreen != null ? startScreen : FindRect("StartScreen");
            loginScreen = loginScreen != null ? loginScreen : FindRect("LoginScreen");
            registerScreen = registerScreen != null ? registerScreen : FindRect("RegisterScreen");
            notificationBanner = notificationBanner != null ? notificationBanner : FindComponent<NotificationBannerView>("NotificationBanner");
            loaderOverlay = loaderOverlay != null ? loaderOverlay : FindComponent<LoaderOverlayView>("LoaderOverlay");

            loginEmailInput = loginEmailInput != null ? loginEmailInput : FindInput("LoginEmailInput");
            loginPasswordInput = loginPasswordInput != null ? loginPasswordInput : FindInput("LoginPasswordInput");
            registerLastNameInput = registerLastNameInput != null ? registerLastNameInput : FindInput("RegisterLastNameInput");
            registerFirstNameInput = registerFirstNameInput != null ? registerFirstNameInput : FindInput("RegisterFirstNameInput");
            registerPatronymicInput = registerPatronymicInput != null ? registerPatronymicInput : FindInput("RegisterPatronymicInput");
            registerEmailInput = registerEmailInput != null ? registerEmailInput : FindInput("RegisterEmailInput");
            registerPasswordInput = registerPasswordInput != null ? registerPasswordInput : FindInput("RegisterPasswordInput");
            registerConfirmPasswordInput = registerConfirmPasswordInput != null ? registerConfirmPasswordInput : FindInput("RegisterConfirmPasswordInput");

            startLoginButton = startLoginButton != null ? startLoginButton : FindButton("LoginButton");
            startRegisterButton = startRegisterButton != null ? startRegisterButton : FindButton("RegisterButton");
            loginSubmitButton = loginSubmitButton != null ? loginSubmitButton : FindButton("LoginSubmit");
            loginToRegisterButton = loginToRegisterButton != null ? loginToRegisterButton : FindButton("GoToRegister");
            registerSubmitButton = registerSubmitButton != null ? registerSubmitButton : FindButton("RegisterSubmit");
            registerBackButton = registerBackButton != null ? registerBackButton : FindButton("BackToLogin");

            if (Application.isPlaying)
            {
                ConfigureResponsiveLayout();
            }
        }

        private void ConfigureResponsiveLayout()
        {
            RectTransform safeArea = FindRect("SafeArea");
            EnsureComponent<ResponsiveSafeArea>(safeArea);

            RectTransform authRoot = FindRectInScope(safeArea, "AuthRoot");
            ResponsiveWidthLimiter widthLimiter = EnsureComponent<ResponsiveWidthLimiter>(authRoot);
            if (widthLimiter != null)
            {
                widthLimiter.Configure(1080f);
            }
        }

        private void BindButtons()
        {
            BindButton(startLoginButton, ShowLoginScreen);
            BindButton(startRegisterButton, ShowRegisterScreen);
            BindButton(loginSubmitButton, OnLoginClicked);
            BindButton(loginToRegisterButton, ShowRegisterScreen);
            BindButton(registerSubmitButton, OnRegisterClicked);
            BindButton(registerBackButton, ShowLoginScreen);
        }

        private void SetScreen(AuthScreen screen)
        {
            Toggle(startScreen, screen == AuthScreen.Start);
            Toggle(loginScreen, screen == AuthScreen.Login);
            Toggle(registerScreen, screen == AuthScreen.Register);

            if (screen != AuthScreen.Start && notificationBanner != null)
            {
                notificationBanner.Hide();
            }
        }

        private bool ValidateLoginForm()
        {
            if (loginEmailInput == null || loginPasswordInput == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(loginEmailInput.text))
            {
                notificationBanner.ShowError("Введите email для входа.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(loginPasswordInput.text))
            {
                notificationBanner.ShowError("Введите пароль.");
                return false;
            }

            return true;
        }

        private bool ValidateRegisterForm()
        {
            if (registerLastNameInput == null ||
                registerFirstNameInput == null ||
                registerEmailInput == null ||
                registerPasswordInput == null ||
                registerConfirmPasswordInput == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(registerLastNameInput.text) ||
                string.IsNullOrWhiteSpace(registerFirstNameInput.text) ||
                string.IsNullOrWhiteSpace(registerEmailInput.text))
            {
                notificationBanner.ShowError("Заполните фамилию, имя и email.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(registerPasswordInput.text))
            {
                notificationBanner.ShowError("Введите пароль.");
                return false;
            }

            if (registerPasswordInput.text.Length < 8)
            {
                notificationBanner.ShowError("Пароль должен быть не короче 8 символов.");
                return false;
            }

            if (registerPasswordInput.text != registerConfirmPasswordInput.text)
            {
                notificationBanner.ShowError("Пароли не совпадают.");
                return false;
            }

            return true;
        }

        private void ClearRegisterInputs()
        {
            if (registerLastNameInput != null)
            {
                registerLastNameInput.text = string.Empty;
            }

            if (registerFirstNameInput != null)
            {
                registerFirstNameInput.text = string.Empty;
            }

            if (registerPatronymicInput != null)
            {
                registerPatronymicInput.text = string.Empty;
            }

            if (registerEmailInput != null)
            {
                registerEmailInput.text = string.Empty;
            }

            if (registerPasswordInput != null)
            {
                registerPasswordInput.text = string.Empty;
            }

            if (registerConfirmPasswordInput != null)
            {
                registerConfirmPasswordInput.text = string.Empty;
            }
        }

        private void SetBusy(bool isBusy, string message = null)
        {
            if (loaderOverlay == null)
            {
                return;
            }

            SetButtonsInteractable(!isBusy);

            if (isBusy)
            {
                loaderOverlay.Show(message);
            }
            else
            {
                loaderOverlay.Hide();
            }
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            var buttons = new[]
            {
                startLoginButton,
                startRegisterButton,
                loginSubmitButton,
                loginToRegisterButton,
                registerSubmitButton,
                registerBackButton
            };

            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    button.interactable = isInteractable;
                }
            }
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

        private void Toggle(RectTransform target, bool isVisible)
        {
            if (target != null)
            {
                target.gameObject.SetActive(isVisible);
            }
        }

        private RectTransform FindRect(string name)
        {
            return FindComponent<RectTransform>(name);
        }

        private Button FindButton(string name)
        {
            return FindComponent<Button>(name);
        }

        private TMP_InputField FindInput(string name)
        {
            return FindComponent<TMP_InputField>(name);
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

        private RectTransform FindRectInScope(Transform scope, string name)
        {
            return FindComponentInScope<RectTransform>(scope, name);
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
