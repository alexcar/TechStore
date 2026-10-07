using ApiTechStore.Common;
using ApiTechStore.Data;
using ApiTechStore.Dtos;
using ApiTechStore.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiTechStore.Services;

public sealed class ProductService(
    TechStoreDbContext dbContext,
    IValidator<ProductRequest> validator) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .Select(product => new ProductResponse(
                product.Id,
                product.CategoryId,
                product.Category!.Name,
                product.Name,
                product.Price))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var response = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(product => new ProductResponse(
                product.Id,
                product.CategoryId,
                product.Category!.Name,
                product.Name,
                product.Price))
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
        {
            return ProductErrors.NotFound(id);
        }

        return response;
    }

    public async Task<Result<ProductResponse>> CreateAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        request = Normalize(request);

        var validation = await ValidateAsync(request, productId: null, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var category = validation.Value;

        var product = new Product
        {
            CategoryId = category.Id,
            Name = request.Name!,
            Price = request.Price
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(product, category);
    }

    public async Task<Result<ProductResponse>> UpdateAsync(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        request = Normalize(request);

        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (product is null)
        {
            return ProductErrors.NotFound(id);
        }

        var validation = await ValidateAsync(request, productId: id, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.Error;
        }

        var category = validation.Value;

        product.CategoryId = category.Id;
        product.Name = request.Name!;
        product.Price = request.Price;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(product, category);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);

        if (product is null)
        {
            return ProductErrors.NotFound(id);
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    /// <summary>
    /// Aplica as regras do produto: formato dos campos (FluentValidation), categoria existente
    /// e nome único. Em caso de sucesso devolve a categoria, já carregada.
    /// </summary>
    /// <param name="request">Dados já normalizados.</param>
    /// <param name="productId">ID do produto em alteração, ou <c>null</c> em uma inclusão.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    private async Task<Result<Category>> ValidateAsync(
        ProductRequest request,
        int? productId,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var details = validationResult.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            return ProductErrors.Invalid(details);
        }

        var category = await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ProductErrors.CategoryNotFound(request.CategoryId);
        }

        // Regra de negócio: não permitir dois produtos com o mesmo nome.
        // Em uma alteração, o próprio produto fica fora da comparação.
        var nameInUse = await dbContext.Products
            .AnyAsync(
                existing => existing.Name == request.Name && (productId == null || existing.Id != productId),
                cancellationToken);

        if (nameInUse)
        {
            return ProductErrors.DuplicateName(request.Name!);
        }

        return category;
    }

    private static ProductRequest Normalize(ProductRequest request) =>
        request with { Name = request.Name?.Trim() };

    private static ProductResponse ToResponse(Product product, Category category) =>
        new(product.Id, product.CategoryId, category.Name, product.Name, product.Price);
}
