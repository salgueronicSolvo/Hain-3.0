using System;
namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class ModelActions
    {
        public Action<ModelData> startPipeline;
        public Action<ModelData> loadJsonDelegate;
        public Action<ModelData> processJsonDelegate;
        public Action<ModelData> loadModelDelegate;
        public Action<ModelData> loadAnimationDelegate;
        public Action<ModelData> addBehavioursDelegate;
        public Action<ModelData, string> factoryDebug;

        public Action<ModelData, string> onSuccessfulStage;
        public Action<ModelData, string> onSuccess;
        public Action<ModelData, string> onFailure;
        public Action<ModelData, Exception, string> onFailureException;
        public Action onSuccessUser;
        public Action onFailureUser;
        public Action<CallbackInfo> onSuccessUserParams;
        public Action<CallbackInfo> onFailureUserParams;
    }
}
