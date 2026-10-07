namespace ApiTechStore.Dtos;

/// <summary>Produto devolvido pela API, já com o nome da categoria.</summary>
public sealed record ProductResponse(int Id, int CategoryId, string CategoryName, string Name, decimal Price);
