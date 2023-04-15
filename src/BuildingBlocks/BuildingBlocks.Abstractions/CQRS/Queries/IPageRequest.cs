namespace BuildingBlocks.Abstractions.CQRS.Queries;

public interface IPageRequest
{
    int PageNumber { get; init; }
    int PageSize { get; init; }
    string? Filters { get; init; }
    string? SortOrder { get; init; }
}
