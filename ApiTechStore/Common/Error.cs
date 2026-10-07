namespace ApiTechStore.Common;

/// <summary>Categoria do erro. Define o status HTTP devolvido pela API.</summary>
public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict
}

/// <summary>
/// Erro esperado de uma operação (regra de negócio, validação, registro não encontrado).
/// Erros esperados trafegam dentro de um <see cref="Result"/>; exceções ficam reservadas
/// para falhas inesperadas, tratadas pelo handler global.
/// </summary>
/// <param name="Code">Código estável, útil para o cliente tratar o erro (ex.: <c>Product.DuplicateName</c>).</param>
/// <param name="Message">Mensagem legível.</param>
/// <param name="Type">Categoria do erro.</param>
/// <param name="Details">Mensagens por campo, preenchido apenas em erros de validação.</param>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? Details = null)
{
    /// <summary>Ausência de erro, usada pelos resultados de sucesso.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]> details) =>
        new(code, message, ErrorType.Validation, details);

    /// <summary>Erro de validação de um único campo.</summary>
    public static Error Validation(string code, string field, string message) =>
        new(code, message, ErrorType.Validation, new Dictionary<string, string[]> { [field] = [message] });
}
