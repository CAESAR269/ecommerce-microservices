using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Services.Shared.Catalogs.Products.Events.v1.Integration;

public record ProductCreatedV1(long Id, string Name, long CategoryId, string CategoryName, int Stock) : IntegrationEvent
{
    public static ProductCreatedV1 Of(long id, string? name, long categoryId, string? categoryName, int stock)
    {
        id.NotBeNegativeOrZero();
        name.NotBeNullOrWhiteSpace();
        categoryId.NotBeNegativeOrZero();
        categoryName.NotBeNullOrWhiteSpace();
        stock.NotBeNegativeOrZero();

        return new ProductCreatedV1(id, name, categoryId, categoryName, stock);
    }
}
