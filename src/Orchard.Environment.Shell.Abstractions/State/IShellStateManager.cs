using Orchard.Environment.Shell.State.Models;

namespace Orchard.Environment.Shell.State {
    public interface IShellStateManager {
        ShellState GetShellState();
        void UpdateEnabledState(ShellFeatureState featureState, ShellFeatureState.State value);
        void UpdateInstalledState(ShellFeatureState featureState, ShellFeatureState.State value);
    }
}