using AnythingWorld.Utilities.Data;
using System;

namespace AnythingWorld.Core
{
    public static class UserCallbacks
    {
        /// <summary>
        /// Subscribe users Actions to the onSuccessUser and onFailureUser delegates.
        /// </summary>
        /// <param name="onFailure">Action to be invoked on model creation failure.</param>
        /// <param name="onSuccess">Action to be invoked on model creation success.</param>
        /// <param name="data">Request these actions will be linked to.</param>
        public static void Subscribe(ModelData data)
        {
            data.actions.onSuccessUser = data.parameters?.onSuccessAction;
            data.actions.onFailureUser = data.parameters?.onFailAction;
            data.actions.onSuccessUserParams = data.parameters?.onSuccessActionCallback;
            data.actions.onFailureUserParams = data.parameters?.onFailActionCallback;
        }
        /// <summary>
        /// Subscribes users Actions to the onSuccessUserParams and onFailureUserParams delegates.
        /// </summary>
        /// <param name="onFailure">Action to be invoked on model creation failure.</param>
        /// <param name="onSuccess">Action to be invoked on model creation success.</param>
        /// <param name="data">Request these actions will be linked to.</param>
        public static void Subscribe(ModelData data, Action<CallbackInfo> onFailure = null, Action<CallbackInfo> onSuccess = null)
        {
            data.actions.onSuccessUserParams = onSuccess;
            data.actions.onFailureUserParams = onFailure;
        }
    }
}
