using Orchard.Events;

namespace Orchard.Environment.Shell.State {
    public interface IShellStateManagerEventHandler : IEventHandler {
        void ApplyChanges();
    }
}