---
phase: 11
title: Docker Configuration
goal: Create optimized Docker configuration for local development and production deployment
status: Planned
---

# Implementation Phase 11: Docker Configuration

### Implementation Phase 11: Docker Configuration

**GOAL-011**: Create optimized Docker configuration for local development and production deployment

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-138 | Create `Dockerfile` in `c:\Projects\Home\AnotherWeatherForecast\Dockerfile` (multi-stage) | | |
| TASK-139 | Build stage: FROM `mcr.microsoft.com/dotnet/sdk:8.0` AS build, WORKDIR /src | | |
| TASK-140 | Copy solution and csproj files, RUN `dotnet restore src/WeatherForecast.Api/WeatherForecast.Api.csproj` | | |
| TASK-141 | Copy all source code, RUN `dotnet build -c Release -o /app/build` | | |
| TASK-142 | Publish stage: FROM build AS publish, RUN `dotnet publish -c Release -o /app/publish /p:UseAppHost=false` | | |
| TASK-143 | Runtime stage: FROM `mcr.microsoft.com/dotnet/aspnet:8.0` AS final, WORKDIR /app | | |
| TASK-144 | Create non-root user: RUN `adduser --disabled-password --gecos '' appuser && chown -R appuser /app`, USER appuser | | |
| TASK-145 | COPY from publish stage, EXPOSE 8080 | | |
| TASK-146 | Add HEALTHCHECK: `--interval=30s --timeout=3s --start-period=5s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1` | | |
| TASK-147 | ENTRYPOINT ["dotnet", "WeatherForecast.Api.dll"] | | |
| TASK-148 | Create `.dockerignore` in `c:\Projects\Home\AnotherWeatherForecast\.dockerignore` | | |
| TASK-149 | Add to .dockerignore: `**/.git`, `**/.vs`, `**/.vscode`, `**/bin`, `**/obj`, `**/.gitignore`, `**/docker-compose*`, `**/Dockerfile*`, `**/*.md`, `**/secrets.json` | | |
| TASK-150 | Create `docker-compose.yml` in `c:\Projects\Home\AnotherWeatherForecast\docker-compose.yml` | | |
| TASK-151 | Configure api service: build context, ports 5000:8080, environment variables (ASPNETCORE_ENVIRONMENT, ConnectionStrings__Redis, WeatherSources keys), depends_on redis | | |
| TASK-152 | Configure redis service: image `redis:7-alpine`, ports 6379:6379, volume `redis-data:/data` | | |
| TASK-153 | Test Docker build locally: `docker build -t weatherforecast:local .` | | |
| TASK-154 | Test docker-compose locally: `docker-compose up`, verify API accessible at http://localhost:5000/health | | |

