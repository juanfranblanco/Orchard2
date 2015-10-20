using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Recipes.Models {
    public class RecipeStepResultRecord : DocumentRecord {
        public string ExecutionId {
            get { return this.RetrieveValue(x => x.ExecutionId); }
            set { this.StoreValue(x => x.ExecutionId, value); }
        }
        public string RecipeName {
            get { return this.RetrieveValue(x => x.RecipeName); }
            set { this.StoreValue(x => x.RecipeName, value); }
        }
        public string StepId {
            get { return this.RetrieveValue(x => x.StepId); }
            set { this.StoreValue(x => x.StepId, value); }
        }
        public string StepName {
            get { return this.RetrieveValue(x => x.StepName); }
            set { this.StoreValue(x => x.StepName, value); }
        }
        public bool IsCompleted {
            get { return this.RetrieveValue(x => x.IsCompleted); }
            set { this.StoreValue(x => x.IsCompleted, value); }
        }
        public bool IsSuccessful {
            get { return this.RetrieveValue(x => x.IsSuccessful); }
            set { this.StoreValue(x => x.IsSuccessful, value); }
        }
        public string ErrorMessage {
            get { return this.RetrieveValue(x => x.ErrorMessage); }
            set { this.StoreValue(x => x.ErrorMessage, value); }
        }
    }
}