# SubMonitor

Unity-клиент проекта SubMonitor. Репозиторий содержит экран авторизации, основной интерфейс приложения, интеграцию с backend API и экран управления подписками.

## Стек

- Unity `2022.3.23f1`
- C#
- UGUI / TextMesh Pro
- Unity Test Framework

## Структура проекта

- `Assets/Scenes/Auth.unity` - стартовая сцена с авторизацией и регистрацией
- `Assets/Scenes/Main.unity` - основной экран приложения после входа
- `Assets/Scripts/App/Services` - HTTP-клиенты и сервисы для работы с API
- `Assets/Scripts/App/Config` - конфигурация клиента
- `Assets/Scripts/App/DTO` - DTO-модели запросов и ответов
- `Assets/Scripts/App/UI` - общие UI-компоненты
- `Assets/Scripts/Auth` - логика экрана авторизации
- `Assets/Scripts/SubscriptionsUI` - логика экрана подписок
- `Packages/manifest.json` - список Unity-пакетов
- `ProjectSettings/` - настройки Unity-проекта
- 
## Настройка API

Базовый адрес backend находится в `Assets/Scripts/App/Config/ApiConfig.cs`.

- API prefix: `/api/v1`
- При необходимости адрес можно переопределить вызовом `ApiConfig.OverrideBaseUrl(...)`

## Основные возможности

- Авторизация и регистрация пользователя
- Получение данных профиля
- Подключение и просмотр email-аккаунтов
- Поиск и просмотр писем
- Работа с подписками пользователя

## Что хранится в git

В репозитории должны находиться только исходники и настройки проекта:

- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `README.md`
- `.gitignore`

Каталоги `Library`, `Temp`, `Logs`, `Builds`, `UserSettings`, а также IDE-файлы исключены через `.gitignore`.

## Быстрый старт с git

Для первой фиксации состояния проекта:

```bash
git add .
git commit -m "Initial Unity project setup"
```
