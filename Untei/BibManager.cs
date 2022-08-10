using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RonbunMatome
{
    class BibManager
    {
        public BibManager()
        {
            string fileName = "library.json";
            JsonString = File.ReadAllText(fileName);

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

        public SortedSet<string> ExtractTags()
        {
            SortedSet<string> tagSet = new SortedSet<string>();

            foreach (var item in BibDictionary)
            {
                tagSet.UnionWith(item.Value.Tags);
            }

            return tagSet;
        }
    }

    class BibItem
    {
        public BibItem()
        {
            Authors = new List<string>();
        }

        private List<string> _authors = new List<string>();

        public string Id { get; set; } = "";
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
                if (_authors.Count >= 2)
                {
                    AuthorSummary = _authors[0] + " et al.";
                }
                else if (_authors.Count == 1)
                {
                    AuthorSummary = _authors[0];
                }
                else
                {
                    AuthorSummary = "";
                }
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
    }
}
