namespace Orchard.ContentManagement.Handlers {
    public abstract class TemplateFilterBase<TPart> : IContentTemplateFilter where TPart : class, IContent {
        protected virtual void GetContentItemMetadata(GetContentItemMetadataContext context, TPart instance) { }

        void IContentTemplateFilter.GetContentItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.Is<TPart>())
                GetContentItemMetadata(context, context.ContentItem.As<TPart>());
        }
    }
}
