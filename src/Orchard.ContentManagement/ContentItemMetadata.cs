namespace Orchard.ContentManagement {
    public class ContentItemMetadata {
        public ContentItemMetadata() {
            Identity = new ContentIdentity();
        }
        public string DisplayText { get; set; }
        public ContentIdentity Identity { get; set; }
    }
}
