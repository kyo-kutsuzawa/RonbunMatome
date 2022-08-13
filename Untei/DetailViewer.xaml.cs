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

        public void Show(BibItem bibItem)
        {
            Title.Text = bibItem.Title;
            Authors.Text = bibItem.AuthorSummary;
            Journal.Text = bibItem.Journal;
            Year.Text = bibItem.Year;
            Tags.Text = bibItem.Tags.ToString();
            Files.Text = bibItem.Files.ToString();
            Doi.Text = bibItem.Doi;
            CommentBox.Text = bibItem.Comment;
        }
    }
}
