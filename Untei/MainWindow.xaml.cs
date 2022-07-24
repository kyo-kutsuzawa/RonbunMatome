using System;
using System.Collections.Generic;
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

namespace Untei
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BibManager bibManager;
        private string selectedId;

        public MainWindow()
        {
            InitializeComponent();

            bibManager = new BibManager();

            BiblioListView.DataContext = new List<BibItem>(bibManager.BibDictionary.Values);

            TagListBox.DataContext = bibManager.ExtractTags();

            selectedId = "";
        }

        void BiblioListViewItem_ShowDetails(object sender, MouseButtonEventArgs e)
        {
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            if (item == null)
            {
                return;
            }

            selectedId = item.Id;
            CommentBox.Text = item.Comment;
        }

        void BiblioListViewItem_OpenPdf(object sender, MouseButtonEventArgs e)
        {
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            if (item == null)
            {
                return;
            }

            // Return if no PDF file is registered
            if (item.Files.Count < 1)
            {
                return;
            }

            // Setup a tab header
            string tabHeader = item.Title;
            const int tabHeaderLength = 20;

            if (tabHeader.Length > tabHeaderLength)
            {
                tabHeader = tabHeader.Substring(0, tabHeaderLength - 3) + "...";
            }

            // Create a PDF browser
            WebBrowser pdfBrowser = new WebBrowser();
            pdfBrowser.Navigate("file://" + item.Files[0] + "#toolbar=1");
            Grid contentGrid = new Grid();
            contentGrid.Children.Add(pdfBrowser);

            // Create a new tab
            TabItem newTabItem = new TabItem
            {
                Header = tabHeader,
                Content = contentGrid
            };
            BiblioTabControl.Items.Add(newTabItem);

            // Change selected tab
            BiblioTabControl.SelectedIndex = BiblioTabControl.Items.Count - 1;
            BiblioTabControl.SelectedItem = newTabItem;
            newTabItem.IsSelected = true;
        }
    }
}
