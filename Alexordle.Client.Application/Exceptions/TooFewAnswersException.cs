namespace Alexordle.Client.Application.Exceptions;
public class TooFewAnswersException(int numberOfAnswers, int minimumNumberOfAnswers) : Exception($"The number of answers '{numberOfAnswers}' is not greater than or equal to the minimum of '{minimumNumberOfAnswers}'.")
{
}
