# Task Manager API

A simple, testable Web API for managing tasks and users. Built in .NET 8 with a focus on clarity, testability,
and background automation.

---

## 🚀 Technologies Used

- ASP.NET Core 8 Web API
- Entity Framework Core (InMemory) for zero-setup persistence
- Background services (IHostedService)
- Swagger (OpenAPI) for docs and testing
- xUnit for unit testing
- Moq for mocking services in tests

---

## ⚙️ Getting Started

### Prerequisites:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Running the API

- Run it: `dotnet run`
- Navigate to Swagger UI: `https://localhost:{port}/swagger`

---

## 🧪 How to Test

Tests are written using xUnit and Moq, targeting core service logic and task reassignment rules. To run all tests:

- `dotnet test`

### ✅ This command will:

- Build the main and test projects
- Run all unit tests
- Output results to the terminal

