using System.Runtime.CompilerServices;

namespace Alexordle.Client.Application.Exceptions;
public class ArgumentGreaterThanException(object? exclusivMaximum, object? value, [CallerArgumentExpression(nameof(value))] string? cllerArgumentExpression = null) : ArgumentOutOfRangeException(cllerArgumentExpression ?? "null", value, $"The provided value must be less than or equal to '{exclusivMaximum ?? "null"}'.")
{
}
