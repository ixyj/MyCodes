using BingKnows;
using Microsoft.AE.Tools.DataProcessingPipeline.ManualConvertion;
using Microsoft.Search.Kif;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace SharedLibrary
{

    // This DLL should be uploaded to: /users/yukaih/BingKnows/SharedLibrary/SharedLibrary.dll
    public class BingKnowsUtility
    {
        public static string GenerateLinkForQuestionTitle(string question)
        {
            return string.Format(@"/knows/search?q={0}&tid={0}", GenerateKeyForQuestionTitle(question));
        }

        public static string GenerateLinkForAnswerTitle(string question, string answerUrl)
        {
            return string.Format(@"/knows/search?q={0}&tid={0}", GenerateKeyForQuestionTitle(question)) + "#" + GenerateKeyForAnswerTitle(answerUrl);
        }

        // rename CalculateTopicPageKey -> GenerateKeyForAnswerTitle
        public static string GenerateKeyForAnswerTitle(string url)
        {
            // for baidu zhidao
            Regex urlPattern = new Regex( @"http://zhidao.baidu.com/question/(\d+).*" );
            if (urlPattern.IsMatch(url))
            {
                long zhidaoID = 0;
                if (Int64.TryParse(urlPattern.Match(url).Groups[1].Value, out zhidaoID))
                {
                    return "bkalpha" + Convert.ToString(zhidaoID, 16).ToLower();
                }
                else
                {
                    return "bkalpha" + urlPattern.Match(url).Groups[1].Value.ToLower();
                }                
            }

            // for sosowenwen
            urlPattern = new Regex( @"http://wenwen.soso.com/z/q(\d+).*");
            if (urlPattern.IsMatch(url))
            {
                long wenwenID = 0;
                if (Int64.TryParse(urlPattern.Match(url).Groups[1].Value, out wenwenID))
                {
                    return "bkbeta" + Convert.ToString(wenwenID, 16).ToLower();
                }
                else
                {
                    return "bkbeta" + urlPattern.Match(url).Groups[1].Value.ToLower();
                }
            }

            // for tianyawenda
            urlPattern = new Regex(@"http://wenda.tianya.cn/question/(.+)");
            if (urlPattern.IsMatch(url))
            {
                return "bkgamma" + urlPattern.Match(url).Groups[1].Value.ToLower();
            }

            // for sinaiask
            urlPattern = new Regex(@"http://iask.sina.com.cn/b/(\d+).*");
            if (urlPattern.IsMatch(url))
            {
                long iaskID = 0;
                if (Int64.TryParse(urlPattern.Match(url).Groups[1].Value, out iaskID))
                {
                    return "bkdelta" + Convert.ToString(iaskID, 16).ToLower();
                }
                else
                {
                    return "bkdelta" + urlPattern.Match(url).Groups[1].Value.ToLower();
                }
            }
                        
            return "bkunknow" + url;
        }

        public static string GenerateKeyForQuestionTitle(string question)
        {
            question = TextHelper.NormalizeText(question);

            question = TextHelper.ConvertEscapeCharacter(question);

            string result = HttpUtility.UrlEncode(Encoding.UTF8.GetBytes(question));

            //need to handle special characters: !
            result = result.Replace("!", "%21").Replace("(", "%28").Replace(")", "%29");
            return result;
        }

        public static string CalculateTopicPageKey(string url)
        {
            return GenerateKeyForAnswerTitle(url);
        }



        public static bool ApplyEditorialRulesOnXml(string originalXml, ref string updatedXml, IDictionary<string, IList<ConvertionRule>> rules)
        {
            if (rules == null || rules.Count == 0) return false;
            
            var root = XElement.Parse(originalXml);
            var ele = root.Element("FullName");
            if (ele == null) return false;
            var entity = ele.Value;
            if (!rules.ContainsKey(entity)) return false;

            // step1, add @Rank attribute
            EditorialUtil.AddRankAttributes(root);

            // step2, apply the rules
            var doc = new XmlDocument();
            doc.LoadXml(root.ToString());
            if (doc == null) return false;

            XmlNode node = doc.FirstChild;
            var updatedRules = UpdateRuleIndex(node, rules[entity]);
            XmlNode result = ConvertionHelpers.Convert(node, updatedRules);

            
            //int appliedRule = DoStatisticsOnRules(node, result, rules[entity], entity, ref debugString);
            //if (appliedRule == 0) return 0; //no rule is applied, return immidiately.            

            string norResult = result.OuterXml.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

            // step3, rerank the data with updated @Rank values
            XElement converted = XElement.Parse(norResult);
            EditorialUtil.RerankNodesByScore(converted, true);

            // step4, add hyper links on the description
            StampHyperLinks(converted);

            updatedXml = converted.ToString(SaveOptions.DisableFormatting);

            return true;
        }

        private static IList<ConvertionRule> UpdateRuleIndex(XmlNode xml, IList<ConvertionRule> originalRules)
        {
            if (xml == null || originalRules == null || !originalRules.Any())
            {
                return originalRules;
            }

            var updatedRules = new List<ConvertionRule>();
            foreach (var rule in originalRules)
            {
                try
                {
                    if (!Regex.IsMatch(rule.xpath, @"\[.+='.+'\]"))
                    {
                        updatedRules.Add(rule);
                        continue;
                    }

                    var key = Regex.Match(rule.xpath, @"(?<=\[).+?(?==')").Value;
                    var value = Regex.Match(rule.xpath, @"(?<==').+?(?='\])").Value.Replace("#quote#", "'");
                    var nodes = xml.SelectNodes(rule.xpath.Substring(0, rule.xpath.IndexOf('[')))
                        .Cast<XmlNode>()
                        .Select(x => x.SelectSingleNode(key)).ToList();
                    for (var i = 0; i < nodes.Count(); i++)
                    {
                        if (nodes[i] != null && nodes[i].InnerText == value)
                        {
                            rule.xpath = Regex.Replace(rule.xpath, @"(?<=\[).+?='.+?'(?=\])", (i + 1).ToString());
                            updatedRules.Add(rule);
                            break;
                        }
                    }
                }
                catch
                {
                    updatedRules.Add(rule);
                }
            }

            return updatedRules;
        }

        public static void StampHyperLinks(XElement root, bool fCheckAnswerType = false, bool hyperLink = true)
        {
            if (root == null) return;
            
            //HyperLinksUtility HLinkUtil = new HyperLinksUtility();

            if (root.Element("Name") == null) return;
            var entityName = root.Element("Name").Value;

            if (root.Element("Description") != null)
            {
                string description = root.Element("Description").Value;

                //string descWithLinks = HLinkUtil.MakeHyperLinks(description, entityName);
                string descWithLinks = hyperLink ? HyperLinkHelper.MakeHyperLinks(description, entityName) : description;

                root.SetElementValue("Description", descWithLinks);

                // update rich value also
                XElement richDesc = root.Element("RichDescription");
                XElement richText = HyperLinkHelper.GenRichText(descWithLinks);
                if (richDesc != null && richText != null)
                {
                    richDesc.Remove();
                    richText.Name = "RichDescription";
                    root.Add(richText);
                }
            }

            // then to handle properties
            var attributesElement = root.Element("AttributeList");
            if (attributesElement != null)
            {
                foreach (var attributeElement in attributesElement.Elements("Attribute"))
                {
                    string value = attributeElement.Element("Value").Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        string valueWithLinks = hyperLink ? HyperLinkHelper.MakeHyperLinks(value, entityName) : value;

                        attributeElement.SetElementValue("Value", valueWithLinks);

                        var richAttribute = attributeElement.Element("RichValue");
                        XElement richValue = HyperLinkHelper.GenRichText(valueWithLinks);

                        if (richAttribute != null && richValue != null)
                        {
                            richAttribute.Remove();
                            richValue.Name = "RichValue";
                            attributeElement.Add(richValue);
                        }
                    }
                }
            }

            if (fCheckAnswerType)
            {
                var QnAElements = root.Element("QnAList");

                if (QnAElements != null)
                {
                    foreach(var QnAElement in QnAElements.Elements("QnA"))
                    {
                        var answerElements = QnAElement.Element("AnswerList");
                        if (answerElements != null)
                        {
                            foreach(var answerElement in answerElements.Elements("Answer"))
                            {
                                var typeElement = answerElement.Element("AnswerType");
                                if(typeElement != null)
                                {
                                    if (typeElement.Value == "3") // it's a card
                                    {
                                        string text = answerElement.Element("Text").Value;
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            string textWithLinks = hyperLink ? HyperLinkHelper.MakeHyperLinks(text, entityName) : text;
                                            answerElement.SetElementValue("Text", textWithLinks);

                                            var richTextElement = answerElement.Element("RichText");
                                            if (richTextElement != null) richTextElement.Remove();

                                            XElement richTextValue = HyperLinkHelper.GenRichText(textWithLinks);
                                            if (richTextValue != null)
                                            {
                                                richTextValue.Name = "RichText";
                                                answerElement.Add(richTextValue);                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static string GetSiteName(string url)
        {
            if (string.IsNullOrEmpty(url)) return "Unknown";

            if (url.StartsWith("http://zhidao.baidu.com/question/")) return "百度知道";
            if (url.StartsWith("http://wenwen.soso.com/z/q")) return "搜搜问问";
            if (url.StartsWith("http://iask.sina.com.cn/b/")) return "新浪爱问";
            if (url.StartsWith("http://wenda.tianya.cn/question/")) return "天涯问答";
            if (url.StartsWith("http://www.zhihu.com/question/")) return "知乎";

            if (url.StartsWith("http://ask.yaolan.com/question/")) return "摇篮网";
            if (url.StartsWith("http://www.babytree.com/ask/detail/")) return "宝宝树";
            if (url.StartsWith("http://ask.qqbaobao.com/question/")) return "亲宝网";
            if (url.StartsWith("http://kuaiwen.pcbaby.com.cn/question/")) return "太平洋亲子网";
            if (url.StartsWith("http://ask.ci123.com/questions/show/")) return "育儿网";
            if (url.StartsWith("http://www.mama.cn/ask/")) return "妈妈网";

            return "互联网";
        }

        public static List<string> SiteNameList = new List<string>() 
                                                  { "百度知道",
                                                    "搜搜问问",
                                                    "新浪爱问",
                                                    "天涯问答",
                                                    "知乎",
                                                    "摇篮网",
                                                    "宝宝树",
                                                    "亲宝网",
                                                    "太平洋亲子网",
                                                    "育儿网",
                                                    "妈妈网"
                                                    };

        private static int DoStatisticsOnRules(XmlNode before, XmlNode after, IList<ConvertionRule> rules, string entity, ref string debugString)
        {
            // do statistics
            int appliedRule = 0;

            foreach (var rule in rules)
            {
                string afterValue = string.Empty;

                // get after value
                XmlNodeList list = after.SelectNodes(rule.xpath);
                if (list != null && list.Count > 0)
                {
                    //afterValue = list[0].Value;
                    appliedRule++;
                }
                else
                {
                    if (after.SelectSingleNode(rule.xpath) != null)
                    {
                        //afterValue = after.SelectSingleNode(rule.xpath).Value;
                        appliedRule++;
                    }
                }

                //if (!string.IsNullOrEmpty(afterValue)) appliedRule++;
            }

            debugString = entity;

            return appliedRule;
        }

        private static int DoStatisticsOnRules2(XmlNode before, XmlNode after, IList<ConvertionRule> rules, string entity, ref string debugString)
        {
            // do statistics
            int appliedRule = 0;
            
            foreach (var rule in rules)
            {
                string beforeValue = string.Empty;
                string afterValue = string.Empty;
                
                // get before value
                XmlNodeList list = before.SelectNodes(rule.xpath);
                if (list != null && list.Count > 0)
                {
                    beforeValue = list[0].Value;
                }
                else
                {
                    if (before.SelectSingleNode(rule.xpath) != null)
                    {
                        beforeValue = before.SelectSingleNode(rule.xpath).Value;
                    }
                }

                // get after value
                list = after.SelectNodes(rule.xpath);
                if (list != null && list.Count > 0)
                {
                    afterValue = list[0].Value;
                }
                else
                {
                    if (after.SelectSingleNode(rule.xpath) != null)
                    {
                        afterValue = after.SelectSingleNode(rule.xpath).Value;
                    }
                }


                if (!string.IsNullOrEmpty(beforeValue) && !string.IsNullOrEmpty(afterValue)
                    && beforeValue == afterValue)
                {
                    appliedRule++;
                }
                if (!string.IsNullOrEmpty(beforeValue) && !string.IsNullOrEmpty(afterValue))
                {
                    debugString += string.Format("Entity: {2}; xpath: {3}; before: {0};  after: {1};  ", beforeValue, afterValue, entity, rule.xpath);
                }

            }


            return appliedRule;
        }

        private static Dictionary<string, string[]> HttpCallPrams = new Dictionary<string, string[]>
        {
            {"bed", new[] {"co3", "hk2"}}
        };

        private static IKifRepository KifRepository = null;
        private static string KifJson2Xml(string kifJson, string kifRepository = null)
        {
            if (string.IsNullOrEmpty(kifJson))
            {
                return null;
            }

            if (KifRepository == null)
            {
                KifConfigSingleton.SetConfig(new ConstKifConfig(APKifConfig.DefaultProtocol, string.IsNullOrWhiteSpace(kifRepository) ? @"\\lsdfs\shares\searchgold\deploy\builds\data\Answers\kifrepositoryV2\KifSchemas" : kifRepository));
                KifRepository = new FileKifRepository();
            }

            try
            {
                // Json -> Kif
                var memoryStream = new MemoryStream(4096);
                JsonToKifConverter.ReadFromStream(new StringReader(kifJson), null, memoryStream, KifRepository, null);
          
                // Kif -> Xml
                var reader = KifBinaryReader.Create(memoryStream.ToArray());
                var doc = KifToXmlConverter.Write(KifRepository, reader, true, false);
                return doc.ToString();

            }
            catch
            {
                return null;
            }
        }

        private static string GetBingKnowsJson(string query, string bed = "hk2")
        {
            try
            {
                var url = string.Format(
                    "http://{0}.origin.bing.com/knows/search?q={1}&format=pbxml&mkt=zh-cn&setflight=0",
                    bed, HttpUtility.UrlEncode(query));

                var doc = new XmlDocument();
                doc.LoadXml(HttpGet(url));

                foreach (XmlNode response in doc.SelectNodes("PropertyBag/s_AnswerResponseCommand/s_AnswerQueryResponse/a_AnswerDataArray/s_AnswerData/k_AnswerDataKifResponse"))
                {
                    try
                    {
                        var json = JObject.Parse(response.InnerText);
                        var kif = json["results"][0]["Kif.Schema"].ToString();
                        if (kif.Contains("BingKnows.Entity"))
                        {
                            if (json["results"][0]["KifException"] != null)
                            {
                                break;
                            }

                            return json["results"][0].ToString();
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string HttpGet(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            for (var i = 0; i < 3; i++)
            {
                HttpWebRequest request = null;
                try
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.Method = "GET";
                    request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                    request.Accept = "text/html,application/xhtml+xml,application/xml";
                    request.Timeout = 30000;

                    var httpWebResponse = request.GetResponse() as HttpWebResponse;
                    if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var resReader = new StreamReader(httpWebResponse.GetResponseStream()))
                        {
                            return resReader.ReadToEnd();
                        }
                    }
                }
                catch
                {
                    // retry
                }
                finally
                {
                    request.Abort(); // release resource used by request
                }
            }

            return null;
        }

        public static void GetEntityAndSave(string entityFile, string saveFile)
        {
            using (var writer = new StreamWriter(saveFile))
            {
                writer.WriteLine("<BingKnowsEntityList>");
                foreach (var line in File.ReadAllLines(entityFile))
                {
                    var entity = GetBingKnowsEntity(line);
                    if (!string.IsNullOrEmpty(entity))
                    {
                        writer.WriteLine(entity);
                        writer.Flush();
                        break;
                    }
                }

                writer.WriteLine("</BingKnowsEntityList>");
                writer.Close();
            }
        }

        public static string GetBingKnowsEntity(string query, bool addRank = true)
        {
            foreach (var bed in HttpCallPrams["bed"])
            {
                try
                {
                    var kifJson = GetBingKnowsJson(query, bed);
                    var kifXml = KifJson2Xml(kifJson);
                    return KifXmlPostProcess(kifXml, addRank);
                }
                catch
                {
                }
            }

            return null;
        }

        private static string KifXmlPostProcess(string kifXml, bool addRank)
        {
            var xml = XElement.Parse(Regex.Replace(kifXml, @"(?<=</?)root(?=[ >])", "item"));
            RemoveRedundancy(xml, @"^BingKnows\.");

            if (addRank)
            {
                EditorialUtil.AddRankAttributes(xml);
            }

            return xml.ToString(SaveOptions.DisableFormatting);
        }

        private static void RemoveRedundancy(XElement node, string regex)
        {
            if (node == null || string.IsNullOrWhiteSpace(regex))
            {
                return;
            }

            foreach (var elem in node.Elements())
            {
                RemoveRedundancy(elem, regex);
            }

            if (Regex.IsMatch(node.Name.LocalName, regex))
            {
                node.Name = Regex.Replace(node.Name.LocalName, regex, string.Empty);
            }
        }
    }
}

