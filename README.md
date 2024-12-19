# Task Management API

A .NET Core Web API for task management featuring Google OAuth authentication, JWT authorization, and comprehensive task management capabilities.

## Setup Instructions

### 1. Prerequisites
- .NET 6.0 SDK or later
- A Google Cloud Platform account (for OAuth setup)

### 2. Google OAuth Configuration
1. Visit [Google Cloud Console](https://console.cloud.google.com)
2. Create a new project or select existing one
3. Navigate to "APIs & Services" → "Credentials"
4. Click "Create Credentials" → "OAuth Client ID"
5. Choose "Web Application"
6. Add authorized redirect URIs:
   - `https://localhost:7157/api/Auth/google-callback`
   - `https://localhost:7157/signin-google`
7. Save your Client ID and Client Secret

### 3. Application Configuration
1. Clone the repository
2. Create `appsettings.json` in the root directory:
```json
{
  "Jwt": {
    "Key": "your-secret-key-min-16-chars",
    "Issuer": "https://localhost:7157",
    "Audience": "https://localhost:7157"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

### 4. Running the Application
1. Open terminal in project directory
2. Run the following commands:
```bash
dotnet restore
dotnet run
```
3. Access Swagger UI at `https://localhost:7157/swagger`

## API Testing Guide

### Authentication 1st Flow 
1. Use the login endpoint:
```http
GET /api/Auth/google-login
* note that you will get a CORS error but this because Swagger is not handling the redirection
  but it will work just ignore the message and go for the next step which is using the second authentication end point " google-callback "
```
2. Complete Google authentication
3. Copy the JWT token from the response
4. Use the token in subsequent requests in the Authorization header:
```
At the top right " Authorize " button paste your token as follow "Bearer {your-token-here}"
```
### Authentication 2nd Flow

1.Nagivate to " https://localhost:7157/login " 
2.Press on Login with google and complete Google authentication
3.follow as 1st authentication flow

### Example API Requests

#### Create Task
```http
POST /api/Tasks
Authorization: Bearer your-token-here
Content-Type: application/json

{
  "title": "Complete Project",
  "description": "Finish the task management API",
  "dueDate": "2024-03-20T14:00:00"
}
```

Response:
```json
{
  "id": 1,
  "title": "Complete Project",
  "description": "Finish the task management API",
  "dueDate": "2024-03-20T14:00:00",
  "status": "Pending",
  "userId": "user123"
}
```

#### Get Tasks with Filtering
```http
GET /api/Tasks?status=pending&sortBy=dueDate&sortDescending=true
Authorization: Bearer your-token-here
```

Response:
```json
[
  {
    "id": 1,
    "title": "Complete Project",
    "description": "Finish the task management API",
    "dueDate": "2024-03-20T14:00:00",
    "status": "Pending",
    "userId": "user123"
  }
]
```

#### Update Task
```http
PUT /api/Tasks/1
Authorization: Bearer your-token-here
Content-Type: application/json

{
  "title": "Updated Title",
  "description": "Updated description",
  "dueDate": "2024-03-21T14:00:00"
}
```

#### Delete Task
```http
DELETE /api/Tasks/1
Authorization: Bearer your-token-here
```

## Query Parameters for Task Filtering

- `status`: Filter by task status (e.g., "Pending", "Completed")
- `sortBy`: Sort by field ("title", "dueDate", "status")
- `sortDescending`: true/false
- `searchTerm`: Search in title and description
- `dueDateFrom`: Filter tasks due after this date
- `dueDateTo`: Filter tasks due before this date
