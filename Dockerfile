# Базовый слой (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Создание безопасного пользователя
RUN useradd -m -u 1001 appuser

# Создание директорий и установка прав
RUN mkdir -p /app /app/Config /app/Logs /app/keys-linux && chown -R appuser:appuser /app

WORKDIR /app
EXPOSE 8080

# Слой сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["CRMService.csproj", "."]
RUN dotnet restore "./CRMService.csproj"

COPY . .
RUN dotnet build "./CRMService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Слой публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CRMService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный слой (runtime + опубликованное приложение)
FROM base AS final

COPY --from=publish /app/publish .

# Переход на безопасного пользователя
USER appuser

ENTRYPOINT ["dotnet", "CRMService.dll"]