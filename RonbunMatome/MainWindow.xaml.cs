using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
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
            if (LibraryListView.Items.SortDescriptions.Count == 0)
            {
                sortDirection = ListSortDirection.Ascending;
            }
            else
            {
                if (LibraryListView.Items.SortDescriptions.Last().PropertyName == propertyName)
                {
                    if (LibraryListView.Items.SortDescriptions.Last().Direction == ListSortDirection.Ascending)
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
                LibraryListView.Items.SortDescriptions.Clear();
            }

            // BiblioListViewを並び替える
            LibraryListView.Items.SortDescriptions.Add(new SortDescription(propertyName, sortDirection));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            BibItem bibItem = new();

            // 新規文献追加のダイアログを出す
            AddReferenceDialog addReferenceDialog = new()
            {
                DataContext = bibItem,
                FontSize = Properties.Settings.Default.FontSize
            };
            bool? diaogResult = addReferenceDialog.ShowDialog();

            switch (diaogResult)
            {
                case true:
                    // 文献をデータベースに追加する
                    ((MainWindowViewModel)DataContext).AddBibItem(bibItem);
                    break;
                case false:
                    break;
                default:
                    break;
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            string jsonString = File.ReadAllText(Properties.Settings.Default.SettingFileDirectory);
            Dictionary<string, string>? settingDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (settingDictionary == null)
            {
                return;
            }

            if (settingDictionary.ContainsKey("FontSize"))
            {
                Properties.Settings.Default.FontSize = int.Parse(settingDictionary["FontSize"]);
            }
        }
    }
}
