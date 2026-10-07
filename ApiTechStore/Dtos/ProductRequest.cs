namespace ApiTechStore.Dtos;

/// <summary>Dados para incluir ou alterar um produto.</summary>
/// <param name="CategoryId">ID de uma categoria existente.</param>
/// <param name="Name">Nome do produto, de 3 a 50 caracteres, único no cadastro.</param>
/// <param name="Price">Preço do produto, maior que zero, com até duas casas decimais.</param>
public sealed record ProductRequest(int CategoryId, string? Name, decimal Price);
