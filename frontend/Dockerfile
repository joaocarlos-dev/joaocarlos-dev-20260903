FROM node:24.20.0-alpine3.24 AS dependencies
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

FROM dependencies AS test
COPY . .
ENTRYPOINT ["npm", "run", "verify:ci"]

FROM dependencies AS build
COPY . .
RUN npm run build

FROM nginx:1.31.4-alpine AS final
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
EXPOSE 80
