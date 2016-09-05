//---------------------------------------------------------------------
// <copyright file="GenericAnswerPostProcessor.cs" company="Microsoft">
//      Copyright 2016 (c) Microsoft Corporation.
// </copyright>
//--------------------------------------------------------------------

namespace Halsey.AE.C3BToC3A.Plugins
{
    using APlus;
    using Bing.Platform.CU;
    using Halsey.AE.C3BToC3A;
    using Kif2Bond.Halsey.Profile;
    using Microsoft.Search.Kif;
    using Platform;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Xap.AnswersWireFormat;
    using Xap.PluginFramework;

    public class GenericAnswerPostProcessor : IPlugin
    {
        private IKifRepository kifRepository = null;
        private PluginServices pluginServices = null;

        private IKifRepository KifRepository
        {
            get { return this.kifRepository ?? (this.kifRepository = new FileKifRepository()); }
        }

        public PluginResult Execute(PluginServices pluginServices,
                    AnswerSelectionConfig_2 osAnswerSelectionConfig,
                    AnswerSelectionConfig_2 answerSelectionConfig,
                    IEnumerable<LegacyQueryResponseData> inputAnswers,
                    IEnumerable<Kif2Bond.Halsey.Profile.Interest_3> interests,
                    Platform.ExpressionResult enableInputAnswerMetadata,
                    PluginOutput<LegacyQueryResponseData> outputAnswers,
                    PluginOutput<AnnotationsArray> outputAnnotationsArray,
                    PluginOutput<StringData> outputInferenceSignal)
        { 
            this.pluginServices = pluginServices;

            var enableAnswerMetadata = enableInputAnswerMetadata == null ? false : enableInputAnswerMetadata.Expression;

            var flattenedInputAnswers = new List<AnswersData>();
            foreach (var item in inputAnswers)
            {
                if (!Util.IsAqrNotEmpty(item))
                {
                    pluginServices.Logger.Error("Invalid [item] in [inputAnswers], ignored");
                    continue;
                }

                flattenedInputAnswers.AddRange(item.LegacyAqr.ListAnswers.Elements);
            }

            if (flattenedInputAnswers.Count == 0)
            {
                pluginServices.Logger.Info("[flattenedInputAnswers] is empty, no further execution");

                var ans = pluginServices.CreateInstance<AnswersData>();
                ans.ServiceName = "MSNJVDataAnswerV2";
                ans.AnswerScenario = "ModuleList";
                ans.AnswerFeed = "ADX.Cortana_BingScore";
                ans.IdInContext = 135;

                var base64 = "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bgAAAAMACgsKXWh0dHBzOi8vd3d3LmJpbmcuY29tL3RoP2lkPU9OLjExQ0VGQzA2MDU1OEU2NUEzRkU3REMzMDdCRTdERDM4JnBpZD1OZXdzJnN6PTQwMHg0MDAmbWt0PXpoLUNOAAYURAYeRAEhVgAAAAEADAxkIUwAAAABAA0LCkHkuIrmtbfooajlvbDlpaXov5DlgaXlhL8g5ZC05pWP6Zye6I6357uI6Lqr5oiQ5bCx5aWW5q+U6IKp5aea5piOAAIhYgAAAAEADg5kIVgAAAABAAwPACFOAAAAAgAPDgohQQAAAAIADBAAIR4AAAADABAGCg0LFA3ohb7orq/kvZPogrIABmQJASEYAAAAAwAQBgoNCxQH5Yia5YiaAAZkCQYUAgZuBg0CIQcAAAAAABEbS2lmLkFuc3dlclByb3ZpZGVyUmVzcG9uc2UAAQsVTXNuSlZEYXRhLk1vZHVsZUxpc3QAAQMTTXNuSlZEYXRhLk1ldGFEYXRhAAIDF01zbkpWRGF0YS5LZXlWYWx1ZVBhaXIAAQAWU3BlZWNoLkNoYXRTdWdnZXN0aW9uAAEBEU1zbkpWRGF0YS5Nb2R1bGUAAQEbTXNuSlZEYXRhLkdlbmVyaWNDb21wb25lbnQAAQEfTXNuSlZEYXRhLkxpbmtXcmFwcGVyQ29tcG9uZW50AAEAHU1zbkpWRGF0YS5Db250YWluZXJDb21wb25lbnQAAQMZTXNuSlZEYXRhLkltYWdlQ29tcG9uZW50AAEDEE1zbkpWRGF0YS5JbWFnZQADAhtNc25KVkRhdGEuU25pcHBldENvbXBvbmVudAABAhxNc25KVkRhdGEuU3VidGl0bGVDb21wb25lbnQAAQAXTXNuSlZEYXRhLlRleHRGcmFnbWVudAACARxNc25KVkRhdGEuRGF0YUxpc3RDb21wb25lbnQAAQkTTXNuSlZEYXRhLkl0ZW1MaXN0AAEJD01zbkpWRGF0YS5JdGVtAAEF";
                ans.KifResponseSegment = new ArraySegment<byte>(Convert.FromBase64String(base64));
                flattenedInputAnswers.Add(ans);
            }
            else
            {
                pluginServices.Logger.Info("Count of [flattenedInputAnswers] is {0}", flattenedInputAnswers.Count);
            }

            // For each main answer:
            //  1) generate LG answer from answer data or LG config
            //  2) generate Action answer from answer data
            //  3) create annotation for both 1) and 2)
            //  4) generate inference signal 
            foreach (var ans in flattenedInputAnswers)
            {               
                var answerMetadata = enableAnswerMetadata ? ExtractAnswerMetadata(ans) : null;
                var lgAnswer = enableAnswerMetadata ? CreateLgAnswer(answerMetadata) : CreateLgAnswer(ans, osAnswerSelectionConfig, answerSelectionConfig);
                var childAnswers = new List<AnswersData>();

                if(lgAnswer != null)
                {
                    AttachAnswerToAqr(outputAnswers, lgAnswer);
                    childAnswers.Add(lgAnswer);
                }

                if (enableAnswerMetadata)
                {
                    var actionAnswer = CreateActionAnswer(answerMetadata, interests);
                    if (actionAnswer != null)
                    {
                        AttachAnswerToAqr(outputAnswers, actionAnswer);
                        childAnswers.Add(actionAnswer);
                    }
                }

                UpdateAnnotationsArray(outputAnnotationsArray, ans, childAnswers);

                UpdateInferenceSignal(outputInferenceSignal, answerMetadata);

            } // end of foreach (var ans in flattenedInputAnswers)

            return PluginResult.Succeeded;
        }

