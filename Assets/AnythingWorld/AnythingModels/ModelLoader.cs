using AnythingWorld.Utilities.Data;
using System.Collections;

namespace AnythingWorld.Models
{
    public static class ModelLoader
    {
        public static void Load(ModelData data)
        {
            CoroutineExtension.StartCoroutine(StartModelLoadingPipeline(data), data.loadingScript);
        }

        private static IEnumerator StartModelLoadingPipeline(ModelData data)
        {
            data.Debug($"Loading model with {data.modelLoadingPipeline} pipeline.");
            switch (data.modelLoadingPipeline)
            {
                case Utilities.ModelLoadingPipeline.RiggedGLB:
                    GltfLoader.Load(data);
                    break;

                case Utilities.ModelLoadingPipeline.GLTF:
                    //data.actions.loadAnimationDelegate?.Invoke(data);
                    break;

                case Utilities.ModelLoadingPipeline.OBJ_Static:
                    ObjSingletonLoader.Load(data);
                    break;

                case Utilities.ModelLoadingPipeline.OBJ_Part_Based:
                    ObjPartsLoader.Load(data);
                    break;
            }
            yield return null;
        }
    }
}