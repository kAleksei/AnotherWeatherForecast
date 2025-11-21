---
phase: 15
title: Postman Collection and Final Integration
goal: Create comprehensive Postman collection with tests and finalize end-to-end integration
status: Planned
---

# Implementation Phase 15: Postman Collection and Final Integration

### Implementation Phase 15: Postman Collection and Final Integration

**GOAL-015**: Create comprehensive Postman collection with tests and finalize end-to-end integration

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-200 | Create Postman Collection v2.1 JSON file: `c:\Projects\Home\AnotherWeatherForecast\postman\WeatherForecast.postman_collection.json` | | |
| TASK-201 | Configure collection variables: `baseUrl` (http://localhost:5000), `date` ({{$isoTimestamp}}), `city` (London), `country` (GB) | | |
| TASK-202 | Create folder "Health Checks" with requests: GET /health, GET /health/ready, GET /health/live | | |
| TASK-203 | Add tests to health check requests: validate status 200, validate response schema (status, checks array) | | |
| TASK-204 | Create folder "Weather Forecast" with request "GET Aggregated Forecast" - URL: `{{baseUrl}}/api/weather/forecast?date={{date}}&city={{city}}&country={{country}}` | | |
| TASK-205 | Add tests to Aggregated Forecast: validate status 200, validate response schema (location, date, aggregatedForecast, sources array, metadata), validate aggregatedForecast.averageTemperatureCelsius is number, validate sources.length >= 3 | | |
| TASK-206 | Create request "GET Filtered by Source" with sources parameter: `sources=OpenMeteo` | | |
| TASK-207 | Add tests to Filtered by Source: validate sources array length = 1, validate sources[0].name = "OpenMeteo" | | |
| TASK-208 | Create request "GET Multiple Sources" with sources parameter: `sources=OpenMeteo,WeatherAPI` | | |
| TASK-209 | Add tests to Multiple Sources: validate sources array length = 2 | | |
| TASK-210 | Create request "GET Future Date" with pre-request script: set date = today + 3 days | | |
| TASK-211 | Create request "GET Historical Date" with pre-request script: set date = today - 3 days | | |
| TASK-212 | Create request "GET Invalid Date" with date = today + 10 days | | |
| TASK-213 | Add tests to Invalid Date: validate status 400, validate error message contains "7 days" | | |
| TASK-214 | Create folder "Cache Behavior" with request "GET First Request" - same as Aggregated Forecast | | |
| TASK-215 | Add tests to First Request: validate metadata.cacheHit = false | | |
| TASK-216 | Create request "GET Second Request" - duplicate of First Request | | |
| TASK-217 | Add tests to Second Request: validate metadata.cacheHit = true, validate responseTimeMs < first request | | |
| TASK-218 | Create folder "Error Scenarios" with request "GET Missing Required Parameters" - omit city parameter | | |
| TASK-219 | Add tests to Missing Required Parameters: validate status 400, validate errors object contains "city" | | |
| TASK-220 | Create request "GET Invalid Country Code" with country = "INVALID" | | |
| TASK-221 | Create request "GET Malformed Date Format" with date = "2025/11/20" (wrong format) | | |
| TASK-222 | Create Postman environment file: `postman\WeatherForecast-Dev.postman_environment.json` with baseUrl = http://localhost:5000 | | |
| TASK-223 | Create Postman environment file: `postman\WeatherForecast-Prod.postman_environment.json` with baseUrl = https://{{AZURE_FQDN}} | | |
| TASK-224 | Add Newman CLI test script to README: `newman run postman/WeatherForecast.postman_collection.json -e postman/WeatherForecast-Dev.postman_environment.json` | | |

