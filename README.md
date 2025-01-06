# Inventory Management API

## Table of Contents
- [Introduction](#introduction)
- [Installation Instructions](#installation-instructions)
- [API Documentation](#api-documentation)
- [Technical Decisions](#technical-decisions)
- [Architecture Diagram](#architecture-diagram)

## Introduction
The Inventory Management API is a RESTful service designed to manage inventory, facilitate stock transfers, and handle low-stock alerts for a multi-store system. This project is built with .NET Core, PostgreSQL, and Serilog for logging.

---

## Installation Instructions

### Prerequisites
1. **.NET 6 SDK** installed.
2. **Docker** and **Docker Compose** installed.
3. **PostgreSQL** installed locally or accessible remotely.

### Steps

#### 1. Clone the Repository
```bash
git clone https://github.com/jfcrobles/Inventory-Management.git
cd inventory-management-api
```

#### 2. Configure Environment Variables
Modify the `appsettings.json` file or use environment variables to set the database connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=inventory_management;Username=postgres;Password=password"
  },
  "AllowedHosts": "*"
}
```

#### 3. Run Locally (Without Docker)
1. Apply migrations to the database:
   ```bash
   dotnet ef database update
   ```
2. Start the application:
   ```bash
   dotnet run
   ```
3. Access the API at [http://localhost:5000](http://localhost:5000).

#### 4. Run with Docker
1. Build and start the containers:
   ```bash
   docker-compose up --build
   ```
2. Access the API at [http://localhost:8080](http://localhost:8080).

---

## API Documentation

### Base URL
- Local: `http://localhost:8080/api`

### Endpoints

#### **1. Get Inventory by Store**
- **Endpoint**: `GET /api/inventory?storeId={storeId}`
- **Description**: Retrieves the inventory for a specific store.
- **Parameters**:
  - `storeId` (query): ID of the store.
- **Response**:
  - `200 OK`: Returns the inventory.
  - `404 Not Found`: No inventory found.

#### **2. Update Minimum Stock**
- **Endpoint**: `PUT /api/inventory/{productId}/minstock`
- **Description**: Updates the minimum stock level for a product in a store.
- **Parameters**:
  - `storeId` (query): ID of the store.
  - `productId` (path): ID of the product.
- **Request Body**:  
    10
  
  ```
- **Response**:
  - `200 OK`: Minimum stock updated.
  - `400 Bad Request`: Invalid request.

#### **3. Transfer Stock**
- **Endpoint**: `POST /api/inventory/transfer`
- **Description**: Transfers stock from one store to another.
- **Request Body**:
  ```json
  {
    "sourceStoreId": 1,
    "destinationStoreId": 2,
    "productId": 123,
    "quantity": 50
  }
  ```
- **Response**:
  - `200 OK`: Stock transfer successful.
  - `400 Bad Request`: Insufficient stock.

#### **4. Get Low Stock Alerts**
- **Endpoint**: `GET /api/inventory/alerts`
- **Description**: Retrieves products with stock below the minimum threshold.
- **Response**:
  - `200 OK`: Returns low-stock items.
  - `200 OK (Empty Array)`: No products with low stock.

---

## Technical Decisions

### 1. **Framework**: .NET Core 6
- Selected for its cross-platform capabilities, performance, and large ecosystem.

### 2. **Database**: PostgreSQL
- Chosen for its robustness, support for JSON fields, and scalability for handling large datasets.

### 3. **Logging**: Serilog
- Provides structured logging for better analysis and monitoring.

### 4. **Swagger**: API Documentation
- Integrated to provide a clear and interactive way to explore API endpoints during development.

### 5. **Deployment**: Docker
- Dockerized for consistent deployment across environments.
- Configured using Docker Compose for multi-container orchestration.

---

## Architecture Diagram
The architecture of the Inventory Management API is modular and designed for scalability.

```text
+-------------------------+          +---------------------------+
|   Client Applications   |          |      Monitoring Tools     |
| (Postman, Frontend App) |          | (e.g., Serilog + ELK)     |
+-----------+-------------+          +-------------+-------------+
            |                                      |
            |                                      |
            v                                      v
+-----------+-------------+          +---------------------------+
|       API Gateway        |---------> Inventory Management API |
| (e.g., Nginx, Traefik)   |          | (Dockerized ASP.NET Core)|
+-----------+-------------+          +-------------+-------------+
            |
            v
+---------------------------+
|   Database (PostgreSQL)   |
| (Hosted on Cloud or Local)|
+---------------------------+
```


