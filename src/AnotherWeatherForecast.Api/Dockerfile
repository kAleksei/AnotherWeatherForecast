FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "./Directory.Packages.props"]
COPY ["Directory.Build.props", "./Directory.Build.props"]
COPY ["src/AnotherWeatherForecast.Api/AnotherWeatherForecast.Api.csproj", "AnotherWeatherForecast.Api/"]
COPY ["src/AnotherWeatherForecast.Application/AnotherWeatherForecast.Application.csproj", "AnotherWeatherForecast.Application/"]
COPY ["src/AnotherWeatherForecast.Domain/AnotherWeatherForecast.Domain.csproj", "AnotherWeatherForecast.Domain/"]
COPY ["src/AnotherWeatherForecast.Infrastructure/AnotherWeatherForecast.Infrastructure.csproj", "AnotherWeatherForecast.Infrastructure/"]

RUN dotnet restore "AnotherWeatherForecast.Api/AnotherWeatherForecast.Api.csproj"

COPY ["src/AnotherWeatherForecast.Api/", "AnotherWeatherForecast.Api/"]
COPY ["src/AnotherWeatherForecast.Application/", "AnotherWeatherForecast.Application/"]
COPY ["src/AnotherWeatherForecast.Domain/", "AnotherWeatherForecast.Domain/"]
COPY ["src/AnotherWeatherForecast.Infrastructure/", "AnotherWeatherForecast.Infrastructure/"]

WORKDIR "/src/AnotherWeatherForecast.Api"
RUN dotnet build "./AnotherWeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/AnotherWeatherForecast.Api"
RUN dotnet publish "./AnotherWeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-build --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnotherWeatherForecast.Api.dll"]
