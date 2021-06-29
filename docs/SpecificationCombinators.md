# SpecificationCombinators

Provides static combinators for building composite specifications from existing `Specification<T>` instances using logical `And`, `Or`, and `Not` operations. The class also exposes the `CombinedSpec` type that represents the result of such combinations and a `ParameterReplaceVisitor` used internally for expression tree rewriting.

## API

### `public static Specification<T> And<T>(Specification<T> left, Specification<T> right)`

Combines two specifications with a logical AND. The resulting specification is satisfied only when both `left` and `right` are satisfied.

- **Parameters**:
  - `left` — the first specification.
  - `right` — the second specification.
- **Return value**: a new `Specification<T>` representing the conjunction of the two inputs.
- **Throws**: `ArgumentNullException` if either argument is `null`.

### `public static Specification<T> Or<T>(Specification<T> left, Specification<T> right)`

Combines two specifications with a logical OR. The resulting specification is satisfied when at least one of `left` or `right` is satisfied.

- **Parameters**:
  - `left` — the first specification.
  - `right` — the second specification.
- **Return value**: a new `Specification<T>` representing the disjunction of the two inputs.
- **Throws**: `ArgumentNullException` if either argument is `null`.

### `public static Specification<T> Not<T>(Specification<T> specification)`

Negates a specification. The resulting specification is satisfied when the original specification is not satisfied.

- **Parameters**:
  - `specification` — the specification to negate.
- **Return value**: a new `Specification<T>` representing the logical negation of the input.
- **Throws**: `ArgumentNullException` if `specification` is `null`.

### `CombinedSpec`

The type that represents a specification produced by `And`, `Or`, or `Not`. It holds the constituent specifications and the logical operator used to combine them, enabling the expression tree to be traversed or rewritten.

### `ParameterReplaceVisitor`

An expression visitor that replaces parameter references in an expression tree. Used internally when merging expression trees from multiple specifications so that they share a single parameter instance, ensuring the combined predicate can be compiled and executed correctly.

## Usage

```csharp
// Build a composite specification for filtering active premium customers
var isActive = new ActiveCustomerSpec();
var isPremium = new PremiumCustomerSpec();
var isActivePremium = SpecificationCombinators.And(isActive, isPremium);

var customers = repository.Find(isActivePremium);
```

```csharp
// Exclude soft-deleted records while including either of two optional filters
var notDeleted = SpecificationCombinators.Not(new SoftDeletedSpec());
var matchesRegion = new RegionSpec("EMEA");
var matchesSegment = new SegmentSpec("Enterprise");
var combinedFilter = SpecificationCombinators.And(
    notDeleted,
    SpecificationCombinators.Or(matchesRegion, matchesSegment)
);

var results = repository.Find(combinedFilter);
```

## Notes

- All combinators return new `Specification<T>` instances; the original specifications are not mutated.
- The `And` and `Or` operations are not short-circuiting at the combinator level — both constituent expressions are always evaluated when the combined predicate executes.
- `Not` wraps the original expression in a unary negation; it does not apply De Morgan’s laws or perform any simplification.
- Deeply nested combinations can produce complex expression trees that may hit provider-specific translation limits when targeting SQL databases.
- The combinators are thread-safe: they operate on immutable inputs and produce a new immutable result without shared mutable state.
- `ParameterReplaceVisitor` is an implementation detail and is not intended for direct use by consumers.
