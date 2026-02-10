FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Создание безопасного пользователя
#RUN useradd -m -u 1001 appuser

# Создание директорий и установка прав
#RUN mkdir -p /app /app/Config /app/Logs /app/keys-linux && chown -R appuser:appuser /app

WORKDIR /app
EXPOSE 8080

# Слой сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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

#RUN mkdir -p /app /app/Config /app/Logs /app/keys-linux && chown -R appuser:appuser /app
RUN mkdir -p /app /app/Config /app/Logs /app/keys-linux && chown -R 1001:1001 /app
#COPY --from=publish /app/publish .
COPY --from=publish --chown=1001:1001 /app/publish .

# Переход на безопасного пользователя
#USER appuser
USER 1001

ENTRYPOINT ["dotnet", "CRMService.dll"]