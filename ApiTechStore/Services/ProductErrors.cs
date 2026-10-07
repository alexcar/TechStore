using ApiTechStore.Common;
using ApiTechStore.Dtos;

namespace ApiTechStore.Services;

/// <summary>Erros esperados do cadastro de produtos.</summary>
public static class ProductErrors
{
    public static Error NotFound(int id) =>
        Error.NotFound("Product.NotFound", $"Produto {id} não encontrado.");

    public static Error DuplicateName(string name) =>
        Error.Conflict("Product.DuplicateName", $"Já existe um produto com o nome '{name}'.");

    public static Error CategoryNotFound(int categoryId) =>
        Error.Validation(
            "Product.CategoryNotFound",
            nameof(ProductRequest.CategoryId),
            $"Categoria {categoryId} não encontrada.");

    public static Error Invalid(IReadOnlyDictionary<string, string[]> details) =>
        Error.Validation("Product.Invalid", "Um ou mais campos do produto são inválidos.", details);
}
