/// <summary>
/// How to Reference bond class:
///  1. Add bond files
///  2. Update .csproj file:
///   a. Add import at top:
///        &lt Import Project="D:\Branches\Suggestions\private\packages\Bond.Library\bond.csharp.props" Condition="Exists('D:\Branches\Suggestions\private\packages\Bond.Library\bond.csharp.props')" /&gt
///   b. Add import at bottom:
///        &lt Import Project="D:\Branches\Suggestions\private\packages\Bond.Library\bond.csharp.targets" /&gt
///   c. Update bond to BONDFILE: 
///        &lt BondFile Include="Bond\MsnJVDataService.GeneralKey.bond" /&gt
///  3. Add required bond dll and open from CoreXT
/// </summary>
                   
namespace BondBuilder
{
    using Halsey.AE.C3BToC3A;
    using Microsoft.Bond;
    using MsnJVDataService;
    using ObjectStoreAccess;
    using System;
    using System.Collections.Generic;
    using System.Linq;
       
    public class Builder
    {
        public static int Main(string[] args)
        {
            try
            {
                var key = new GeneralKey {Key = "zhcn_c3btoc3a"};
                var value = HackBondConfig();
                var data = new Dictionary<GeneralKey, BondedGeneralResponse> { { key, value } };

                var nameSpace = "SatoriAnswer";
                var table = "AnswerRealtimeV2";
                //Test PROD
                {
                    Console.WriteLine("Write to PROD...");
                    var osConfig = ObjectStoreController.GetOsConfig(ObjectStoreContext.PRODs, nameSpace, table);
                    var signals = ObjectStoreController.Write(osConfig, data);
                    Console.WriteLine("Write to Prod: {0}", signals.All(x => x.IsSuccessful) ? "Succeeded" : "Failed");
                    Console.WriteLine("Read PROD...");
                    var rets = ObjectStoreController.Read<GeneralKey, BondedGeneralResponse>(ObjectStoreContext.HK2, nameSpace, table, new List<GeneralKey> { key });
                    foreach (var r in rets)
                    {
                        var generalResponse = r.BondedResponse.Cast<GeneralResponse>();
                        var lgConfig = r.BondedResponse.Cast<LGConfigResponse>();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        public static BondedGeneralResponse HackBondConfig()
        {
            var lgConfig = new LGConfigResponse
            {
                Type = DataType.LGConfig,
                Config = new AnswerSelectionConfig_2
                {
                    Enabled = true,
                    LGConfig = new Dictionary<string, LGConfig_1>()
                }
            };

            var lg = new LGConfig_1
            {
                DisplayText = "为您找到如下结果",
                SpokenText = "为您找到如下结果",
                SpokenSsml =
                    "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\" xmlns:emo=\"http://www.w3.org/2009/10/emotionml\" xml:lang=\"zh-CN\"><mstts:prompt domain=\"generic\"/><emo:emotion category-set=\"http://www.example.com/emotion/category/everyday-emotions.xml\"><emo:category name=\"CALM\" value=\"1.0\"/>为您找到如下结果</emo:emotion></speak>"
            };

            lgConfig.Config.LGConfig.Add("LG-Demo", lg);

            var blocklist = new[]
            {
                "MsnJVDataAnswer:UX_53:BKQNAAnswer.BingKnowsQNA",
                "MsnJVDataAnswerV2:ModuleList:BKQNAAnswer.BingKnowsDirectAnswerV2",
                "MsnJVDataAnswerV2:ModuleList:BingKnowsWikiAnswer.BingKnowsWikiCardMobile",
                "MsnJVDataAnswerV2:ModuleList:EntityAnswer.QnAAnswer",
                "MsnJVDataAnswer:UX_53:Commerce.mobile_Series_zol",
                "MsnJVDataAnswerV2:ModuleList:BKQNAAnswer.BingKnowsQNA",
                "MsnJVDataAnswer:UX_53:Maps.GlobalMap",
                "MsnJVDataAnswer:UX_53:Auto.Mobile_Yiche",
                "MsnJVDataAnswerV2:ModuleList:EntityFacts.FactAnswerMobile",
                "MsnJVDataAnswer:UX_53:Auto.Mobile_Yiche_Info",
                "MsnJVDataAnswer:UX_53:Auto.Mobile_Yiche_RelatedEntity",
                "MsnJVDataAnswer:UX_53:Education.Shici_xdf",
                "MsnJVDataAnswerV2:ModuleList:BingKnowsOnlyAnswer.OthersBrand",
                "MsnJVDataAnswer:UX_53:HouseV2.RealEstate",
                "MsnJVDataAnswer:UX_53:Image.ImageAnswer_beauty",
                "MsnJVDataAnswer:UX_53:Movie.HotMovieListing2",
                "MsnJVDataAnswer:UX_53:Travel.CtripTicket",
                "MsnJVDataAnswer:Mobile_Recipe:Recipe.Mobile_Meishijie",
                "MsnJVDataAnswer:UX_53:Image.ImageAnswer_image",
                "MsnJVDataAnswer:UX_53:Tools.Horoscope",
                "MsnJVDataAnswer:UX_53:Image.ImageAnswer_cartoon",
                "MsnJVDataAnswerV2:ModuleList:BKRichFactPC.RichFactAnswer",
            };

            lgConfig.Config.Blocklist = blocklist.Select(x => x.Trim().ToLowerInvariant()).ToList();

            return new BondedGeneralResponse { BondedResponse = new Bonded<GeneralResponse>(lgConfig) };
        }
    }
}
