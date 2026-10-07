using ApiTechStore.Dtos;
using ApiTechStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTechStore.Controllers;

/// <summary>Consulta de categorias, usada para preencher a lista do formulário de produto.</summary>
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    /// <summary>Lista todas as categorias em ordem alfabética.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);

        return Ok(categories);
    }
}
