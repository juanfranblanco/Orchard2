using Orchard.Environment.Recipes.Models;
using Orchard.Setup.Annotations;
using System.Collections.Generic;

namespace Orchard.Setup.ViewModels {
    public class SetupViewModel {
        [SiteNameValid(maximumLength: 70)]
        public string SiteName { get; set; }
        [UserNameValid(minimumLength: 3, maximumLength: 25)]
        public string AdminUsername { get; set; }
        [PasswordValid(minimumLength: 7, maximumLength: 50)]
        public string AdminPassword { get; set; }

        public IEnumerable<Recipe> Recipes { get; set; }
        public string Recipe { get; set; }
        public string RecipeDescription { get; set; }
    }
}
