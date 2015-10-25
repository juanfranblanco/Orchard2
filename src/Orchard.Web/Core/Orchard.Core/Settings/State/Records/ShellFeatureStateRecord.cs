using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Shell.State.Models;

namespace Orchard.Core.Settings.State.Records {
    public class ShellFeatureStateRecord : DocumentRecord {
        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
        public ShellFeatureState.State InstallState {
            get { return this.RetrieveValue(x => x.InstallState); }
            set { this.StoreValue(x => x.InstallState, value); }
        }
        public ShellFeatureState.State EnableState {
            get { return this.RetrieveValue(x => x.EnableState); }
            set { this.StoreValue(x => x.EnableState, value); }
        }
    }
}
