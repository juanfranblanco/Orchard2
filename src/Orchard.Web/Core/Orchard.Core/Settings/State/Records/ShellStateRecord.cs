using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Orchard.Core.Settings.State.Records {
    public class ShellStateRecord : DocumentRecord {
        public ShellStateRecord() {
            Features = new List<ShellFeatureStateRecord>();
        }

        /// <summary>
        /// Workaround SqlCe: There is apparently no way to insert a row in a table with a single column IDENTITY ID primary key.
        /// See: http://www.sqldev.org/transactsql/insert-only-identity-column-value-in-sql-compact-edition-95267.shtml
        /// So we added this "Unused" column.
        ///  </summary>
        public string Unused {
            get { return this.RetrieveValue(x => x.Unused); }
            set { this.StoreValue(x => x.Unused, value); }
        }

        //[CascadeAllDeleteOrphan]
        public virtual IList<ShellFeatureStateRecord> Features { get; set; }
    }
}