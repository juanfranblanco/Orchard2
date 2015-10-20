using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell;
using Orchard.Localization;
using Orchard.Setup.Services;
using Orchard.Setup.ViewModels;
using System;
using System.Linq;

namespace Orchard.Setup.Controllers {
    public class SetupController : Controller {
        private readonly ISetupService _setupService;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private const string DefaultRecipe = "Default";

        public SetupController(ISetupService setupService,
            ShellSettings shellSettings,
            ILoggerFactory loggerFactory) {
            _setupService = setupService;
            _shellSettings = shellSettings;
            _logger = loggerFactory.CreateLogger<SetupController>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private ActionResult IndexViewResult(SetupViewModel model) {
            return View(model);
        }

        public ActionResult Index() {
            var initialSettings = _setupService.Prime();
            var recipes = _setupService.Recipes().ToList();
            string recipeDescription = null;

            if (recipes.Any()) {
                recipeDescription = recipes[0].Description;
            }

            return IndexViewResult(new SetupViewModel {
                AdminUsername = "admin",
                Recipes = recipes,
                RecipeDescription = recipeDescription
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(SetupViewModel model) {
            var recipes = _setupService.Recipes().ToList();

            if (model.Recipe == null) {
                if (!(recipes.Select(r => r.Name).Contains(DefaultRecipe))) {
                    ModelState.AddModelError("Recipe", T("No recipes were found."));
                }
                else {
                    model.Recipe = DefaultRecipe;
                }
            }
            if (!ModelState.IsValid) {
                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }

                return IndexViewResult(model);
            }

            try {
                var recipe = _setupService.Recipes().GetRecipeByName(model.Recipe);

                var setupContext = new SetupContext {
                    SiteName = model.SiteName,
                    AdminUsername = model.AdminUsername,
                    AdminPassword = model.AdminPassword,
                    EnabledFeatures = null, // Default list
                    Recipe = recipe
                };

                var executionId = _setupService.Setup(setupContext);

                // redirect to the welcome page.
                return Redirect("~/" + _shellSettings.RequestUrlPrefix + "home/index");
            }
            catch (Exception ex) {
                _logger.LogError("Setup failed", ex);

                model.Recipes = recipes;
                foreach (var recipe in recipes.Where(recipe => recipe.Name == model.Recipe)) {
                    model.RecipeDescription = recipe.Description;
                }

                return IndexViewResult(model);
            }
        }
    }
}
