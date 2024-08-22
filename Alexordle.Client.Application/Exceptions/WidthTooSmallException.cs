namespace Alexordle.Client.Application.Exceptions;
public class WidthTooSmallException(int width, int minimumSize) : Exception($"The width '{width}' is not greater than or equal to the minimum of '{minimumSize}'.")
{
}
