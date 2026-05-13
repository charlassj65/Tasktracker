# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files first for better layer caching.
# npm ci only re-runs when package-lock.json changes.
COPY package.json package-lock.json ./
RUN npm ci

# The API base URL is baked into the bundle at build time.
# Override with: docker-compose build --build-arg VITE_API_BASE_URL=https://your-api
ARG VITE_API_BASE_URL=http://localhost:5200
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

COPY . .
RUN npm run build

# ── Stage 2: Serve with Nginx ─────────────────────────────────────────────────
FROM nginx:stable-alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
