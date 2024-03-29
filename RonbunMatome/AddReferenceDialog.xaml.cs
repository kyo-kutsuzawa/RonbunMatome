﻿using System;
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
using System.Windows.Shapes;

namespace RonbunMatome
{
    /// <summary>
    /// AddReferenceDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AddReferenceDialog : Window
    {
        public AddReferenceDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BibItem)
            {
                DialogResult = true;
            }
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
}
