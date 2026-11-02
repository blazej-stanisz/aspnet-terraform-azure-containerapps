# MySimpleNetApp

A small demo application that shows the full path from source code to a running container:
an ASP.NET Core web app with a PostgreSQL database, a REST API, Swagger, built-in metrics and tests.

This is a showcase project, not a production product. The goal is to keep one simple app and
show how it is built, tested, containerized and (soon) deployed to Azure.

## What it does

- Todo list stored in PostgreSQL: add a task on the page, see the list
- REST API over the same data: list, read, create, update, delete
- Swagger UI for the API
- Metrics for the running app: an in-app dashboard and a Prometheus endpoint
- Database status indicator on the home page
- Unit tests and Playwright UI tests
- The whole stack starts with a single `docker compose up`

## Stack

| Layer | Technology |
|---|---|
| Web app | ASP.NET Core MVC, .NET 8 |
| Data | PostgreSQL 16, EF Core 8, Npgsql |
| API | ASP.NET Core controllers, Swashbuckle (OpenAPI) |
| Monitoring | prometheus-net with a small dashboard page |
| Tests | xUnit, Playwright .NET (NUnit runner) |
| Runtime | Docker, Docker Compose |

## Docker image

The container image is published on Docker Hub:

**https://hub.docker.com/repository/docker/blazejs/mysimplenetapp_ui/general**

## Quick start

```bash
cd MySimpleNetApp
cp .env.example .env
docker compose up -d --build
```

| What | Address |
|---|---|
| Web app | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| Metrics dashboard | http://localhost:8080/AppMetrics |
| Prometheus endpoint | http://localhost:8080/metrics |
| PostgreSQL | localhost:5432, database `todos`, user `app` |

Stop the stack with `docker compose down` (add `-v` to drop the database volume as well).

## API

| Method | Path | Result |
|---|---|---|
| GET | `/api/todos` | list of tasks |
| GET | `/api/todos/{id}` | single task or 404 |
| POST | `/api/todos` | 201 with the created task |
| PUT | `/api/todos/{id}` | 204 or 404 |
| DELETE | `/api/todos/{id}` | 204 or 404 |

