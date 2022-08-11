﻿using System;
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

            // Create a new tab
            PaperTabItem newTabItem = new(item.Id);
            newTabItem.OpenPdf(item.Files[0]);  // Open a PDF file
            newTabItem.SetHeaderTitle(item.Title);  // Set the tab header title
            BiblioTabControl.Items.Add(newTabItem);

            // Change selected tab
            BiblioTabControl.SelectedIndex = BiblioTabControl.Items.Count - 1;
            BiblioTabControl.SelectedItem = newTabItem;
            newTabItem.IsSelected = true;
        }
    }
}
