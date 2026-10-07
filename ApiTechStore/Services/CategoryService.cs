using ApiTechStore.Data;
using ApiTechStore.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ApiTechStore.Services;

public sealed class CategoryService(TechStoreDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .ToListAsync(cancellationToken);
    }
}
