using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RonbunMatome
{
    internal static class DoiApi
    {
        // CrossRef APIのURL。この後にDOIをつなげると文献情報が得られる
        private static readonly string UrlPrefix = "https://api.crossref.org/works/";

        /// <summary>
        /// 文献のDOIから文献情報（メタデータ）を取得する。
        /// </summary>
        /// <param name="bibItem">文献データ（DOIに値が設定されている必要あり）</param>
        /// <returns>文献情報が取得できたらtrue。bibItemのDOIが空文字列だったり、文献メタデータが取得できなかったりしたらfalse。</returns>
        public static async Task<bool> FillInFromDoi(BibItem bibItem)
        {
            JsonNode BibInfo;

            // DOIが空なら即戻る
            if (bibItem.Doi == String.Empty)
            {
                return false;
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

            bibItem.Abstract = GetValueFromNode(BibInfo, "abstract");
            bibItem.Address = string.Empty;
            bibItem.Arxivid = string.Empty;

            JsonNode ? authorsNode = BibInfo["author"];
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

            if (bibItem.EntryType == EntryType.InProceedings)
            {
                bibItem.BookTitle = GetValueFromNode(BibInfo, "container-title");
            }

            bibItem.Chapter = string.Empty;
            bibItem.CitationKey = string.Empty;
            bibItem.Comment = string.Empty;
            bibItem.CrossRef = string.Empty;
            bibItem.Edition = string.Empty;

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

            bibItem.Eprint = string.Empty;
            bibItem.Files = new();
            bibItem.Isbn = string.Empty;
            bibItem.Issn = GetValueFromNode(BibInfo, "issn");

            if (bibItem.EntryType == EntryType.Article)
            {
                bibItem.Journal = GetValueFromNode(BibInfo, "container-title");
            }

            bibItem.Keywords = string.Empty;

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

            bibItem.Number = GetValueFromNode(BibInfo, "issue");
            bibItem.Pages = GetValueFromNode(BibInfo, "page");
            bibItem.Pmid = string.Empty;
            bibItem.Publisher = GetValueFromNode(BibInfo, "publisher");
            bibItem.School = string.Empty;
            bibItem.Series = string.Empty;
            bibItem.Title = GetValueFromNode(BibInfo, "title");
            bibItem.Urls = new();  // resource/primary/URL
            bibItem.Volume = GetValueFromNode(BibInfo, "volume");

            return true;
        }

        /// <summary>
        /// 与えられたJSONノード内から、指定された名前のノードを文字列として取得する。
        /// </summary>
        /// <param name="node">JSONノード</param>
        /// <param name="nodeName">取得したいノードの名前</param>
        /// <returns>指定されたノードの中身の文字列。ノードが配列なら、その最初の要素を返す。</returns>
        private static string GetValueFromNode(JsonNode node, string nodeName)
        {
            JsonNode? targetNode = node[nodeName];

            // 対象ノードがなければ空文字列を返す
            if (targetNode == null)
            {
                return string.Empty;
            }

            // 対象ノードが配列なら、その最初の要素を文字列化して返す
            if (targetNode is JsonArray)
            {
                JsonNode? element = targetNode[0];
                if (element == null)
                {
                    return string.Empty;
                }

                return element.GetValue<string>();
            }

            // 対象ノードを文字列化して返す
            return targetNode.GetValue<string>();
        }
    }
}
