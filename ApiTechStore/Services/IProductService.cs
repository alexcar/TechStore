using ApiTechStore.Common;
using ApiTechStore.Dtos;

namespace ApiTechStore.Services;

public interface IProductService
{
    /// <summary>Lista os produtos em ordem alfabética.</summary>
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Result<ProductResponse>> CreateAsync(ProductRequest request, CancellationToken cancellationToken);

    Task<Result<ProductResponse>> UpdateAsync(int id, ProductRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
