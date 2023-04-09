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

        private void DoiButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not BibItem)
            {
                return;
            }

            DoiApi.FillInFromDoi(((BibItem)DataContext));
        }
    }

    public class TitleFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double)
            {
                return DependencyProperty.UnsetValue;
            }

            double fontSize = (double)value * 4 / 3;

            return fontSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
