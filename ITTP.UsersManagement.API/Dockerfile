# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ITTP.UsersManagement.API/ITTP.UsersManagement.API.csproj", "ITTP.UsersManagement.API/"]
COPY ["ITTP.UsersManagement.API.Application/ITTP.UsersManagement.API.Application.csproj", "ITTP.UsersManagement.API.Application/"]
COPY ["ITTP.UsersManagement.API.Core/ITTP.UsersManagement.API.Core.csproj", "ITTP.UsersManagement.API.Core/"]
COPY ["ITTP.UsersManagement.API.DataAccess/ITTP.UsersManagement.API.DataAccess.csproj", "ITTP.UsersManagement.API.DataAccess/"]
RUN dotnet restore "./ITTP.UsersManagement.API/ITTP.UsersManagement.API.csproj"
COPY . .
WORKDIR "/src/ITTP.UsersManagement.API"
RUN dotnet build "./ITTP.UsersManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ITTP.UsersManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ITTP.UsersManagement.API.dll"]