# API Contract Documentation

This folder contains the version-controlled API contract notes for the Event Parking Reservation System.

Feature-specific API documentation will be added by the owning work package.

Planned contract areas:

- Authentication and customer management
- Venue and category management
- Event management and discovery
- Seat and parking layouts
- Booking lifecycle
- Payment simulation
- Notifications
- Dashboards

## API Conventions

- Base path: `/api`
- HTTPS development URL: `https://localhost:7239`
- Swagger development URL: `https://localhost:7239/swagger`
- Health endpoint: `GET /api/health`
- Standard API failures use `ProblemDetails`
- Validation failures use appropriate 4xx responses
- Business-state conflicts use `409 Conflict`
- Authentication and authorization are enforced by the API

Detailed request and response contracts will be added as each feature is implemented.