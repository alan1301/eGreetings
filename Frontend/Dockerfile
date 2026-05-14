# syntax=docker/dockerfile:1.7
# ── Stage 1: build Angular app ───────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build -- --configuration production

# ── Stage 2: nginx runtime ──────────────────────────────────────────
FROM nginx:alpine AS runtime

# Angular 17+ (application builder) xuất ra dist/<project>/browser
COPY --from=build /app/dist/Frontend3/browser/ /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
