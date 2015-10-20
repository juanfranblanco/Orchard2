namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetContentItemMetadata(GetContentItemMetadataContext context);
    }
}
