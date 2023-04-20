namespace Osmy.Cli
{
    internal class TableWriter
    {
        private readonly int _columnCount;

        private readonly int[] _columnWidthList;

        public TableWriter(params int[] columnWidthList)
        {
            _columnCount = columnWidthList.Length;
            _columnWidthList = columnWidthList;
        }

        public void WriteHeader(params string[] columnNames)
        {
            if (columnNames.Length != _columnCount)
            {
                throw new ArgumentException(null, nameof(columnNames));
            }

            int index = 0;
            foreach (var name in columnNames)
            {
                Console.Write($"| {name.PadRight(_columnWidthList[index++])} ");
            }
            Console.WriteLine("|");

            for (int i = 0; i < _columnCount; i++)
            {
                Console.Write("|" + new string('-', _columnWidthList[i] + 2));
            }
            Console.WriteLine("|");
        }

        public void WriteRow(params string?[] values)
        {
            if (values.Length != _columnCount)
            {
                throw new ArgumentException(null, nameof(values));
            }

            int index = 0;
            foreach (var value in values)
            {
                Console.Write($"| {value?.PadRight(_columnWidthList[index++]) ?? new string(' ', _columnWidthList[index++])} ");
            }
            Console.WriteLine("|");
        }
    }
}
