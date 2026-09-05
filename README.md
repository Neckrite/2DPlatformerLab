# Лабораторная работа №1: Автоматизация сборки 2D игры через CLI

## Цель работы
Освоить автоматизацию сборки Unity-проекта через интерфейс командной строки (CLI), научиться использовать BuildPipeline.BuildPlayer() для программной компиляции WebGL-билда, и настроить Git-репозиторий с ветвлением и Pull Request.

---

## Шаг 1: Инициализация проекта

Был создан новый Unity-проект на базе 2D-шаблона (Universal 2D) с использованием Unity 6000.4.8f1 (версия с установленным WebGL-модулем).

- **Путь к проекту:** C:\Users\nikit\Documents\2DPlatformerLab
- **Версия Unity:** 6000.4.8f1
- **Основная сцена:** Assets/Scenes/Game.unity
- Сцена добавлена в Build Settings как активная

## Шаг 2: Создание C# скрипта сборщика

В папке Assets/Editor/ создан скрипт BuildManager.cs, содержащий:

- BuildWebGL() — статический метод, который:
  1. Получает список активных сцен из Build Settings
  2. Конфигурирует BuildPlayerOptions для платформы WebGL
  3. Запускает компиляцию через BuildPipeline.BuildPlayer()
  4. Анализирует результат и выводит лог с временем и размером билда
- GetScenes() — вспомогательный метод сбора активных сцен
- ExitWithCode() — корректное завершение Unity в batchmode

Код расположен в папке Editor, поэтому не попадает в финальную сборку игры.

## Шаг 3: Отключение сжатия для WebGL

В Edit → Project Settings → Player → WebGL → Publishing Settings:
- **Compression Format:** Disabled

Это необходимо для корректной работы Live Server при локальном запуске (без CORS-ошибок при загрузке несжатых файлов).

Также добавлен скрипт SetupBuildSettings.cs, который программно устанавливает PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled перед сборкой.

## Шаг 4: Проверка скрипта через интерфейс Unity

Проект успешно скомпилирован в Unity — ошибок компиляции C#-скриптов не обнаружено. Метод BuildManager.BuildWebGL() доступен для вызова через CLI.

## Шаг 5: Локальная сборка через терминал (CLI)

Команда запуска автоматической сборки:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.8f1\Editor\Unity.exe" 
  -batchmode 
  -nographics 
  -projectPath "C:\Users\nikit\Documents\2DPlatformerLab" 
  -executeMethod SetupBuildSettings.SetupAndBuild 
  -quit 
  -logFile build_webgl.log
```

### Разбор флагов:
| Флаг | Назначение |
|------|-----------|
| -batchmode | Запуск Unity без GUI |
| -nographics | Отключение инициализации графического движка |
| -executeMethod SetupBuildSettings.SetupAndBuild | Выполнение статического метода сразу после загрузки проекта |
| -quit | Автоматическое закрытие Unity после выполнения метода |
| -logFile build_webgl.log | Перенаправление вывода в лог-файл |

## Шаг 6: Анализ результатов и логов сборки

Результат из лог-файла uild_webgl.log:

`
[Setup] Adding scene to Build Settings...
[Setup] WebGL compression set to Disabled
[CI/CD] Starting automatic WebGL build process...
[CI/CD] SUCCESS! WebGL build created successfully.
[CI/CD] Build time: 11,00 sec. Size: 17205113 bytes.
`

### Структура WebGL-билда (Builds/WebGL/):
`
Builds/WebGL/
├── index.html
├── Build/
│   ├── WebGL.data
│   ├── WebGL.framework.js
│   ├── WebGL.loader.js
│   └── WebGL.wasm
└── TemplateData/
    ├── style.css
    ├── favicon.ico
    └── ...
`

Сжатие отключено — файлы без .gz / .br суффиксов.

## Шаг 7: Проверка работоспособности (Локальный запуск)

Для запуска WebGL-билда локально необходимо использовать Live Server в VS Code:

1. Открыть папку Builds/WebGL/ в VS Code
2. Запустить расширение Live Server (кнопка «Go Live»)
3. Браузер откроет http://127.0.0.1:5500/index.html
4. Игра загружается и отображается корректно

> **Внимание:** Двойной клик по index.html не работает из-за CORS-политик браузера.

## Шаг 8: Настройка Git и публикация

### Git-коммиты:

1. **main:** chore: initializing a 2D Platformer Microgame project — базовое состояние проекта без BuildManager.cs
2. **LR1:** eat: added BuildManager script for build automation — добавлен скрипт сборщика

### .gitignore
Файл .gitignore настроен для Unity-проекта: исключены Library/, Temp/, Obj/, Builds/, лог-файлы и прочие служебные файлы.

## Шаг 9: Документирование и Pull Request

Данный README.md является отчётом по лабораторной работе.

---

## Контрольные вопросы

### 1. Зачем использовать флаг -nographics и какую роль он сыграет на удалённом сервере?

Флаг -nographics отключает инициализацию графического движка и видеокарты. На удалённом сервере сборки (Headless Linux) нет GPU и графической оболочки — без этого флага Unity не сможет запуститься, так как по умолчанию инициализирует рендер-движок. С -nographics Unity работает в чисто вычислительном режиме, используя только CPU для компиляции и сборки проекта.

### 2. Что произойдёт, если в Build Settings нет активных сцен? Какая строка кода обрабатывает это?

Если в Build Settings нет ни одной активной сцены, вызов BuildPipeline.BuildPlayer() получит пустой массив сцен, что приведёт к ошибке сборки. Наш код обрабатывает эту ситуацию в методе GetScenes(): если ctiveCount == 0, метод вернёт пустой массив, а затем в BuildWebGL() сработает проверка:

```csharp
if (levels.Length == 0)
{
    Debug.LogError("[CI/CD] Error: No active scenes found in Build Settings!");
    ExitWithCode(1);
    return;
}
```

Это предотвращает бессмысленный вызов BuildPipeline.BuildPlayer() без сцен.

### 3. Почему класс BuildManager и его методы должны быть public static?

- **static** — методы, вызываемые через -executeMethod, должны быть статическими, так как Unity не создаёт экземпляры классов при вызове из CLI. Статический метод можно вызвать без создания объекта: BuildManager.BuildWebGL().
- **public** — модификатор public необходим для того, чтобы Unity могла обнаружить метод через рефлексию при обработке флага -executeMethod. Если метод будет private или internal, Unity не сможет его вызвать.

---

## Используемые инструменты

- **Unity:** 6000.4.8f1 (с WebGL-модулем)
- **Git:** 2.55.0
- **Сборка:** WebGL (Compression: Disabled)
- **Сборочное время:** ~11 сек
- **Размер билда:** ~17.2 МБ
