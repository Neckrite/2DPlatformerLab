# LR5 — Автоматическая компиляция WebGL-игры в облаке и деплой на GitHub Pages

**Дата:** ноябрь 2026 · **Проект:** [Neckrite/2DPlatformerLab](https://github.com/Neckrite/2DPlatformerLab) · **Файлы:** [main.yml](.github/workflows/main.yml) · [ProjectSettings](ProjectSettings/ProjectSettings.asset)

## Цель

Настроить сквозной пайплайн, который после успешных тестов компилирует
реальный WebGL-билд игры в облаке, публикует его на GitHub Pages и
собирает Docker-образ с живой игрой вместо файлов-заглушек.

## Шаг 1. Подготовка Unity 6 к деплою на GitHub Pages

GitHub Pages не умеет раздавать заголовок `Content-Encoding: gzip`, поэтому
в `ProjectSettings/ProjectSettings.asset` включена резервная декомпрессия:

```
webGLCompressionFormat: 1      (Gzip)
webGLDecompressionFallback: 1  (ON — встроенный JS-распаковщик Unity)
```

Примечание: порядковый номер enum в Unity 6000.4 отличается от указанного
в методичке (`0`) — важен сам факт: Gzip + Decompression Fallback ON.
Без фолбэка игра на статическом хостинге зависает на этапе распаковки.

## Шаг 2. Задача build_game (облачная компиляция)

В `main.yml` добавлена задача `build_game` (`needs: run_tests`):

- `game-ci/unity-builder@v6` — новый движок GameCI (тонкая обёртка над
  game-ci/cli, лицензия Personal активируется по `UNITY_EMAIL` +
  `UNITY_PASSWORD` напрямую из секретов);
- `targetPlatform: WebGL`, `buildMethod: BuildManager.BuildWebGL` —
  вызов C# метода автоматизации из ЛР №1 (завершение через
  `EditorApplication.Exit`, поэтому результат виден пайплайну);
- `manualExit: true` (метод сам завершает редактор);
- кэш `Library-Build-*` сокращает повторные сборки;
- артефакт `WebGL-Build` (`actions/upload-artifact`) — настоящий
  скомпилированный билд передается дальше по пайплайну.

Задача `build_docker_image` переведена с генерации заглушек `touch` на
`actions/download-artifact`: Docker-образ собирается с живой игрой.

## Шаг 3. Задача deploy_to_pages

`deploy_to_pages` (`needs: build_game`) скачивает артефакт `WebGL-Build`
и публикует папку в служебную ветку `gh-pages` через
`JamesIves/github-pages-deploy-action@v4` (permissions `contents: write`).

## Шаг 4. Включение GitHub Pages

Однократно в Settings → Pages выбран источник (Deploy from a branch /
gh-pages / root). После этого каждый зеленый пайплайн автоматически
обновляет страницу игры.

## Результат

- `build_game` компилирует WebGL-билд в облаке методом из ЛР №1.
- Образ `ghcr.io/neckrite/platformer-webgl` содержит живую игру.
- Игра доступна на GitHub Pages.

## Контрольные вопросы

1. **Почему папка Builds/WebGL закрыта .gitignore и как файлы попадают
   в build_docker_image и deploy_to_pages?** Билд — результат компиляции,
   его не хранят в Git; файлы передаются между задачами артефактом
   WebGL-Build (upload/download-artifact).
2. **Зачем build_game needs: run_tests?** Гейт качества: при падении
   любого теста компиляция, деплой и сборка образа не запускаются.
3. **Роль BuildManager.BuildWebGL?** Выполняет компиляцию нашим
   настроенным пайплайном (сцена, WebGL, путь Builds/WebGL) и возвращает
   код завершения через EditorApplication.Exit — пайплайн видит успех/неуспех.
4. **Почему Pages требует Decompression Fallback, а Nginx — нет?**
   Pages не отдает Content-Encoding: gzip, распаковку делает JS-фолбэк
   Unity; Nginx отдает заголовок сам (ЛР №3).
5. **Pages vs Docker-образ в GHCR?** Pages — бесплатный хостинг для
   проверки; Docker-образ — переносимый артефакт для любого сервера;
   теги latest и github.sha — «всегда свежая» и «точная ревизия для
   отката».
6. **404 при зеленом deploy_to_pages?** Задача публикует ветку gh-pages,
   но не включает хостинг: нужно один раз включить Pages в настройках.
