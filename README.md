# 2D Platformer Lab — АРПО (лабораторные работы)

Проект 2D-платформера (Unity 6, шаблон Microgame) с наращиваемой DevOps-инфраструктурой: CI/CD на GitHub Actions, зеркалирование в резервный репозиторий, Docker-контейнеризация WebGL-сборки.

**Играть:** <https://neckrite.github.io/2DPlatformerLab/> · **CI:** [Actions](https://github.com/Neckrite/2DPlatformerLab/actions) · **Registry:** [Packages](https://github.com/Neckrite?tab=packages)

| Лабораторная работа | Тема | Отчёт |
|---|---|---|
| ЛР №1 | Создание 2D-платформера и базовая сборка WebGL | [LR1.md](LR1.md) |
| ЛР №2 | Настройка репозиториев GitHub, Sanity Check и зеркалирование | [LR2.md](LR2.md) |
| ЛР №3 | Контейнеризация веб-сервера в Docker и раздача сжатых файлов Nginx | [LR3.md](LR3.md) |

## Инфраструктура репозитория

- `.github/workflows/main.yml` — единый пайплайн из трёх задач: `sanity_check` → `mirror_repo` → `docker_ghcr`.
- `nginx.conf` + `Dockerfile` — веб-сервер Nginx (Alpine) для раздачи Gzip-сборки WebGL с заголовками `Content-Encoding: gzip`.
- `Builds/WebGL` — локальная WebGL-сборка (не попадает в git, пересоздаётся командой `-executeMethod SetupBuildGzip.SetupAndBuildGzip`).
