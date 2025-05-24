# Authentication API

A secure .NET Core 8 API for user authentication, token generation, and verification.

## Features

---

## 1. User Registration

- **Endpoint**: `POST /api/account/register`

- **Request**:
  ```json
  {
    "firstName": "First Name",
    "lastName": "Last Name",
    "emailAddress": "user@example.com",
    "password": "SecurePassword123!"
  }
  ```

- **Response**:
  - **Success**: `201 Created` with user details
  - **Failure**: `400 Bad Request` with error details

- **Behavior**:
  - Validates email format and password complexity
  - Sends a confirmation email with an OTP code

---

## 2. Email Confirmation

- **Endpoint**: `POST /api/account/confirm-email`

- **Request**:
  ```json
  {
    "email": "user@example.com",
    "token": "otp-in-email"
  }
  ```

- **Response**:
  - **Success**: `200 OK` with confirmation status
  - **Failure**: `400 Bad Request` with error message

---

## 3. User Login

- **Endpoint**: `POST /api/account/login`

- **Request**:
  ```json
  {
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }
  ```

- **Response**:
  - **Success**: `200 OK`
    ```json
    {
      "success": true,
      "message": "Login Successful",
      "data": {
        "userId": "string",
        "emailAddress": "user@example.com",
        "userName": "user_name",
        "token": "JWT"
      }
    }
    ```
  - **Failure**: `400 Bad Request` with error message

---

## 4. Token Generation

- **Endpoint**: `POST /api/account/generate` _(Requires authentication)_

- **Request**:
  ```json
  {
    "lifetimeInHours": 24
  }
  ```

- **Response**:
  - **Success**: `200 OK`
    ```json
    {
      "token": "6 characters",
      "expiresAt": "Date Time",
      "message": "Token lifetime was capped to maximum 3 days or parameter value is greater than 3"
    }
    ```

- **Behavior**:
  - Generates a 6-digit alphanumeric token
  - Caps the lifetime at 3 days (72 hours) if the requested duration exceeds this limit
  - Minimum lifetime: 1 hour

---

## 5. Token Verification

- **Endpoint**: `POST /api/account/validate` _(Requires authentication)_

- **Request**:
  ```json
  {
    "token": "A1B2C3"
  }
  ```

- **Response**:
  - **Valid**: `200 OK`
    ```json
    {
      "isValid": true,
      "message": "Token is valid"
    }
    ```
  - **Invalid**: `400 Bad Request`
    ```json
    {
      "isValid": false,
      "message": "Token has expired"
    }
    ```

---

## Configuration (`appsettings.json`)

```json
{
  "TokenSettings": {
    "Key": "The Key",
    "Issuer": "Issuer string",
    "JwtIssuer": "JwtIssuer string",
    "ExpiresInDays": 7,
    "AllowedChars": "AllowedChars",
    "TokenLength": 6,
    "MaxTokenLifetimeDays": 3
  },
  "AppMailSettings": {
    "BaseUrl": "url",
    "ContentType": "application/json",
    "From": "email",
    "SendMailToken": "Api Key"
  },
  "AllowedHosts": "*"
}
```
