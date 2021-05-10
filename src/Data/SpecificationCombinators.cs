#nullable enable

using System.Linq.Expressions;

namespace DotnetMicroOrm.Data;

/// <summary>Extension methods to compose specifications: spec1.And(spec2), spec1.Or(spec2), spec.Not().</summary>
public static class SpecificationCombinators
{
    /// <summary>Returns a new specification whose Criteria is left.Criteria AND right.Criteria (null criteria treated as always-true). Includes and IncludeStrings are unioned.</summary>
    public static Specification<T> And<T>(this Specification<T> left, Specification<T> right) where T : class
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));

        var criteria = Combine(left.Criteria, right.Criteria, Expression.AndAlso);

        var combined = new CombinedSpec<T>(criteria);

        // Union includes
        combined.Includes.AddRange(left.Includes);
        combined.Includes.AddRange(right.Includes);

        // Union include strings
        combined.IncludeStrings.AddRange(left.IncludeStrings);
        combined.IncludeStrings.AddRange(right.IncludeStrings);

        return combined;
    }

    /// <summary>Returns a new specification whose Criteria is left.Criteria OR right.Criteria.</summary>
    public static Specification<T> Or<T>(this Specification<T> left, Specification<T> right) where T : class
    {
        if (left == null) throw new ArgumentNullException(nameof(left));
        if (right == null) throw new ArgumentNullException(nameof(right));

        var criteria = Combine(left.Criteria, right.Criteria, Expression.OrElse);

        var combined = new CombinedSpec<T>(criteria);

        // Union includes
        combined.Includes.AddRange(left.Includes);
        combined.Includes.AddRange(right.Includes);

        // Union include strings
        combined.IncludeStrings.AddRange(left.IncludeStrings);
        combined.IncludeStrings.AddRange(right.IncludeStrings);

        return combined;
    }

    /// <summary>Returns a new specification whose Criteria is the logical negation of spec.Criteria.</summary>
    public static Specification<T> Not<T>(this Specification<T> spec) where T : class
    {
        if (spec == null) throw new ArgumentNullException(nameof(spec));

        Expression<Func<T, bool>>? negatedCriteria = null;

        if (spec.Criteria != null)
        {
            negatedCriteria = Expression.Lambda<Func<T, bool>>(
                Expression.Not(spec.Criteria.Body),
                spec.Criteria.Parameters);
        }

        var combined = new CombinedSpec<T>(negatedCriteria);

        // Copy includes
        combined.Includes.AddRange(spec.Includes);

        // Copy include strings
        combined.IncludeStrings.AddRange(spec.IncludeStrings);

        return combined;
    }

    /// <summary>Combines two boolean lambdas with the given binary op, rebinding the right lambda's parameter onto the left's.</summary>
    private static Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>>? left, Expression<Func<T, bool>>? right, Func<Expression, Expression, BinaryExpression> op)
    {
        // If both are null, return always-true
        if (left == null && right == null)
        {
            return _ => true;
        }

        // If left is null, return right (treat as always-true)
        if (left == null)
        {
            return right!;
        }

        // If right is null, return left (treat as always-true)
        if (right == null)
        {
            return left;
        }

        // Both have criteria - combine them
        var visitor = new ParameterReplaceVisitor(right.Parameters[0], left.Parameters[0]);
        var rightBody = visitor.Visit(right.Body);

        var combinedBody = op(left.Body, rightBody);

        return Expression.Lambda<Func<T, bool>>(combinedBody, left.Parameters);
    }

    /// <summary>Concrete spec used to carry the combined criteria/includes.</summary>
    private sealed class CombinedSpec<T> : Specification<T> where T : class
    {
        public CombinedSpec(Expression<Func<T, bool>>? criteria)
        {
            Criteria = criteria;
        }
    }

    /// <summary>ExpressionVisitor that replaces one ParameterExpression with another.</summary>
    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : node;
        }
    }
}
