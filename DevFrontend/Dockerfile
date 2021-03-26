# Build
FROM node:10-alpine as build-step

RUN mkdir -p /app/build

WORKDIR /app

COPY package.json /app
COPY package-lock.json /app

RUN npm ci

COPY . /app

RUN npm run build -- --output-path=./dist/out

# Deployment

FROM nginx:1.19-alpine

# Replacing default config with Angular specific
RUN rm /etc/nginx/conf.d/default.conf
COPY src/etc/nginx/conf.d/ng.conf /etc/nginx/conf.d/

COPY --from=build-step /app/dist/out/ /usr/share/nginx/html
