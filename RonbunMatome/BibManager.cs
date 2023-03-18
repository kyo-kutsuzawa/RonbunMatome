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

namespace RonbunMatome
{
    class BibManager
    {
        private const string libraryFileName = "C:\\Users\\kyo\\source\\repos\\RonbunMatome\\RonbunMatome\\bin\\Debug\\net6.0-windows\\library.json";

        public BibManager()
        {
            JsonString = File.ReadAllText(libraryFileName);
            Dictionary<string, BibItem> bibDictionary = JsonSerializer.Deserialize<Dictionary<string, BibItem>>(JsonString) ?? new Dictionary<string, BibItem>();
            BibList = bibDictionary.Values.ToList();
        }

        /// <summary>
        /// すべての文献データ
        /// </summary>
        public List<BibItem> BibList { get; private set; }

        /// <summary>
        /// 文献データベースのJson文字列
        /// </summary>
        public string JsonString { get; private set; }

        public List<string> TagList
        {
            get
            {
                return ExtractTags();
            }
            private set
            {

            }
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
        public List<BibItem> NarrowDownWithTag(string tagName)
        {
            // Allなら全文献を返す
            if (tagName == "All")
            {
                return BibList;
            }

            var list = BibList.FindAll(x => x.Tags.Contains(tagName));
            return (List<BibItem>)list;
        }

        /// <summary>
        /// 文献をデータベースに追加する
        /// </summary>
        /// <param name="item">追加する文献</param>
        /// <returns></returns>
        public bool AddReference(BibItem item)
        {
            // string key = item.Citationkey;

            Guid guid = Guid.NewGuid();
            string key = guid.ToString();

            BibList.Add(item);

            return true;
        }

        public bool Save()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            string text = JsonSerializer.Serialize(BibList, options);
            File.WriteAllText(libraryFileName, text, Encoding.UTF8);
            return true;
        }

        public bool ExportToBibtex(string fileName)
        {
            string text = "";
            foreach (BibItem item in BibList)
            {
                text += item.ToBibtexString() + "\n";
            }
            File.WriteAllText(fileName, text, Encoding.UTF8);

            return true;
        }
    }

    public class BibItem
    {
        public BibItem()
        {
            Authors = new List<string>();
        }

        private List<string> _authors = new List<string>();

        public string Citationkey { get; set; } = "";
        public string EntryType { get; set; } = "";
        public string Title { get; set; } = "";
        public List<string> Authors
        {
            get
            {
                return _authors;
            }
            set
            {
                _authors = value;

                // 著者名が変更されたときは AuthorSummary も合わせて変更する
                AuthorSummary = ConvertAuthorSummary(_authors);
            }
        }
        public string Year { get; set; } = "";
        public string Month { get; set; } = "";
        public string Doi { get; set; } = "";
        public string Journal { get; set; } = "";
        public string Abstract { get; set; } = "";
        public string Arxivid { get; set; } = "";
        public List<string> Urls { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string Comment { get; set; } = "";
        public string Keywords { get; set; } = "";
        public List<string> Files { get; set; } = new List<string>();
        public string Eprint { get; set; } = "";
        public string Archiveprefix { get; set; } = "";

        public string AuthorSummary { get; private set; } = "";

        /// <summary>
        /// BibTeX形式の文字列に変換する
        /// </summary>
        /// <returns>BibTeX形式の文字列</returns>
        public string ToBibtexString()
        {
            string content = "";

            switch (EntryType)
            {
                case "Journal":
                    content += "@article{";
                    break;

                case "InProceedings":
                    content += "@inproceedings{";
                    break;

                default:
                    content += "@misc{";
                    break;
            }

            content += Citationkey + "\n";
            content += (Authors.Count != 0) ? "author = {" + string.Join(" and ", Authors) + "},\n" : "";
            content += (Title != "") ? "journal = {" + Title + "},\n" : "";
            content += (Year != "") ? "year = {" + Year + "},\n" : "";
            content += (Month != "") ? "month = {" + Month + "},\n" : "";
            content += (Doi != "") ? "doi = {" + Doi + "},\n" : "";

            content += "}";

            return content;
        }

        /// <summary>
        /// 著者名のリストを文字列に変換する。
        /// 著者が2人以上なら "First Author et al." のように省略する。
        /// </summary>
        /// <param name="authors">List of author names</param>
        /// <returns></returns>
        static string ConvertAuthorSummary(List<string> authors)
        {
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

    public class FilenamesConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((List<string>)value).Count < 1)
            {
                return new Uri("about:blank");
            }

            return new Uri(((List<string>)value)[0]);
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
