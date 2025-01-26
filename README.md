# CurrencyConverter


## Overview

The **Currency Converter API** is a RESTful service built using C# to interact with the Frankfurter API. It provides three main functionalities:

1. Retrieve the latest exchange rates for a specific base currency.
2. Convert amounts between different currencies (excluding TRY, PLN, THB, and MXN).
3. Fetch historical exchange rates for a given period with pagination support.
4. Added Swagger UI for better API exploration and client generation.

This application is designed to handle high traffic efficiently and ensures fault tolerance with retry and bulkhead policies.

---

## Prerequisites

Before running the application, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or any preferred IDE
- [Postman](https://www.postman.com/) or another API testing tool (optional)

---

## How to Run the Application

### 1. Clone the Repository

```bash
git clone https://github.com/nomansafdar67/CurrencyConverter-Latest.git
cd CurrencyConverter-Latest


### 2. Build the Application
Run the following command to restore dependencies and build the project: dotnet build

###  Run the Application
Start the application using: dotnet run



Assumptions:

Unavailable Currencies: The currencies TRY, PLN, THB, and MXN are excluded from conversions and return a 400 Bad Request response.
Frankfurter API Downtime: The application handles Frankfurter API failures with a retry mechanism (up to 3 attempts).
Pagination: Historical rates are paginated based on the page and pageSize parameters.
Base Currency Validation: The base currency is validated to ensure compliance with the Frankfurter API.


Unit Testing
The application includes unit tests for all major functionalities. To run the tests: dotnet test



License
This project is licensed under the MIT License.