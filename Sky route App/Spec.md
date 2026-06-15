# SkyRoute API – Specification

## 1. Overview

SkyRoute is an ASP.NET Core Web API for searching flights and creating/retrieving bookings. It uses Entity Framework Core with an in-memory database, seeded at startup with sample airports and flights.

**Namespace root:** `sky_route_app`
**Database:** `SkyRouteDbContext` (EF Core, In-Memory provider, database name `SkyRouteDb`)
**CORS:** Any origin, method, and header allowed.
**API docs:** OpenAPI/Swagger enabled in Development environment (`/swagger`).

---

## 2. Architecture

```
Controllers/
  FlightsController   -> IFlightService  -> FlightService
  BookingsController  -> IBookingService -> BookingService

Services/
  FlightService  (IFlightService)
  BookingService (IBookingService)

Data/
  SkyRouteDbContext (DbSet<Airport>, DbSet<Flight>, DbSet<Booking>)

Models/
  Airport, Flight, Booking, CustomerBookingData

DTOs/
  FlightSearchRequest, BookingRequest, CustomerDetails
```

Dependency injection (Program.cs):
- `SkyRouteDbContext` — scoped, in-memory DB
- `IFlightService` → `FlightService` — scoped
- `IBookingService` → `BookingService` — scoped

---

## 3. Data Models

### 3.1 Airport
| Field | Type | Constraints |
|---|---|---|
| Id | int | Primary key |
| Code | string | Required, 1–10 chars |
| City | string | Required, 1–100 chars |
| Country | string | Required, 1–100 chars |

### 3.2 Flight
| Field | Type | Constraints |
|---|---|---|
| Id | int | Primary key |
| Provider | string | Required, 2–100 chars |
| FlightNumber | string | Required, 2–20 chars, pattern `^[A-Z0-9]{2,20}$` |
| OriginAirportId | int | Required, ≥ 1 |
| DestinationAirportId | int | Required, ≥ 1 |
| DepartureTime | DateTime | Required |
| ArrivalTime | DateTime | Required |
| CabinClass | string | Required, 2–50 chars |
| BaseFare | decimal | Required, > 0 |

### 3.3 Booking
| Field | Type | Constraints |
|---|---|---|
| Id | int | Primary key |
| FlightId | int | Required, ≥ 1 |
| ReferenceCode | string | ≤ 20 chars (auto-generated, 8-char uppercase GUID segment) |
| BookingDate | DateOnly | Required (set to current date) |
| AirLine | string | Required, 2–100 chars |
| Customers | List\<CustomerBookingData\> | Required |

### 3.4 CustomerBookingData
| Field | Type | Constraints |
|---|---|---|
| Id | int | Primary key |
| FullName | string | Required, 2–100 chars |
| Email | string | Required, valid email, ≤ 255 chars |
| PhoneNumber | string | Required, valid phone, ≤ 20 chars |
| DocumentNumber | string | Required, 5–50 chars |

---

## 4. DTOs

### 4.1 FlightSearchRequest
| Field | Type | Constraints |
|---|---|---|
| OriginAirportId | int | Required, ≥ 1 |
| DestinationAirportId | int | Required, ≥ 1 |
| DepartureDate | DateTime | Required, date only |
| Passengers | int | Required, 1–10 |
| CabinClass | string | Required |

### 4.2 BookingRequest
| Field | Type | Constraints |
|---|---|---|
| FlightId | int | Required, ≥ 1 |
| AirLine | string | Required, 5–20 chars; must be `"GlobalAir"` or `"BudgetWings"` |
| customerDetails | List\<CustomerDetails\> | Required |

### 4.3 CustomerDetails
| Field | Type | Constraints |
|---|---|---|
| FullName | string | 2–100 chars |
| Email | string | Required, valid email |
| DocumentNumber | string | Required, 5–50 chars |
| PhoneNumber | string | Required, valid phone, 5–20 chars |

---

## 5. API Endpoints

### 5.1 `GET /api/Flights/airports`
Returns the list of all airports.

**Response 200:**
```json
[
  { "id": 1, "code": "JFK", "city": "New York", "country": "USA" },
  ...
]
```

---

### 5.2 `POST /api/Flights/search`
Search for available flights matching origin, destination, date, cabin class, and passenger count.

**Request body:** `FlightSearchRequest`
```json
{
  "originAirportId": 1,
  "destinationAirportId": 3,
  "departureDate": "2026-06-20",
  "passengers": 2,
  "cabinClass": "Economy"
}
```

