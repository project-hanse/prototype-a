# Build
FROM node:21.5-alpine as build-step

ARG NG_CONFIG=production

RUN npm install -g @angular/cli

WORKDIR /app

COPY package.json /app
COPY package-lock.json /app

RUN npm ci

COPY . /app

RUN ng build --output-path=./dist/out --configuration ${NG_CONFIG}

# Deployment

FROM nginx:1.25.3-alpine as run-step
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

# Replacing default config with Angular specific
RUN rm /etc/nginx/conf.d/default.conf
COPY src/etc/nginx/conf.d/ng.conf /etc/nginx/conf.d/

COPY --from=build-step /app/dist/out/ /usr/share/nginx/html
