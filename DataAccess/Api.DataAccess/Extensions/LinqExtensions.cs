using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Api.DataAccess.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression ReplaceParameter(this Expression expression, ParameterExpression source, Expression target)
        {
            return new ParameterReplacer { Source = source, Target = target }.Visit(expression);
        }

        class ParameterReplacer : ExpressionVisitor
        {
            public ParameterExpression Source;
            public Expression Target;
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == Source ? Target : base.VisitParameter(node);
            }
        }
    }

    public static class LinqExtensions
    {
        static Expression<Func<TInput, TReturn>> ConvertReturnValue<TInput, TReturn>(Expression<Func<TInput, object>> inputExpression)
        {
            Expression convertedExpressionBody = Expression.Convert(
                inputExpression.Body, typeof(TReturn)
            );

            return Expression.Lambda<Func<TInput, TReturn>>(
                convertedExpressionBody, inputExpression.Parameters
            );
        }

        public static IQueryable<TSource> WhereAny<TSource>(this IQueryable<TSource> queryable, params Expression<Func<TSource, bool>>[] predicates)
        {
            var parameter = Expression.Parameter(typeof(TSource));
            return queryable.Where(Expression.Lambda<Func<TSource, bool>>(predicates.Aggregate<Expression<Func<TSource, bool>>, Expression>(null,
                    (current, predicate) =>
                    {
                        var visitor = new ParameterSubstitutionVisitor(predicate.Parameters[0], parameter);
                        return current != null ? Expression.OrElse(current, visitor.Visit(predicate.Body)) : visitor.Visit(predicate.Body);
                    }), parameter));
        }

        public static IQueryable<TSource> Like<TSource, TItem>(this IQueryable<TSource> source, Expression<Func<TSource, object>> expression, params TItem[] words)
        {
            var keywords = words?.Select(x => x?.ToString())?.Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
            if (keywords == null || keywords.Length == 0)
                return source;

            Expression<Func<TSource, string>> convertedExpression = ConvertReturnValue<TSource, string>(expression);

            Expression<Func<string, string, bool>> efLikeSource = (o, s) => EF.Functions.Like(o, "%" + s + "%");
            Expression<Func<TSource, bool>> wordsAny = x => keywords.Any(word => true);

            var wordsAnyBody = wordsAny.Body as MethodCallExpression;
            var efLikeBody = new ReplacingExpressionVisitor(new[] { efLikeSource.Parameters[0] }, new[] { convertedExpression.Body }).Visit(efLikeSource.Body);
            var efLikeLambda = Expression.Lambda<Func<string, bool>>(efLikeBody, efLikeSource.Parameters[1]);
            var wordsAnyNewBody = Expression.Call(wordsAnyBody.Method, wordsAnyBody.Arguments.First(), efLikeLambda);
            var result = Expression.Lambda<Func<TSource, bool>>(wordsAnyNewBody, expression.Parameters);
            return source.Where(result);
        }

        public static IQueryable<T> Includes<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] includes) where T : class
        {
            if (includes != null && includes.Length > 0)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return query;
        }


        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
            => condition ? source.Where(predicate) : source;

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, string dynamicPredicate, params object[] values)
            => condition ? source.Where(dynamicPredicate, values) : source;

        public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
            => condition ? source.Where(predicate) : source;

        public static IEnumerable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
                this IEnumerable<TOuter> outer,
                IEnumerable<TInner> inner,
                Func<TOuter, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, TInner, TResult> resultSelector)
        {
            return from o in outer
                   join i in inner on outerKeySelector(o) equals innerKeySelector(i) into joinData
                   from subI in joinData.DefaultIfEmpty()
                   select resultSelector(o, subI);
        }

        public static IEnumerable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(
            this IEnumerable<TOuter> outer,
            IEnumerable<TInner> inner,
            Func<TOuter, TKey> outerKeySelector,
            Func<TInner, TKey> innerKeySelector,
            Func<TOuter, TInner, TResult> resultSelector)
        {
            return from i in inner
                   join o in outer on innerKeySelector(i) equals outerKeySelector(o) into joinData
                   from subO in joinData.DefaultIfEmpty()
                   select resultSelector(subO, i);
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            MethodInfo groupJoin = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] GroupJoin[TOuter,TInner,TKey,TResult](System.Linq.IQueryable`1[TOuter], System.Collections.Generic.IEnumerable`1[TInner], System.Linq.Expressions.Expression`1[System.Func`2[TOuter,TKey]], System.Linq.Expressions.Expression`1[System.Func`2[TInner,TKey]], System.Linq.Expressions.Expression`1[System.Func`3[TOuter,System.Collections.Generic.IEnumerable`1[TInner],TResult]])")
                .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(TKey), typeof(LeftJoinIntermediate<TOuter, TInner>));
            MethodInfo selectMany = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
                .MakeGenericMethod(typeof(LeftJoinIntermediate<TOuter, TInner>), typeof(TInner), typeof(TResult));

            var groupJoinResultSelector = (Expression<Func<TOuter, IEnumerable<TInner>, LeftJoinIntermediate<TOuter, TInner>>>)
                ((oneOuter, manyInners) => new LeftJoinIntermediate<TOuter, TInner> { OneOuter = oneOuter, ManyInners = manyInners });

            MethodCallExpression exprGroupJoin = Expression.Call(groupJoin, outer.Expression, inner.Expression, outerKeySelector, innerKeySelector, groupJoinResultSelector);

			var selectManyCollectionSelector = (Expression<Func<LeftJoinIntermediate<TOuter, TInner>, IEnumerable<TInner>>>)(t => t.ManyInners.DefaultIfEmpty());
			//var selectManyCollectionSelector = (Expression<Func<LeftJoinIntermediate<TOuter, TInner>, IEnumerable<TInner>>>)(t => t.ManyInners);

			ParameterExpression paramUser = resultSelector.Parameters.First();

            ParameterExpression paramNew = Expression.Parameter(typeof(LeftJoinIntermediate<TOuter, TInner>), "t");
            MemberExpression propExpr = Expression.Property(paramNew, "OneOuter");

            LambdaExpression selectManyResultSelector = Expression.Lambda(new Replacer(paramUser, propExpr).Visit(resultSelector.Body), paramNew, resultSelector.Parameters.Skip(1).First());

            MethodCallExpression exprSelectMany = Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector);

            return outer.Provider.CreateQuery<TResult>(exprSelectMany);
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, string outerKeySelector, string innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            // Ensure the property paths are parsed dynamically but resolve to the correct type
            var outerKeyLambda = DynamicExpressionParser.ParseLambda<TOuter, object>(
                ParsingConfig.Default, false, outerKeySelector);
            var innerKeyLambda = DynamicExpressionParser.ParseLambda<TInner, object>(
                ParsingConfig.Default, false, innerKeySelector);

            // Call the strongly-typed LeftJoin
            return outer.LeftJoin(
                inner,
                (Expression<Func<TOuter, object>>)outerKeyLambda,
                (Expression<Func<TInner, object>>)innerKeyLambda,
                resultSelector
            );
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Expression<Func<TOuter, object>>[] outerKeySelectors, Expression<Func<TInner, object>>[] innerKeySelectors, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            if (outerKeySelectors.Length != innerKeySelectors.Length)
                throw new ArgumentException("The number of outer and inner key selectors must be the same.");

            MethodInfo groupJoin = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] GroupJoin[TOuter,TInner,TKey,TResult](System.Linq.IQueryable`1[TOuter], System.Collections.Generic.IEnumerable`1[TInner], System.Linq.Expressions.Expression`1[System.Func`2[TOuter,TKey]], System.Linq.Expressions.Expression`1[System.Func`2[TInner,TKey]], System.Linq.Expressions.Expression`1[System.Func`3[TOuter,System.Collections.Generic.IEnumerable`1[TInner],TResult]])")
                .MakeGenericMethod(typeof(TOuter), typeof(TInner), typeof(object), typeof(LeftJoinIntermediate<TOuter, TInner>));

            MethodInfo selectMany = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
                .MakeGenericMethod(typeof(LeftJoinIntermediate<TOuter, TInner>), typeof(TInner), typeof(TResult));

            var groupJoinResultSelector = (Expression<Func<TOuter, IEnumerable<TInner>, LeftJoinIntermediate<TOuter, TInner>>>)
                ((oneOuter, manyInners) => new LeftJoinIntermediate<TOuter, TInner> { OneOuter = oneOuter, ManyInners = manyInners });

            MethodCallExpression exprGroupJoin = Expression.Call(groupJoin, outer.Expression, inner.Expression, outerKeySelectors[0], innerKeySelectors[0], groupJoinResultSelector);

            var selectManyCollectionSelector = (Expression<Func<LeftJoinIntermediate<TOuter, TInner>, IEnumerable<TInner>>>)(t => t.ManyInners.DefaultIfEmpty());

            ParameterExpression paramUser = resultSelector.Parameters.First();
            ParameterExpression paramNew = Expression.Parameter(typeof(LeftJoinIntermediate<TOuter, TInner>), "t");
            MemberExpression propExpr = Expression.Property(paramNew, "OneOuter");

            LambdaExpression selectManyResultSelector = Expression.Lambda(new Replacer(paramUser, propExpr).Visit(resultSelector.Body), paramNew, resultSelector.Parameters.Skip(1).First());

            MethodCallExpression exprSelectMany = Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector);

            return outer.Provider.CreateQuery<TResult>(exprSelectMany);
        }

        public static IQueryable<TResult> LeftJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, string[] outerKeySelectors, string[] innerKeySelectors, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            // Ensure the outerKeySelectors and innerKeySelectors are of the same length
            if (outerKeySelectors.Length != innerKeySelectors.Length)
                throw new ArgumentException("The number of outer and inner key selectors must be the same.");

            // Create an array of lambda expressions for outerKeySelectors and innerKeySelectors
            var outerKeyLambdas = outerKeySelectors.Select(selector =>
                DynamicExpressionParser.ParseLambda<TOuter, object>(
                    ParsingConfig.Default, false, selector)).ToArray();

            var innerKeyLambdas = innerKeySelectors.Select(selector =>
                DynamicExpressionParser.ParseLambda<TInner, object>(
                    ParsingConfig.Default, false, selector)).ToArray();

            // Call the strongly-typed LeftJoin
            var outerKeySelectorsTyped = outerKeyLambdas.Cast<Expression<Func<TOuter, object>>>().ToArray();
            var innerKeySelectorsTyped = innerKeyLambdas.Cast<Expression<Func<TInner, object>>>().ToArray();

            // Combine the key selectors in a way that each outer and inner pair is matched
            return outer.LeftJoin(
                inner,
                outerKeySelectorsTyped,
                innerKeySelectorsTyped,
                resultSelector
            );
        }


        public static IQueryable<TResult> RightJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            MethodInfo groupJoin = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] GroupJoin[TOuter,TInner,TKey,TResult](System.Linq.IQueryable`1[TOuter], System.Collections.Generic.IEnumerable`1[TInner], System.Linq.Expressions.Expression`1[System.Func`2[TOuter,TKey]], System.Linq.Expressions.Expression`1[System.Func`2[TInner,TKey]], System.Linq.Expressions.Expression`1[System.Func`3[TOuter,System.Collections.Generic.IEnumerable`1[TInner],TResult]])")
                .MakeGenericMethod(typeof(TInner), typeof(TOuter), typeof(TKey), typeof(RightJoinIntermediate<TInner, TOuter>));
            MethodInfo selectMany = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
                .MakeGenericMethod(typeof(RightJoinIntermediate<TInner, TOuter>), typeof(TOuter), typeof(TResult));

            var groupJoinResultSelector = (Expression<Func<TInner, IEnumerable<TOuter>, RightJoinIntermediate<TInner, TOuter>>>)
                ((oneInner, manyOuters) => new RightJoinIntermediate<TInner, TOuter> { OneInner = oneInner, ManyOuters = manyOuters });

            MethodCallExpression exprGroupJoin = Expression.Call(groupJoin, inner.Expression, outer.Expression, innerKeySelector, outerKeySelector, groupJoinResultSelector);

            var selectManyCollectionSelector = (Expression<Func<RightJoinIntermediate<TInner, TOuter>, IEnumerable<TOuter>>>)(t => t.ManyOuters.DefaultIfEmpty());

            ParameterExpression paramUser = resultSelector.Parameters.First();

            ParameterExpression paramNew = Expression.Parameter(typeof(RightJoinIntermediate<TInner, TOuter>), "t");
            MemberExpression propExpr = Expression.Property(paramNew, "OneInner");

            LambdaExpression selectManyResultSelector = Expression.Lambda(new Replacer(paramUser, propExpr).Visit(resultSelector.Body), paramNew, resultSelector.Parameters.Skip(1).First());

            MethodCallExpression exprSelectMany = Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector);

            return inner.Provider.CreateQuery<TResult>(exprSelectMany);
        }

        public static IQueryable<TResult> RightJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, string outerKeySelector, string innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            // Parse key selectors from strings
            var outerKeyLambda = DynamicExpressionParser.ParseLambda<TOuter, object>(
                ParsingConfig.Default, false, outerKeySelector);
            var innerKeyLambda = DynamicExpressionParser.ParseLambda<TInner, object>(
                ParsingConfig.Default, false, innerKeySelector);

            // Call the strongly-typed RightJoin implementation
            return outer.RightJoin(
                inner,
                (Expression<Func<TOuter, object>>)outerKeyLambda,
                (Expression<Func<TInner, object>>)innerKeyLambda,
                resultSelector
            );
        }

        public static IQueryable<TResult> RightJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, Expression<Func<TOuter, object>>[] outerKeySelectors, Expression<Func<TInner, object>>[] innerKeySelectors, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            if (outerKeySelectors.Length != innerKeySelectors.Length)
                throw new ArgumentException("The number of outer and inner key selectors must be the same.");

            MethodInfo groupJoin = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] GroupJoin[TOuter,TInner,TKey,TResult](System.Linq.IQueryable`1[TOuter], System.Collections.Generic.IEnumerable`1[TInner], System.Linq.Expressions.Expression`1[System.Func`2[TOuter,TKey]], System.Linq.Expressions.Expression`1[System.Func`2[TInner,TKey]], System.Linq.Expressions.Expression`1[System.Func`3[TOuter,System.Collections.Generic.IEnumerable`1[TInner],TResult]])")
                .MakeGenericMethod(typeof(TInner), typeof(TOuter), typeof(object), typeof(RightJoinIntermediate<TInner, TOuter>));

            MethodInfo selectMany = typeof(Queryable).GetMethods()
                .Single(m => m.ToString() == "System.Linq.IQueryable`1[TResult] SelectMany[TSource,TCollection,TResult](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Collections.Generic.IEnumerable`1[TCollection]]], System.Linq.Expressions.Expression`1[System.Func`3[TSource,TCollection,TResult]])")
                .MakeGenericMethod(typeof(RightJoinIntermediate<TInner, TOuter>), typeof(TOuter), typeof(TResult));

            var groupJoinResultSelector = (Expression<Func<TInner, IEnumerable<TOuter>, RightJoinIntermediate<TInner, TOuter>>>)
                ((oneInner, manyOuters) => new RightJoinIntermediate<TInner, TOuter> { OneInner = oneInner, ManyOuters = manyOuters });

            MethodCallExpression exprGroupJoin = Expression.Call(groupJoin, inner.Expression, outer.Expression, outerKeySelectors[0], innerKeySelectors[0], groupJoinResultSelector);

            var selectManyCollectionSelector = (Expression<Func<RightJoinIntermediate<TInner, TOuter>, IEnumerable<TOuter>>>)(t => t.ManyOuters.DefaultIfEmpty());

            ParameterExpression paramUser = resultSelector.Parameters.First();
            ParameterExpression paramNew = Expression.Parameter(typeof(RightJoinIntermediate<TInner, TOuter>), "t");
            MemberExpression propExpr = Expression.Property(paramNew, "OneInner");

            LambdaExpression selectManyResultSelector = Expression.Lambda(new Replacer(paramUser, propExpr).Visit(resultSelector.Body), paramNew, resultSelector.Parameters.Skip(1).First());

            MethodCallExpression exprSelectMany = Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector);

            return inner.Provider.CreateQuery<TResult>(exprSelectMany);
        }

        public static IQueryable<TResult> RightJoin<TOuter, TInner, TResult>(this IQueryable<TOuter> outer, IQueryable<TInner> inner, string[] outerKeySelectors, string[] innerKeySelectors, Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            // Ensure the outerKeySelectors and innerKeySelectors are of the same length
            if (outerKeySelectors.Length != innerKeySelectors.Length)
                throw new ArgumentException("The number of outer and inner key selectors must be the same.");

            // Create an array of lambda expressions for outerKeySelectors and innerKeySelectors
            var outerKeyLambdas = outerKeySelectors.Select(selector =>
                DynamicExpressionParser.ParseLambda<TOuter, object>(
                    ParsingConfig.Default, false, selector)).ToArray();

            var innerKeyLambdas = innerKeySelectors.Select(selector =>
                DynamicExpressionParser.ParseLambda<TInner, object>(
                    ParsingConfig.Default, false, selector)).ToArray();

            // Call the strongly-typed RightJoin
            var outerKeySelectorsTyped = outerKeyLambdas.Cast<Expression<Func<TOuter, object>>>().ToArray();
            var innerKeySelectorsTyped = innerKeyLambdas.Cast<Expression<Func<TInner, object>>>().ToArray();

            // Combine the key selectors in a way that each outer and inner pair is matched
            return outer.RightJoin(
                inner,
                outerKeySelectorsTyped,
                innerKeySelectorsTyped,
                resultSelector
            );
        }


        private class RightJoinIntermediate<TInner, TOuter>
        {
            public TInner OneInner { get; set; }
            public IEnumerable<TOuter> ManyOuters { get; set; }
        }

        private class LeftJoinIntermediate<TOuter, TInner>
        {
            public TOuter OneOuter { get; set; }
            public IEnumerable<TInner> ManyInners { get; set; }
        }

        private class Replacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly Expression _replacement;

            public Replacer(ParameterExpression oldParam, Expression replacement)
            {
                _oldParam = oldParam;
                _replacement = replacement;
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == _oldParam)
                {
                    return _replacement;
                }

                return base.Visit(exp);
            }
        }

        private class ParameterSubstitutionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _destination;
            private readonly ParameterExpression _source;

            public ParameterSubstitutionVisitor(ParameterExpression source, ParameterExpression destination)
            {
                _source = source;
                _destination = destination;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ReferenceEquals(node, _source) ? _destination : base.VisitParameter(node);
            }
        }

    }
}
