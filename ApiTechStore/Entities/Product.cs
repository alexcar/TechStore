namespace ApiTechStore.Entities;

public sealed class Product
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 50;

    /// <summary>Maior valor aceito pela coluna DECIMAL(10,2).</summary>
    public const decimal MaxPrice = 99_999_999.99m;

    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public Category? Category { get; set; }
}
