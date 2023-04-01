using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Shapes;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace RonbunMatome
{
    class BibManager
    {
        /// <summary>
        /// すべての文献データ
        /// </summary>
        public ObservableCollection<BibItem> BibList { get; private set; }

        /// <summary>
        /// 文献データベースのJson文字列
        /// </summary>
        public string JsonString { get; private set; }

        public BibManager()
        {
            LoadSettings();

            JsonString = File.ReadAllText(Properties.Settings.Default.libraryJsonDirectory);
            BibList = JsonSerializer.Deserialize<ObservableCollection<BibItem>>(JsonString) ?? new();
        }

        private static bool LoadSettings()
        {
            string jsonString = File.ReadAllText(Properties.Settings.Default.SettingFileDirectory);
            Dictionary<string, string>? settingDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (settingDictionary == null)
            {
                return false;
            }

            if (settingDictionary.ContainsKey("libraryJsonDirectory"))
            {
                Properties.Settings.Default.libraryJsonDirectory = settingDictionary["libraryJsonDirectory"];
            }

            if (settingDictionary.ContainsKey("libraryBibDirectory"))
            {
                Properties.Settings.Default.libraryBibDirectory = settingDictionary["libraryBibDirectory"];
            }

            return true;
        }

        /// <summary>
        /// 文献データベースのタグ一覧を得る
        /// </summary>
        /// <returns>タグのリスト</returns>
        public List<string> ExtractTags()
        {
            // 重複を自動的に省くためにSetを使う
            SortedSet<string> tagSet = new();

            // 各文献のタグを追加する。Setなので重複は自動的に省かれる
            foreach (var item in BibList)
            {
                tagSet.UnionWith(item.Tags);
            }

            // タグ一覧をリストに変換する
            List<string> tagList = tagSet.ToList();

            // リストの最初に全文献を表す要素 "All" を足す
            tagList.Insert(0, "All");

            return tagList;
        }

        /// <summary>
        /// 文献データベースを指定されたタグで絞り込む
        /// </summary>
        /// <param name="tagName">絞り込みのタグ</param>
        /// <returns>指定されたタグをもつ文献の一覧</returns>
        public ObservableCollection<BibItem> NarrowDownWithTag(string tagName)
        {
            // Allなら全文献を返す
            if (tagName == "All")
            {
                return BibList;
            }

            var list = BibList.Where(x => x.Tags.Contains(tagName));
            return new ObservableCollection<BibItem>(list);
        }

        /// <summary>
        /// 文献をデータベースに追加する
        /// </summary>
        /// <param name="item">追加する文献</param>
        /// <returns></returns>
        public bool AddReference(BibItem item)
        {
            BibList.Insert(0, item);

            return true;
        }

        public bool Save()
        {
            string libraryFileName = Properties.Settings.Default.libraryJsonDirectory;

            // 文献一覧を保存する
            JsonSerializerOptions options = new()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            string text = JsonSerializer.Serialize(BibList, options);
            File.WriteAllText(libraryFileName, text, Encoding.UTF8);

            string libraryDirName = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(libraryFileName)) ?? ".";

            // .gitフォルダが存在しなければ、gitレポジトリを初期化する
            if (Directory.Exists(System.IO.Path.Join(libraryDirName, ".git")) == false)
            {
                ProcessStartInfo psInfoToInitialize = new("cmd")
                {
                    Arguments =
                        "/c cd \"" + libraryDirName + "\" & " +
                        "git init & " +
                        "git commit --allow-empty -m \"initial commit\" & " +
                        "git branch -m master main & " +
                        "git add \"" + libraryFileName + "\" & " +
                        "git commit -m \"Library added\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                Process? processToInitialize = Process.Start(psInfoToInitialize);

                if (processToInitialize != null)
                {
                    processToInitialize.WaitForExit();
                    processToInitialize.Close();
                }
            }

            // 差分を記録する
            ProcessStartInfo psInfoToSave = new("cmd")
            {
                Arguments =
                    "/c cd \"" + libraryDirName + "\" & " +
                    "git add \"" + libraryFileName + "\" & " +
                    "git commit -m \"Library changed\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process? processToSave = Process.Start(psInfoToSave);

            if (processToSave != null)
            {
                processToSave.WaitForExit();
                processToSave.Close();
            }

            return true;
        }

        public bool ExportToBibtex(string fileName)
        {
            // BibTeX文字列を作成する
            string text = "";
            foreach (BibItem item in BibList)
            {
                text += item.ToBibtexString() + "\n";
            }
            File.WriteAllText(fileName, text, Encoding.UTF8);

            return true;
        }
        public bool ExportToBibtex() => ExportToBibtex(Properties.Settings.Default.libraryBibDirectory);
    }

    public class BibItem : INotifyPropertyChanged
    {
        public BibItem()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _abstract = string.Empty;
        [JsonPropertyName("abstract")]
        public string Abstract
        {
            get { return _abstract; }
            set { _abstract = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Abstract))); }
        }

        private string _address = string.Empty;
        [JsonPropertyName("address")]
        public string Address
        {
            get { return _address; }
            set { _address = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Address))); }
        }

        private string _arxivid = string.Empty;
        [JsonPropertyName("arxivid")]
        public string Arxivid
        {
            get { return _arxivid; }
            set { _arxivid = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Arxivid))); }
        }

        private List<string> _authors = new();
        [JsonPropertyName("author")]
        public List<string> Authors
        {
            get { return _authors; }
            set { _authors = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Authors))); }
        }

        private string _booktitle = string.Empty;
        [JsonPropertyName("booktitle")]
        public string BookTitle
        {
            get { return _booktitle; }
            set { _booktitle = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BookTitle))); }
        }

        private string _chapter = string.Empty;
        [JsonPropertyName("chapter")]
        public string Chapter
        {
            get { return _chapter; }
            set { _chapter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chapter))); }
        }

        private string _citationKey = string.Empty;
        [JsonPropertyName("ID")]  public string CitationKey
        {
            get { return _citationKey; }
            set { _citationKey = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CitationKey))); }
        }

        private string _crossref = string.Empty;
        [JsonPropertyName("crossref")]
        public string CrossRef
        {
            get { return _crossref; }
            set { _crossref = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CrossRef))); }
        }

        private string _comment = string.Empty;
        [JsonPropertyName("comment")]
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comment))); }
        }

        private string _doi = string.Empty;
        [JsonPropertyName("doi")]
        public string Doi
        {
            get { return _doi; }
            set { _doi = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Doi))); }
        }

        private string _edition = string.Empty;
        [JsonPropertyName("edition")]
        public string Edition
        {
            get { return _edition; }
            set { _edition = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Edition))); }
        }

        private string _editor = string.Empty;
        [JsonPropertyName("editor")]
        public string Editor
        {
            get { return _editor; }
            set { _editor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Editor))); }
        }

        private string _entryType = string.Empty;
        [JsonPropertyName("ENTRYTYPE")] public string EntryType
        {
            get { return _entryType; }
            set { _entryType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EntryType))); }
        }

        private string _eprint = string.Empty;
        [JsonPropertyName("eprint")]
        public string Eprint
        {
            get { return _eprint; }
            set { _eprint = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Eprint))); }
        }

        private List<string> _files = new();
        [JsonPropertyName("file")]
        public List<string> Files
        {
            get { return _files; }
            set { _files = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Files))); }
        }

        private string _isbn = string.Empty;
        [JsonPropertyName("isbn")]
        public string Isbn
        {
            get { return _isbn; }
            set { _isbn = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Isbn))); }
        }

        private string _issn = string.Empty;
        [JsonPropertyName("issn")]
        public string Issn
        {
            get { return _issn; }
            set { _issn = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Issn))); }
        }

        private string _journal = string.Empty;
        [JsonPropertyName("journal")]
        public string Journal
        {
            get { return _journal; }
            set { _journal = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Journal))); }
        }

        private string _keywords = string.Empty;
        [JsonPropertyName("keywords")]
        public string Keywords
        {
            get { return _keywords; }
            set { _keywords = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keywords))); }
        }

        private string _month = string.Empty;
        [JsonPropertyName("month")]
        public string Month
        {
            get { return _month; }
            set { _month = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Month))); }
        }

        private string _number = string.Empty;
        [JsonPropertyName("number")]
        public string Number
        {
            get { return _number; }
            set { _number = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Number))); }
        }

        private string _pages = string.Empty;
        [JsonPropertyName("pages")]
        public string Pages
        {
            get { return _pages; }
            set { _pages = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pages))); }
        }

        private string _pmid = string.Empty;
        [JsonPropertyName("pmid")]
        public string Pmid
        {
            get { return _pmid; }
            set { _pmid = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pmid))); }
        }

        private string _publisher = string.Empty;
        [JsonPropertyName("publisher")]
        public string Publisher
        {
            get { return _publisher; }
            set { _publisher = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Publisher))); }
        }

        private string _school = string.Empty;
        [JsonPropertyName("school")]
        public string School
        {
            get { return _school; }
            set { _school = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(School))); }
        }

        private string _series = string.Empty;
        [JsonPropertyName("series")]
        public string Series
        {
            get { return _series; }
            set { _series = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Series))); }
        }

        private List<string> _tags = new();
        [JsonPropertyName("tag")]
        public List<string> Tags
        {
            get { return _tags; }
            set { _tags = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty)); }
        }

        private string _title = string.Empty;
        [JsonPropertyName("title")] public string Title
        {
            get { return _title; }
            set { _title = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title))); }
        }

        private List<string> _urls = new();
        [JsonPropertyName("url")]
        public List<string> Urls
        {
            get { return _urls; }
            set { _urls = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Urls))); }
        }

        private string _volume = string.Empty;
        [JsonPropertyName("volume")]
        public string Volume
        {
            get { return _volume; }
            set { _volume = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume))); }
        }

        private string _year = string.Empty;
        [JsonPropertyName("year")] public string Year
        {
            get { return _year; }
            set { _year = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Year))); }
        }

        /// <summary>
        /// BibTeX形式の文字列に変換する
        /// </summary>
        /// <returns>BibTeX形式の文字列</returns>
        public string ToBibtexString()
        {
            string content = String.Empty;

            content += "@" + EntryType + "{";
            content += CitationKey + "\n";
            content += (Authors.Count != 0) ? "author = {" + string.Join(" and ", Authors) + "},\n" : "";
            content += (Title != "") ? "title = {" + Title + "},\n" : "";
            content += (Journal != "") ? "journal = {" + Journal + "},\n" : "";
            content += (BookTitle != "") ? "booktitle = {" + BookTitle + "},\n" : "";
            content += (School != "") ? "school = {" + School + "},\n" : "";
            content += (Volume!= "") ? "volume = {" + Volume + "},\n" : "";
            content += (Number != "") ? "number = {" + Number + "},\n" : "";
            content += (Pages != "") ? "pages = {" + Pages + "},\n" : "";
            content += (Publisher != "") ? "publisher = {" + Publisher + "},\n" : "";
            content += (Year != "") ? "year = {" + Year + "},\n" : "";
            content += (Month != "") ? "month = {" + Month + "},\n" : "";
            content += (Doi != "") ? "doi = {" + Doi + "},\n" : "";
            content += "}";

            return content;
        }
    }

    /// <summary>
    /// 文字列のリストをセミコロンで区切った単一文字列に変換する。
    /// 例えば ["aaa", "bbb"] は "aaa; bbb" になる。
    /// セミコロンの直後には空白文字が入る。
    /// </summary>
    public class ListStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not List<string>)
            {
                return DependencyProperty.UnsetValue;
            }

            if (((List<string>)value).Count < 1)
            {
                return "";
            }

            string concatAuthors = ((List<string>)value).Aggregate((x, y) => x + "; " + y);

            return concatAuthors;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> newAuthors = ((string)value).Split("; ").ToList();

            return newAuthors;
        }
    }

    public class AuthorSummaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not List<string>)
            {
                return DependencyProperty.UnsetValue;
            }

            List<string> authors = (List<string>)value;

            if (authors.Count >= 2)
            {
                return authors[0] + " et al.";
            }
            else if (authors.Count == 1)
            {
                return authors[0];
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilenamesConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((List<string>)value).Count < 1)
            {
                return new Uri("about:blank");
            }

            try
            {
                Uri uri = new(((List<string>)value)[0]);
                return uri;
            }
            catch
            {
                return new Uri("about:blank");
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LongStringConverter : IValueConverter
    {
        const int titleLength = 30;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string)
            {
                return DependencyProperty.UnsetValue;
            }

            string titleString = (string)value;

            // 文字列長が一定以上なら、末尾を"..."に置き換えて省略する
            if (titleString.Length > titleLength)
            {
                titleString = string.Concat(titleString.AsSpan(0, titleLength - 3), "...");
            }

            return titleString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
