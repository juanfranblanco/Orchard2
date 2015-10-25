﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Recipes.Models;
using Orchard.Recipes.ViewModels;
using Orchard.Environment.Recipes.Services;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Recipes.Models;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Data;

namespace Orchard.Recipes.Providers.Builders {
    public class ContentStep : RecipeBuilderStep {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionWriter _contentDefinitionWriter;
        private readonly IContentExporter _contentExporter;
        private readonly IContentItemStore _contentItemStore;

        public ContentStep(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IContentDefinitionWriter contentDefinitionWriter,
            IContentExporter contentExporter,
            IContentItemStore contentItemStore,
            ILoggerFactory loggerFactory) : base(loggerFactory) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _contentDefinitionWriter = contentDefinitionWriter;
            _contentExporter = contentExporter;
            _contentItemStore = contentItemStore;

            VersionHistoryOptions = VersionHistoryOptions.Published;
            SchemaContentTypes = new List<string>();
            DataContentTypes = new List<string>();
        }

        public override string Name {
            get { return "Content"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Content and Content Definition"); }
        }

        public override LocalizedString Description {
            get { return T("Exports content items and content item definitions."); }
        }

        public override int Priority { get { return 20; } }
        public override int Position { get { return 20; } }

        public IList<string> SchemaContentTypes { get; set; }
        public IList<string> DataContentTypes { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var contentTypeViewModels = _contentDefinitionManager.ListTypeDefinitions()
                .OrderBy(x => x.Name)
                .Select(x => new ContentTypeEntry { Name = x.Name, DisplayName = x.DisplayName })
                .ToList();

            var viewModel = new ContentBuilderStepViewModel {
                ContentTypes = contentTypeViewModels
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                DataContentTypes = viewModel.ContentTypes.Where(x => x.ExportData).Select(x => x.Name).ToList();
                SchemaContentTypes = viewModel.ContentTypes.Where(x => x.ExportSchema).Select(x => x.Name).ToList();
                VersionHistoryOptions = viewModel.VersionHistoryOptions;
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/Content", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(RecipeBuilderStepConfigurationContext context) {
            var schemaContentTypeNames = context.ConfigurationElement.Attr("SchemaContentTypes");
            var dataContentTypeNames = context.ConfigurationElement.Attr("DataContentTypes");
            var versionHistoryOptions = context.ConfigurationElement.Attr<VersionHistoryOptions>("VersionHistoryOptions");

            if (!string.IsNullOrWhiteSpace(schemaContentTypeNames))
                SchemaContentTypes = schemaContentTypeNames.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!string.IsNullOrWhiteSpace(dataContentTypeNames))
                DataContentTypes = dataContentTypeNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            VersionHistoryOptions = versionHistoryOptions;
        }

        public override void ConfigureDefault() {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(x => x.Name).ToList();
            SchemaContentTypes = DataContentTypes = contentTypeDefinitions.Select(x => x.Name).ToList();
            VersionHistoryOptions = VersionHistoryOptions.Published;
        }

        public override void Build(BuildContext context) {
            var dataContentTypes = DataContentTypes;
            var schemaContentTypes = SchemaContentTypes;
            var exportVersionOptions = GetContentExportVersionOptions(VersionHistoryOptions);
            var contentItems = dataContentTypes.Any()
                ? _contentItemStore.GetMany(exportVersionOptions, x => dataContentTypes.Contains(x.ContentItemRecord.ContentType.Name))
                : Enumerable.Empty<ContentItem>();

            if(schemaContentTypes.Any())
                context.RecipeDocument.Element("Orchard").Add(ExportMetadata(schemaContentTypes));

            if(contentItems.Any())
                context.RecipeDocument.Element("Orchard").Add(ExportData(dataContentTypes, contentItems));
        }

        private XElement ExportMetadata(IEnumerable<string> contentTypes) {
            var typesElement = new XElement("Types");
            var partsElement = new XElement("Parts");
            var typesToExport = _contentDefinitionManager.ListTypeDefinitions()
                .Where(typeDefinition => contentTypes.Contains(typeDefinition.Name))
                .ToList();
            var partsToExport = new Dictionary<string, ContentPartDefinition>();

            foreach (var contentTypeDefinition in typesToExport.OrderBy(x => x.Name)) {
                foreach (var contentPartDefinition in contentTypeDefinition.Parts.OrderBy(x => x.PartDefinition.Name)) {
                    if (partsToExport.ContainsKey(contentPartDefinition.PartDefinition.Name)) {
                        continue;
                    }
                    partsToExport.Add(contentPartDefinition.PartDefinition.Name, contentPartDefinition.PartDefinition);
                }
                typesElement.Add(_contentDefinitionWriter.Export(contentTypeDefinition));
            }

            foreach (var part in partsToExport.Values.OrderBy(x => x.Name)) {
                partsElement.Add(_contentDefinitionWriter.Export(part));
            }

            return new XElement("ContentDefinition", typesElement, partsElement);
        }

        private XElement ExportData(IEnumerable<string> contentTypes, IEnumerable<ContentItem> contentItems) {
            var data = new XElement("Content");
            
            var orderedContentItemsQuery =
                from contentItem in contentItems
                let identity = _contentManager.GetItemMetadata(contentItem).Identity.ToString()
                orderby identity
                select contentItem;

            var orderedContentItems = orderedContentItemsQuery.ToList();

            foreach (var contentType in contentTypes.OrderBy(x => x)) {
                var type = contentType;
                var items = orderedContentItems.Where(i => i.ContentType == type);
                foreach (var contentItem in items) {
                    var contentItemElement = ExportContentItem(contentItem);
                    if (contentItemElement != null)
                        data.Add(contentItemElement);
                }
            }

            return data;
        }

        private VersionOptions GetContentExportVersionOptions(VersionHistoryOptions versionHistoryOptions) {
            switch (versionHistoryOptions) {
                case VersionHistoryOptions.Draft:
                    return VersionOptions.Draft;
                case VersionHistoryOptions.Latest:
                    return VersionOptions.Latest;
                case VersionHistoryOptions.Published:
                default:
                    return VersionOptions.Published;
            }
        }

        private XElement ExportContentItem(ContentItem contentItem) {
            return _contentExporter.Export(contentItem);
        }
    }
}