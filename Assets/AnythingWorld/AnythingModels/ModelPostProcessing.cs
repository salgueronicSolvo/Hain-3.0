using AnythingWorld.Utilities.Data;
using System;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ModelPostProcessing
    {
        public static void Process(ModelData data, Action<ModelData> onSuccess)
        {
            ModelScaling.Scale(data,onSuccess);
        }
    }
}