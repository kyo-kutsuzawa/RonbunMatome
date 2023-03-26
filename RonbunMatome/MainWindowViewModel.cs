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

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 表示される文献リスト
        /// </summary>
        public ObservableCollection<BibItem> DisplayedBibList { get; private set; }

        public AddBibItemCommand AddBibItemCommand { get; private set; }
        public SaveBibListCommand SaveBibListCommand { get; private set; }
        public ExportBibListCommand ExportBibListCommand { get; private set; }

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

        public List<string> TagList { get; private set; }

        public MainWindowViewModel()
        {
            bibManager = new BibManager();
            bibManager.BibList.CollectionChanged += BibList_CollectionChanged;

            foreach (var bibItem in bibManager.BibList)
            {
                bibItem.PropertyChanged += BibItem_PropertyChanged;
            }

            DisplayedBibList = bibManager.BibList;
            TagList = bibManager.ExtractTags();
            selectedBibItem = new();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList)));

            AddBibItemCommand = new(this);
            SaveBibListCommand = new(this);
            ExportBibListCommand = new(this);
        }

        private void BibItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTagList();
        }

        private void BibList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTagList();
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

        public void UpdateTagList()
        {
            TagList = bibManager.ExtractTags();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList)));
        }

        public void SaveLibrary()
        {
            bibManager.Save();
        }

        public void ExportToBibTex()
        {
            bibManager.ExportToBibtex();
        }
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
            Vm.SaveLibrary();
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
}
