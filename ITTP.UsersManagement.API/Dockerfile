FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ITTP.UsersManagement.API/ITTP.UsersManagement.API.csproj", "ITTP.UsersManagement.API/"]
COPY ["ITTP.UsersManagement.API.Core/ITTP.UsersManagement.API.Core.csproj", "ITTP.UsersManagement.API.Core/"]
COPY ["ITTP.UsersManagement.API.DataAccess/ITTP.UsersManagement.API.DataAccess.csproj", "ITTP.UsersManagement.API.DataAccess/"]
COPY ["ITTP.UsersManagement.API.Application/ITTP.UsersManagement.API.Application.csproj", "ITTP.UsersManagement.API.Application/"]
RUN dotnet restore "ITTP.UsersManagement.API/ITTP.UsersManagement.API.csproj"
COPY . .
WORKDIR "/src/ITTP.UsersManagement.API"
RUN dotnet build "./ITTP.UsersManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ITTP.UsersManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ITTP.UsersManagement.API.dll"]
