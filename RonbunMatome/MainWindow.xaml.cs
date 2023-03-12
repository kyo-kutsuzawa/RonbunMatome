using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
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
        public MainWindow()
        {
            InitializeComponent();

            // これ、なくしたい
            TagListBox.DataContext = ((MainWindowViewModel)DataContext).TagList;
        }

        private void ListViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var content = ((ListViewItem)sender).Content;

            if (content == null)
            {
                return;
            }

            string key = ((KeyValuePair<string, BibItem>)content).Key;

            ((MainWindowViewModel)DataContext).ChangeShownBibItem(key);
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (sender is not ListBoxItem)
            {
                return;
            }

            string tagName = (string)((ListBoxItem)sender).DataContext;

            ((MainWindowViewModel)DataContext).NarrowDownWithTag(tagName);
        }

        void BiblioListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var content = ((ListViewItem)sender).Content;

            if (content == null)
            {
                return;
            }

            string key = ((KeyValuePair<string, BibItem>)content).Key;

            ((MainWindowViewModel)DataContext).OpenPdf(key);
        }

        /*
        public MainWindow()
        {
            InitializeComponent();

            bibManager = new BibManager();

            BiblioListView.DataContext = bibManager.BibDictionary;

            TagListBox.DataContext = bibManager.ExtractTags();
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

            BibItem item = bibManager.BibDictionary[bibItemId];
            Details.DataContext = item;
        }

        /// <summary>
        /// Open a pdf file of the given bibliography item
        /// </summary>
        /// <param name="bibItem"></param>
        void OpenNewPdfTab(KeyValuePair<string, BibItem> bibKeyValue)
        {
            // Return if no PDF file is registered
            if (bibKeyValue.Value.Files.Count < 1)
            {
                return;
            }

            // Create a new tab
            PaperTabItem newTabItem = new(bibKeyValue.Key);
            newTabItem.OpenPdf(bibKeyValue.Value.Files[0]);  // Open a PDF file
            newTabItem.SetHeaderTitle(bibKeyValue.Value.Title);  // Set the tab header title
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
            var item = ((ListViewItem)sender).Content;

            if (item == null)
            {
                return;
            }

            ShowDetails(((KeyValuePair<string, BibItem>)item).Key);
        }

        /// <summary>
        /// Open a PDF file when a listview item is double-clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BiblioListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get the selected item
            var item = ((ListViewItem)sender).Content;

            // Return if no item is selected
            if (item == null)
            {
                return;
            }

            OpenNewPdfTab((KeyValuePair<string, BibItem>)item);
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
            var item = ((ListViewItem)sender).Content;

            // Return if no item is selected
            if (item == null)
            {
                return;
            }

            OpenNewPdfTab((KeyValuePair<string, BibItem>)item);
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
                var selectedItem = BiblioListView.SelectedItem;

                if (selectedItem == null)
                {
                    return;
                }

                ShowDetails(((KeyValuePair<string, BibItem>)selectedItem).Key);
            }
            else
            {
                PaperTabItem selectedTab = (PaperTabItem)BiblioTabControl.SelectedItem;
                ShowDetails(selectedTab.Id);
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is GridViewColumnHeader))
            {
                return;
            }

            // Get the name of the column header; this should be the same as the member name of BibItem.
            string propertyName = "Value." + ((GridViewColumnHeader)sender).Tag;

            ListSortDirection sortDirection;

            // Determine the sort order (Ascending/Descending).
            // If the column has already been sorted yet, sort in the opposite order.
            // Otherwise, sort in the ascending order.
            if (BiblioListView.Items.SortDescriptions.Count == 0)
            {
                sortDirection = ListSortDirection.Ascending;
            }
            else
            {
                if (BiblioListView.Items.SortDescriptions.Last().PropertyName == propertyName)
                {
                    if (BiblioListView.Items.SortDescriptions.Last().Direction == ListSortDirection.Ascending)
                    {
                        sortDirection = ListSortDirection.Descending;
                    }
                    else
                    {
                        sortDirection = ListSortDirection.Ascending;
                    }
                }
                else
                {
                    sortDirection = ListSortDirection.Ascending;
                }

                // Clear the previous SortDescription
                BiblioListView.Items.SortDescriptions.Clear();
            }

            // Sort BiblioListView
            BiblioListView.Items.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
        }

        /// <summary>
        /// Narrow down with the selected tag when a tag-list item is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (!(sender is ListBoxItem))
            {
                return;
            }

            string tagName = (string)((ListBoxItem)sender).DataContext;

            bibManager.NarrowDownWithTag(tagName);

            //var narrowedDownDictionary = bibManager.NarrowDownWithTag(tagName);
            //BiblioListView.DataContext = narrowedDownDictionary;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((string)((MenuItem)sender).Header)
            {
                case "Add":
                    AddReferenceDialog dialog = new AddReferenceDialog();
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        bibManager.AddReference(dialog.bibItem);
                        //((BibManager)BiblioListView.DataContext).AddReference(dialog.bibItem);
                        bibManager.AddReference(dialog.bibItem);
                    }

                    break;

                case "Save":
                    bibManager.Save();
                    break;

                case "Export":
                    bibManager.ExportToBibtex("library.bib");
                    break;

                default:
                    break;
            }
        }
        */
    }
}
