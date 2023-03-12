using ObservableCollections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RonbunMatome
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly BibManager bibManager;
        private BibItem selectedBibItem;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 表示される文献リスト
        /// </summary>
        public List<BibItem> DisplayedBibList{ get; private set; }

        /// <summary>
        /// 選択中の文献データ
        /// </summary>
        public BibItem SelectedBibItem
        {
            get
            {
                return selectedBibItem;
            }
            set
            {
                selectedBibItem = value;

                // ShownBibItemの変更をUIに通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBibItem)));
            }
        }

        public List<string> TagList { get; private set; }

        public MainWindowViewModel()
        {
            bibManager = new BibManager();

            DisplayedBibList = bibManager.BibList;

            TagList = bibManager.ExtractTags();

            SelectedBibItem = new();
        }

        /// <summary>
        /// タグで文献一覧を絞り込む
        /// </summary>
        /// <param name="tagName">絞り込むタグ</param>
        public void NarrowDownWithTag(string tagName)
        {
            DisplayedBibList = bibManager.NarrowDownWithTag(tagName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayedBibList)));
        }

        public void OpenPdf(BibItem item)
        {
        }
    }
}