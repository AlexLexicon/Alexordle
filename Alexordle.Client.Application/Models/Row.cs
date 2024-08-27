//namespace Alexordle.Client.Application.Models;
//public class Row
//{
//    public required IReadOnlyList<Cell> Cells { get; init; }
//    public required Annotations Annotation { get; init; }
//    public required bool IsGuessed { get; init; }

//    private string? _rowString;
//    public override string ToString()
//    {
//        if (_rowString is null)
//        {
//            _rowString = string.Empty;

//            for (int index = 0; index < Cells.Count; index++)
//            {
//                if (index is 0)
//                {
//                    _rowString += "|";
//                }

//                _rowString += $"{Cells[index]}|";
//            }
//        }

//        return _rowString;
//    }
//}