using System;
using System.Linq;
using Orchard.Data;
using Orchard.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Models;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.State;

namespace Orchard.Recipes.Services {
    public class RecipeManager : Component, IRecipeManager {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;
        private readonly IRecipeExecuteEventHandler _recipeExecuteEventHandler;
        private readonly IContentStorageManager _contentStorageManager;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeManager(
            IRecipeStepQueue recipeStepQueue,
            IRecipeScheduler recipeScheduler,
            IRecipeExecuteEventHandler recipeExecuteEventHandler,
            IContentStorageManager contentStorageManager,
            ILoggerFactory loggerFactory) : base(loggerFactory) {

            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _recipeExecuteEventHandler = recipeExecuteEventHandler;
            _contentStorageManager = contentStorageManager;
        }

        public string Execute(Recipe recipe) {
            if (recipe == null) {
                throw new ArgumentNullException("recipe");
            }

            if (!recipe.RecipeSteps.Any()) {
                Logger.LogInformation("Recipe '{0}' contains no steps. No work has been scheduled.");
                return null;
            }

            var executionId = Guid.NewGuid().ToString("n");

            _executionIds.SetState(executionId);
            
            try {
                Logger.LogInformation("Executing recipe '{0}'.", recipe.Name);
                _recipeExecuteEventHandler.ExecutionStart(executionId, recipe);

                foreach (var recipeStep in recipe.RecipeSteps) {
                    _recipeStepQueue.Enqueue(executionId, recipeStep);
                    _contentStorageManager.Store(new RecipeStepResultRecord {
                        ExecutionId = executionId,
                        RecipeName = recipe.Name,
                        StepId = recipeStep.Id,
                        StepName = recipeStep.Name
                    });
                }
                _recipeScheduler.ScheduleWork(executionId);

                return executionId;
            }
            finally {
                _executionIds.SetState(null);
            }
        }
    }
}