using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Utilities.Networking
{
    [Serializable]
    public class NetworkErrorMessage
    {
        public string code = "";
        public string message = "";
        public NetworkErrorMessage(string json)
        {
            var _ = JsonUtility.FromJson<NetworkErrorMessage>(json);
            code = _.code;
            message = _.message;
        }
    }
}
