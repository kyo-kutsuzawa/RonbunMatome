using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RonbunMatome
{
    /// <summary>
    /// DetailViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailViewer : UserControl
    {
        public DetailViewer()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// Convert a list of string to a string with a delimiter of "; "
    /// For example, a list ["aaa", "bbb"] is converted to "aaa; bbb".
    /// Note that a space is placed at the next to the semi-colon.
    /// </summary>
    public class ListStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<string>))
            {
                return DependencyProperty.UnsetValue;
            }

            string concatAuthors = ((List<string>)value).Aggregate((x, y) => x + "; " + y);

            return concatAuthors;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> newAuthors = ((string)value).Split("; ").ToList();

            return newAuthors;
        }
    }
}
