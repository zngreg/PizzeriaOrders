# Pizzeria Orders

Pizzeria Orders is a .NET project designed to manage and process orders for a pizzeria. It includes functionality for validating orders, calculating prices, aggregating ingredients, and more. The project is structured into multiple components to ensure modularity and maintainability.

## Project Structure

The project is organized into the following directories:

- **PizzeriaOrders.Cli/**: Contains the command-line interface for interacting with the application. Includes configuration files like `appsettings.json` and data files such as `ingredients.json`, `orders.csv`, `orders.json`, and `products.json`.
- **PizzeriaOrders.Models/**: Defines the data models used across the application, such as `Order`, `Product`, and `Ingredient`.
- **PizzeriaOrders.Services/**: Implements the core business logic, including file parsing, order validation, ingredient aggregation, and price calculation.
- **Tests/PizzeriaOrders.Services.Unit.Tests/**: Contains unit tests for the services to ensure correctness and reliability.

## Dependencies

The project uses the following dependencies:

- **.NET 9.0**: The application is built using .NET 9.0.
- **Newtonsoft.Json**: For JSON parsing and serialization.
- **MSTest**: For unit testing.

## Configuration Files

- `appsettings.json`: Contains application configuration settings.
- `ingredients.json`: Lists available ingredients and their details.
- `orders.csv`: A CSV file containing order data.
- `orders.json`: A JSON file containing order data.
- `products.json`: Defines the available products and their details.

## How to Run

1. **Clone the Repository**:

   ```bash
   git clone <repository-url>
   cd PizzeriaOrders
   ```

2. **Build the Solution**:
   Ensure you have the .NET 9.0 SDK installed, then run:

   ```bash
   dotnet build PizzeriaOrders.sln
   ```

3. **Run the Application**:
   Navigate to the `PizzeriaOrders.Cli` directory and execute:

   ```bash
   dotnet run --project PizzeriaOrders.Cli
   ```

4. **Run Unit Tests**:
   Navigate to the `Tests/PizzeriaOrders.Services.Unit.Tests` directory and execute:
   ```bash
   dotnet test
   ```

## Additional Notes

- Ensure that the required data files (`ingredients.json`, `orders.csv` and/or `orders.json`, `products.json`) are present in the `PizzeriaOrders.Cli` directory before running the application.
- The application uses a mock queue service for demonstration purposes, which can be extended or replaced with a real queue service if needed.

Feel free to contribute to the project by submitting issues or pull requests!
