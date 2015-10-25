using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records {
    public class ContentPartFieldDefinitionRecord : DocumentRecord {
        public virtual ContentFieldDefinitionRecord ContentFieldDefinitionRecord { get; set; }
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
        [StringLengthMax]
        public string Settings {
            get { return this.RetrieveValue(x => x.Settings); }
            set { this.StoreValue(x => x.Settings, value); }
        }
    }
}