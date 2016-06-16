

namespace pbxmlParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Newtonsoft.Json.Linq;

    internal class Answer
    {
        public string service;
        public string scenario;
        public string feed;
        public double confidence;
        public uint id;
        public JObject kif;
        public string badKif;
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(@"C:\Users\yajxu\Desktop\test.xml");
                var answers = GetAnswers(doc);
                foreach (var ans in GetAnswerByPosition(answers))
                {
                    Console.WriteLine(ans.service);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        
        public static IEnumerable<Answer> GetAnswers(XmlDocument xmlResponse)
        {
            foreach (XmlNode node in xmlResponse.SelectNodes("//s_AnswerData"))
            {
                var answer = new Answer
                {  
                    service = node.SelectSingleNode("c_AnswerServiceName").InnerText,
                    scenario = node.SelectSingleNode("c_AnswerDataScenario").InnerText,
                    feed = node.SelectSingleNode("c_AnswerDataFeed").InnerText,
                    confidence = Convert.ToDouble(node.SelectSingleNode("f_AnswerDataConfidence").InnerText),
                    id = Convert.ToUInt32(node.SelectSingleNode("d_AnswerDataIDInContext").InnerText), 
                    badKif = node.SelectSingleNode("k_AnswerDataKifResponse").InnerText,
                };
                try
                {
                    answer.kif = JObject.Parse(answer.badKif); 
                }
                catch  
                {    
                }

                yield return answer;
            }
        }

        public static IEnumerable<Answer> GetAnswerByPosition(IEnumerable<Answer> answers,
            string finalPlacement = "POLE,TOP")
        {
            var aplus =
                answers.FirstOrDefault(
                    x => x.service.ToLowerInvariant() == "positioner" && x.scenario.ToLowerInvariant() == "queryrequest" && x.kif != null);
            if (aplus == null)
            {
                yield break;
            }

            var positions = finalPlacement.ToLowerInvariant().Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var answerDebugs = (JArray)aplus.kif["results"][0]["answersDebug"]["answers"]["Kif.Value"];
            foreach (
                var ans in
                    answerDebugs.Where(
                        x =>
                            x["finalPlacement"] != null &&
                            positions.Contains(x["finalPlacement"].ToString().ToLowerInvariant())))
            {
                yield return answers.FirstOrDefault(x => x.id == Convert.ToUInt32(ans["answerID"].ToString()));
            }
        }
    }
}
