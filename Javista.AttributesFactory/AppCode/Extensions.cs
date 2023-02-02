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

            for (int i = range.Length - 1; i >= 0; i--)
            {
                int letterPosition = range.ToUpper()[i] - 64;

                position += (i != range.Length - 1 ? letterPosition * 25 : 0) + range.ToUpper()[i] - 64;
            }

            return position;
        }

        public static void SetAutoWidth(this Label label)
        {
            label.Width = TextRenderer.MeasureText(label.Text, label.Font).Width;
        }
    }
}