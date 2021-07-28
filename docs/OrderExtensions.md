# OrderExtensions

Provides utility methods for working with `Order` entities in the dotnet-micro-orm project. These extensions simplify common operations such as calculating total weight, checking urgency, formatting display strings, and retrieving estimated delivery dates.

## API

### `public static decimal GetTotalWeight(Order order)`

Calculates the total weight of an order by summing the weights of all its items.

- **Parameters**
  - `order`: The `Order` instance whose total weight is to be calculated. Must not be `null`.
- **Return value**
  - The sum of weights of all items in the order, expressed in kilograms.
- **Exceptions**
  - Throws `ArgumentNullException` if `order` is `null`.

---

### `public static bool IsUrgent(Order order)`

Determines whether an order qualifies as urgent based on its creation date and total value.

- **Parameters**
  - `order`: The `Order` instance to evaluate. Must not be `null`.
- **Return value**
  - `true` if the order was created within the last 24 hours and its total value exceeds 1000; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `order` is `null`.

---

### `public static string ToDisplayString(Order order)`

Generates a human-readable string representation of an order for display purposes.

- **Parameters**
  - `order`: The `Order` instance to format. Must not be `null`.
- **Return value**
  - A string in the format `"Order #{Id} - {CustomerName} - Total: {Total:C}"`.
- **Exceptions**
  - Throws `ArgumentNullException` if `order` is `null`.

---

### `public static DateTime? GetEstimatedDeliveryDate(Order order)`

Computes the estimated delivery date for an order based on its shipping method and location.

- **Parameters**
  - `order`: The `Order` instance for which to calculate the delivery date. Must not be `null`.
- **Return value**
  - The estimated delivery date if calculable; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `order` is `null`.

## Usage
