using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Validation.Extensions;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using FluentValidation;
using MediatR;

namespace ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock.v1;

// https://event-driven.io/en/explicit_validation_in_csharp_just_got_simpler/
// https://event-driven.io/en/how_to_validate_business_logic/
// https://event-driven.io/en/notes_about_csharp_records_and_nullable_reference_types/
// https://buildplease.com/pages/vos-in-events/
// https://codeopinion.com/leaking-value-objects-from-your-domain/
// https://www.youtube.com/watch?v=CdanF8PWJng
// we don't pass value-objects and domains to our commands and events, just primitive types
public record ReplenishProductStock(long ProductId, int Quantity) : ITxCommand
{
    public static ReplenishProductStock Of(long productId, int quantity)
    {
        return new ReplenishingProductStockValidator().HandleValidation(new ReplenishProductStock(productId, quantity));
    }
}

internal class ReplenishingProductStockValidator : AbstractValidator<ReplenishProductStock>
{
    public ReplenishingProductStockValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("ProductId must be greater than 0");
    }
}

internal class ReplenishingProductStockHandler : ICommandHandler<ReplenishProductStock>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ReplenishingProductStockHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Unit> Handle(ReplenishProductStock command, CancellationToken cancellationToken)
    {
        command.NotBeNull();

        var (productId, quantity) = command;

        var product = await _catalogDbContext.FindProductByIdAsync(ProductId.Of(productId));
        if (product is null)
            throw new ProductNotFoundException(productId);

        product.ReplenishStock(quantity);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
