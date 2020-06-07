using System.Collections.Generic;
using System.IO;
using System.Text;
using TestModels;

namespace KekeDataStore.Binary.Test
{
    public static class DrawContactTableExtension
    {
        private const int TABLE_WIDTH = 185;
        private const int ROW_COUNT_COL_WIDTH = 14;
        private const int ID_COL_WIDTH = 40;
        private const int LOAD_TIME_POSITION_SPACES = 140;

        /// <summary>
        /// Displays a list of Contact objects that are in the Phonebooks binary file
        /// </summary>
        /// <param name="contacts">List of phonebooks to be displayed</param>
        public static void VisualizeToFile(this IEnumerable<Contact> contacts, string path = null)
        {
            string outputFilePath = path ?? @"C:\Users\Keke\Desktop\MyNuGet\KekeDataStore\Visuals\OutputFiles\Contacts.txt";

            var sb = new StringBuilder();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            File.WriteAllText(outputFilePath, string.Empty);

            sb.AppendLine();
            sb.Append(new string(' ', TABLE_WIDTH / 2 - 20));
            sb.AppendLine($"{typeof(Contact).Name}s Table");
            sb.AppendLine();

            sb.AppendLine(PrintLine());
            sb.AppendLine(PrintRow(Column.Row_Count, Column.Id, Column.First_Name, Column.Last_Name, Column.Phone_Number, Column.Phone_Type));
            sb.AppendLine(PrintLine());
            
            var rowCount = 1;
            foreach (var item in contacts)
            {
                // Every 100 records draw a line
                if (rowCount % 101 == 0)
                    sb.AppendLine(PrintLine());

                sb.AppendLine(PrintRow(rowCount++.ToString(), item.Id.ToString(), item.Person.FirstName, item.Person.LastName, item.Phone.PhoneNumber, item.Phone.Type.ToString()));
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            var countText = $" Count: {rowCount - 1}";
            var loadTime = $" Load time: {elapsedMs} ms";

            sb.AppendLine(PrintLine());
            sb.Append(countText);
            sb.Append(new string(' ', LOAD_TIME_POSITION_SPACES));
            sb.Append(loadTime);

            File.AppendAllText(outputFilePath, sb.ToString());
        }

        private static string PrintLine()
        {
            var lineSb = new StringBuilder();
            lineSb.Append("+");
            lineSb.Append(new string('-', TABLE_WIDTH - 10));
            lineSb.Append("+");
            return lineSb.ToString();
        }

        private static string PrintRow(params string[] columns)
        {
            var rowSb = new StringBuilder();

            var width = (TABLE_WIDTH - columns.Length) / columns.Length;

            rowSb.Append("|");

            for (byte i = 0; i < columns.Length; i++)
            {
                switch ((RowIndexes)i)
                {
                    case RowIndexes.ROW_COUNT:
                    {
                        rowSb.Append(AlignCenter(columns[i], ROW_COUNT_COL_WIDTH));
                        break;
                    }
                    case RowIndexes.ID:
                    {
                        rowSb.Append(AlignCenter(columns[i], ID_COL_WIDTH));
                        break;
                    }
                    default:
                    {
                        rowSb.Append(AlignCenter(columns[i], width));
                        break;
                    }
                };
                rowSb.Append("|");
            }

            return rowSb.ToString();
        }

        private static string AlignCenter(string text, int width)
        {
            text = text ?? "NULL";
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }

        /// <summary>
        /// Column names
        /// </summary>
        private static class Column
        {
            public const string Row_Count = "Row_Count";
            public const string Id = "Id";
            public const string First_Name = "First Name";
            public const string Last_Name = "Last Name";
            public const string Phone_Number = "Phone Number";
            public const string Phone_Type = "Phone Type";
        }

        private enum RowIndexes : byte
        {
            ROW_COUNT,
            ID,
            FIRST_NAME,
            LAST_NAME,
            PHONE_NUMBER,
            PHONE_TYPE
        }
    }
}
