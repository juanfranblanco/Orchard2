using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Shell;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        ShellSettings Prime();
        Task<IEnumerable<Recipe>> Recipes();
        Task<string> Setup(SetupContext context);
    }
}