using System;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Extensions.DependencyInjection;
using Orchard.FileSystem;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Builders;
using Orchard.Services;
using Orchard.Hosting.Services;
using Orchard.Environment.Shell.Configuration;
using Orchard.Environment.Shell.State;

namespace Orchard.Hosting {
    public static class HostServiceExtensions {
        public static IServiceCollection AddHost(
            this IServiceCollection services, Action<IServiceCollection> additionalDependencies) {
            services.AddSingleton<IClock, Clock>();
            services.AddSingleton<IOrchardLibraryManager, OrchardLibraryManager>();

            services.AddSingleton<IOrchardHost, DefaultOrchardHost>();
            {
                services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

                services.AddSingleton<IShellContextFactory, ShellContextFactory>();
                {
                    services.AddSingleton<ICompositionStrategy, CompositionStrategy>();

                    services.AddSingleton<IShellContainerFactory, ShellContainerFactory>();
                }
                services.AddSingleton<IProcessingEngine, DefaultProcessingEngine>();
            }
            services.AddSingleton<IRunningShellTable, RunningShellTable>();


            services.AddFileSystems();

            // Caching - Move out
            services.AddInstance<ICacheContextAccessor>(new CacheContextAccessor());
            services.AddSingleton<ICache, Cache>();

            additionalDependencies(services);
            
            services.AddTransient<IOrchardShellHost, DefaultOrchardShellHost>();

            return services.AddFallback();
        }
    }
}