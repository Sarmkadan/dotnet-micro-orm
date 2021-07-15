# ExceptionTests

`ExceptionTests` is a unit test class that verifies the construction and behavior of exception types within the dotnet-micro-orm library. It ensures that custom exceptions are instantiated correctly with the expected parameters and that they maintain consistent behavior across different scenarios.

## API

### `public void OrmException_WithMessage_CreatesInstance()`
Tests that an `OrmException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void OrmException_WithMessageAndErrorCode_CreatesInstance()`
Tests that an `OrmException` can be created with a message string and an error code. The exception should store both values and allow retrieval of the error code.

### `public void OrmException_WithMessageInnerExceptionAndErrorCode_CreatesInstance()`
Tests that an `OrmException` can be created with a message string, an inner exception, and an error code. The exception should chain the inner exception and store the error code.

### `public void OrmException_WithContext_AddsContext()`
Tests that an `OrmException` can have contextual data added via the `AddContext` method. The context should be stored and retrievable.

### `public void DatabaseConnectionException_WithMessage_CreatesInstance()`
Tests that a `DatabaseConnectionException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void EntityMappingException_WithMessage_CreatesInstance()`
Tests that an `EntityMappingException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void EntityMappingException_WithMessageAndPropertyName_CreatesInstance()`
Tests that an `EntityMappingException` can be created with a message string and a property name. The exception should store both values and allow retrieval of the property name.

### `public void QueryExecutionException_WithMessage_CreatesInstance()`
Tests that a `QueryExecutionException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void QueryExecutionException_WithMessageAndQuery_CreatesInstance()`
Tests that a `QueryExecutionException` can be created with a message string and a query string. The exception should store both values and allow retrieval of the query.

### `public void EntityValidationException_WithMessage_CreatesInstance()`
Tests that an `EntityValidationException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void EntityValidationException_WithMessageAndErrors_CreatesInstance()`
Tests that an `EntityValidationException` can be created with a message string and a collection of validation errors. The exception should store both values and allow retrieval of the errors.

### `public void ConcurrencyException_WithMessage_CreatesInstance()`
Tests that a `ConcurrencyException` can be created with only a message string. The exception should be instantiated without errors and should contain the provided message.

### `public void ConcurrencyException_WithMessageAndEntityKey_CreatesInstance()`
Tests that a `ConcurrencyException` can be created with a message string and an entity key. The exception should store both values and allow retrieval of the entity key.

## Usage
