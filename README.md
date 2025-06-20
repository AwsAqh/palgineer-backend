# Palgineer Backend Service - local host

This repository contains the **Palgineer** backend service, built with ASP.NET Core 8 Web API. It exposes RESTful endpoints for engineer registration, authentication (JWT), and CRUD operations backed by MongoDB.
This is the backend used in palgineer-connect repo project (front end).
## Table of Contents

* [Features](#features)
* [Prerequisites](#prerequisites)
* [Getting Started](#getting-started)

  * [Clone the Repository](#clone-the-repository)
  * [Configuration](#configuration)
  * [Add User Secrets (Optional)](#add-user-secrets-optional)
  * [Build and Run](#build-and-run)
* [API Endpoints](#api-endpoints)

  * [Authentication](#authentication)
  * [Engineer Management](#engineer-management)
* [File Uploads](#file-uploads)
* [CORS Policy](#cors-policy)
* [Error Handling](#error-handling)
* [Contributing](#contributing)
* [License](#license)

## Features

* **User Registration & Login** with JWT-based authentication
* **Secure** endpoints protected by `[Authorize]`
* **Engineer CRUD** operations (Create, Read, Update, Delete)
* **File uploads** for engineer avatars and resumes
* **MongoDB** as the data store via `EngineerService`
* **CORS** configured for a React frontend (default: `http://localhost:5174`)

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download)
* [MongoDB](https://www.mongodb.com/try/download/community) (local or remote)
* (Optional) [.NET User Secrets](https://docs.microsoft.com/aspnet/core/security/app-secrets)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/awsaqh/palgineer-backend.git
cd palgineer
```

### Configuration

1. Copy the sample configuration and fill in your own values:

   ```bash
   cp appsettings.json.example appsettings.json
   ```
2. Open `appsettings.json` and set:

   * `MongoDBSettings:ConnectionString` to your MongoDB URI
   * `MongoDBSettings:DatabaseName` to your database
   * `MongoDBSettings:EngineerCollectionName` to the collection name
   * `Jwt:Key` to a strong secret key (keep this safe!)
   * `Jwt:Issuer` and `Jwt:Audience`
   * `Jwt:DurationInDays` for token lifetime

### Add User Secrets (Optional)

Instead of storing secrets in `appsettings.json`, you can use the User Secrets feature:

```bash
cd my-app-backend
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "YOUR_VERY_SECRET_KEY"
# repeat for other sensitive values
```

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run the API
dotnet run --configuration Release
```

By default, the API will listen on `https://localhost:7050` (see `launchSettings.json`).

## API Endpoints

### Authentication

| Method | Endpoint             | Description             | Body                                                                                                                           |
| ------ | -------------------- | ----------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| POST   | `/api/auth/register` | Register a new engineer | `multipart/form-data` with fields: `name`, `email`, `password`, `summary`, `skills`, `links`, `avatar` (file), `resume` (file) |
| POST   | `/api/auth/login`    | Login and retrieve JWT  | JSON: `{ "email": "...", "password": "..." }`                                                                                  |

**Success Response** (both):

```json
{
  "token": "<JWT>",
  "expires": "2025-06-25T12:34:56Z",
  "engineer": { /* engineer object */ }
}
```

### Engineer Management

All endpoints below require the header:

```
Authorization: Bearer <JWT>
```

| Method | Endpoint              | Description                         |
| ------ | --------------------- | ----------------------------------- |
| GET    | `/api/crud`      | Get all engineers                   |
| GET    | `/api/crud/{id}` | Get engineer by ID                  |
| PUT    | `/api/crud/{id}` | Update engineer profile (form-data) |
| DELETE | `/api/crud/{id}` | Delete engineer                     |

* **Update** (`PUT`) uses `multipart/form-data` to optionally replace avatar and resume files. Use `[Consumes("multipart/form-data")]` on the endpoint.

## File Uploads

* All uploaded avatars and resumes are saved via `FileServices.saveFileAsync` into the `/uploads` directory at the project root.
* If the `/uploads` directory does not exist, the service will create it automatically before saving files.
* The stored file names (relative paths under `/uploads`) are returned in the engineer object when retrieving or updating profiles.

## CORS Policy Policy

* Configured to allow origins:

  * `Your react server url` (React dev server)
* Modify in `ServiceConfiguration` (or `DIServices`) if your front-end runs elsewhere.

## Error Handling

* **400 Bad Request**: validation errors or missing required fields
* **401 Unauthorized**: missing/invalid JWT
* **403 Forbidden**: attempting to modify another user's resource
* **404 Not Found**: engineer or endpoint not found

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/name`)
3. Commit your changes (`git commit -m 'Add feature'`)
4. Push to branch (`git push origin feature/name`)
5. Open a Pull Request

Please write tests for new functionality and adhere to the existing code style.

## License

This project is licensed under the [MIT License](LICENSE).