        private AnswersData CreateLgAnswer(IReadOnlyDictionary<string, string> answerMetadata)
        {
            if (answerMetadata == null || !answerMetadata.Any())
            {
                return null;
            }

            string displayText, spokenText, spokenSsml;
            if (!answerMetadata.TryGetValue(StrPool.DisplayText, out displayText)
                || !answerMetadata.TryGetValue(StrPool.SpokenText, out spokenText)
                || !answerMetadata.TryGetValue(StrPool.SpokenSsml, out spokenSsml))
            {
                return null;
            }

            var lgObj = pluginServices.CreateInstance<LanguageGenerationResponse2_1>();
            lgObj.DisplayText = displayText;
            lgObj.SpokenText = spokenText;
            lgObj.SpokenSsml = spokenSsml;

            if (answerMetadata.ContainsKey(StrPool.SuggestionText))
            {
                lgObj.SuggestionText = answerMetadata[StrPool.SuggestionText];
            }

            pluginServices.Logger.Info("Create LG from Answer Data");

            return CreateLgAnswer(Util.ToXml(lgObj));

        }

        private AnswersData CreateLgAnswer(AnswersData mainAnswer,
            AnswerSelectionConfig_2 osConfig,
            AnswerSelectionConfig_2 localConfig)
        {
            if (mainAnswer == null)
            {
                return null;
            }

            var sig1 = mainAnswer.ServiceName.ToLowerInvariant();
            var sig2 = string.Concat(sig1, ":", mainAnswer.AnswerScenario.ToLowerInvariant());
            var sig3 = string.Concat(sig2, ":", mainAnswer.AnswerFeed.ToLowerInvariant());

            pluginServices.Logger.Info("sig1={0}, sig2={1}, sig3={2}", sig1, sig2, sig3);

            LGConfig_1 lgConfig = null;
            if (osConfig != null && osConfig.Enabled)
            {
                if (osConfig.Blocklist != null && (osConfig.Blocklist.Contains(sig3) || osConfig.Blocklist.Contains(sig2) || osConfig.Blocklist.Contains(sig1)))
                {
                    pluginServices.Logger.Info("LG Answer for {0} is blocked due to osConfig", sig3);
                    return null;
                }

                if (osConfig.LGConfig.TryGetValue(sig3, out lgConfig)
                    || osConfig.LGConfig.TryGetValue(sig2, out lgConfig)
                    || osConfig.LGConfig.TryGetValue(sig1, out lgConfig))
                {
                    pluginServices.Logger.Info("Find osLG config");
                }
                else if (osConfig.LGConfig.TryGetValue("default", out lgConfig))
                {
                    pluginServices.Logger.Info("Find osLG config with section 'default'");
                }
            }

            if (lgConfig == null && localConfig != null && localConfig.Enabled)
            {
                if (localConfig.Blocklist != null && (localConfig.Blocklist.Contains(sig3) || localConfig.Blocklist.Contains(sig2) || localConfig.Blocklist.Contains(sig1)))
                {
                    pluginServices.Logger.Info("LG Answer for {0} is blocked due to localConfig", sig3);
                    return null;
                }

                if (localConfig.LGConfig.TryGetValue(sig3, out lgConfig)
                    || localConfig.LGConfig.TryGetValue(sig2, out lgConfig)
                    || localConfig.LGConfig.TryGetValue(sig1, out lgConfig))
                {
                    pluginServices.Logger.Info("Find local LG config");
                }
                else if (localConfig.LGConfig.TryGetValue("default", out lgConfig))
                {
                    pluginServices.Logger.Info("Find local LG config with section 'default'");
                }
            }

            if (lgConfig == null)
            {
                pluginServices.Logger.Error("Create LG answer failed as [lgConfig] is null");
                return null;
            }

            return CreateLgAnswer(lgConfig);
        }

