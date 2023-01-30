using OfficeOpenXml;
using System;
using System.Windows.Forms;

namespace Javista.AttributesFactory.AppCode
{
    public static class Extensions
    {
        public static T GetValue<T>(this ExcelWorksheet sheet, int line, string range)
        {
            return sheet.GetValue<T>(line, range.PositionFromExcelColumn());
        }

        public static int PositionFromExcelColumn(this string range)
        {
            int position = 0;

            for (int i = 0; i < range.Length; i++)
            {
                position += (i != 0 ? 25 : 0) + ((int)range.ToUpper()[i]) - 64;
            }

            return position;
        }

        public static void SetAutoWidth(this Label label)
        {
            label.Width = TextRenderer.MeasureText(label.Text, label.Font).Width;
        }
    }
}