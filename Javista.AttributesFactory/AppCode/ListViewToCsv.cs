using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Javista.AttributesFactory.AppCode
{
    internal static class ListViewToCsv
    {
        public static void ToCsv(this ListView listView, string filePath)
        {
            //make header string
            StringBuilder result = new StringBuilder();
            WriteCsvRow(result, listView.Columns.Count, i => listView.Columns[i].Text);

            //export data rows
            foreach (ListViewItem listItem in listView.Items)
                WriteCsvRow(result, listView.Columns.Count, i => listItem.SubItems[i].Text);

            File.WriteAllText(filePath, result.ToString());
        }

        private static void WriteCsvRow(StringBuilder result, int itemsCount, Func<int, string> columnValue)
        {
            bool isFirstTime = true;
            for (int i = 0; i < itemsCount; i++)
            {
                if (!isFirstTime)
                    result.Append(",");
                isFirstTime = false;

                result.Append($"\"{columnValue(i)}\"");
            }
            result.AppendLine();
        }
    }
}