using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnythingWorld.Utilities.Data
{
    [Serializable]
    public class RequestParamObject
    {
        public bool placeOnGrid = false;
        public bool animateModel = true;
        public Vector3Param position = new Vector3Param();
        public Vector3Param scale = new Vector3Param();
        public Quaternion rotation = Quaternion.identity;
        public float scaleMultiplier = 1;
        public ScaleType scaleType;
        public TransformSpace transformSpace;
        public Transform parentTransform;
        public Action onSuccessAction;
        public Action onFailAction;
        public Action<CallbackInfo> onSuccessActionCallback;
        public Action<CallbackInfo> onFailActionCallback;
        public Type[] behaviours;
        public Dictionary<AnimationPipeline, Type> qualifiedBehaviours;

    }
}
