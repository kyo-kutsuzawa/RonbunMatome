using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Untei
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

        /// <summary>
        /// Get a summary data of a bibliography item.
        /// </summary>
        /// <param name="key">Citation key</param>
        /// <returns>List of the author, title, and publication year of the given `key`</returns>
        public List<string> GetBibSummary(string key)
        {
            List<string> list = new()
            {
                BibDictionary[key].Author,
                BibDictionary[key].Title,
                BibDictionary[key].Year
            };

            return list;
        }
    }

    class BibItem
    {
        public BibItem()
        {
            Id = "";
            EntryType = "";
            Title = "";
            Author = "";
            Year = "";
            Month = "";
            Doi = "";
            Journal = "";
            Abstract = "";
            Arxivid = "";
            Url = "";
            Tags = "";
            Comment = "";
            Keywords = "";
            File = "";
            Eprint = "";
            Archiveprefix = "";
        }

        public string Id { get; set; }
        public string EntryType { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Doi { get; set; }
        public string Journal { get; set; }
        public string Abstract { get; set; }
        public string Arxivid { get; set; }
        public string Url { get; set; }
        public string Tags { get; set; }
        public string Comment { get; set; }
        public string Keywords { get; set; }
        public string File { get; set; }
        public string Eprint { get; set; }
        public string Archiveprefix { get; set; }
    }
}
