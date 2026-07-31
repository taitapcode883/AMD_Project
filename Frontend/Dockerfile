# Build context: repo root (docker build -f Frontend/Dockerfile .)
FROM node:20-alpine AS build
WORKDIR /app

COPY Frontend/package*.json ./
RUN npm ci

COPY Frontend/ .

# VITE_API_URL được inline vào bundle lúc build (Vite build-time env),
# nên phải truyền qua --build-arg, không dùng ENV lúc container chạy được.
# Để trống ("") nếu ApiGateway phục vụ cùng origin với frontend.
ARG VITE_API_URL=""
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY Frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
