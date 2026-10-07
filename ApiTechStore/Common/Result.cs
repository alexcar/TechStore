namespace ApiTechStore.Common;

/// <summary>
/// Resultado de uma operação que pode falhar por um motivo esperado (Result Pattern).
/// Em vez de lançar exceção, a operação devolve sucesso ou um <see cref="Common.Error"/>.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("Um resultado de sucesso não pode ter erro.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("Um resultado de falha precisa de um erro.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>Resultado que carrega um valor quando a operação tem sucesso.</summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Valor da operação. Só pode ser lido quando <see cref="Result.IsSuccess"/> é verdadeiro.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível ler o valor de um resultado de falha.");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
