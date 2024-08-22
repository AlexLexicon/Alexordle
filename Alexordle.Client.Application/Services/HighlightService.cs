using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Services;
public interface IHighlightService
{
    Task BuildAsync(Guid puzzleId);
    Task<Highlights> ProcessHighlightAsync(char? invariantCharacter, int position, bool isCommitted, bool isDesignAnswers);
    Task<bool> PostProcessHighlightAsync(IReadOnlyList<Cell> cells);
}
public class HighlightService : IHighlightService
{
    private readonly IAnswerService _answerService;
    private readonly ILetterService _letterService;

    public HighlightService(
        IAnswerService answerService,
        ILetterService letterService)
    {
        _answerService = answerService;
        _letterService = letterService;

        AnswersInvariantCharacters = [];
        AnswersPositionsToInvariantCharacters = [];
        PreviousPositionsToInvariantCharacters = [];
        DuplicateCharactersInAnswerToCount = [];
    }

    private List<List<char>> AnswersInvariantCharacters { get; }
    private Dictionary<int, HashSet<char>> AnswersPositionsToInvariantCharacters { get; }
    private Dictionary<int, HashSet<char>> PreviousPositionsToInvariantCharacters { get; }
    private Dictionary<char, int> DuplicateCharactersInAnswerToCount { get; }

    public async Task BuildAsync(Guid puzzleId)
    {
        var answers = await _answerService.GetAnswersAsync(puzzleId);
        foreach (var answer in answers)
        {
            var letters = await _letterService.GetLettersAsync(answer.WordId);

            var AnswerInvariantCharacters = new List<char>();
            for (int position = 0; position < letters.Count; position++)
            {
                char? nullableInvariantCharacter = letters[position].InvariantCharacter;

                if (nullableInvariantCharacter is not null)
                {
                    char invariantCharacter = nullableInvariantCharacter.Value;

                    AnswerInvariantCharacters.Add(invariantCharacter);
                    if (AnswersPositionsToInvariantCharacters.TryGetValue(position, out HashSet<char>? invariantCharacters))
                    {
                        invariantCharacters.Add(invariantCharacter);
                    }
                    else
                    {
                        AnswersPositionsToInvariantCharacters.Add(position,
                        [
                            invariantCharacter,
                        ]);
                    }
                    
                    if (DuplicateCharactersInAnswerToCount.ContainsKey(invariantCharacter))
                    {
                        DuplicateCharactersInAnswerToCount[invariantCharacter]++;
                    }
                    else
                    {
                        DuplicateCharactersInAnswerToCount.Add(invariantCharacter, 1);
                    }
                }
            }

            AnswersInvariantCharacters.Add(AnswerInvariantCharacters);
        }

        
    }

    public Task<Highlights> ProcessHighlightAsync(char? invariantCharacter, int position, bool isCommitted, bool isDesignAnswers)
    {
        Highlights highlight = Highlights.None;

        if (invariantCharacter is not null)
        {
            highlight = CalculateHighlight(invariantCharacter.Value, position, isCommitted, isDesignAnswers);

            Save(invariantCharacter.Value, position);
        }

        return Task.FromResult(highlight);
    }

    public Task<bool> PostProcessHighlightAsync(IReadOnlyList<Cell> cells)
    {
        int isFinishedCount = 0;
        var checkedCharacters = new HashSet<char>();

        foreach (Cell cell in cells)
        {
            char invariantCharacter = cell.InvariantCharacter;

            if (!checkedCharacters.Contains(invariantCharacter) && DuplicateCharactersInAnswerToCount.TryGetValue(invariantCharacter, out int maximumCount) && maximumCount is > 0)
            {
                int committedCorrectCount = 0;
                foreach (Cell checkCell in cells)
                {
                    if (checkCell.InvariantCharacter == invariantCharacter && checkCell.Highlight is Highlights.CommittedCorrect)
                    {
                        committedCorrectCount++;
                    }
                }

                int remainingCount = maximumCount - committedCorrectCount;
                foreach (Cell changeCell in cells)
                {
                    if (changeCell.InvariantCharacter == invariantCharacter && changeCell.Highlight is Highlights.CommittedElsewhere)
                    {
                        if (remainingCount is > 0)
                        {
                            remainingCount--;
                        }
                        else
                        {
                            changeCell.Highlight = Highlights.CommittedIncorrect;
                        }
                    }
                }

                checkedCharacters.Add(invariantCharacter);
            }

            if (cell.Highlight is Highlights.CommittedCorrect)
            {
                isFinishedCount++;
            }
        }

        bool isFinished = isFinishedCount >= cells.Count;

        if (isFinished)
        {
            foreach (Cell cell in cells)
            {
                cell.IsFinished = true;
            }
        }

        return Task.FromResult(isFinished);
    }

    private void Save(char invariantCharacter, int position)
    {
        if (PreviousPositionsToInvariantCharacters.TryGetValue(position, out HashSet<char>? previousCharacters))
        {
            previousCharacters.Add(invariantCharacter);
        }
        else
        {
            PreviousPositionsToInvariantCharacters.Add(position,
            [
                invariantCharacter,
            ]);
        }
    }

    private Highlights CalculateHighlight(char invariantCharacter, int position, bool isCommitted, bool isDesignAnswers)
    {
        if (invariantCharacter is 'A')
        {

        }

        bool isAlreadyGuessed = IsAlreadyGuessed(invariantCharacter, position);

        int? characterCountInAnswer = GetCharacterCountInAnswer(invariantCharacter, isDesignAnswers);

        //duplicate
        if (isDesignAnswers && characterCountInAnswer is null)
        {
            if (isCommitted)
            {
                return Highlights.CommittedIllegal;
            }
            else
            {
                return Highlights.Illegal;
            }
        }

        //correct
        if (AnswersPositionsToInvariantCharacters.TryGetValue(position, out HashSet<char>? invariantCharacters))
        {
            if (invariantCharacters.Contains(invariantCharacter))
            {
                if (isCommitted)
                {
                    return Highlights.CommittedCorrect;
                }
                else if (isAlreadyGuessed)
                {
                    return Highlights.Correct;
                }
            }
        }

        //elsewhere
        if (characterCountInAnswer is > 0)
        {
            if (isCommitted)
            {
                return Highlights.CommittedElsewhere;
            }
            else if (isAlreadyGuessed)
            {
                return Highlights.Elsewhere;
            }
        }

        //wrong
        if (isCommitted)
        {
            return Highlights.CommittedIncorrect;
        }
        else if (isAlreadyGuessed)
        {
            return Highlights.Wrong;
        }

        return Highlights.None;
    }

    private int? GetCharacterCountInAnswer(char invariantCharacter, bool isDesignAnswers)
    {
        bool found = false;

        int count = 0;
        foreach (List<char> answerInvariantCharacters in AnswersInvariantCharacters)
        {
            foreach (char answerInvariantCharacter in answerInvariantCharacters)
            {
                if (answerInvariantCharacter == invariantCharacter)
                {
                    if (found)
                    {
                        //we found a seperate duplicate answer
                        return null;
                    }

                    count++;
                }
            }

            found = count is > 0;

            if (found && !isDesignAnswers)
            {
                return count;
            }
        }

        return count;
    }

    private bool IsAlreadyGuessed(char invariantCharacter, int position)
    {
        if (PreviousPositionsToInvariantCharacters.TryGetValue(position, out HashSet<char>? previousCharacters))
        {
            if (previousCharacters.Contains(invariantCharacter))
            {
                return true;
            }
        }

        return false;
    }
}
