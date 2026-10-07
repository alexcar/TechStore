using ApiTechStore.Dtos;

namespace ApiTechStore.Services;

public interface ICategoryService
{
    /// <summary>Lista as categorias em ordem alfabética.</summary>
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken);
}
