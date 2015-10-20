using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Services;
using Orchard.Hosting;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Recipes.Services;

namespace Orchard.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            new ShellModule().Configure(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();

            serviceCollection.AddScoped<IRecipeHarvester, RecipeHarvester>();
            serviceCollection.AddScoped<IRecipeParser, RecipeParser>();
        }
    }
}
