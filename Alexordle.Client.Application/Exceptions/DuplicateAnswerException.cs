namespace Alexordle.Client.Application.Exceptions;
public class DuplicateAnswerException(string answer) : Exception($"Multiple answers '{answer}' provided.")
{
}
