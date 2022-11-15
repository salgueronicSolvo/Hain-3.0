using AnythingWorld.Utilities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities;
namespace AnythingWorld.Behaviour
{
    public static class BehaviourHandler
    {
        public static void AddBehaviours(ModelData data)
        {

            if (data.parameters.behaviours != null)
            {
                foreach (var behaviour in data.parameters.behaviours)
                {
                    data.model.AddComponent(behaviour);
                }
            }
            if (data.parameters.qualifiedBehaviours != null)
            {
                var dict = data.parameters.qualifiedBehaviours;
                switch (data.animationPipeline)
                {
                    case AnimationPipeline.Unset:
                        TrySetBehaviour(data, dict);
                        break;
                    case AnimationPipeline.Static:
                        TrySetBehaviour(data, dict);
                        break;
                    case AnimationPipeline.Rigged:
                        TrySetBehaviour(data, dict);
                        break;
                    case AnimationPipeline.WheeledVehicle:
                        TrySetBehaviour(data, dict);
                        break;
                    case AnimationPipeline.PropellorVehicle:
                        TrySetBehaviour(data, dict);
                        break;
                    case AnimationPipeline.Shader:
                        TrySetBehaviour(data, dict);
                        break;
                }
            }
            data.actions.onSuccess?.Invoke(data, "Succesfully made");
        }

        private static void TrySetBehaviour(ModelData data, Dictionary<AnimationPipeline, System.Type> dict)
        {
            if (dict.TryGetValue(data.animationPipeline, out var scriptType))
            {
                data.model.AddComponent(scriptType);
            }
        }
    }

}
