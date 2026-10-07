using ApiTechStore.Dtos;
using ApiTechStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTechStore.Controllers;

/// <summary>CRUD do cadastro de produtos.</summary>
[Route("api/products")]
public sealed class ProductsController(IProductService productService) : ApiControllerBase
{
    /// <summary>Lista todos os produtos em ordem alfabética.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);

        return Ok(products);
    }

    /// <summary>Obtém um produto pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Inclui um produto.</summary>
    /// <remarks>
    /// Regras: o nome é obrigatório, tem de 3 a 50 caracteres e não pode repetir o de outro produto;
    /// o preço não pode ser zero; a categoria precisa existir.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ToProblem(result.Error);
    }

    /// <summary>Altera um produto.</summary>
    /// <remarks>Aplica as mesmas regras da inclusão.</remarks>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.UpdateAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    /// <summary>Exclui um produto.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await productService.DeleteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ToProblem(result.Error);
    }
}
