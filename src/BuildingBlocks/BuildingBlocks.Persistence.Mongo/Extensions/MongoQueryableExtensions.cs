using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions.CQRS;
using BuildingBlocks.Abstractions.CQRS.Queries;
using BuildingBlocks.Core.CQRS.Queries;
using BuildingBlocks.Core.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace BuildingBlocks.Persistence.Mongo;

public static class MongoQueryableExtensions
{
    public static async Task<ListResultModel<R>> ApplyPagingAsync<T, R, TSortKey>(
        this IMongoQueryable<T> collection,
        IPageRequest pageRequest,
        IConfigurationProvider configuration,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, TSortKey>>? sortExpression = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        IMongoQueryable<T> query = collection;
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (pageRequest.Filters is not null)
        {
            query = query.ApplyFilter(pageRequest.Filters);
        }

        if (sortExpression is not null)
        {
            query = query.OrderByDescending(sortExpression);
        }

        return await query.ApplyPagingAsync<T, R>(
            configuration,
            pageRequest.Page,
            pageRequest.PageSize,
            cancellationToken
        );
    }

    public static async Task<ListResultModel<T>> ApplyPagingAsync<T, TSortKey>(
        this IMongoQueryable<T> collection,
        IPageRequest pageRequest,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, TSortKey>>? sortExpression = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        IMongoQueryable<T> query = collection;
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (pageRequest.Filters is not null)
        {
            query = query.ApplyFilter(pageRequest.Filters);
        }

        if (sortExpression is not null)
        {
            query = query.OrderByDescending(sortExpression);
        }

        return await query.ApplyPagingAsync(pageRequest.Page, pageRequest.PageSize, cancellationToken);
    }

    public static async Task<ListResultModel<T>> ApplyPagingAsync<T>(
        this IMongoQueryable<T> collection,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty)
            return ListResultModel<T>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = await collection.Skip(page, pageSize).ToListAsync(cancellationToken: cancellationToken);

        return ListResultModel<T>.Create(data, totalItems, page, pageSize);
    }

    public static async Task<ListResultModel<R>> ApplyPagingAsync<T, R>(
        this IMongoQueryable<T> collection,
        IConfigurationProvider configuration,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty)
            return ListResultModel<R>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = collection.Skip(page, pageSize).ProjectTo<R>(configuration).ToList();

        return ListResultModel<R>.Create(data, totalItems, page, pageSize);
    }

    public static IMongoQueryable<T> Skip<T>(this IMongoQueryable<T> collection, int page = 1, int resultsPerPage = 10)
        where T : class
    {
        if (page <= 0)
            page = 1;

        if (resultsPerPage <= 0)
            resultsPerPage = 10;

        var skip = (page - 1) * resultsPerPage;
        var data = MongoQueryable.Skip(collection, skip).Take(resultsPerPage);

        return data;
    }

    public static IMongoQueryable<TEntity> ApplyFilter<TEntity>(
        this IMongoQueryable<TEntity> source,
        IEnumerable<FilterModel>? filters
    )
        where TEntity : class
    {
        if (filters is null)
            return source;

        List<Expression<Func<TEntity, bool>>> filterExpressions = new List<Expression<Func<TEntity, bool>>>();

        foreach (var (fieldName, comparision, fieldValue) in filters)
        {
            Expression<Func<TEntity, bool>> expr = PredicateBuilder.Build<TEntity>(fieldName, comparision, fieldValue);
            filterExpressions.Add(expr);
        }

        return source.Where(filterExpressions.Aggregate((expr1, expr2) => expr1.And(expr2)));
    }
}