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

            // 文献データを読み込む
            JsonSerializerOptions options = new ()
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                },
                PropertyNameCaseInsensitive = true
            };
            JsonString = File.ReadAllText(Properties.Settings.Default.LibraryJsonDirectory);
            BibList = JsonSerializer.Deserialize<ObservableCollection<BibItem>>(JsonString, options) ?? new();
        }

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        /// <returns>設定ファイルが読み込めたらtrue、読み込めなかったらfalse</returns>
        private static bool LoadSettings()
        {
            string jsonString = File.ReadAllText(Properties.Settings.Default.SettingFileDirectory);
            Dictionary<string, string>? settingDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

            if (settingDictionary == null)
            {
                return false;
            }

            // 文献データのあるディレクトリを読み込む
            if (settingDictionary.ContainsKey("LibraryJsonDirectory"))
            {
                Properties.Settings.Default.LibraryJsonDirectory = settingDictionary["LibraryJsonDirectory"];
            }

            // BibTeXの出力先しディレクトリを読み込む
            if (settingDictionary.ContainsKey("LibraryBibDirectory"))
            {
                Properties.Settings.Default.LibraryBibDirectory = settingDictionary["LibraryBibDirectory"];
            }

            // 文献PDFのフォルダを読み込む
            if (settingDictionary.ContainsKey("FilesDirectory"))
            {
                Properties.Settings.Default.FilesDirectory = settingDictionary["FilesDirectory"];
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

        /// <summary>
        /// 文献データベースを保存する
        /// </summary>
        /// <param name="saveDiff">trueならGitで差分を保存する</param>
        /// <returns></returns>
        public bool Save(bool saveDiff)
        {
            string libraryFileName = Properties.Settings.Default.LibraryJsonDirectory;

            // 文献一覧を保存する
            JsonSerializerOptions options = new()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            string text = JsonSerializer.Serialize(BibList, options);
            File.WriteAllText(libraryFileName, text, Encoding.UTF8);

            if (saveDiff)
            {
                string libraryDirName = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(libraryFileName)) ?? ".";
                string libraryBaseName = System.IO.Path.GetFileName(System.IO.Path.GetFullPath(libraryFileName)) ?? ".";

                // .gitフォルダが存在しなければ、gitレポジトリを初期化する
                if (Directory.Exists(System.IO.Path.Join(libraryDirName, ".git")) == false)
                {
                    ProcessStartInfo psInfoToInitialize = new("cmd")
                    {
                        Arguments =
                            "/c cd /d \"" + libraryDirName + "\" & " +
                            "git init & " +
                            "git commit --allow-empty -m \"initial commit\" & " +
                            "git branch -m master main & " +
                            "git add \"" + libraryBaseName + "\" & " +
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
                        "/c cd /d \"" + libraryDirName + "\" & " +
                        "git add \"" + libraryBaseName + "\" & " +
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
            }

            return true;
        }

        /// <summary>
        /// 文献データをBibTeX形式で出力する
        /// </summary>
        /// <param name="fileName">出力先のディレクトリ</param>
        /// <returns></returns>
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
        public bool ExportToBibtex() => ExportToBibtex(Properties.Settings.Default.LibraryBibDirectory);

        /// <summary>
        /// 指定されたタグをもつ文献のコメントをひとつのtexファイルにまとめて出力する
        /// </summary>
        /// <param name="tagName">文献のタグ</param>
        /// <returns></returns>
        public bool ExportComments(string tagName)
        {
            ObservableCollection<BibItem> extractedRefs = NarrowDownWithTag(tagName);

            string reviewString = "\\subsection{Papers on " + tagName + "}\n\n";

            foreach (BibItem item in extractedRefs)
            {
                if (item.CitationKey == string.Empty)
                {
                    reviewString += "[" + AuthorSummaryConverter.Convert(item.Authors);
                    reviewString += (item.Year != string.Empty) ? ", " + item.Year : string.Empty;
                    reviewString += "]";
                }
                else
                {
                    reviewString += "\\cite{" + item.CitationKey + "}";
                }

                reviewString += "\n";
                reviewString += item.Comment;
                reviewString += "\n\n";
            }

            // コメント一覧をファイルに保存する
            string libraryFileName = Properties.Settings.Default.LibraryJsonDirectory;
            string libraryDirName = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(libraryFileName)) ?? ".";
            string fileName = System.IO.Path.Join(libraryDirName, "review_" + tagName + ".txt");
            File.WriteAllText(fileName, reviewString, Encoding.UTF8);

            return true;
        }
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

        private string _chapter = string.Empty;
        [JsonPropertyName("chapter")]
        public string Chapter
        {
            get { return _chapter; }
            set { _chapter = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Chapter))); }
        }

        private string _citationKey = string.Empty;
        [JsonPropertyName("id")]  public string CitationKey
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

        private string _container = string.Empty;
        [JsonPropertyName("container")]
        public string Container
        {
            get { return _container; }
            set { _container = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Container))); }
        }

        private string _containerShort = string.Empty;
        [JsonPropertyName("container-short")]
        public string ContainerShort
        {
            get { return _containerShort; }
            set { _containerShort = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContainerShort))); }
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

        private EntryType _entryType = EntryType.Misc;
        [JsonPropertyName("entrytype")] public EntryType EntryType
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
            string content = string.Empty;

            // Container の呼び方を指定する
            string containerType = EntryType switch
            {
                EntryType.Article => "journal",
                EntryType.InProceedings => "booktitle",
                EntryType.Misc => "journal",
                _ => "container",
            };

            string monthString = ConvertMonth(Month);

            content += "@" + EntryTypeConverter.Convert(EntryType) + "{";
            content += (CitationKey != string.Empty) ? CitationKey + ",\n" : "NULL,\n";
            content += (Authors.Count != 0) ? "author = {" + string.Join(" and ", Authors) + "},\n" : string.Empty;
            content += (Title != string.Empty) ? "title = {" + Title + "},\n" : string.Empty;
            content += (Container != string.Empty) ? containerType + " = {" + Container + "},\n" : string.Empty;
            content += (School != string.Empty) ? "school = {" + School + "},\n" : string.Empty;
            content += (Volume!= string.Empty) ? "volume = {" + Volume + "},\n" : ""    ;
            content += (Number != string.Empty) ? "number = {" + Number + "},\n" : string.Empty;
            content += (Pages != string.Empty) ? "pages = {" + Pages + "},\n" : ""  ;
            content += (Publisher != string.Empty) ? "publisher = {" + Publisher + "},\n" : string.Empty;
            content += (Year != string.Empty) ? "year = {" + Year + "},\n" : string.Empty;
            content += (monthString != string.Empty) ? "month = " + Month + ",\n" : string.Empty;
            content += (Doi != string.Empty) ? "doi = {" + Doi + "},\n" : string.Empty;
            content += "}";

            return content;
        }

        private static string ConvertMonth(string month)
        {
            string[] monthList = 
            {
                "jan",
                "feb",
                "mar",
                "apr",
                "may",
                "jun",
                "jul",
                "aug",
                "sep",
                "oct",
                "nov",
                "dec",
            };


            if (int.TryParse(month, out int idx1))
            {
                return monthList[idx1 - 1];
            }

            int idx2 = Array.IndexOf(monthList, month);
            if (idx2 != -1)
            {
                return monthList[idx2];
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// 文字列のリストをセミコロンで区切った単一文字列に変換する。
    /// 例えば ["aaa", "bbb"] は "aaa; bbb" になる。
    /// セミコロンの直後には空白文字が入る。
    /// </summary>
    public class ListStringConverter : IValueConverter
    {
        public static string Convert(List<string> stringList)
        {
            if (stringList.Count < 1)
            {
                return "";
            }

            string concatString = stringList.Aggregate((x, y) => x + "; " + y);

            return concatString;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not List<string>)
            {
                return DependencyProperty.UnsetValue;
            }

            return Convert((List<string>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> newAuthors = ((string)value).Split("; ").ToList();

            return newAuthors;
        }
    }

    /// <summary>
    /// 著者名をひとつの文字列につなげる。
    /// 1人なら"Author"、2人以上なら"Author et al."の形式とする。
    /// </summary>
    public class AuthorSummaryConverter : IValueConverter
    {
        public static string Convert(List<string> authors)
        {
            if (authors.Count >= 2)
            {
                // 2人以上なら"Author et al."の形式
                return authors[0] + " et al.";
            }
            else if (authors.Count == 1)
            {
                // 1人なら"Author"の形式
                return authors[0];
            }
            else
            {
                // 0人なら空文字列
                return string.Empty;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not List<string>)
            {
                return DependencyProperty.UnsetValue;
            }

            string summarizedAuthors = Convert((List<string>)value);

            return summarizedAuthors;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ファイル名のリストをURLに変換する。
    /// リストの最初の要素をURLにして返すか、それが無理だったら空タブのURLを返す
    /// </summary>
    public class FilenamesConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // リストが空なら空タブのURLを返す
            if (((List<string>)value).Count < 1)
            {
                return new Uri("about:blank");
            }

            try
            {
                // リストの最初の要素をURLにして返す
                string pdfUriString = ((List<string>)value)[0];

                // URL文字列が相対パスだったら、FilesDirectory を頭に付ける
                if (!System.IO.Path.IsPathRooted(pdfUriString))
                {
                    pdfUriString = System.IO.Path.Join(Properties.Settings.Default.FilesDirectory, pdfUriString);
                }

                Uri uri = new(pdfUriString);
                return uri;
            }
            catch
            {
                // リストの最初の要素がURLとして不正なら、空タブのURLを返す
                return new Uri("about:blank");
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 長い文字列の後半を"..."に省略する
    /// </summary>
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