        private AnswersData CreateLgAnswer(LGConfig_1 lgConfig)
        {
            if (lgConfig == null)
            {
                return null;
            }

            var lgObj = pluginServices.CreateInstance<LanguageGenerationResponse2_1>();
            lgObj.DisplayText = lgConfig.DisplayText;
            lgObj.SpokenText = lgConfig.SpokenText;
            lgObj.SpokenSsml = lgConfig.SpokenSsml;

            return CreateLgAnswer(Util.ToXml(lgObj));
        }

        private AnswersData CreateLgAnswer(XElement lgXml)
        {
            try
            {
                var lgAns = pluginServices.CreateInstance<AnswersData>();
                lgAns.Initialize();
                lgAns.ServiceName = StrPool.AnswerServiceName_LG;
                lgAns.AnswerScenario = StrPool.AnswerScenarioName_LG;
                lgAns.AnswerFeed = StrPool.AnswerDataFeedName_LG;
                lgAns.IdInContext = (uint)pluginServices.LegacyShimRequestContext.GetNextLegacyAdoContextId();
                lgAns.UxDisplayHint = StrPool.GenericKif;
                lgAns.UxDataSchema = StrPool.GenericKif;

                Util.InjectAttribute(
                    lgXml,
                    StrPool.KifSchema,
                    StrPool.CU_LanguageGenerationResponse2_1_3);

                var lgNodeRewrapped = Util.WrapInAnswerProviderResponse(lgXml);

                using (var memStream = pluginServices.CreateInstance<MemoryStream>())
                {
                    XmlToKifConverter.Read(lgNodeRewrapped, memStream, this.KifRepository);
                    lgAns.KifResponseSegment = new ArraySegment<byte>(memStream.ToArray());
                }

                pluginServices.Logger.Info("Success to Create LG Answer");

                return lgAns;
            }
            #region Exception handling
            catch (KifException kifEx)
            {
                pluginServices.Logger.Error("KifException caught: {0}", kifEx.ToString());
            }
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
            }
            catch (ArgumentException argEx)
            {
                pluginServices.Logger.Error("ArgumentException caught: {0}", argEx.ToString());
            }

