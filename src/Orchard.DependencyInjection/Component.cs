using Microsoft.Extensions.Logging;
using Orchard.Localization;

namespace Orchard.DependencyInjection {
    public abstract class Component : IDependency {
        protected Component(ILoggerFactory loggerFactory) {
            T = NullLocalizer.Instance;
            Logger = loggerFactory.CreateLogger(GetType().Name);
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
    }
}
