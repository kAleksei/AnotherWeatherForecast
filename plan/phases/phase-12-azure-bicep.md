---
phase: 12
title: Azure Infrastructure as Code (Bicep)
goal: Create Bicep templates for all Azure resources with parameterization for dev/prod environments
status: Planned
---

# Implementation Phase 12: Azure Infrastructure as Code (Bicep)

### Implementation Phase 12: Azure Infrastructure as Code (Bicep)

**GOAL-012**: Create Bicep templates for all Azure resources with parameterization for dev/prod environments

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-155 | Create directory structure: `c:\Projects\Home\AnotherWeatherForecast\infra\`, `infra\modules\`, `infra\parameters\` | | |
| TASK-156 | Create `main.bicep` in `infra\main.bicep` with parameters: environmentName, location, appName, redisEnabled | | |
| TASK-157 | Create `container-registry.bicep` in `infra\modules\container-registry.bicep`: Azure Container Registry (Basic SKU), admin enabled, output: loginServer, name | | |
| TASK-158 | Create `log-analytics.bicep` in `infra\modules\log-analytics.bicep`: Log Analytics Workspace, retention 90 days, output: workspaceId | | |
| TASK-159 | Create `app-insights.bicep` in `infra\modules\app-insights.bicep`: Application Insights linked to Log Analytics, output: connectionString, instrumentationKey | | |
| TASK-160 | Create `redis.bicep` in `infra\modules\redis.bicep`: Azure Cache for Redis (Basic C0 250MB), non-SSL port enabled for testing, output: connectionString | | |
| TASK-161 | Create `container-app.bicep` in `infra\modules\container-app.bicep`: Container Apps Environment, Container App with image reference, environment variables, scaling rules (0-10 replicas) | | |
| TASK-162 | Configure Container App environment variables in Bicep: ConnectionStrings__Redis (from redis output), WeatherSources keys (from parameters), ApplicationInsights__ConnectionString | | |
| TASK-163 | Configure Container App ingress: external=true, targetPort=8080, allowInsecure=false (HTTPS) | | |
| TASK-164 | Configure Container App scaling: minReplicas=0, maxReplicas=10, rules based on HTTP concurrent requests (10) | | |
| TASK-165 | In main.bicep, orchestrate modules: (1) Log Analytics (2) App Insights (3) Container Registry (4) Redis (if enabled) (5) Container App | | |
| TASK-166 | Output from main.bicep: containerAppFqdn, containerRegistryLoginServer, appInsightsConnectionString | | |
| TASK-167 | Create `dev.parameters.json` in `infra\parameters\dev.parameters.json` with values: environmentName=dev, location=eastus, appName=weatherforecast-dev, redisEnabled=false (use memory only) | | |
| TASK-168 | Create `prod.parameters.json` in `infra\parameters\prod.parameters.json` with values: environmentName=prod, location=eastus, appName=weatherforecast-prod, redisEnabled=true | | |