            return null;
            #endregion
        }

        private AnswersData CreateActionAnswer(Dictionary<string, string> answerMetadata,
            IEnumerable<Kif2Bond.Halsey.Profile.Interest_3> interests)
        {
            if (answerMetadata == null)
            {
                return null;
            }

            if (PreprocessActionAnswerData(answerMetadata, interests))
            {
                pluginServices.Logger.Info("Updated ActionAnswerData");
            }

            string actionId, providerId, url, method, protocolVersion, data;
            if (!answerMetadata.TryGetValue(StrPool.ActionID, out actionId) ||
                !answerMetadata.TryGetValue(StrPool.ActionProviderID, out providerId) ||
                !answerMetadata.TryGetValue(StrPool.URI, out url) ||
                !answerMetadata.TryGetValue(StrPool.ActionMethod, out method) ||
                !answerMetadata.TryGetValue(StrPool.ActionProtocolVersion, out protocolVersion) ||
                !answerMetadata.TryGetValue(StrPool.Data, out data))
            {
                return null;
            }

            try
            {
                var actionNode = XElement.Parse(string.Format(StrPool.ActionAnswerFormat, actionId, providerId, url, method, protocolVersion, data));
                var actionAnswer = pluginServices.CreateInstance<AnswersData>();
                actionAnswer.Initialize();
                actionAnswer.ServiceName = StrPool.AnswerServiceName_Action;
                actionAnswer.AnswerScenario = StrPool.AnswerScenarioName_Action;
                actionAnswer.AnswerFeed = StrPool.AnswerDataFeedName_Action;
                actionAnswer.IdInContext = (uint)pluginServices.LegacyShimRequestContext.GetNextLegacyAdoContextId();
                actionAnswer.UxDisplayHint = StrPool.GenericKif;
                actionAnswer.UxDataSchema = StrPool.GenericKif;

                Util.InjectAttribute(
                    actionNode,
                    StrPool.KifSchema,
                    StrPool.AppResult_ActionAnnotationResponse_1_1);

                var actionNodeRewrapped = Util.WrapInAnswerProviderResponse(actionNode);

                using (var memStream = pluginServices.CreateInstance<MemoryStream>())
                {
                    XmlToKifConverter.Read(actionNodeRewrapped, memStream, this.KifRepository);
                    actionAnswer.KifResponseSegment = new ArraySegment<byte>(memStream.ToArray());
                }

                pluginServices.Logger.Info("Succeed to Create Action Answer");

                return actionAnswer;
            }
            catch (KifException kifEx)
            {
                pluginServices.Logger.Error("KifException caught: {0}", kifEx.ToString());
            }
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
            }

