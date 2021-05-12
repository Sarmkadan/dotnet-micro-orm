using System;
using System.Linq.Expressions;

namespace DotnetMicroOrm.Benchmarks
{
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
        /// <returns>A new expression that represents a lambda expression.</returns>
        public static Expression<Func<T>> CreateExpression<T>(
            this ExpressionAndCachingBenchmarks benchmarks, 
            string paramName, 
            Expression body)
        {
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
        /// <returns>A new expression that represents a complex lambda expression.</returns>
        public static Expression<Func<T, bool>> CreateComplexExpression<T>(
            this ExpressionAndCachingBenchmarks benchmarks, 
            string paramName, 
            Expression body)
        {
            var param = Expression.Parameter(typeof(T), paramName);
            return Expression.Lambda<Func<T, bool>>(body, param);
        }

        /// <summary>
        /// Clones the given expression. 
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="expression">The expression to clone.</param>
        /// <returns>A cloned expression.</returns>
        public static Expression CloneExpression(
            this ExpressionAndCachingBenchmarks benchmarks, 
            Expression expression)
        {
            return expression;
        }

        /// <summary>
        /// Gets the body of the given expression. 
        /// </summary>
        /// <param name="benchmarks">The benchmarks instance.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The body of the expression.</returns>
        public static Expression GetBody(
            this ExpressionAndCachingBenchmarks benchmarks, 
            Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return lambdaExpression.Body;
            }

            throw new InvalidOperationException("The expression is not a lambda expression.");
        }
    }
}
