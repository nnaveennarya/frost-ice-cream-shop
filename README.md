# Frost

Frost manages inventory and customer records for an ice cream shop. It includes a Laravel API backed by MySQL and a C# WPF desktop client for Windows.

## Features

- Inventory and customer CRUD APIs, with inventory search.
- Owner, Manager, and Cashier permissions enforced by backend policies.
- Browser sign-in using Laravel Passport OAuth 2.0 Authorization Code with PKCE (S256).
- Access tokens expire after 15 minutes and are kept in desktop memory. Sign in again when a token expires; refresh tokens are disabled.
- Request validation, pagination, audit events, and version checks to prevent overwriting concurrent changes.

| Role | View items | Add items | Edit items | Delete items | View/add/edit customers | Delete customers |
| --- | --- | --- | --- | --- | --- | --- |
| Owner | Yes | Yes | Yes | Yes | Yes | Yes |
| Manager | Yes | Yes | Yes | No | Yes | No |
| Cashier | Yes | No | Yes | No | Yes | No |

## Demo package login accounts

Start `START-FROST.ps1` in the supplied Windows demonstration package, wait for the desktop app to open, choose Sign in, and enter one of these accounts in the browser:

| Role | Login email | Password |
| --- | --- | --- |
| Owner | `owner@icecream.test` | Provided separately with the demo package |
| Manager | `manager@icecream.test` | Provided separately with the demo package |
| Cashier | `cashier@icecream.test` | Provided separately with the demo package |

Request the private demo login details from the project owner. Copy the supplied password exactly, including its final `=`, without surrounding spaces or backticks. Use these accounts only with the local demonstration database.

A fresh database created from source uses the passwords you supply in `.local/user-credentials.json` during setup below. Cloning this repository does not install the private demonstration database or its account passwords.

## Source layout

- `backend/`: Laravel application, migrations, seeders, and configuration templates.
- `desktop-source/`: WPF application and its solution.
- `docs/passwords.example.json`: structure for supplying your own local demo account passwords.

The public source uses migrations and synthetic seed data. The private demo database, sessions, customer records, signing keys, portable runtimes, and compiled binaries are excluded.

## Requirements

- PHP 8.4 or compatible later version, with Composer 2. Enable Laravel's required extensions, including PDO MySQL, OpenSSL, mbstring, fileinfo, and an Argon2id-capable PHP build.
- MySQL 8.4 (the version used in the demonstration package).
- Windows and the .NET 10 SDK for the WPF client (`net10.0-windows`).
- Git to clone this repository.

The backend serves its browser login pages directly with Blade. Node/Vite is not required for the documented demo workflow.

## 1. Create a local database

In MySQL, run the following, replacing the password with a new local password:

```sql
CREATE DATABASE ice_cream_assessment CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'ice_app'@'127.0.0.1' IDENTIFIED BY 'REPLACE_WITH_YOUR_LOCAL_DB_PASSWORD';
GRANT ALL PRIVILEGES ON ice_cream_assessment.* TO 'ice_app'@'127.0.0.1';
```

These privileges allow migrations on this local assessment database. Use a separate restricted application user for a deployed server.

## 2. Install and configure the backend

Run these commands in PowerShell from the repository root:

```powershell
New-Item -ItemType Directory -Force .local | Out-Null
Copy-Item docs/passwords.example.json .local/user-credentials.json
Copy-Item backend/.env.example backend/.env
```

Edit `backend/.env`: set `DB_PASSWORD` to the database password selected above. Keep `APP_ENV=local` and `DB_DATABASE=ice_cream_assessment` for the setup command. The template uses MySQL on `127.0.0.1:3306` and the API on `http://127.0.0.1:8000`.

Edit `.local/user-credentials.json` and replace every placeholder with your own password of 16–1024 bytes. These are application account passwords, separate from the MySQL password. This file and `.env` are ignored by Git.

```powershell
Set-Location backend
composer install
php artisan key:generate
php artisan migrate
php artisan passport:keys
php artisan shop:setup --password-file=../.local/user-credentials.json
```

