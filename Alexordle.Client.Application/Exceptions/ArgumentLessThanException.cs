using System.Runtime.CompilerServices;

namespace Alexordle.Client.Application.Exceptions;
public class ArgumentLessThanException(object? exclusiveMinimum, object? value, [CallerArgumentExpression(nameof(value))] string? cllerArgumentExpression = null) : ArgumentOutOfRangeException(cllerArgumentExpression ?? "null", value, $"The provided value must be greater than or equal to '{exclusiveMinimum ?? "null"}'.")
{
}