**Matching logic:** flights where `OriginAirportId`, `DestinationAirportId`, `DepartureTime.Date`, and `CabinClass` exactly match the request.

**Pricing logic:**
1. Per-passenger base price by provider:
   - `GlobalAir`: `BaseFare * 1.15` (rounded to 2 decimals)
   - `BudgetWings`: `max(BaseFare * 0.9, 29.99)` (rounded to 2 decimals)
   - Other providers: `BaseFare` unchanged
2. Add cabin-class surcharge:
   - `Economy`: +100
   - `Business`: +200
   - `First`: +300
   - Other: +0
3. `TotalPrice = PricePerPassenger * Passengers`

**Response 200:**
```json
[
  {
    "id": 1,
    "provider": "GlobalAir",
    "flightNumber": "GA100",
    "departureTime": "2026-06-20T08:00:00",
    "arrivalTime": "2026-06-20T16:00:00",
    "duration": "08:00:00",
    "cabinClass": "Economy",
    "pricePerPassenger": 445.00,
    "totalPrice": 890.00
  }
]
```

---

### 5.3 `POST /api/Bookings`
Create a new booking for a flight.

**Request body:** `BookingRequest`
```json
{
  "flightId": 1,
  "airLine": "GlobalAir",
  "customerDetails": [
    {
      "fullName": "John Doe",
      "email": "john@example.com",
      "documentNumber": "P1234567",
      "phoneNumber": "+15551234567"
    }
  ]
}
```

**Validation:**
- `AirLine` must be `"GlobalAir"` or `"BudgetWings"`; otherwise **400 Bad Request**:
  ```json
  { "error": "Invalid AirLine. Must be 'GlobalAir' or 'BudgetWings'." }
  ```
- Model validation per `BookingRequest`/`CustomerDetails` annotations.
- If the referenced `FlightId` does not exist, the service throws `InvalidOperationException` (currently unhandled at the controller level — results in a 500 response).

**Processing:**
- Generates a `ReferenceCode` (first 8 characters of a new GUID, uppercase).
- Sets `BookingDate` to the current date.
- Persists `Booking` with associated `CustomerBookingData` records.

**Response 201 Created:**
```json
{
  "id": 1,
  "referenceCode": "A1B2C3D4"
}
```
Location header points to `GET /api/Bookings?id={id}`.

---

### 5.4 `GET /api/Bookings?id={id}`
Retrieve full booking details, merged with flight and airport information.

**Path/Query parameter:** `id` (int, must be > 0)

**Processing:**
- Loads `Booking` (with `Customers`) by id; throws `InvalidOperationException` if not found (results in 500 — not currently mapped to 404).
- Loads associated `Flight` by `FlightId`.
- Loads `DepartureAirport` (Flight.OriginAirportId) and `ArrivalAirport` (Flight.DestinationAirportId).

**Response 200:**
```json
{
  "id": 1,
  "bookingDate": "2026-06-15",
  "airLine": "GlobalAir",
  "customers": [
    {
      "fullName": "John Doe",
      "email": "john@example.com",
      "phoneNumber": "+15551234567",
      "documentNumber": "P1234567"
    }
  ],
  "flight": {
    "id": 1,
    "flightNumber": "GA100",
    "departureAirport": { "id": 1, "code": "JFK", "city": "New York", "country": "USA" },
    "arrivalAirport": { "id": 3, "code": "LHR", "city": "London", "country": "UK" },
    "departureTime": "2026-06-16T08:00:00",
    "arrivalTime": "2026-06-16T16:00:00",
    "cabinClass": "Economy"
  }
}
```

---

## 6. Seed Data

### Airports
| Id | Code | City | Country |
|---|---|---|---|
| 1 | JFK | New York | USA |
| 2 | LAX | Los Angeles | USA |
| 3 | LHR | London | UK |
| 4 | CDG | Paris | France |
| 5 | FRA | Frankfurt | Germany |
| 6 | DXB | Dubai | UAE |

### Flights
| Id | Provider | FlightNumber | Origin | Destination | Departure | Arrival | CabinClass | BaseFare |
|---|---|---|---|---|---|---|---|---|
| 1 | GlobalAir | GA100 | 1 (JFK) | 3 (LHR) | Today+1, 08:00 | Today+1, 16:00 | Economy | 300 |
| 2 | BudgetWings | BW200 | 2 (LAX) | 4 (CDG) | Today+2, 09:00 | Today+2, 17:00 | Business | 400 |