            return null;
        }

        private bool PreprocessActionAnswerData(
           Dictionary<string, string> answerProperties,
           IEnumerable<Kif2Bond.Halsey.Profile.Interest_3> interests)
        {
            if (answerProperties == null || !answerProperties.ContainsKey(StrPool.Data))
            {
                return false;
            }

            try
            {
                var interest = MsnjvInterestExistOrNot(answerProperties[StrPool.Data], interests);
                return interest != null && UpdateActionAnswerData(StrPool.MSNJV, answerProperties, interest);
            }
            #region Exception handling
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
            }

            return false;
            #endregion
        }

        private bool UpdateActionAnswerData(
            string category,
            Dictionary<string, string> ansProperties,
            Kif2Bond.Halsey.Profile.Interest_3 interest)
        {
            if (ansProperties == null)
            {
                return false;
            }

            string actionIdNode, uriNode, dataNode;
            if (!ansProperties.TryGetValue(StrPool.ActionID, out actionIdNode)
                || !ansProperties.TryGetValue(StrPool.URI, out uriNode)
                || !ansProperties.TryGetValue(StrPool.Data, out dataNode))
            {
                return false;
            }

            try
            {
                var newDataValue = string.Empty;
                var newActionIdValue = string.Empty;

                if (interest.SelectedTrackers != null && interest.SelectedTrackers.Count > 0)
                {
                    switch (category)
                    {
                        case StrPool.MSNJV:
                            // For msnjv interest, it has only one SelectedTracker
                            var selectedTracker = interest.SelectedTrackers.First();

                            if (selectedTracker.Value.IsTrackerOn)
                            {
                                newDataValue = string.Format(CultureInfo.InvariantCulture, StrPool.UnTrackInterestFormat, interest.Id, selectedTracker.Value.TrackerId);
                                newActionIdValue = StrPool.MSNJV_untrackinterest;
                            }
                            else
                            {
                                newDataValue = string.Format(CultureInfo.InvariantCulture, StrPool.ReTrackInterestFormat, interest.Id, selectedTracker.Value.TrackerId);
                                newActionIdValue = actionIdNode.ToLower().Replace(StrPool.subscribe, StrPool.resubscribe);
                            }
                            break;

                        case StrPool.Flight:
                            var flightTracker = interest.SelectedTrackers.First(t =>
                                t != null &&
                                t.Value != null &&
                                StrPool.ShowFlightInfo_TrackerId.Equals(t.Value.TrackerId, StringComparison.InvariantCultureIgnoreCase));
                            if (flightTracker != null)
                            {
                                if (flightTracker.Value.IsTrackerOn)
                                {
                                    newDataValue = string.Format(CultureInfo.InvariantCulture, StrPool.UnTrackInterestFormat, interest.Id, StrPool.ShowFlightInfo_TrackerId);
                                    newActionIdValue = actionIdNode.Replace("trackinterest", "untrackinterest");
                                }
                                else
                                {
                                    newDataValue = string.Format(CultureInfo.InvariantCulture, StrPool.ReTrackInterestFormat, interest.Id, StrPool.ShowFlightInfo_TrackerId);
                                    newActionIdValue = actionIdNode.Replace("trackinterest", "retrackinterest");
                                }

                            }
                            break;
                    }

                    ansProperties[StrPool.ActionID] = newActionIdValue;
                    ansProperties[StrPool.URI] = string.Format(CultureInfo.InvariantCulture, StrPool.URIFormat, interest.Id);
                    ansProperties[StrPool.Data] = newDataValue;

                    pluginServices.Logger.Verbose("newActionIDValue :{0}, newURIVale: {1}, newDataValue: {2}", newActionIdValue, ansProperties[StrPool.URI], newDataValue);

                    return true;
                }
            }
            #region Exception handling
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
                return false;
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
                return false;
            }
            #endregion

            return false;
        }

        private Interest_3 MsnjvInterestExistOrNot(
           string actionData,
           IEnumerable<Kif2Bond.Halsey.Profile.Interest_3> msnjvInterests)
        {
            if (string.IsNullOrEmpty(actionData) || msnjvInterests == null || !msnjvInterests.Any())
            {
                return null;
            }
            try
            {
                var signatureFromData = Util.ExtractValueFromJson(actionData, StrPool.TriggerQuery);
                pluginServices.Logger.Verbose("Msnjv signatureFromData is {0}.", signatureFromData);

                foreach (var interestItem in msnjvInterests)
                {
                    if (interestItem.Properties != null && interestItem.Properties.Count > 0)
                    {
                        foreach (var prop in interestItem.Properties)
                        {
                            if (string.Equals(prop.Name, StrPool.TriggerQuery, StringComparison.InvariantCultureIgnoreCase))
                            {
                                var triggerQuery = prop.Value;

                                #region Debug Info

                                pluginServices.Logger.Verbose("TriggerQuery from interest Properties is {1}.", prop.Value, triggerQuery);

                                #endregion

                                if (string.Equals(signatureFromData, triggerQuery, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    pluginServices.Logger.Verbose("Find a matched msnjv interest, interest ID is {0}", interestItem.Id);
                                    return interestItem;
                                }

                                // Go to check next interest
                                break;
                            }
                        }
                    }
                }
            }
            #region Exception handling
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
            }
            catch (Newtonsoft.Json.JsonReaderException jsEx)
            {
                pluginServices.Logger.Error("JsonReaderException caught: {0}", jsEx.ToString());
            }
            #endregion

            return null;
        }

        private void AttachAnswerToAqr(PluginOutput<LegacyQueryResponseData> outputAqr,
            AnswersData answer)
        {
            if (outputAqr == null || answer == null)
            {
                return;
            }

            if (outputAqr.Data == null)
            {
                outputAqr.Data = pluginServices.CreateInstance<LegacyQueryResponseData>();
            }

            outputAqr.Data.LegacyAqr.ListAnswers.Elements.Add(answer);
        }

        private void UpdateAnnotationsArray(PluginOutput<AnnotationsArray> annotationsArray,
            AnswersData parent,
            IEnumerable<AnswersData> children)
        {
            if (annotationsArray == null || parent == null || children == null || !children.Any())
            {
                return;
            }

            if (annotationsArray.Data == null)
            {
                annotationsArray.Data = pluginServices.CreateInstance<AnnotationsArray>();
            }
            if (annotationsArray.Data.AnnotatedAnswers == null)
            {
                annotationsArray.Data.AnnotatedAnswers = pluginServices.CreateInstance<ICollection<Annotations>>();
            }

            var annotation = pluginServices.CreateInstance<Annotations>();
            annotation.RankingId = RankingType.KeepOrder;
            annotation.Annotee = MakeParticipatingAnswer(parent);
            annotation.Annotares = pluginServices.CreateInstance<ICollection<ParticipatingAnswer>>();
            foreach (var child in children)
            {
                annotation.Annotares.Add(MakeParticipatingAnswer(child));
            }

            annotationsArray.Data.AnnotatedAnswers.Add(annotation);

            pluginServices.Logger.Verbose("Succeed to update annotations array with child answer count: {0}", children.Count());
        }

        private ParticipatingAnswer MakeParticipatingAnswer(AnswersData ans)
        {
            var participatingAnswer = this.pluginServices.CreateInstance<ParticipatingAnswer>();
            participatingAnswer.ServiceName = ans.ServiceName;
            participatingAnswer.AnswerScenario = ans.AnswerScenario;
            participatingAnswer.AnswerId = ans.IdInContext;

            return participatingAnswer;
        }

        private void UpdateInferenceSignal(PluginOutput<StringData> inferenceSignal,
            Dictionary<string, string> ansProperties)
        {
            string base64Signal;
            if (ansProperties != null && ansProperties.TryGetValue(StrPool.InferenceKey, out base64Signal))
            {
                inferenceSignal.Data = pluginServices.CreateInstance<StringData>();
                inferenceSignal.Data.Value = base64Signal;

                pluginServices.Logger.Verbose("Succeed to update inference signal");
            }
        }

        private Dictionary<string, string> ExtractAnswerMetadata(AnswersData answerData)
        {
            if (answerData == null)
            {
                return null;
            }

            try
            {
                var buffer = answerData.KifResponseSegment.ToArray();
                var kifReader = KifBinaryReader.Create(buffer, 0, buffer.Length);
                var ansXml = KifToXmlConverter.Write(this.KifRepository, kifReader, true, false);
                var metadata = ansXml.Descendants(StrPool.AnswerMetadata).FirstOrDefault();
                return metadata == null ? null : metadata.Elements().Where(x => x.Element(StrPool.AnswerMetadataKey) != null && x.Element(StrPool.AnswerMetadataValue) != null).ToDictionary(x => x.Element(StrPool.AnswerMetadataKey).Value, x => x.Element(StrPool.AnswerMetadataValue).Value);
            }
            catch (KifException kifEx)
            {
                pluginServices.Logger.Error("KifException caught: {0}", kifEx.ToString());
            }
            catch (XmlException xmlEx)
            {
                pluginServices.Logger.Error("XmlException caught: {0}", xmlEx.ToString());
            }
            catch (NullReferenceException nullEx)
            {
                pluginServices.Logger.Error("NullReferenceException caught: {0}", nullEx.ToString());
            }

            return null;
        }
    }
}
