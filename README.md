# Backend-Mobile

Microservices backend for a mobile reading and subscription platform.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat)
![Terraform](https://img.shields.io/badge/Terraform-IaC-844FBA?style=flat)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=flat)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=flat)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=flat)

## Architecture
- API Gateway as the single entry point
- Services: User, Book, Subscription, Payment, AI
- Clean Architecture per service (Domain/Application/Infrastructure/WebApi)
- RabbitMQ for events, Redis for cache, PostgreSQL per service

## Tech Stack
- .NET 9, ASP.NET Core
- PostgreSQL, Redis, RabbitMQ
- Docker Compose, Nginx
- Terraform (AWS)

## Quick Start
```bash
cd Backend
docker compose up -d --build
```

## Screenshots
<p align="center">
  <img src="./.github/workflows/image/home%20.png" width="23%" alt="Home" />
  <img src="./.github/workflows/image/search.png" width="23%" alt="Search" />
  <img src="./.github/workflows/image/book%20details.png" width="23%" alt="Book details" />
  <img src="./.github/workflows/image/read%20book.png" width="23%" alt="Read book" />
</p>
