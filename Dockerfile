# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# В зависимости от операционной системы хост-компьютеров, которые будут выполнять сборку контейнеров или запускать их, может потребоваться изменить образ, указанный в инструкции FROM.
# Дополнительные сведения см. на странице https://aka.ms/containercompat

# -----------------------------
# Этап 1: сборка бэкенда (.NET 8)
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj для кэша и restore
COPY ../SocialNet.Core/SocialNet.Core.csproj SocialNet.Core/
COPY ../SocialNet.Domain/SocialNet.Domain.csproj SocialNet.Domain/
COPY ../SocialNet.Infrastructure/SocialNet.Infrastructure.csproj SocialNet.Infrastructure/
COPY SocialNet.WebAPI.csproj SocialNet.WebAPI/

# Восстановление зависимостей
WORKDIR /src/SocialNet.WebAPI
RUN dotnet restore SocialNet.WebAPI.csproj

# Копируем исходники
WORKDIR /src
COPY ../SocialNet.Core/ SocialNet.Core/
COPY ../SocialNet.Domain/ SocialNet.Domain/
COPY ../SocialNet.Infrastructure/ SocialNet.Infrastructure/
COPY ./ SocialNet.WebAPI/

# Копируем шаблоны почты, если нужны
COPY ../SocialNet.Core/Services/Emails/Templates/ SocialNet.Core/Services/Emails/Templates/

# Сборка и публикация
WORKDIR /src/SocialNet.WebAPI
RUN dotnet build SocialNet.WebAPI.csproj -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish SocialNet.WebAPI.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# -----------------------------
# Этап 2: финальный образ
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./backend

EXPOSE 8080 8081 80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:8080;http://+:8081

ENTRYPOINT ["dotnet", "backend/SocialNet.WebAPI.dll"]