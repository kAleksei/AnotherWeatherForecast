---
phase: 13
title: CI/CD Pipeline (GitHub Actions)
goal: Implement automated build, test, docker push, infrastructure deployment, and application deployment pipeline
status: Planned
---

# Implementation Phase 13: CI/CD Pipeline (GitHub Actions)

### Implementation Phase 13: CI/CD Pipeline (GitHub Actions)

**GOAL-013**: Implement automated build, test, docker push, infrastructure deployment, and application deployment pipeline

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-169 | Create `.github\workflows\deploy.yml` in `c:\Projects\Home\AnotherWeatherForecast\.github\workflows\deploy.yml` | | |
| TASK-170 | Configure workflow triggers: push to main branch, pull_request (build/test only), workflow_dispatch | | |
| TASK-171 | Define workflow environment variables: AZURE_RESOURCE_GROUP, AZURE_LOCATION, APP_NAME, REGISTRY_NAME | | |
| TASK-172 | Create Job 1 "build-and-test": runs-on ubuntu-latest, steps: checkout, setup .NET 8, restore, build, test with coverage | | |
| TASK-173 | Upload test results and coverage artifacts in build-and-test job | | |
| TASK-174 | Create Job 2 "docker-build-push": needs build-and-test, runs only on push to main | | |
| TASK-175 | Docker build-push steps: checkout, Azure login (OIDC), ACR login, Docker build with tags (git SHA + latest), Docker push | | |
| TASK-176 | Create Job 3 "infrastructure-deploy": needs build-and-test, runs only on push to main | | |
| TASK-177 | Infrastructure deploy steps: checkout, Azure login, deploy Bicep (az deployment group create), capture outputs | | |
| TASK-178 | Create Job 4 "app-deploy": needs [docker-build-push, infrastructure-deploy], runs only on push to main | | |
| TASK-179 | App deploy steps: Azure login, update Container App with new image (az containerapp update), wait for deployment | | |
| TASK-180 | Create Job 5 "post-deploy-tests": needs app-deploy, runs only on push to main | | |
| TASK-181 | Post-deploy test steps: health check curl (retry 5 times), smoke test GET /api/weather/forecast with valid parameters, validate response schema | | |
| TASK-182 | Configure GitHub secrets requirements in README: AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID, WEATHERAPI_KEY, OPENWEATHERMAP_KEY | | |
| TASK-183 | Add rollback step in app-deploy: if health check fails after deployment, rollback to previous revision | | |

