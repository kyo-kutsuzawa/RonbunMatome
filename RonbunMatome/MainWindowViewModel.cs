using ObservableCollections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public List<BibItem> DisplayedBibList { get; private set; }

        public ObservableCollection<Tuple<MainWindowViewModel?, BibItem, Uri>> PapersList { get; private set; }

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
                selectedBibItem = value;

                // SelectedBibItemの変更をUIに通知する
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBibItem)));
            }
        }

        public List<string> TagList { get; private set; }

        public MainWindowViewModel()
        {
            bibManager = new BibManager();

            DisplayedBibList = bibManager.BibList;
            TagList = bibManager.ExtractTags();
            selectedBibItem = new();
            PapersList = new();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagList)));

            AddBibItemCommand = new(this);
            SaveBibListCommand = new(this);
            ExportBibListCommand = new(this);

            BibItem item = new()
            {
                Title = "Bibliography"
            };
            Uri paperUri = new("http://www.example.com/");
            PapersList.Add(new Tuple<MainWindowViewModel?, BibItem, Uri>(this, item, paperUri));
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
            if (item.Files.Count < 1)
            {
                return;
            }

            // PDFのファイル名のURIを取得する
            // TODO: どのPDFを参照するか指定できるようにする。
            string fileName = item.Files[0];
            Uri paperUri = new(fileName);

            PapersList.Add(new Tuple<MainWindowViewModel?, BibItem, Uri>(null, item, paperUri));
        }

        public void SaveLibrary()
        {
            bibManager.Save();
        }

        public void ExportToBibTex()
        {
            bibManager.ExportToBibtex("library.bib");
        }
    }

    public class TabTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (((Tuple<MainWindowViewModel?, BibItem, Uri>)item).Item1 != null)
            {
                return (DataTemplate)Application.Current.MainWindow.FindResource("LibraryTabTemplate");
            }
            else
            {
                return (DataTemplate)Application.Current.MainWindow.FindResource("PaperTabTemplate");
            }
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
