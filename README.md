# Auto Service Booking System

A full-stack portfolio project for booking vehicle maintenance appointments. Customers manage vehicles and request service times; administrators manage services, users, and booking status.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 + SQLite
- JWT authentication + BCrypt password hashing
- FluentValidation
- React 19 + Vite
- xUnit integration tests

## Features

### Customer

- Register, sign in, and view profile
- Add, edit, and delete vehicles
- Browse active services
- Request an appointment Monday-Saturday, 09:00-18:00 (Tbilisi time)
- View booking history and cancel pending bookings
- Overlapping active bookings are rejected

### Admin

- View and process every booking
- Confirm, reject, complete, or cancel bookings using valid status transitions
- Add and activate/deactivate services
- Block or unblock customers

## Run locally

The API and frontend run in separate terminals.

### API

```bash
cd AutoServiceBookingSystem
dotnet restore
dotnet run
```

API: `http://localhost:5033`  
OpenAPI document: `http://localhost:5033/openapi/v1.json`

The SQLite database and initial migration are applied automatically. Seed data includes three services and an administrator:

```text
Username: admin
Password: Admin123!
```

The JWT key and seed password in `appsettings.json` are development defaults. Replace them with environment variables or user secrets outside local development.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

UI: `http://localhost:5173`

Vite proxies `/api` requests to `http://localhost:5033`.

## Tests

```bash
dotnet test
```

Integration tests cover JWT registration/profile access, booking overlap prevention, booking status transitions, and blocked-user restrictions.

## Main API routes

### Authentication

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

### Customer

- `GET|POST /api/vehicles`
- `PUT|DELETE /api/vehicles/{id}`
- `GET /api/services`
- `GET|POST /api/bookings`
- `GET /api/bookings/{id}`
- `PUT /api/bookings/{id}/cancel`

### Admin

- `GET /api/services/admin`
- `POST /api/services`
- `PUT|DELETE /api/services/{id}`
- `GET /api/bookings/admin`
- `PUT /api/bookings/admin/{id}/status`
- `GET /api/admin/users`
- `PUT /api/admin/users/{id}/block`
# AutoServiceBookingSystem
