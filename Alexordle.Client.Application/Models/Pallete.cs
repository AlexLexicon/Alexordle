//namespace Alexordle.Client.Application.Models;
//public class Pallete
//{
//    public required int Width { get; init; }
//    public required IReadOnlyList<Row> Clues { get; init; }
//    public required IReadOnlyList<Row> Rows { get; init; }

//    private string? _palletString;
//    public override string ToString()
//    {
//        if (_palletString is null)
//        {
//            _palletString = string.Empty;

//            int cluesMaxLength = 0;
//            if (Clues.Count is > 0)
//            {
//                cluesMaxLength = Clues
//                    .Select(c => c.ToString().Length)
//                    .Max();
//            }

//            int rowsMaxLength = 0;
//            if (Rows.Count is > 0)
//            {
//                rowsMaxLength = Rows
//                    .Select(r => r.ToString().Length)
//                    .Max();
//            }

//            int length = Math.Max(rowsMaxLength, cluesMaxLength);

//            if (Clues.Count is > 0)
//            {
//                foreach (Row row in Clues)
//                {
//                    _palletString += CreateLine(length);
//                    _palletString += $"{row}{Environment.NewLine}";
//                }
//                _palletString += CreateLine(length);
//            }

//            _palletString += $"{Environment.NewLine}";

//            if (Rows.Count is > 0)
//            {
//                foreach (Row row in Rows)
//                {
//                    _palletString += CreateLine(length);
//                    _palletString += $"{row}{Environment.NewLine}";
//                }
//                _palletString += CreateLine(length);
//            }
//        }

//        return _palletString;

//        string CreateLine(int length) => length is <= 0 ? string.Empty : $"{new string('-', length)}{Environment.NewLine}";
//    }
//}
