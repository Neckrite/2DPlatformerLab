# LR1 — Создание 2D-платформера и базовая сборка WebGL

**Дата:** сентябрь 2026 · **Проект:** [Neckrite/2DPlatformerLab](https://github.com/Neckrite/2DPlatformerLab) · **Unity:** 6000.4.8f1

## Шаги выполнения

1. **Создание проекта.** Unity Hub → Unity 6 (6000.4.8f1), шаблон *2D Platformer Microgame* (URP). Проект проинициализирован как git-репозиторий, первый коммит `chore: initializing a 2D Platformer Microgame project` отправлен в GitHub.
2. **Изучение структуры.** `Assets/Scenes/Game.unity` — игровая сцена; скрипты движения персонажа (`*Controller.cs`) и менеджера сборки (`BuildManager.cs`); `.gitignore` отсекает временные каталоги Unity.
3. **Настройка сборки WebGL.** `File → Build Profiles → Player Settings → Player → WebGL → Publishing Settings`: Compression Format = **Disabled** (по условию ЛР1), шаблон страницы — по умолчанию.
4. **Автоматическая сборка.** Подготовлен editor-скрипт `SetupBuildSettings.SetupAndBuild` (добавляет сцену в Build Settings, задаёт compression Disabled и вызывает `BuildManager.BuildWebGL()`). Сборка запускалась headless:

   ```bat
   Unity.exe -batchmode -quit -projectPath D:\PSU\KURS_4\АРПО\lab1\2DPlatformerLab -executeMethod SetupBuildSettings.SetupAndBuild -logFile build_webgl2.log
   ```

   Результат — папка `Builds/WebGL` с `index.html`, `Build/WebGL.loader.js`, `WebGL.framework.js`, `WebGL.wasm`, `WebGL.data`.
5. **Публикация на GitHub Pages.** Собранная папка размещена через ветку `gh-pages` (Pages build and deployment — success). Игра доступна по адресу: <https://neckrite.github.io/2DPlatformerLab/>
6. **Pull Request.** Изменения оформлены в ветке `LR1` → pull request → **Merge pull request #1** в `main`.

## Результат

- WebGL-версия 2D-платформера собирается автоматически одной командой и работает на GitHub Pages.
- Ссылка на игру: <https://neckrite.github.io/2DPlatformerLab/>

## Контрольные вопросы (ЛР1)

- **Зачем отключать сжатие на этапе ЛР1?** Чтобы сборку можно было раздавать любому статическому хостингу (GitHub Pages) без управления HTTP-заголовками `Content-Encoding`.
- **Что делает `-executeMethod`?** Позволяет вызвать C#-метод из Editor-сборки в headless-режиме — основа автоматизации CI.
