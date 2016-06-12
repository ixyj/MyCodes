//---------------------------------------------------------------------
// <copyright file="FakeQasResponse.cs" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
// </copyright>
//--------------------------------------------------------------------

namespace AutoSuggest.Plugins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Bond;
    using Platform;
    using QAS.Inmemory.QueryRepresentation;
    using Xap.PluginFramework;

    using AnalyzedQuery = QAS.Inmemory.QueryRepresentation.AnalyzedQuery;
    using QueryRepresentationResponse = QAS.Inmemory.QueryRepresentation.QueryRepresentationResponse;

    /// <summary>
    /// Not Yet Written.
    /// </summary>
    public sealed class FakeQasResponse : IPlugin
    {
        /// <summary>
        /// The Execute
        /// </summary>
        /// <param name="pluginServices">The pluginServices</param>
        /// <param name="query">The query</param>
        /// <param name="classifiers">The classifiers</param>
        /// <param name="domains">The domains</param>
        /// <param name="output">The output</param>
        /// <returns>The PluginResult</returns>
        public PluginResult Execute(
            PluginServices pluginServices,
            Platform.Query query,
            IEnumerable<StringData> classifiers,
            IEnumerable<StringData> domains,
            PluginOutput<QueryRepresentationResponse> output)
        {
            if (!classifiers.Any() || !domains.Any())
            {
                return PluginResult.Failed("Empty input");
            }

            output.Data = pluginServices.CreateInstance<QueryRepresentationResponse>();
            output.Data.Version = 1;

            output.Data.AnalyzedQueries = pluginServices.CreateInstance<IList<AnalyzedQuery>>();
            var analyzedQuery = pluginServices.CreateInstance<AnalyzedQuery>();

            output.Data.AnalyzedQueries.Add(analyzedQuery);
            analyzedQuery.Query = pluginServices.CreateInstance<QAS.Inmemory.QueryRepresentation.Query>();
            analyzedQuery.Query.RawQuery = query.RawQuery;
            analyzedQuery.Query.QueryContext = pluginServices.CreateInstance<QueryContext>();

            analyzedQuery.Domains = pluginServices.CreateInstance<IDictionary<string, Domain>>();
            foreach (var classifier in classifiers)
            {
                var domain = pluginServices.CreateInstance<Domain>();
                analyzedQuery.Domains.Add(classifier.Value, domain);
                domain.DomainClassification = pluginServices.CreateInstance<DomainClassification>();
                domain.DomainClassification.ConfidenceLevel = 1.0f;
                domain.DomainClassification.DomainClassificationlevel = 4;
                domain.QueryParses = pluginServices.CreateInstance<IList<QueryParse>>();
                var queryParse = pluginServices.CreateInstance<QueryParse>();
                domain.QueryParses.Add(queryParse);
                queryParse.ConfidenceLevel = 1.0f;
                queryParse.Entities = pluginServices.CreateInstance<IList<QueryEntity>>();
                var entity = pluginServices.CreateInstance<QueryEntity>();
                queryParse.Entities.Add(entity);
                entity.EntityName = "Entity";
                entity.Text = query.RawQuery;
                entity.QueryInformationItem = pluginServices.CreateInstance<QueryInformationItem>();
                entity.QueryInformationItem.MetadataItems = pluginServices.CreateInstance<IList<MetadataItem>>();
                foreach (var dom in domains)
                {
                    var metadata = pluginServices.CreateInstance<MetadataItem>();
                    entity.QueryInformationItem.MetadataItems.Add(metadata);
                    metadata.TypeName = string.Empty;
                    metadata.Value = dom.Value;
                }
            }
               
            return PluginResult.Succeeded;
        }
    }
}
