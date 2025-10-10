# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update \
    && apt-get install -y --no-install-recommends fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SocialNet.WebAPI/SocialNet.WebAPI.csproj", "SocialNet.WebAPI/"]
COPY ["SocialNet.Core/SocialNet.Core.csproj", "SocialNet.Core/"]
COPY ["SocialNet.Domain/SocialNet.Domain.csproj", "SocialNet.Domain/"]
COPY ["SocialNet.Infrastructure/SocialNet.Infrastructure.csproj", "SocialNet.Infrastructure/"]
RUN dotnet restore "./SocialNet.WebAPI/SocialNet.WebAPI.csproj"
COPY . .
WORKDIR "/src/SocialNet.WebAPI"
RUN dotnet build "./SocialNet.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SocialNet.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialNet.WebAPI.dll"]