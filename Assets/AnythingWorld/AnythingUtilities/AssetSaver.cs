using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Utilities
{
    public static class AssetSaver
    {
        public static string rootPath = "Assets/SavedAssets";

        public static void CreateAssetFromData(CallbackInfo callbackData)
        {

            var animationClipDict = callbackData.data.loadedData.gltf.animationClips;
#if UNITY_EDITOR
            Debug.Log("Creating asset");

            foreach (var kvp in animationClipDict)
            {
                CreateAsset<Animation>(kvp.Value, kvp.Key, callbackData.data.json.name);
            }
#endif
        }
#if UNITY_EDITOR
        public static void CreateAsset<T>(UnityEngine.Object asset, string name, string guid)
        {

            if (AssetDatabase.Contains(asset))
            {
                Debug.Log($"{asset} already serialized within database.");
                return;
            }

            CreateDefaultFolder();
            CreateFolder(rootPath, guid);

            var safeFilterName = GenerateSafeFilePath(name);
            var assetPath = $"{rootPath}/{guid}/{safeFilterName}.asset";
            if (!AssetDatabase.LoadAssetAtPath(assetPath, typeof(T)))
            {
                try
                {
                    AssetDatabase.CreateAsset(asset, assetPath);
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }


        }
        private static string CreateDefaultFolder()
        {
            if (AssetDatabase.IsValidFolder(rootPath)) return AssetDatabase.AssetPathToGUID(rootPath);
            else return CreateFolder("Assets", "SavedAssets");

        }

        static string CreateFolder(string rootDirectory, string name)
        {
            string newDirectory = rootDirectory + "/" + name;
            if (AssetDatabase.IsValidFolder(newDirectory)) return AssetDatabase.AssetPathToGUID(newDirectory);


            string guid = AssetDatabase.CreateFolder(rootDirectory, name);
            string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log(newFolderPath);
            return newFolderPath;
        }
        private static string GenerateSafeFilePath(string inputPath)
        {
            string illegalChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(illegalChars)));
            var safePath = r.Replace(inputPath, "");
            return safePath;
        }
#endif
    }
}

