using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records {
    public class ContentTypePartDefinitionRecord : DocumentRecord {
        public virtual ContentPartDefinitionRecord ContentPartDefinitionRecord { get; set; }

        [StringLengthMax]
        public string Settings {
            get { return this.RetrieveValue(x => x.Settings); }
            set { this.StoreValue(x => x.Settings, value); }
        }
    }
}
