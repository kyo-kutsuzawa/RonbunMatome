using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace RonbunMatome
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly BibManager bibManager;
        private BibItem selectedBibItem;
        private string selectedTag;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 表示される文献リスト
        /// </summary>
        public ObservableCollection<BibItem> DisplayedBibList { get; private set; }

        /// <summary>
        /// タグ一覧
        /// </summary>
        public List<string> TagList { get; private set; }

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
                if (value == null)
                {
                    return;
                }

                selectedBibItem = value;

                // SelectedBibItemの変更をUIに通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBibItem)));
            }
        }

        /// <summary>
        /// 選択中のタグ
        /// </summary>
        public string SelectedTag
        {
            get
            {
                return selectedTag;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                selectedTag = value;

                // SelectedTagの変更をUIに通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTag)));
            }
        }

        public AddBibItemCommand AddBibItemCommand { get; private set; }
        public SaveBibListCommand SaveBibListCommand { get; private set; }
        public ExportBibListCommand ExportBibListCommand { get; private set; }
        public ExportCommentsCommand ExportCommentsCommand { get; private set; }
        public SearchCommand SearchCommand { get; private set; }

        public MainWindowViewModel()
        {
            bibManager = new BibManager();

            // 文献データに変更があればイベントを起こす
            bibManager.BibList.CollectionChanged += BibList_CollectionChanged;
            foreach (var bibItem in bibManager.BibList)
            {
                bibItem.PropertyChanged += BibItem_PropertyChanged;
            }

            // 変数を初期化する
            DisplayedBibList = bibManager.BibList;
            TagList = bibManager.ExtractTags();
            selectedTag = TagList[0];
            selectedBibItem = new();
            AddBibItemCommand = new(this);
            SaveBibListCommand = new(this);
            ExportBibListCommand = new(this);
            ExportCommentsCommand = new(this);
            SearchCommand = new(this);

            // タグ一覧を更新する。これがないと、なぜかタグ一覧が表示されない
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList)));
        }

        private void BibItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTagList();
            SaveLibrary(false);
        }

        private void BibList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTagList();
            SaveLibrary(false);
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

        /// <summary>
        /// タグ一覧を更新する
        /// </summary>
        public void UpdateTagList()
        {
            TagList = bibManager.ExtractTags();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList)));
        }

        /// <summary>
        /// 文字列で文献を絞り込む。
        /// AND検索やOR検索は未対応。
        /// </summary>
        /// <param name="searchText">検索文字列</param>
        public void Search(string searchText)
        {
            DisplayedBibList = bibManager.NarrowDownWithTag(SelectedTag);

            // 文字列が空だったら、検索文字列での絞り込みを解除する
            if (searchText == string.Empty)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayedBibList)));
                return;
            }

            // 絞り込まれた文献を入れる一次的な変数
            ObservableCollection<BibItem> tmp = new();

            // 各文献について、検索条件に合うか調べる
            foreach (BibItem bibItem in DisplayedBibList)
            {
                if (bibItem.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }

                if (bibItem.CitationKey.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }

                if (ListStringConverter.Convert(bibItem.Authors).Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }

                if (bibItem.Container.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }

                if (bibItem.Abstract.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }

                if (bibItem.Comment.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    tmp.Add(bibItem);
                    continue;
                }
            }

            // 表示する文献一覧を更新する
            DisplayedBibList = tmp;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayedBibList)));
        }

        public bool AddBibItem(BibItem bibItem) => bibManager.AddReference(bibItem);

        public void SaveLibrary(bool saveDiff) => bibManager.Save(saveDiff);

        public void ExportToBibTex() => bibManager.ExportToBibtex();

        public void ExportComments(string tagName) => bibManager.ExportComments(tagName);
    }

    internal class AddBibItemCommand : ICommand
    {
        private MainWindowViewModel Vm { get; set; }

        public AddBibItemCommand(MainWindowViewModel vm)
        {
            Vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
        }
    }

    internal class SaveBibListCommand : ICommand
    {
        private MainWindowViewModel Vm { get; set; }

        public SaveBibListCommand(MainWindowViewModel vm)
        {
            Vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Vm.SaveLibrary(true);
        }
    }

    internal class ExportBibListCommand : ICommand
    {
        private MainWindowViewModel Vm { get; set; }

        public ExportBibListCommand(MainWindowViewModel vm)
        {
            Vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Vm.ExportToBibTex();
        }
    }
    internal class ExportCommentsCommand : ICommand
    {
        private MainWindowViewModel Vm { get; set; }

        public ExportCommentsCommand(MainWindowViewModel vm)
        {
            Vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not string)
            {
                return;
            }
            Vm.ExportComments((string)parameter);
        }
    }
    internal class SearchCommand : ICommand
    {
        private MainWindowViewModel Vm { get; set; }

        public SearchCommand(MainWindowViewModel vm)
        {
            Vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter != null)
            {
                if (parameter is not string)
                {
                    return;
                }

                Vm.Search((string)parameter);
            }
        }
    }
}
