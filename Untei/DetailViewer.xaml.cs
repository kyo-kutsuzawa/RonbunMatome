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

namespace RonbunMatome
{
    /// <summary>
    /// DetailViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class DetailViewer : UserControl
    {
        private BibItem currentBibItem;

        public DetailViewer()
        {
            InitializeComponent();

            currentBibItem = new();
        }

        public void Show(BibItem bibItem)
        {
            currentBibItem = bibItem;

            Title.Text = currentBibItem.Title;
            Authors.Text = currentBibItem.AuthorSummary;
            Journal.Text = currentBibItem.Journal;
            Year.Text = currentBibItem.Year;
            Tags.Text = currentBibItem.Tags.ToString();
            Files.Text = currentBibItem.Files.ToString();
            Doi.Text = currentBibItem.Doi;
            CommentBox.Text = currentBibItem.Comment;
        }
    }
}
