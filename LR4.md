# LR4 — Автоматизация тестирования (Unity Test Runner) в CI/CD

**Дата:** октябрь 2026 · **Проект:** [Neckrite/2DPlatformerLab](https://github.com/Neckrite/2DPlatformerLab) · **Вариант:** 8 · **Файлы:** [Tests/EditMode](Assets/Tests/EditMode/) · [Tests/PlayMode](Assets/Tests/PlayMode/) · [main.yml](.github/workflows/main.yml)

## Цель

Настроить автоматический запуск Unity-тестов (EditMode и PlayMode) в облачном пайплайне так, чтобы сборка Docker-образа и зеркалирование выполнялись только после успешного прохождения всех тестов.

## Шаг 1. Разделение кодовой базы (Assembly Definitions)

1. `Assets/Scripts/` → Create → **Assembly Definition** → `GameAssembly` — весь игровой код выделен в отдельную сборку.
2. В корне `Assets/` создана папка `Tests` с подпапками `EditMode` и `PlayMode`.
3. Тестовые сборки:
   - `Assets/Tests/EditMode/Tests.asmdef` — `includePlatforms: ["Editor"]` (тесты EditMode);
   - `Assets/Tests/PlayMode/PlayModeTests.asmdef` — без `includePlatforms` (тесты PlayMode).
4. В обеих сборках в **Assembly Definition References** добавлена ссылка на `GameAssembly` — тесты получают доступ к коду игры.

Важно: режим запуска определяется настройкой сборки — `includePlatforms: Editor` запускается только в EditMode, сборка без параметра — в PlayMode.

## Шаг 2. Тесты

Три теста (2 базовых + 1 по варианту 8). Механики проверяются из реального проекта (у проекта своя реализация `PlayerController`, поэтому формулировки адаптированы под реальные скрипты).

| Тест | Сборка | Что проверяет |
|---|---|---|
| `EditMode_SceneSetup_IsCorrect` | Tests (EditMode) | Сцена `Game.unity` открывается, на ней есть `GameManager` (строит весь уровень в рантайме), сцена включена в Build Settings |
| `PlayMode_FallBelowWorld_RespawnsAtSpawnPoint` | PlayModeTests | Игрок, упавший ниже границы мира (y < −8), автоматически возрождается в точке спавна (x = −12), скорость обнуляется |
| `PlayMode_ControlEnabledTrue_InputChangesMoveDirection` — **вариант 8** | PlayModeTests | При `controlEnabled = true` ввод меняет направление движения: «вправо» → `velocity.x > 0`, «влево» → `velocity.x < 0` (зеркально); при `controlEnabled = false` горизонтальный ввод обнуляется |

Для проверяемости варианта 8 в `PlayerController` добавлено поле `public bool controlEnabled = true` и метод `ApplyInput(float axis)` (ввод выделен в тестируемый метод; `Update()` читает клавиши только при `controlEnabled`).

Локальный headless-прогон (Unity CLI):

```bat
Unity.exe -batchmode -projectPath . -runTests ^
  -testPlatform PlayMode ^
  -testResults playmode-results.xml -logFile tests_playmode.log
```

Результат: **EditMode 1/1 passed, PlayMode 2/2 passed** (все зелёные).

## Шаг 3. Автозапуск тестов в GitHub Actions

В `.github/workflows/main.yml` добавлена задача `run_tests` (GameCI CLI, контейнер `unityci/editor`):

- `needs: sanity_check` — после проверки структуры;
- кэш папки `Library` (`actions/cache@v4`) — импорт ассетов один раз;
- запуск обеих платформ: `--testPlatforms='playmode;editmode;COMBINE_RESULTS'`;
- `--engineVersion=6000.4.8f1` явно (ProjectVersion.txt в .gitignore);
- `--coverageEnabled=false` — инструментирование кода роняет Unity 6 в контейнере (SIGSEGV, exit 139);
- CLI вызывается напрямую: экшен `game-ci/unity-test-runner@v4` формирует флаг `--no-coverageEnabled`, которого CLI v0.1.72 не понимает;
- результаты сохраняются артефактом `test-results` даже при падении.

Гейт качества: `mirror_repo` и `docker_ghcr` теперь начинаются с `needs: run_tests` — при падении любого теста сборка и публикация не запускаются.

Секреты репозитория (Settings → Secrets and variables → Actions):

- `UNITY_LICENSE` — содержимое файла `Unity_lic.ulf` (Unity Personal);
- `UNITY_EMAIL`, `UNITY_PASSWORD` — данные аккаунта Unity (для активации на раннере).

## Шаг 4. Защита пайплайна

Проверено: при искусственной ошибке в тесте (`Assert.AreEqual(1, 2)`) задача `run_tests` становится красной, а `mirror_repo` и `docker_ghcr` не запускаются (сломанный коммит заблокирован). После возврата исправленного теста пайплайн снова зелёный.

## Результат

- Тестовая структура: `Assets/Tests/{EditMode,PlayMode}` со сборками, ссылающимися на `GameAssembly`.
- 3 теста проходят локально и в облаке; тест варианта 8 проверяет изменение направления движения при `controlEnabled = true`.
- Пайплайн: тесты — обязательный этап перед зеркалированием и сборкой Docker-образа.
