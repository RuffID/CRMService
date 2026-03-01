FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["nuget.config", "."]
COPY ["CRMService.Web/CRMService.csproj", "CRMService.Web/"]
COPY ["CRMService.Application/CRMService.Application.csproj", "CRMService.Application/"]
COPY ["CRMService.Contracts/CRMService.Contracts.csproj", "CRMService.Contracts/"]
COPY ["CRMService.Domain/CRMService.Domain.csproj", "CRMService.Domain/"]
COPY ["CRMService.Infrastructure/CRMService.Infrastructure.csproj", "CRMService.Infrastructure/"]
RUN dotnet restore "./CRMService.Web/CRMService.csproj"

COPY . .
RUN dotnet build "./CRMService.Web/CRMService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CRMService.Web/CRMService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

RUN mkdir -p /app /app/Config /app/Logs /app/keys-linux && chown -R 1001:1001 /app
COPY --from=publish --chown=1001:1001 /app/publish .

USER 1001

ENTRYPOINT ["dotnet", "CRMService.dll"]
