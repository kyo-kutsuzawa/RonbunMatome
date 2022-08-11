using System;
using System.Collections;
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
        /// Show details of a paper with the specified id to the detail panel
        /// </summary>
        /// <param name="bibItemId">Id of a bibliography item</param>
        void ShowDetails(string bibItemId)
        {
            if (!bibManager.BibDictionary.ContainsKey(bibItemId))
            {
                return;
            }

            selectedId = bibItemId;

            BibItem item = bibManager.BibDictionary[bibItemId];
            CommentBox.Text = item.Comment;
        }

        /// <summary>
        /// Open a pdf file of the given bibliography item
        /// </summary>
        /// <param name="bibItem"></param>
        void OpenNewPdfTab(BibItem bibItem)
        {
            // Return if no PDF file is registered
            if (bibItem.Files.Count < 1)
            {
                return;
            }

            // Create a new tab
            PaperTabItem newTabItem = new(bibItem.Id);
            newTabItem.OpenPdf(bibItem.Files[0]);  // Open a PDF file
            newTabItem.SetHeaderTitle(bibItem.Title);  // Set the tab header title
            BiblioTabControl.Items.Add(newTabItem);

            // Change selected tab
            BiblioTabControl.SelectedIndex = BiblioTabControl.Items.Count - 1;
            BiblioTabControl.SelectedItem = newTabItem;
            newTabItem.IsSelected = true;
        }

        /// <summary>
        /// Show details of the selected item
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

            ShowDetails(item.Id);
        }

        /// <summary>
        /// Open a PDF file when a listview item is double-clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BiblioListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected item
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            // Return if no item is selected
            if (item == null)
            {
                return;
            }

            OpenNewPdfTab(item);
        }

        /// <summary>
        /// Open a PDF file when Enter key is pressed on a listview item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BiblioListViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            // Do nothing and return if the pressed key is not Enter
            if (e.Key != Key.Return)
            {
                return;
            }

            // Get the selected item
            BibItem? item = ((ListViewItem)sender).Content as BibItem;

            // Return if no item is selected
            if (item == null)
            {
                return;
            }

            OpenNewPdfTab(item);
        }

        /// <summary>
        /// Update the detail panel when the selected tab is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BiblioTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected tab
            int selectedTabIndex = BiblioTabControl.SelectedIndex;

            // Which item to show is changed, depending on the current tab.
            // If the current tab is the listview tab, the selected item in the listview is shown;
            // otherwise, details of the current tab is shown.
            if (selectedTabIndex == -1)  // When no tab is selected
            {
                return;
            }
            else if (selectedTabIndex == 0)
            {
                BibItem? selectedItem = (BibItem)BiblioListView.SelectedItem;

                if (selectedItem == null)
                {
                    return;
                }

                ShowDetails(selectedItem.Id);
            }
            else
            {
                PaperTabItem selectedTab = (PaperTabItem)BiblioTabControl.SelectedItem;
                ShowDetails(selectedTab.Id);
            }
        }
    }
}
