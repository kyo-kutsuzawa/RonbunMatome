using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        /// <summary>
        /// Show details and comments of the selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BiblioListViewItem_ShowDetails(object sender, MouseButtonEventArgs e)
        {
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            if (item == null)
            {
                return;
            }

            selectedId = item.Id;
            //PaperTitleBox.Text = item.Title;
            CommentBox.Text = item.Comment;
        }

        void BiblioListViewItem_OpenPdf(object sender, MouseButtonEventArgs e)
        {
            // Get the selected item
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            // Return if no item is selected
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

            // Limit the text length of the tab header
            if (tabHeader.Length > tabHeaderLength)
            {
                tabHeader = string.Concat(tabHeader.AsSpan(0, tabHeaderLength - 3), "...");
            }

            // Create a PDF browser
            WebBrowser pdfBrowser = new WebBrowser();
            Uri pdfUri = new("file://" + item.Files[0] + "#toolbar=1");
            pdfBrowser.Navigate(pdfUri);  // Open a PDF file
            Grid contentGrid = new();
            contentGrid.Children.Add(pdfBrowser);

            // Create a new tab
            TabItem newTabItem = new()
            {
                Content = contentGrid  // Add the PDF viewer
            };
            BiblioTabControl.Items.Add(newTabItem);

            // Create a tab header
            ClosableTabHeader header = new ClosableTabHeader();
            header.Title.Content = tabHeader;
            header.CloseButton.Click += new RoutedEventHandler(ClosePaperTab);  // Close the tab when x-button is clicked
            newTabItem.Header = header;

            // Change selected tab
            BiblioTabControl.SelectedIndex = BiblioTabControl.Items.Count - 1;
            BiblioTabControl.SelectedItem = newTabItem;
            newTabItem.IsSelected = true;
        }

        /// <summary>
        /// Close a tab
        /// </summary>
        /// <param name="sender">This must be a Button in a tab header.</param>
        /// <param name="e"></param>
        void ClosePaperTab(object sender, RoutedEventArgs e)
        {
            // Get the parent tab
            TabItem? clickedTab = (TabItem?)((ClosableTabHeader)((Grid)((Button)sender).Parent).Parent).Parent;

            if (clickedTab == null)
            {
                return;
            }

            if (clickedTab.Parent != BiblioTabControl)
            {
                return;
            }

            // Remove the clicked tab
            BiblioTabControl.Items.Remove(clickedTab);
        }
    }
}
