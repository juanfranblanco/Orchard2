using Orchard.Data;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Recipes.Models;
using System;
using System.Linq;

namespace Orchard.Recipes.Services {
    public class RecipeResultAccessor : IRecipeResultAccessor {
        private readonly IContentStorageManager _contentStorageManager;

        public RecipeResultAccessor(IContentStorageManager contentStorageManager) {
            _contentStorageManager = contentStorageManager;
        }

        public RecipeResult GetResult(string executionId) {
            var records = _contentStorageManager
                .Query<RecipeStepResultRecord>(x => x.ExecutionId == executionId)
                .ToArray();

            if (!records.Any())
                throw new Exception(string.Format("No records were found in the database for recipe execution ID {0}.", executionId));

            return new RecipeResult() {
                ExecutionId = executionId,
                Steps =
                    from record in records
                    select new RecipeStepResult {
                        RecipeName = record.RecipeName,
                        StepName = record.StepName,
                        IsCompleted = record.IsCompleted,
                        IsSuccessful = record.IsSuccessful,
                        ErrorMessage = record.ErrorMessage
                    }
            };
        }
    }
}