# Docker-образ веб-сервера Nginx (Alpine) с Unity 6 WebGL-игрой (Gzip).
# Собирается локально:  docker build -t platformer-compressed:v1 .
# Запуск:               docker run -d -p 8080:80 --name unity_compressed_container platformer-compressed:v1
# Проверка:             http://localhost:8080  (F12 -> Network -> у .gz файлов заголовок Content-Encoding: gzip)
FROM nginx:alpine

# Удаляем стандартную конфигурацию Nginx (не умеет отдавать Unity-архивы)
RUN rm /etc/nginx/conf.d/default.conf

# Подставляем нашу конфигурацию с заголовками Content-Encoding: gzip
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Копируем WebGL-сборку игры (index.html + Build/*.gz) в корень сервера
COPY Builds/WebGL/ /usr/share/nginx/html/

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
