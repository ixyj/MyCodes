//---------------------------------------------------------------------
// <copyright file="AeAnswerResponseParser.cs" company="Microsoft">
//      Copyright 2015 (c) Microsoft Corporation.
// </copyright>
//--------------------------------------------------------------------

namespace AutoSuggest.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoSuggest.AggregatorLogic;
    using AutoSuggest.AggregatorLogic.Constants;
    using AutoSuggest.AggregatorLogic.ObjectModel;
    using AutoSuggest.AggregatorLogic.Ranking.PageZero;
    using AutoSuggest.Answers.Config;
    using AutoSuggest.Helpers;
    using Microsoft.Search.Kif;
    using Microsoft.Search.PageZero;
    using Platform;
    using QueryFormulation.Config.V1;
    using QueryFormulation.Config.V3;
    using QueryFormulation.V1;
    using QueryFormulation.V1.Queries;
    using QueryFormulation.V1.Suggestion;
    using QueryFormulation.V2;
    using QueryFormulation.V2.Suggestion;
    using Suggestions.Common;
    using Xap.AnswersWireFormat;
    using Xap.PluginFramework;

    /// <summary>
    /// The AEAnswer response parser 
    /// </summary>
    public class AeAnswerResponseParser : IPlugin
    {
        /// <summary>
        /// The executor method
        /// </summary>
        /// <param name="pluginServices">The plugin services</param>
        /// <param name="query">The query for the AE answer</param>
        /// <param name="queryForMsnjv">The query for the msnJvData answer</param>
        /// <param name="answerResponses">The AE answer response list</param>
        /// <param name="answersConfig">The answers config</param>
        /// <param name="triggerConfig">The trigger config</param>
        /// <param name="config">The aggregator config</param>
        /// <param name="outputSuggestionList">The output querySuggestionList</param>
        /// <param name="outputAqr">The output AE answer response</param>
        /// <returns>The plugin result</returns>
        public PluginResult Execute(
            PluginServices pluginServices,
            Query query,
            Query queryForMsnjv,
            IEnumerable<LegacyQueryResponseData> answerResponses,
            AnswersConfig answersConfig,
            ConditionalTriggerConfig triggerConfig,
            AutoSuggestAggregatorConfig config,
            PluginOutput<QuerySuggestionList> outputSuggestionList,
            PluginOutput<LegacyQueryResponseData> outputAqr)
        {
        }

        /// <summary>
        /// Hack answer response
        /// </summary>
        /// <param name="pluginServices">The pluginServices</param>
        /// <param name="answerData">the answer data</param>
        /// <returns>The answer response</returns>
        private static LegacyQueryResponseData Hack(PluginServices pluginServices, out AnswersData answerData)
        {
            answerData = null;
            try
            {
                answerData = pluginServices.CreateInstance<AnswersData>();
                answerData.Initialize();
                answerData.ServiceName = "IceWeatherAnswer";
                answerData.AnswerScenario = "WeatherSummary";
                answerData.AnswerFeed = "WeatherTest.Tianqi_QF";
                answerData.IdInContext = (uint)pluginServices.LegacyShimRequestContext.GetNextLegacyAdoContextId();
                answerData.UxDisplayHint = "GenericKif";
                answerData.UxDataSchema = "GenericKif";
                var base64 = "S0lGMQxCBgAAqAQAAAEAAA0BIZYEAAABAAwAIY0EAAACAAEMASEiAAAABAACCQG6nR9CCQKn0OhCAgvABwseB+WMl+S6rAAMAiFfBAAACAADDwEFDAshQwAAAAkABAkBAACgQQkGAACgQQELAA8MKg8NAAkVAACAPwIWAAIaggEMMyEXAAAAAgAFCAGAkfH44avx6AECAsAHDgwhbwMAAAUADAYAIa0AAAAFAAYMASEXAAAAAgAFCAGAgPDIt5jx6AECAsAHDwIBDAshVQAAAAgABwkBAADIQQkCAACQQQELAA8MCQkVAACAPwEfAAwzIRcAAAACAAUIAYDcg8y0nfHoAQICwAcMNCEXAAAAAgAFCAGA9PHji63x6AECAsAHDBUhFwAAAAQACAELAA8MFg8NAAkVAACAPwwWIRcAAAAEAAgBCwAPDAkPDQAJFQAAgD8BIa0AAAAFAAYMASEXAAAAAgAFCAGAgJecyrHx6AECAsAHDwICDAshVQAAAAgABwkBAADIQQkCAACQQQELAA8MFgkVAACAPwEfAAwzIRcAAAACAAUIAYDcqp/HtvHoAQICwAcMNCEXAAAAAgAFCAGAgKbVoMbx6AECAsAHDBUhFwAAAAQACAELAA8MFg8NAAkVAACAPwwWIRcAAAAEAAgBCwAPDBIPDQAJFQAAgD8CIa0AAAAFAAYMASEXAAAAAgAFCAGAgL7v3Mrx6AECAsAHDwIDDAshVQAAAAgABwkBAADoQQkCAACIQQELAA8MLAkVAABAQAEfAAwzIRcAAAACAAUIAYDc0fLZz/HoAQICwAcMNCEXAAAAAgAFCAGAgM2os9/x6AECAsAHDBUhFwAAAAQACAELAA8MLA8NAAkVAABAQAwWIRcAAAAEAAgBCwAPDC4PDQAJFQAAQEADIa0AAAAFAAYMASEXAAAAAgAFCAGAgOXC7+Px6AECAsAHDwIEDAshVQAAAAgABwkBAAAAQgkCAACgQQELAA8MLAkVAACAPwEfAAwzIRcAAAACAAUIAYDc+MXs6PHoAQICwAcMNCEXAAAAAgAFCAGAjIGayPjx6AECAsAHDBUhFwAAAAQACAELAA8MLA8NAAkVAACAPwwWIRcAAAAEAAgBCwAPDCwPDQAJFQAAgD8EIa0AAAAFAAYMASEXAAAAAgAFCAGAgIyWgv3x6AECAsAHDwIFDAshVQAAAAgABwkBAAAAQgkCAACoQQELAA8MLAkVAACAPwEfAAwzIRcAAAACAAUIAYDcn5n/gfLoAQICwAcMNCEXAAAAAgAFCAGAjKjt2pHy6AECAsAHDBUhFwAAAAQACAELAA8MLA8NAAkVAACAPwwWIRcAAAAEAAgBCwAPDCoPDQAJFQAAgD8ODSEdAAAAAQAMCQAhEwAAAAEACQ4LIQkAAAAAAAwKDg4hXQAAAAEADAsAIVMAAAAEAAsMASEXAAAAAgAFCAGA1OSZlqPx6AECAsAHDAIhFwAAAAIABQgBgNSL7ai88egBAgLABw8DMgsLE+mbt+eUteiTneiJsumihOitpgAPFQIPFgEMHyEYAAAAAwAMCQG6nR9CCQKn0OhCAgvABw0bS2lmLkFuc3dlclByb3ZpZGVyUmVzcG9uc2UAAQAfV2VhdGhlci5TZWFyY2guU3VtbWFyeVJlc3BvbnNlAAEFH1dlYXRoZXIuU2VhcmNoLlJlcXVlc3RMb2NhdGlvbgABARtXZWF0aGVyLlNlYXJjaC5XZWF0aGVyRGF0YQABBCBXZWF0aGVyLlNlYXJjaC5DdXJyZW50Q29uZGl0aW9uAAECFFdlYXRoZXIuU2VhcmNoLlRpbWUAAQAeV2VhdGhlci5TZWFyY2guRGFpbHlDb25kaXRpb24AAQEcV2VhdGhlci5TZWFyY2guRGF5Q29uZGl0aW9uAAEBIFdlYXRoZXIuU2VhcmNoLldlYXRoZXJDb25kaXRpb24AAQEfV2VhdGhlci5TZWFyY2guSG91cmx5Q29uZGl0aW9uAAEBHVdlYXRoZXIuU2VhcmNoLkhvdXJDb25kaXRpb24AAQEVV2VhdGhlci5TZWFyY2guQWxlcnQAAQAfV2VhdGhlci5TZWFyY2guU3RhdGlvbkxvY2F0aW9uAAEA";
                answerData.KifResponseSegment = new ArraySegment<byte>(Convert.FromBase64String(base64));
            }
            catch
            {
            }
              
            var output = pluginServices.CreateInstance<LegacyQueryResponseData>();
            output.LegacyAqr.ListAnswers.Elements.Add(answerData);

            return output;
        }
    }
}
