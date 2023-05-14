using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;

namespace RonbunMatome
{
    internal static class DoiApi
    {
        // CrossRef APIのURL。この後にDOIをつなげると文献情報が得られる
        private static readonly string UrlPrefix = "https://api.crossref.org/works/";

        // ArXiv APIのURL。この後にarXiv識別子をつなげると文献情報が得られる
        private static readonly string arXivSearhPrefix = "https://export.arxiv.org/api/query?id_list=";

        // ArXivの文献のDOI
        private static readonly string arXivDoiPrefix = "10.48550/arXiv.";

        /// <summary>
        /// 文献のDOIから文献情報（メタデータ）を取得する。
        /// </summary>
        /// <param name="bibItem">文献データ（DOIに値が設定されている必要あり）</param>
        /// <returns>文献情報が取得できたらtrue。bibItemのDOIが空文字列だったり、文献メタデータが取得できなかったりしたらfalse。</returns>
        public static async Task<bool> FillInFromDoi(BibItem bibItem)
        {
            JsonNode BibInfo;

            // DOIが空なら即戻る
            if (bibItem.Doi == string.Empty)
            {
                return false;
            }

            if (bibItem.Doi.StartsWith(arXivDoiPrefix))
            {
                bool retValue = await FillInFromArxivId(bibItem);
                return retValue;
            }

            // 文献情報を問い合わせるURLをつくる
            Uri ReferenceUrl = new(UrlPrefix + bibItem.Doi);

            // 文献情報をJSON形式で取得する
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(ReferenceUrl);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                JsonNode? responseNodes = JsonNode.Parse(jsonResponse);
                if (responseNodes != null)
                {
                    JsonNode? tmp = responseNodes["message"];
                    if (tmp != null)
                    {
                        BibInfo = tmp;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            JsonNode? entryTypeNode = BibInfo["type"];
            if (entryTypeNode != null)
            {
                switch (entryTypeNode.GetValue<string>())
                {
                    case "journal-article":
                        bibItem.EntryType = EntryType.Article;
                        break;
                    case "proceedings-article":
                        bibItem.EntryType = EntryType.InProceedings;
                        break;
                    default:
                        bibItem.EntryType = EntryType.Misc;
                        break;
                }
            }

            bibItem.Abstract = GetValueFromNode(BibInfo, "abstract") ?? bibItem.Abstract;
            //bibItem.Address = string.Empty;
            //bibItem.Arxivid = string.Empty;

            JsonNode? authorsNode = BibInfo["author"];
            if (authorsNode != null)
            {
                JsonArray authorsArray = authorsNode.AsArray();
                List<string> authorsList = new();

                for (int i = 0; i < authorsArray.Count; i++)
                {
                    string authorName = authorsArray[i]["family"] + ", " + authorsArray[i]["given"];
                    authorsList.Add(authorName);
                }
                bibItem.Authors = authorsList;
            }

            //bibItem.Chapter = string.Empty;
            //bibItem.CitationKey = string.Empty;
            //bibItem.Comment = string.Empty;
            bibItem.Container = GetValueFromNode(BibInfo, "container-title") ?? bibItem.Container;
            //bibItem.CrossRef = string.Empty;
            //bibItem.Edition = string.Empty;

            JsonNode? editorNode = BibInfo["editor"];
            if (editorNode != null)
            {
                JsonArray editorArray = editorNode.AsArray();

                string editorName = editorArray[0]["family"] + ", " + editorArray[0]["given"];
                bibItem.Editor = editorName;

                //for (int i = 0; i < editorArray.Count; i++)
                //{
                //
                //}
            }

            //bibItem.Eprint = string.Empty;
            //bibItem.Files = new();
            //bibItem.Isbn = string.Empty;
            bibItem.Issn = GetValueFromNode(BibInfo, "issn") ?? bibItem.Issn;

            //bibItem.Keywords = string.Empty;

            JsonNode? publishedNode = BibInfo["published"];
            if (publishedNode != null)
            {
                JsonNode? datePartsNode = publishedNode["date-parts"];
                if (datePartsNode != null)
                {
                    if (datePartsNode is JsonArray)
                    {
                        JsonNode? datePartsElement = datePartsNode[0];
                        if (datePartsElement != null)
                        {
                            if (datePartsElement is JsonArray array)
                            {
                                if (array.Count >= 1)
                                {
                                    JsonNode? yearNode = array[0];
                                    if (yearNode != null)
                                    {
                                        bibItem.Year = yearNode.GetValue<int>().ToString();
                                    }
                                }

                                if (array.Count >= 2)
                                {
                                    JsonNode? monthNode = array[1];
                                    if (monthNode != null)
                                    {
                                        bibItem.Month = monthNode.GetValue<int>().ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            bibItem.Number = GetValueFromNode(BibInfo, "issue") ?? bibItem.Number;
            bibItem.Pages = GetValueFromNode(BibInfo, "page") ?? bibItem.Pages;
            //bibItem.Pmid = string.Empty;
            bibItem.Publisher = GetValueFromNode(BibInfo, "publisher") ?? bibItem.Publisher;
            //bibItem.School = string.Empty;
            //bibItem.Series = string.Empty;
            bibItem.Title = GetValueFromNode(BibInfo, "title") ?? bibItem.Title;
            //bibItem.Urls = new();  // resource/primary/URL
            bibItem.Volume = GetValueFromNode(BibInfo, "volume") ?? bibItem.Volume;

            return true;
        }

        /// <summary>
        /// 与えられたJSONノード内から、指定された名前のノードを文字列として取得する。
        /// </summary>
        /// <param name="node">JSONノード</param>
        /// <param name="nodeName">取得したいノードの名前</param>
        /// <returns>指定されたノードの中身の文字列。ノードが配列なら、その最初の要素を返す。</returns>
        private static string? GetValueFromNode(JsonNode node, string nodeName)
        {
            JsonNode? targetNode = node[nodeName];

            // 対象ノードがなければ空文字列を返す
            if (targetNode == null)
            {
                return null;
            }

            // 対象ノードが配列なら、その最初の要素を対象ノードに設定し直す
            if (targetNode is JsonArray)
            {
                JsonNode? element = targetNode[0];
                if (element == null)
                {
                    return null;
                }

                targetNode = element;
            }

            // 対象ノードを文字列化して返す
            string elementString = targetNode.GetValue<string>();
            if (elementString == string.Empty)
            {
                return null;
            }
            return elementString;
        }

        private static async Task<bool> FillInFromArxivId(BibItem bibItem)
        {
            XmlDocument document = new();

            // DOIからArXiv識別子を抽出する
            string arXivId = bibItem.Doi.Substring(arXivDoiPrefix.Length);

            // 文献情報を問い合わせるURLをつくる
            Uri ReferenceUrl = new(arXivSearhPrefix + arXivId);

            // 文献情報をJSON形式で取得する
            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(ReferenceUrl);
                response.EnsureSuccessStatusCode();
                string xmlResponse = await response.Content.ReadAsStringAsync();

                document.LoadXml(xmlResponse);
                if (document.DocumentElement == null)
                {
                    return false;
                }
            }

            XmlElement rootNode = document.DocumentElement;
            XmlNamespaceManager nsManager = new(document.NameTable);
            nsManager.AddNamespace("myns", "http://www.w3.org/2005/Atom");
            XmlNode? entryNode = rootNode.SelectSingleNode("myns:entry", nsManager);

            if (entryNode == null)
            {
                return false;
            }

            // 題名を抽出する
            XmlNode? titleNode = entryNode.SelectSingleNode("myns:title", nsManager);
            if (titleNode != null)
            {
                bibItem.Title = titleNode.InnerText;
            }

            // 著者名を抽出する
            XmlNodeList? authorsNodeList = entryNode.SelectNodes("myns:author", nsManager);
            if (authorsNodeList != null)
            {
                foreach (XmlNode authorNode in authorsNodeList)
                {
                    XmlNode? nameNode = authorNode.SelectSingleNode("myns:name", nsManager);
                    if (nameNode != null)
                    {
                        string formattedName = string.Empty;

                        string[] nameParts = nameNode.InnerText.Split(' ');
                        if (nameParts.Length >= 2)
                        {
                            string familyName = nameParts[nameParts.Length - 1];
                            formattedName = familyName + ", " + string.Join(' ', nameParts[..(nameParts.Length - 1)]);
                        }
                        else
                        {
                            formattedName = nameNode.InnerText;
                        }

                        bibItem.Authors.Add(formattedName);
                    }
                }
            }

            // 年（更新年）を抽出する
            XmlNode? yearNode = entryNode.SelectSingleNode("myns:updated", nsManager);
            if (yearNode != null)
            {
                bibItem.Year = yearNode.InnerText[..4];
            }

            return true;
        }
    }
}
