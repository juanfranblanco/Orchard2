using System.Linq;
using Orchard.Core.Settings.State.Records;
using Orchard.Data;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.State.Models;
using Orchard.Environment.Shell.State;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orchard.Core.Settings.State {
    public class ShellStateManager : Component, IShellStateManager {
        private readonly IContentStorageManager _contentStorageManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public ShellStateManager(
            IContentStorageManager contentStorageManager,
            IShellDescriptorManager shellDescriptorManager,
            ILoggerFactory loggerFactory) : base(loggerFactory) {
            _contentStorageManager = contentStorageManager;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public ShellState GetShellState() {
            var stateRecord = _contentStorageManager.Query<ShellStateRecord>(x => x != null).FirstOrDefault() ?? new ShellStateRecord();
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            var extraFeatures = descriptor == null ? Enumerable.Empty<string>() : descriptor.Features
                .Select(r => r.Name)
                .Except(stateRecord.Features.Select(r => r.Name));

            return new ShellState {
                Features = stateRecord.Features
                    .Select(featureStateRecord => new ShellFeatureState {
                        Name = featureStateRecord.Name,
                        EnableState = featureStateRecord.EnableState,
                        InstallState = featureStateRecord.InstallState
                    })
                    .Concat(extraFeatures.Select(name => new ShellFeatureState {
                        Name = name
                    }))
                    .ToArray(),
            };
        }

        private ShellFeatureStateRecord FeatureRecord(string name) {
            var stateRecord = _contentStorageManager.Query<ShellStateRecord>(x => x != null).FirstOrDefault() ?? new ShellStateRecord();
            var record = stateRecord.Features.SingleOrDefault(x => x.Name == name);
            if (record == null) {
                record = new ShellFeatureStateRecord { Name = name };
                stateRecord.Features.Add(record);
            }
            if (stateRecord.Id == 0) {
                _contentStorageManager.Store(stateRecord);
            }
            return record;
        }

        public void UpdateEnabledState(ShellFeatureState featureState, ShellFeatureState.State value) {
            Logger.LogDebug("Feature {0} EnableState changed from {1} to {2}",
                         featureState.Name, featureState.EnableState, value);

            var featureStateRecord = FeatureRecord(featureState.Name);
            if (featureStateRecord.EnableState != featureState.EnableState) {
                Logger.LogWarning("Feature {0} prior EnableState was {1} when {2} was expected",
                               featureState.Name, featureStateRecord.EnableState, featureState.EnableState);
            }
            featureStateRecord.EnableState = value;
            featureState.EnableState = value;
        }


        public void UpdateInstalledState(ShellFeatureState featureState, ShellFeatureState.State value) {
            Logger.LogDebug("Feature {0} InstallState changed from {1} to {2}",
                         featureState.Name, featureState.InstallState, value);

            var featureStateRecord = FeatureRecord(featureState.Name);
            if (featureStateRecord.InstallState != featureState.InstallState) {
                Logger.LogWarning("Feature {0} prior InstallState was {1} when {2} was expected",
                               featureState.Name, featureStateRecord.InstallState, featureState.InstallState);
            }
            featureStateRecord.InstallState = value;
            featureState.InstallState = value;
        }
    }

}