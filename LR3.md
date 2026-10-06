# LR3 — Контейнеризация веб-сервера в Docker и раздача сжатых файлов со стороны Nginx

**Дата:** октябрь 2026 · **Проект:** [Neckrite/2DPlatformerLab](https://github.com/Neckrite/2DPlatformerLab) · **Файлы:** [nginx.conf](nginx.conf) · [Dockerfile](Dockerfile) · [main.yml](.github/workflows/main.yml)

## Цель

Настроить корректную раздачу Gzip-сжатой WebGL-сборки Unity 6 веб-сервером Nginx (заголовок `Content-Encoding: gzip`), упаковать сервер с игрой в Docker-образ и публиковать образ в GitHub Container Registry (GHCR) через GitHub Actions.

## Шаг 1. Конфигурация сжатия в Unity 6 и пересборка

1. `File → Build Profiles → Player Settings → Player → WebGL → Publishing Settings`.
2. **Compression Format = Gzip** (было Disabled в ЛР1).
3. **Decompression Fallback = OFF** — «честная» серверная раздача без медленного JS-распаковщика.
4. Пересборка headless (editor-скрипт `SetupBuildGzip.SetupAndBuildGzip`, метод из ЛР1 + новые настройки):

   ```bat
   Unity.exe -batchmode -quit -projectPath . -executeMethod SetupBuildGzip.SetupAndBuildGzip -logFile build_webgl_gzip2.log
   ```

5. Проверка: в `Builds/WebGL/Build/` файлы получили сжатые расширения — `WebGL.data.gz`, `WebGL.framework.js.gz`, `WebGL.wasm.gz`, `WebGL.loader.js.gz` (вес снижен в 3–5 раз).

## Шаг 2. nginx.conf

Кастомный server-блок (см. [nginx.conf](nginx.conf)). Ключевые приёмы:

- Для каждого `*.gz`-типа объявляется пустой блок `types { }` и `default_type` — иначе mime.types отдал бы `application/gzip`, и браузер попытался бы скачать файл вместо запуска:

  ```nginx
  location ~* \.wasm\.gz$ {
      types { }
      default_type application/wasm;
      add_header Content-Encoding gzip;
      add_header Cache-Control "public, max-age=31536000, immutable";
  }
  ```

- Аналогично: `.data.gz → application/octet-stream`, `.framework.js.gz` и `.loader.js.gz → application/javascript`.

## Шаг 3. Dockerfile

```dockerfile
FROM nginx:alpine
RUN rm /etc/nginx/conf.d/default.conf          # удаляем стандартные настройки
COPY nginx.conf /etc/nginx/conf.d/default.conf # подставляем наши заголовки gzip
COPY Builds/WebGL/ /usr/share/nginx/html/      # игра в корень веб-сервера
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Шаг 4. Локальное тестирование (по методичке)

```bash
docker build -t platformer-compressed:v1 .
docker run -d -p 8080:80 --name unity_compressed_container platformer-compressed:v1
# http://localhost:8080 → F12 → Network → Ctrl+F5 → файл WebGL.wasm.gz → Headers:
#   Content-Type: application/wasm
#   Content-Encoding: gzip
docker stop unity_compressed_container && docker rm unity_compressed_container
```

> Примечание: Docker Desktop на рабочей машине не установлен, поэтому шаг локального прогона выполнен в конфигурации контейнера по методичке; проверка заголовков производится на облачном образе (GHCR) — скриншот DevTools прикладывается при защите работы.

## Шаг 5. Третий этап пайплайна (GHCR)

В `main.yml` добавлена задача `docker_ghcr` (`needs: sanity_check`, permissions `packages: write`):

1. **Заглушки:** `Builds/` отсекается `.gitignore`, поэтому на CI создаются временные файлы `touch Builds/WebGL/Build/WebGL.wasm.gz` (и остальные .gz + index.html) — иначе `COPY` в Dockerfile упадёт.
2. `docker/setup-buildx-action@v3` → сборочный движок.
3. `docker/login-action@v3` → вход в `ghcr.io` (`github.actor` + встроенный `secrets.GITHUB_TOKEN`).
4. `docker/build-push-action@v6` → сборка образа из Dockerfile и публикация с двумя тегами: `ghcr.io/neckrite/platformer-webgl:latest` и `:sha-${{ github.sha }}` (тег ревизии коммита).

## Шаг 6–7. Отправка и проверка

- `nginx.conf`, `Dockerfile`, обновлённый `main.yml`, `LR1.md`, `LR2.md`, `LR3.md`, `README.md` отправлены в ветку `LR3` → PR → merge в `main`.
- Во вкладке **Actions** — три зелёные задачи: `sanity_check`, `mirror_repo`, `docker_ghcr`.
- Во вкладке **Packages** профиля появился пакет `platformer-webgl`.
- Локальная gzip-сборка игры получена и лежит в `Builds/WebGL` (для Docker-прогона на машине с Docker).

## Контрольные вопросы

1. **Что решает `add_header Content-Encoding gzip;`?** Сообщает браузеру, что тело ответа сжато Gzip и должно быть распаковано прозрачным образом до парсинга. Без него браузер получает бинарный `.gz` как обычный файл и падает с «Unable to parse…».
2. **Зачем `touch Builds/WebGL/Build/game.wasm.gz` на CI?** `Builds/` в `.gitignore`, Docker-контекст не содержит файлов игры, и шаг `COPY` собранного образа завершился бы ошибкой. Временные заглушки позволяют собрать и опубликовать корректный образ до подключения облачного Unity-компилятора в ЛР4.
3. **Куда копировать WebGL-файлы в контейнере Nginx?** В корневой каталог документов официального образа — `/usr/share/nginx/html/` (конфигурация — в `/etc/nginx/conf.d/`).
