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

            ((MainWindowViewModel)DataContext).OpenPdf((BibItem)content);
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not GridViewColumnHeader)
            {
                return;
            }

            // 列の名前を取得する（このとき、取得する名前はBibItemのプロパティと同じになる必要がある）
            string propertyName = (string)((GridViewColumnHeader)sender).Tag;

            ListSortDirection sortDirection;

            // 並び替えの昇降（Ascending/Descending）を決める。
            // 並び替えがまだなら昇順にして、すでに並び替えされていればそれと逆順にする。
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

                // 直前の並び替えを消去する
                BiblioListView.Items.SortDescriptions.Clear();
            }

            // BiblioListViewを並び替える
            BiblioListView.Items.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
        }

        /*
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
