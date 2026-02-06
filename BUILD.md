# Как собрать и скачать проект / How to Build and Download

## Русский

### Автоматическая сборка через GitHub Actions

1. Перейдите на вкладку [Actions](../../actions) в этом репозитории
2. Выберите workflow "Manual Build and Publish" в левом меню
3. Нажмите кнопку "Run workflow" (запустить рабочий процесс)
4. (Опционально) Укажите версию, если требуется (по умолчанию 0.0.1)
5. Нажмите зелёную кнопку "Run workflow"
6. Дождитесь завершения сборки (обычно занимает несколько минут)
7. После завершения, перейдите в завершённый запуск workflow
8. Прокрутите вниз до секции "Artifacts" и скачайте нужный zip-архив

### Доступные варианты сборки

Будут созданы следующие артефакты:
- `WireSockUI-{version}-AnyCPU.zip` - Версия с UWP уведомлениями для любого процессора
- `WireSockUI-{version}-AnyCPU-no-uwp.zip` - Версия без UWP уведомлений для любого процессора
- `WireSockUI-{version}-ARM64.zip` - Версия с UWP уведомлениями для ARM64
- `WireSockUI-{version}-ARM64-no-uwp.zip` - Версия без UWP уведомлений для ARM64

### Локальная сборка

Если вы хотите собрать проект локально:

1. Установите [.NET SDK 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) или новее
2. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/NeFeroN/WireSockUI.git
   cd WireSockUI
   ```
3. Соберите проект:
   ```bash
   dotnet publish WireSockUI/WireSockUI.csproj --configuration Release --framework net472-windows --no-self-contained /p:Platform=AnyCPU /p:Version=1.0.0
   ```
4. Результаты сборки будут в папке `bin/AnyCPU/Release/net472-windows/publish/`

---

## English

### Automatic Build via GitHub Actions

1. Go to the [Actions](../../actions) tab in this repository
2. Select the "Manual Build and Publish" workflow in the left menu
3. Click the "Run workflow" button
4. (Optional) Specify a version if needed (default is 0.0.1)
5. Click the green "Run workflow" button
6. Wait for the build to complete (usually takes a few minutes)
7. Once completed, go to the finished workflow run
8. Scroll down to the "Artifacts" section and download the desired zip archive

### Available Build Variants

The following artifacts will be created:
- `WireSockUI-{version}-AnyCPU.zip` - UWP-enabled version for any CPU
- `WireSockUI-{version}-AnyCPU-no-uwp.zip` - Non-UWP version for any CPU
- `WireSockUI-{version}-ARM64.zip` - UWP-enabled version for ARM64
- `WireSockUI-{version}-ARM64-no-uwp.zip` - Non-UWP version for ARM64

### Local Build

If you want to build the project locally:

1. Install [.NET SDK 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) or newer
2. Clone the repository:
   ```bash
   git clone https://github.com/NeFeroN/WireSockUI.git
   cd WireSockUI
   ```
3. Build the project:
   ```bash
   dotnet publish WireSockUI/WireSockUI.csproj --configuration Release --framework net472-windows --no-self-contained /p:Platform=AnyCPU /p:Version=1.0.0
   ```
4. Build results will be in `bin/AnyCPU/Release/net472-windows/publish/`
