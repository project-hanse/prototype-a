FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

ENV Logging__Console__FormatterName=Simple

WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "FileService.dll"]
