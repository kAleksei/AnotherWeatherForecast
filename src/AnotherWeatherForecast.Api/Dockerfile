FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AnotherWeatherForecast.Api/AnotherWeatherForecast.Api.csproj", "AnotherWeatherForecast.Api/"]
COPY ["AnotherWeatherForecast.Application/AnotherWeatherForecast.Application.csproj", "AnotherWeatherForecast.Application/"]
COPY ["AnotherWeatherForecast.Domain/AnotherWeatherForecast.Domain.csproj", "AnotherWeatherForecast.Domain/"]
COPY ["AnotherWeatherForecast.Infrastructure/AnotherWeatherForecast.Infrastructure.csproj", "AnotherWeatherForecast.Infrastructure/"]
RUN dotnet restore "AnotherWeatherForecast.Api/AnotherWeatherForecast.Api.csproj"
COPY . .
WORKDIR "/src/AnotherWeatherForecast.Api"
RUN dotnet build "./AnotherWeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AnotherWeatherForecast.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnotherWeatherForecast.Api.dll"]
