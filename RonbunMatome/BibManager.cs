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

namespace RonbunMatome
{
    class BibManager
    {
        private const string libraryFileName = "library.json";

        public BibManager()
        {
            JsonString = File.ReadAllText(libraryFileName);

            BibDictionary = JsonSerializer.Deserialize<Dictionary<string, BibItem>>(JsonString);
        }

        /// <summary>
        /// Dictionary of all bibliography data
        /// </summary>
        public Dictionary<string, BibItem> BibDictionary { get; private set; }

        /// <summary>
        /// Json string of the bibliography data
        /// </summary>
        public string JsonString { get; private set; }

        /// <summary>
        /// Get a list of keys.
        /// </summary>
        /// <returns>List of keys</returns>
        public List<string> GetKeys()
        {
            List<string> keys = BibDictionary.Keys.ToList();
            keys.Reverse();

            return keys;
        }

        public List<string> ExtractTags()
        {
            SortedSet<string> tagSet = new SortedSet<string>();

            foreach (var item in BibDictionary)
            {
                tagSet.UnionWith(item.Value.Tags);
            }

            List<string> tagList = tagSet.ToList();
            tagList.Insert(0, "All");

            return tagList;
        }

        public Dictionary<string, BibItem> NarrowDownWithTag(string tagName)
        {
            if (tagName == "All")
            {
                return BibDictionary;
            }
            else
            {
                var dict = BibDictionary.Where(x => x.Value.Tags.Contains(tagName)).ToDictionary(x => x.Key, x => x.Value);
                return dict;
            }
        }

        public bool Save()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };
            string text = JsonSerializer.Serialize(BibDictionary, options);
            File.WriteAllText(libraryFileName, text, Encoding.UTF8);
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

                // Set AuthorSummary
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
        /// Convert a list of author names to a summarized string.
        /// If the number of authors is more than 2, the authors except for the first author are abbreviated to "et al.".
        /// </summary>
        /// <param name="authors">List of author names</param>
        /// <returns></returns>
        string ConvertAuthorSummary(List<string> authors)
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
}
