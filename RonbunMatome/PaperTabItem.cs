using Microsoft.Web.WebView2.Wpf;
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
    public class PaperTabItem : TabItem
    {
        private readonly Grid contentGrid;
        private const int titleLength = 20;
        public ClosableTabHeader ClosableHeader { get; set; }
        public WebView2 PdfViewer { get; private set; }
        public string Id { get; set; } = "";

        static PaperTabItem()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(PaperTabItem), new FrameworkPropertyMetadata(typeof(PaperTabItem)));
        }

        public PaperTabItem()
        {
            // Initialize control items
            contentGrid = new Grid();
            ClosableHeader = new ClosableTabHeader();
            PdfViewer = new WebView2();

            // Add the control items to children
            this.AddChild(contentGrid);
            this.Header = ClosableHeader;
            contentGrid.Children.Add(PdfViewer);

            // Register a closing event handler; close the tab when x-button is clicked
            ClosableHeader.CloseButton.Click += new RoutedEventHandler(ClosePaperTab);
        }

        public PaperTabItem(string id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Set the tab header title
        /// </summary>
        /// 
        /// If the length is longer than 20, the rest part is hidden.
        /// <param name="title">Header title</param>
        public void SetHeaderTitle(string title)
        {
            string titleString = title;

            // Limit the text length of the tab header
            if (titleString.Length > titleLength)
            {
                titleString = string.Concat(titleString.AsSpan(0, titleLength - 3), "...");
            }

            ClosableHeader.Title.Content = titleString;
        }

        /// <summary>
        /// Open a PDF file
        /// </summary>
        /// <param name="fileName">Path of the PDF file to open</param>
        public void OpenPdf(string fileName)
        {
            Uri pdfUri = new(fileName);
            PdfViewer.Source = pdfUri;  // Open a PDF file
        }

        /// <summary>
        /// Close this tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClosePaperTab(object sender, RoutedEventArgs e)
        {
            // Remove this tab
            ((TabControl)this.Parent).Items.Remove(this);
        }

    }
}
