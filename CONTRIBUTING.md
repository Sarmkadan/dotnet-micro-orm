// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Contributing to DotnetMicroOrm

Thank you for your interest in contributing! This document provides guidelines and instructions for participating in the project.

## Code of Conduct

- Be respectful and constructive
- Welcome diverse perspectives
- Focus on the code, not the person
- Help others learn and grow

## Getting Started

### Prerequisites

- .NET 10 SDK
- Git
- Visual Studio 2024, VS Code, or equivalent
- SQL Server (local for testing)

### Setup Development Environment

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-micro-orm.git
cd dotnet-micro-orm

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run tests
dotnet test
```

## Development Workflow

### 1. Fork and Clone

```bash
# Fork on GitHub, then clone your fork
git clone https://github.com/YOUR_USERNAME/dotnet-micro-orm.git
cd dotnet-micro-orm

# Add upstream remote
git remote add upstream https://github.com/Sarmkadan/dotnet-micro-orm.git
```

### 2. Create Feature Branch

```bash
# Update from upstream
git fetch upstream
git checkout -b feature/my-feature upstream/main

# Or from develop for work-in-progress features
git checkout -b feature/my-feature upstream/develop
```

### 3. Make Changes

- Keep commits focused and atomic
- Write clear commit messages
- Follow code standards (see below)

### 4. Test Your Changes

```bash
# Run full test suite
dotnet test

# Run specific test file
dotnet test tests/MyTests.cs

# With coverage
dotnet test /p:CollectCoverage=true
```

### 5. Submit Pull Request

- Push to your fork
- Open PR against `main` branch
- Reference any related issues
- Provide clear description of changes

## Code Standards

### File Headers

Every .cs file MUST start with:

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
```

### Naming Conventions

```csharp
// Classes and public members - PascalCase
public class ProductRepository { }
public void GetProductsAsync() { }

// Private members - camelCase
private string _productName;
private async Task FetchDataAsync() { }

// Constants - UPPER_SNAKE_CASE
private const string DEFAULT_TIMEOUT = "30";
```

### Code Style

Use `dotnet format` to automatically format:

```bash
dotnet format
```

EditorConfig settings are in `.editorconfig`.

**Key rules:**
- 4-space indentation
- Braces on new line
- 80-character line width for comments
- No trailing whitespace

### Documentation

```csharp
/// <summary>
/// Gets a product by ID with caching support.
/// </summary>
/// <param name="id">The product ID</param>
/// <returns>Product if found, null otherwise</returns>
/// <remarks>
/// Results are cached for 5 minutes to improve performance.
/// </remarks>
public async Task<Product> GetProductByIdAsync(int id)
{
    // Implementation
}
```

**Guidelines:**
- Document public methods and properties
- Include `<summary>`, `<param>`, `<returns>` tags
- Add `<remarks>` for important notes
- Keep comments up to date with code

### Testing

```csharp
// AAA Pattern: Arrange, Act, Assert
[TestMethod]
public async Task GetProductById_WithValidId_ReturnsProduct()
{
    // Arrange
    var productId = 1;
    var expected = new Product { Id = 1, Name = "Test" };

    // Act
    var result = await repository.GetByIdAsync(productId);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(expected.Name, result.Name);
}

// Naming: MethodName_Scenario_ExpectedResult
```

**Requirements:**
- Write tests for new features
- Test happy path and edge cases
- Maintain >80% code coverage
- Name tests descriptively

## Commit Messages

Follow conventional commits format:

```
type(scope): subject

body

footer
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style (formatting, missing semicolons)
- `refactor`: Code reorganization without behavior change
- `perf`: Performance improvement
- `test`: Test addition or modification
- `chore`: Build, dependencies, release tasks

**Examples:**

```
feat(repository): add compiled expression caching

Implement automatic LINQ expression compilation and caching
for 5-10x performance improvement on repeated queries.

Closes #123
```

```
fix(cache): prevent stale data after concurrent updates

Fixed race condition where cache wasn't invalidated when
multiple threads updated same entity simultaneously.
```

## Pull Request Process

### Before Submitting

1. **Update from upstream:**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Pass all tests:**
   ```bash
   dotnet test
   dotnet format --verify-no-changes
   ```

3. **Update documentation:**
   - Update README if needed
   - Add entry to CHANGELOG.md
   - Update API docs if applicable

### PR Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## How to Test
Steps to verify the changes

## Testing Performed
- [ ] Unit tests added/updated
- [ ] Manual testing done
- [ ] Existing tests pass

## Checklist
- [ ] Code follows style guidelines
- [ ] Comments added for complex logic
- [ ] Documentation updated
- [ ] No new warnings generated
- [ ] Tests added/updated
```

## Issue Reporting

### Bug Reports

Include:
1. Reproduction steps
2. Expected behavior
3. Actual behavior
4. Error messages
5. Environment (OS, .NET version, database)

### Feature Requests

Include:
1. Use case and motivation
2. Proposed solution
3. Alternative approaches
4. Additional context

## Areas for Contribution

### High Priority
- Performance optimizations
- Bug fixes
- Test coverage
- Documentation

### Medium Priority
- Feature enhancements
- Database dialect improvements
- Caching strategies

### Lower Priority (Discuss First)
- Major architectural changes
- Breaking changes
- New dependencies

## Review Process

1. **Automated Checks:**
   - Build succeeds
   - All tests pass
   - Code coverage maintained
   - Code style compliant

2. **Code Review:**
   - Functionality correctness
   - Performance impact
   - Code quality
   - Test adequacy
   - Documentation completeness

3. **Approval:**
   - At least one approval required
   - All discussions resolved
   - Ready to merge

## Merge and Release

### Merge Process
- Squash commits for cleaner history
- Delete branch after merge
- Update related issues

### Release Workflow
1. Update CHANGELOG.md
2. Update version in .csproj
3. Create git tag (vX.Y.Z)
4. Push tag to trigger package release
5. Publish release notes on GitHub

### Version Strategy
Uses Semantic Versioning:
- MAJOR.MINOR.PATCH
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

## Recognition

Contributors are recognized in:
- CHANGELOG.md (for significant contributions)
- GitHub contributors page (automatic)
- Project documentation

## Questions?

- Check existing issues and discussions
- Review documentation in `docs/`
- Review existing examples in `examples/`
- Ask in GitHub Discussions

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to DotnetMicroOrm!

Built by [Vladyslav Zaiets](https://sarmkadan.com)
