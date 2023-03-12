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

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 表示される文献データ
        /// </summary>
        public Dictionary<string, BibItem> DisplayedBibDictionary { get; private set; }

        public BibItem ShownBibItem { get; private set; }

        public List<string> TagList { get; private set; }

        //public ICommand NarrowDownWithTagCommand { get; set; }

        public MainWindowViewModel()
        {
            bibManager = new BibManager();

            DisplayedBibDictionary = bibManager.BibDictionary;

            TagList = bibManager.ExtractTags();

            ShownBibItem = new();
        }

        /// <summary>
        /// 選択中の文献を変更する
        /// </summary>
        /// <param name="key">新たに選択する文献のキー</param>
        public void ChangeShownBibItem(string key)
        {
            ShownBibItem = bibManager.BibDictionary[key];

            // ShownBibItemの変更をUIに通知する
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownBibItem)));
        }

        /// <summary>
        /// タグで文献一覧を絞り込む
        /// </summary>
        /// <param name="tagName">絞り込むタグ</param>
        public void NarrowDownWithTag(string tagName)
        {
            DisplayedBibDictionary = bibManager.NarrowDownWithTag(tagName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayedBibDictionary)));
        }

        public void OpenPdf(string key)
        {
            BibItem selectedItem = bibManager.BibDictionary[key];
        }
    }
}
