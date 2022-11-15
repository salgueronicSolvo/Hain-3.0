using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Core
{
    public class FactoryCallbacks
    {
        public static void Subscribe(ModelData data)
        {
            if (data.requestType == RequestType.Json)
            {
                data.actions.onFailure = FactoryCallbacks.OnJsonLoadFailure;
                data.actions.onFailureException = FactoryCallbacks.OnJsonLoadFailure;
            }
            else
            {
                data.actions.onFailure = FactoryCallbacks.OnFailure;
                data.actions.onFailureException = FactoryCallbacks.OnFailure;
            }

            data.actions.onSuccessfulStage = FactoryCallbacks.OnSuccessfulLoadingStage;
            data.actions.onSuccess = FactoryCallbacks.OnSuccess;
            data.actions.factoryDebug = FactoryCallbacks.OnFactoryDebug;
        }
        private static void OnFailure(ModelData data, Exception e, string message)
        {
            Debug.LogException(e);
            OnFailure(data,message);
        }
        private static void OnFailure(ModelData data, string message)
        {
            data.actions.onFailureUser?.Invoke();
            data.actions.onFailureUserParams?.Invoke(new CallbackInfo(data, message));
            Debug.LogWarning($"Failed to make {data.searchTerm}: {message}");
            Destroy.GameObject(data.model);
        }
        private static void OnJsonLoadFailure(ModelData data, Exception e, string message)
        {
            Debug.LogException(e);
            OnJsonLoadFailure(data, message);
        }
        private static void OnJsonLoadFailure(ModelData data, string message)
        {
            Debug.LogWarning($"Failed to make {data.searchTerm} from JSON: {message}, retrying via search");
            data.requestType = RequestType.Search;
            data.loadedData = new LoadedData();
            data.json = null;
            data.actions.onFailure = OnFailure;
            data.actions.onFailureException = OnFailure;
            data.actions.loadJsonDelegate?.Invoke(data);
        }
        private static void OnSuccess(ModelData data, string message = null)
        {
            data.actions.onSuccessUser?.Invoke();
            data.actions.onSuccessUserParams?.Invoke(new CallbackInfo(data, message));
            data.Debug(message);
            Destroy.MonoBehaviour(data.loadingScript);

        }
        private static void OnFactoryDebug(ModelData data, string message = null)
        {
            if (AnythingSettings.DebugEnabled)
            {
                Debug.Log($"Debug ({data.guid}): {message}", data?.model);
            }

        }
        private static void OnSuccessfulLoadingStage(ModelData data, string message)
        {
            Debug.Log($"{data.guid}:{message}");
        }

      
    }
}
