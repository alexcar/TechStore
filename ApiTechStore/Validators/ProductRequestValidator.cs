using ApiTechStore.Dtos;
using ApiTechStore.Entities;
using FluentValidation;

namespace ApiTechStore.Validators;

/// <summary>
/// Valida os dados de entrada do produto. As regras que dependem do banco
/// (categoria existente e nome único) ficam no serviço.
/// </summary>
public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(request => request.CategoryId)
            .GreaterThan(0)
            .WithMessage("A categoria é obrigatória.");

        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("O nome é obrigatório.")
            .MinimumLength(Product.NameMinLength)
            .WithMessage($"O nome deve ter pelo menos {Product.NameMinLength} caracteres.")
            .MaximumLength(Product.NameMaxLength)
            .WithMessage($"O nome não pode ter mais de {Product.NameMaxLength} caracteres.");

        RuleFor(request => request.Price)
            .Cascade(CascadeMode.Stop)
            .NotEqual(0m)
            .WithMessage("O preço não pode ser igual a zero.")
            .GreaterThan(0m)
            .WithMessage("O preço deve ser maior que zero.")
            .LessThanOrEqualTo(Product.MaxPrice)
            .WithMessage("O preço não pode ser maior que 99.999.999,99.")
            .Must(price => decimal.Round(price, 2) == price)
            .WithMessage("O preço deve ter no máximo duas casas decimais.");
    }
}
