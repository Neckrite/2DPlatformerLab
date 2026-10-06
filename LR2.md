# LR2 — Настройка репозиториев на GitHub и базовая автоматическая проверка проекта (Sanity Check)

**Дата:** сентябрь 2026 · **Проект:** [Neckrite/2DPlatformerLab](https://github.com/Neckrite/2DPlatformerLab) · **CI:** [.github/workflows/main.yml](.github/workflows/main.yml)

## Цель

Изучение декларативных сценариев CI на платформе GitHub Actions: автоматическая верификация структуры проекта (Sanity Check), безопасное управление секретами и автоматическое зеркалирование репозитория в резервный контур (Disaster Recovery).

## Шаги выполнения

### Шаг 1. Резервный репозиторий

Создан пустой приватный репозиторий [Neckrite/2DPlatformerLab_backup](https://github.com/Neckrite/2DPlatformerLab_backup) — **без** README и .gitignore, чтобы push полного графа коммитов прошёл без конфликтов.

### Шаг 2. Генерация PAT-токена

`Settings → Developer settings → Personal access tokens → Tokens (classic) → Generate new token (classic)`: имя `Backup-Token`, области **repo** + **workflow**. Токен скопирован сразу (показывается один раз).

### Шаг 3. Секрет в основном репозитории

`Settings → Secrets and variables → Actions → New repository secret`: имя **BACKUP_TOKEN**, значение — токен из шага 2. Проверка: `gh secret list` показывает `BACKUP_TOKEN`.

### Шаг 4. YAML-пайплайн `.github/workflows/main.yml`

Файл создан строго в служебной директории `.github/workflows/`. Две задачи:

```yaml
on:
  push:
    branches: [ "main" ]   # триггер: пуш в main

jobs:
  sanity_check:            # Задача 1 — проверка структуры проекта
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Check Unity Directories        # ProjectSettings/, Packages/ обязаны существовать
      - name: Locate 2D Platformer Scripts   # поиск *Controller.cs и BuildManager.cs в Assets/

  mirror_repo:             # Задача 2 — зеркалирование (Disaster Recovery)
    needs: sanity_check    # запускается ТОЛЬКО после успешной проверки
    if: github.repository == 'Neckrite/2DPlatformerLab'   # защита от цикла mirror→mirror
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0          # вся история коммитов, а не только последний
          persist-credentials: false
      - name: Verify token is set               # fail-fast, если секрет пуст
      - name: Push to Backup Repository
        run: git push --force https://x-access-token:${BACKUP_TOKEN}@github.com/Neckrite/2DPlatformerLab_backup.git main
```

### Шаг 5–6. Отправка и проверка

Ветка `LR2` → Pull Request → **Merge pull request #2** в `main`. Push в `main` вызвал workflow, обе задачи завершились зелёными галочками (**22 s**): sanity_check и mirror_repo. Резервный репозиторий перестал быть пустым — Actions сам продублировал код и историю коммитов.

## Контрольные вопросы

1. **run vs uses.** `run` выполняет произвольные shell-команды на runner'е; `uses` подключает готовый многоразовый Action (плагин) из маркетплейса или репозитория (например, `actions/checkout@v4`).
2. **YAML-вложенность.** Иерархия задаётся отступами из пробелов (регистр важен); списки — `- элемент`, словари — `ключ: значение`. Символ **Tab запрещён** и вызывает ошибку парсинга.
3. **needs.** Задаёт зависимость задач: `mirror_repo` стартует только после успешного `sanity_check`. Без `needs` задачи выполняются параллельно и зеркалирование могло бы запустить непроверенный (сломанный) код.
4. **fetch-depth: 0.** По умолчанию checkout забирает только последний коммит (shallow-clone) — этого недостаточно для зеркалирования: в backup нужен весь граф коммитов и история, поэтому fetch-depth: 0.
5. **Почему Secrets.** Токен в коде попал бы в открытый репозиторий (утечка учётных данных). Repository Secrets шифруются GitHub и подставляются в переменные окружения во время выполнения, не попадая в логи и исходники.
6. **Триггер.** `on: push: branches: [main]` — автоматический запуск при каждом push в основную ветку.
7. **Sanity Check до зеркалирования.** Проверка соответствия индивидуальному варианту (структура проекта, наличие ключевых скриптов) отбраковывает сломанные коммиты: в резервный контур попадает только верифицированный код, backup остаётся консистентным и рабочим.
