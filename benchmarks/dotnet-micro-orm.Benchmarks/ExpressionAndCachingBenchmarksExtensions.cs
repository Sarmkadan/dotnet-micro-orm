using System;
using System.Linq.Expressions;

namespace DotnetMicroOrm.Benchmarks
{
    /// <summary>
    /// Provides extension methods for benchmarking expression tree operations and caching mechanisms.
    /// </summary>
    public static class ExpressionAndCachingBenchmarksExtensions
    {
        /// <summary>
        /// Creates a new expression that represents a lambda expression
        /// with the given parameters and body.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="body">The body of the expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="paramName"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="body"/> is null.</exception>
        /// <returns>A new expression that represents a lambda expression.</returns>
        public static Expression<Func<T>> CreateExpression<T>(
            this ExpressionAndCachingBenchmarks benchmarks,
            string paramName,
            Expression body)
        {
            ArgumentNullException.ThrowIfNull(paramName);
            ArgumentNullException.ThrowIfNull(body);

            var param = Expression.Parameter(typeof(T), paramName);
            return Expression.Lambda<Func<T>>(body, param);
        }

        /// <summary>
        /// Creates a new expression that represents a complex lambda expression
        /// with the given parameters and body.
        /// </summary>
        /// <typeparam name="T">The type of the parameter.</typeparam>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <param name="body">The body of the expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="paramName"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="body"/> is null.</exception>
        /// <returns>A new expression that represents a complex lambda expression.</returns>
        public static Expression<Func<T, bool>> CreateComplexExpression<T>(
            this ExpressionAndCachingBenchmarks benchmarks,
            string paramName,
            Expression body)
        {
            ArgumentNullException.ThrowIfNull(paramName);
            ArgumentNullException.ThrowIfNull(body);

            var param = Expression.Parameter(typeof(T), paramName);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        /// <summary>
        /// Clones the given expression by creating a deep copy.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="expression">The expression to clone.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
        /// <returns>A cloned expression.</returns>
        public static Expression CloneExpression(
            this ExpressionAndCachingBenchmarks benchmarks,
            Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            // Use ExpressionVisitor to create a deep copy of the expression tree
            return ExpressionCloneVisitor.Clone(expression);
        }

        /// <summary>
        /// Gets the body of the given expression.
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="expression">The expression.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The expression is not a lambda expression.</exception>
        /// <returns>The body of the expression.</returns>
        public static Expression GetBody(
            this ExpressionAndCachingBenchmarks benchmarks,
            Expression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            if (expression is LambdaExpression lambdaExpression)
            {
                return lambdaExpression.Body;
            }

            throw new InvalidOperationException("The expression is not a lambda expression.");
        }

        /// <summary>
        /// Visitor for cloning expression trees.
        /// </summary>
        private sealed class ExpressionCloneVisitor : ExpressionVisitor
        {
            public static Expression Clone(Expression expression)
            {
                var visitor = new ExpressionCloneVisitor();
                return visitor.Visit(expression);
            }
        }
    }
}