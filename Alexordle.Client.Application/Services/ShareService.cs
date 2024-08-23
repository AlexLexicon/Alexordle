using Alexordle.Client.Application.Extensions;
using Alexordle.Client.Application.Models;

namespace Alexordle.Client.Application.Services;
public interface IShareService
{
    Task<string> GenerateShareAsync(Guid puzzleId, string code);
}
public class ShareService : IShareService
{
    private readonly IPalleteService _palleteService;
    private readonly IStateService _stateService;
    private readonly IUrlService _urlService;

    public ShareService(
        IPalleteService palleteService,
        IStateService stateService,
        IUrlService urlService)
    {
        _palleteService = palleteService;
        _stateService = stateService;
        _urlService = urlService;
    }

    public async Task<string> GenerateShareAsync(Guid puzzleId, string code)
    {
        State state = await _stateService.GetStateAsync(puzzleId);

        Pallete pallete = await _palleteService.GeneratePalleteAsync(puzzleId, state, isDesigner: false);

        string share = $"alexordle {(state.IsDefeat ? "?" : state.TotalGuesses)}/{state.MaximumGuesses}{Environment.NewLine}";

        foreach (Row clue in pallete.Clues)
        {
            share += GenerateRow(clue);
        }

        for (int i = 0; i < pallete.Width; i++)
        {
            string block = GetBlock(null);

            share += block;
        }
        share += Environment.NewLine;

        foreach (Row row in pallete.Rows)
        {
            if (row.IsGuessed)
            {
                share += GenerateRow(row);
            }
        }

        share += await _urlService.GetPuzzleUrlAsync(code);

        return share;
    }

    private string GenerateRow(Row row)
    {
        string rowString = string.Empty;
        foreach (Cell cell in row.Cells)
        {
            string block = GetBlock(cell);

            rowString += block;
        }

        return $"{rowString}{Environment.NewLine}";
    }

    private string GetBlock(Cell? cell)
    {
        if (cell is null)
        {
            return "🔹";
        }
        else if (cell.Highlight.IsIllegal())
        {
            return "🟥";
        }
        else if (cell.Highlight.IsCorrect())
        {
            return "🟩";
        }
        else if (cell.Highlight.IsElsewhere())
        {
            return "🟨";
        }
        else
        {
            return "⬛";
        }
    }
}
