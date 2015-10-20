using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Data;
using Orchard.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Recipes.Services {
    public class RecipeStepExecutor : Component, IRecipeStepExecutor {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IEnumerable<IRecipeHandler> _recipeHandlers;
        private readonly IRecipeExecuteEventHandler _recipeExecuteEventHandler;
        private readonly IContentStorageManager _contentStorageManager;

        public RecipeStepExecutor(
            IRecipeStepQueue recipeStepQueue,
            IEnumerable<IRecipeHandler> recipeHandlers,
            IRecipeExecuteEventHandler recipeExecuteEventHandler,
            IContentStorageManager contentStorageManager,
            ILoggerFactory loggerFactory) : base(loggerFactory) {

            _recipeStepQueue = recipeStepQueue;
            _recipeHandlers = recipeHandlers;
            _recipeExecuteEventHandler = recipeExecuteEventHandler;
            _contentStorageManager = contentStorageManager;
        }

        public bool ExecuteNextStep(string executionId) {
            var nextRecipeStep = _recipeStepQueue.Dequeue(executionId);
            if (nextRecipeStep == null) {
                Logger.LogInformation("No more recipe steps left to execute.");
                _recipeExecuteEventHandler.ExecutionComplete(executionId);
                return false;
            }

            Logger.LogInformation("Executing recipe step '{0}'.", nextRecipeStep.Name);

            var recipeContext = new RecipeContext { RecipeStep = nextRecipeStep, Executed = false, ExecutionId = executionId };

            try {
                _recipeExecuteEventHandler.RecipeStepExecuting(executionId, recipeContext);

                foreach (var recipeHandler in _recipeHandlers) {
                    recipeHandler.ExecuteRecipeStep(recipeContext);
                }

                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, nextRecipeStep.Name, isSuccessful: true);
                _recipeExecuteEventHandler.RecipeStepExecuted(executionId, recipeContext);
            }
            catch (Exception ex) {
                UpdateStepResultRecord(executionId, nextRecipeStep.RecipeName, nextRecipeStep.Id, nextRecipeStep.Name, isSuccessful: false, errorMessage: ex.Message);
                Logger.LogError(string.Format("Recipe execution failed because the step '{0}' failed.", nextRecipeStep.Name), ex);
                while (_recipeStepQueue.Dequeue(executionId) != null);
                var message = T("Recipe execution with ID {0} failed because the step '{1}' failed to execute. The following exception was thrown:\n{2}\nRefer to the error logs for more information.", executionId, nextRecipeStep.Name, ex.Message);
                throw new OrchardCoreException(message);
            }

            if (!recipeContext.Executed) {
                Logger.LogError("Recipe execution failed because no matching handler for recipe step '{0}' was found.", recipeContext.RecipeStep.Name);
                while (_recipeStepQueue.Dequeue(executionId) != null);
                var message = T("Recipe execution with ID {0} failed because no matching handler for recipe step '{1}' was found. Refer to the error logs for more information.", executionId, nextRecipeStep.Name);
                throw new OrchardCoreException(message);
            }

            return true;
        }

        private void UpdateStepResultRecord(string executionId, string recipeName, string stepId, string stepName, bool isSuccessful, string errorMessage = null) {
            var query =
                from record in _contentStorageManager.Query<RecipeStepResultRecord>(record =>
                    record.ExecutionId == executionId && record.StepId == stepId && record.StepName == stepName)
                select record;

            if (!string.IsNullOrWhiteSpace(recipeName)) {
                query = from record in query where record.RecipeName == recipeName select record;
            }

            var stepResultRecord = query.SingleOrDefault();

            if (stepResultRecord == null)
                // No step result record was created when scheduling the step, so simply ignore.
                // The only reason where one would not create such a record would be Setup,
                // when no database exists to store the record but still wants to schedule a recipe step (such as the "StopViewsBackgroundCompilationStep").
                return; 

            stepResultRecord.IsCompleted = true;
            stepResultRecord.IsSuccessful = isSuccessful;
            stepResultRecord.ErrorMessage = errorMessage;

            _contentStorageManager.Store(stepResultRecord);
        }
    }
}