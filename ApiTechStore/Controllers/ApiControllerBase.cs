using ApiTechStore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiTechStore.Controllers;

/// <summary>Base dos controllers: converte um <see cref="Error"/> do Result Pattern em resposta HTTP (Problem Details).</summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ToProblem(Error error)
    {
        if (error.Type == ErrorType.Validation)
        {
            var modelState = new ModelStateDictionary();

            foreach (var (field, messages) in error.Details ?? new Dictionary<string, string[]>())
            {
                foreach (var message in messages)
                {
                    modelState.AddModelError(field, message);
                }
            }

            return ValidationProblem(
                detail: error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Dados inválidos",
                modelStateDictionary: modelState,
                extensions: new Dictionary<string, object?> { ["code"] = error.Code });
        }

        var (statusCode, title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflito"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
        };

        return Problem(
            detail: error.Message,
            statusCode: statusCode,
            title: title,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
