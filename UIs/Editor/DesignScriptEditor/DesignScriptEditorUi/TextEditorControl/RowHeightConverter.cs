using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace DesignScript.Editor.Ui
{
    class RowHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double row1Height = (double)values[1];
            double row2Height = (double)values[2];
            double row3Height = (double)values[3];
            double textEditorHeight = (double)values[0];
            double row5Height = (double)values[4];
            double row7Height = (double)values[5];
            double row6MaxHeight = 0;

            row6MaxHeight = textEditorHeight - row1Height - row2Height - row3Height - row7Height - row5Height;

            return row6MaxHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            string[] splitValues = ((string)value).Split(' ');
            return splitValues;
        }
    }
}
