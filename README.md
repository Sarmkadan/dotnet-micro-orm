// entire file content ...

// ... goes in between

## EndToEndWorkflowTests

The `EndToEndWorkflowTests` class provides comprehensive end-to-end tests for the DotnetMicroOrm library. It covers various scenarios, including product creation, inventory management, caching, and user management.

### Example Usage

```csharp
using DotnetMicroOrm.Tests;

// Create an instance of the test class
var tests = new EndToEndWorkflowTests();

// Test product creation and retrieval
await tests.CreateAndRetrieveProduct_FullWorkflow();

// Test inventory management
await tests.InventoryManagement_IncreaseAndDecreaseStock();

// Test profit calculation
await tests.ProfitCalculation_WithAndWithoutCost();

// Test batch product creation
await tests.BatchProductCreation_Simulation();

// Test caching strategy
await tests.CachingStrategy_GetOrSet();

// Test user management
await tests.UserManagement_CompleteLifecycle();

// Test specification pattern
await tests.SpecificationPattern_ProductFiltering();

// Test concurrency scenario
await tests.ConcurrencyScenario_MultipleOperations();

// Test validation error handling
await tests.ValidationErrorHandling_Comprehensive();

// Test cache invalidation
await tests.CacheInvalidation_PatternBased();

// Test order management
await tests.OrderManagement_Scenario();

// Test data export scenario
await tests.DataExportScenario_MultipleFormats();
```

// ... goes in between
