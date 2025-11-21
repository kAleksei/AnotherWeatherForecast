---
phase: 17
title: Azure Deployment and Validation
goal: Deploy infrastructure and application to Azure, validate end-to-end functionality in production
status: Planned
---

# Implementation Phase 17: Azure Deployment and Validation

### Implementation Phase 17: Azure Deployment and Validation

**GOAL-017**: Deploy infrastructure and application to Azure, validate end-to-end functionality in production

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-241 | Set up Azure service principal with OIDC for GitHub Actions: `az ad sp create-for-rbac --name weatherforecast-github --role contributor --scopes /subscriptions/{subscription-id}` | | |
| TASK-242 | Configure GitHub secrets: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID from service principal output | | |
| TASK-243 | Configure GitHub secrets for API keys: WEATHERAPI_KEY, OPENWEATHERMAP_KEY | | |
| TASK-244 | Create Azure Resource Group: `az group create --name rg-weatherforecast-prod --location eastus` | | |
| TASK-245 | Deploy infrastructure via Bicep: `az deployment group create --resource-group rg-weatherforecast-prod --template-file infra/main.bicep --parameters infra/parameters/prod.parameters.json` | | |
| TASK-246 | Capture Bicep outputs: containerRegistryLoginServer, appInsightsConnectionString, containerAppFqdn | | |
| TASK-247 | Build Docker image with Azure Container Registry: `az acr build --registry {ACR_NAME} --image weatherforecast:latest --image weatherforecast:1.0 .` | | |
| TASK-248 | Update Container App with image and environment variables: `az containerapp update --name weatherforecast-prod --resource-group rg-weatherforecast-prod --image {ACR_LOGIN_SERVER}/weatherforecast:latest` | | |
| TASK-249 | Configure environment variables in Container App: ConnectionStrings__Redis, WeatherSources API keys, ApplicationInsights connection string | | |
| TASK-250 | Test health endpoint: `curl https://{CONTAINER_APP_FQDN}/health` - verify status Healthy | | |
| TASK-251 | Test weather forecast endpoint: `curl "https://{CONTAINER_APP_FQDN}/api/weather/forecast?date=2025-11-20&city=London&country=GB"` - verify aggregated response | | |
| TASK-252 | Import Postman collection, update Prod environment with CONTAINER_APP_FQDN, run all requests - verify all tests pass | | |
| TASK-253 | Verify OpenTelemetry data in Azure Application Insights: check request traces, dependencies, custom metrics | | |
| TASK-254 | Verify structured logs in Log Analytics: query logs with KQL, verify request/response logs present | | |
| TASK-255 | Test auto-scaling: generate load using Apache Bench or k6, verify Container App scales up to 10 replicas | | |
| TASK-256 | Test cache behavior: make same request twice, verify second request has lower response time and cacheHit=true | | |
| TASK-257 | Test graceful degradation: disable one weather source API key, verify service returns degraded health but continues serving partial data | | |
| TASK-258 | Commit all changes to Git: `git add .`, `git commit -m "feat: complete weather forecast microservice with Azure deployment"` | | |
| TASK-259 | Push to GitHub main branch: `git push origin main` - verify GitHub Actions workflow triggers and completes successfully | | |
| TASK-260 | Verify CI/CD pipeline: check workflow run in GitHub Actions, verify all jobs pass (build, test, docker push, infrastructure deploy, app deploy, post-deploy tests) | | |