The setup command creates these accounts:

| Email | Role | Password |
| --- | --- | --- |
| owner@icecream.test | Owner | Your JSON `owner` value |
| manager@icecream.test | Manager | Your JSON `manager` value |
| cashier@icecream.test | Cashier | Your JSON `cashier` value |

It also seeds Chocolate, Vanilla, and Strawberry categories, sample inventory, and a synthetic customer. It prints separate public OAuth client IDs for WPF and Postman. Save those IDs for configuration; public clients do not use a client secret. Repeating this command preserves existing account passwords.

Start the backend and keep the terminal open:

```powershell
php artisan serve --host=127.0.0.1 --port=8000
```

The health endpoint is `http://127.0.0.1:8000/up`. Stop the server with Ctrl+C. MySQL must remain running while using the app.

## 3. Configure and run the WPF client

From a second PowerShell terminal at the repository root:

```powershell
Copy-Item desktop-source/src/IceCreamShop.Desktop/appsettings.json desktop-source/src/IceCreamShop.Desktop/appsettings.local.json
```

Edit `appsettings.local.json`, replacing `ClientId` with the **WPF public client ID** printed by `shop:setup`:

```json
{
  "ApiBaseUrl": "http://127.0.0.1:8000/",
  "ClientId": "YOUR_WPF_PUBLIC_CLIENT_ID",
  "RedirectUri": "http://127.0.0.1:43821/callback/"
}
```

Keep the redirect URI exactly as shown; it must match the client's registered redirect URI. Port 43821 must be available.

```powershell
dotnet restore desktop-source/IceCreamShop.sln
dotnet build desktop-source/IceCreamShop.sln --configuration Release --no-restore
dotnet run --project desktop-source/src/IceCreamShop.Desktop/IceCreamShop.Desktop.csproj --configuration Release
```

Sign in in the system browser using one of the accounts above. The browser returns an authorization code to the desktop's loopback callback, and the desktop exchanges that code for an access token using the PKCE verifier.

Close the desktop window and stop the backend terminal when finished. Database changes persist in MySQL.

## API notes

Protected routes require `Authorization: Bearer <access_token>` with `shop:access` scope. Send `Accept: application/json`, and `Content-Type: application/json` for JSON request bodies.

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/auth/me` | Current user and permissions |
| POST | `/api/auth/logout` | Revoke the current access token |
| GET | `/api/categories` | List categories |
| GET, POST | `/api/items` | List or create inventory |
| GET, PATCH, DELETE | `/api/items/{id}` | Read, update, or delete inventory |
| GET, POST | `/api/customers` | List or create customers |
| GET, PATCH, DELETE | `/api/customers/{id}` | Read, update, or delete a customer |

`PATCH` requests require the record's current `version` in the JSON body. `DELETE` requests require an `If-Match` header containing the quoted version, such as `"1"`, and do not accept body or query fields. Authorization still applies to every operation.

Authentication starts at `/oauth/authorize`, exchanges codes at `/oauth/token`, and uses a browser session and CSRF protection for login. The `/api/auth/login` route is a browser-session login route, not a password-for-bearer-token endpoint. For Postman, choose Authorization Code with PKCE, S256, the Postman public client ID, `shop:access` scope, and callback `https://oauth.pstmn.io/v1/browser-callback`. Use an unpredictable base64url state of 32–256 characters. Do not configure a client secret.

## Assessment status

This source repository was prepared from the supplied Windows demonstration package. That package did not include the backend tests or the desktop test/tool projects referenced by its original solution. The solution here includes only the supplied desktop application. Automated test coverage is an outstanding assessment requirement; no passing test results are claimed.

The [Postman collection and instructions](docs/postman/README.md) and [ER diagram and editable schema](docs/database/README.md) are included in `docs/`. The Google Drive demo video is a separate submission deliverable. A new source import does not reproduce the project's earlier development commit history.
