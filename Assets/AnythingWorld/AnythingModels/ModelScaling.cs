using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Models
{
    public static class ModelScaling
    {
        public static void Scale(ModelData data, Action<ModelData> onSuccess)
        {
            //Parse dimension dictionary from db into vector 3
            LoadDimensionsVectorFromDB(data);
            if (ParseScaleType(data))
            {
                ModelPositioning.ApplyTransforms(data, onSuccess);
            }
        }
        private static bool ParseScaleType(ModelData data)
        {
            if (data.parameters.scale.IsSet)
            {
                data.loadedData.bounds = data.parameters.scale.value * data.parameters.scaleMultiplier;
                switch (data.parameters.scaleType)
                {
                    case Utilities.ScaleType.SetRealWorld:
                        return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);

                    case Utilities.ScaleType.ScaleRealWorld:
                        return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);

                    case Utilities.ScaleType.Absolute:
                        data.model.transform.localScale = data.loadedData.bounds;
                        return true;
                }
            }
            else
            {
                data.loadedData.bounds = data.loadedData.dbDimensionsVector * data.parameters.scaleMultiplier;
                return TryCenterAndSizeObjectToBounds(data, data.loadedData.bounds);       
            }    
            return false;
        }
        private static bool TryCenterAndSizeObjectToBounds(ModelData data, Vector3 targetBoundingDimensions)
        {
            //Get renderers and object bounds from renderers.
            var renderers = data.model.GetComponentsInChildren<Renderer>();
            var objectBounds = GetObjectBounds(renderers);
            var meshfilters = data.model.GetComponentsInChildren<MeshFilter>();
            var bounds = GetObjectBounds(meshfilters);
            var objectOrigin = bounds.center;
            var boundsYOffset = bounds.extents.y;
            var relativeScale = CalculateRelativeScaleForDimension(objectBounds, targetBoundingDimensions);
            if (float.IsNaN(relativeScale) || float.IsPositiveInfinity(relativeScale) || float.IsNegativeInfinity(relativeScale))
            {
                //Debug.Log($"Bounds: {objectBounds}, target bounds: {targetBoundingDimensions}");
                string error = $"Error while calculating bounds scale for {data.guid}, calculated bound was {relativeScale.ToString()}";
                data?.actions?.onFailure?.Invoke(data, error);
                return false;
            }

            boundsYOffset *= relativeScale;

            if (data.json.preserveOriginalScale == false)
            {
                data.model.transform.localScale = new Vector3(relativeScale, relativeScale, relativeScale);
            }
            if (data.json.preserveOriginalPosition == false)
            {
                data.model.transform.position -= objectOrigin * relativeScale;
            }
            return true;
        }

        private static void CenterAndSizeObjectToBounds(ModelData data, Vector3 targetBoundingDimensions, Vector3 scalar)
        {
            targetBoundingDimensions.Scale(scalar);
            //Get renderers and object bounds from renderers.
            var renderers = data.model.GetComponentsInChildren<Renderer>();
            var objectBounds = GetObjectBounds(renderers);
            var meshfilters = data.model.GetComponentsInChildren<MeshFilter>();
            var bounds = GetObjectBounds(meshfilters);
            var objectOrigin = bounds.center;
            var boundsYOffset = bounds.extents.y;
            var relativeScale = CalculateRelativeScaleForDimension(objectBounds, targetBoundingDimensions);
            if (float.IsNaN(relativeScale) || float.IsPositiveInfinity(relativeScale) || float.IsNegativeInfinity(relativeScale))
            {
                string error = $"Error while calculating bounds scale for {data.guid}, calculated bound was {relativeScale.ToString()}";
                data?.actions?.onFailure?.Invoke(data, error);
                return;
            }

            boundsYOffset *= relativeScale;

            if (data.json.preserveOriginalScale == false)
            {
                data.model.transform.localScale = new Vector3(relativeScale, relativeScale, relativeScale);
            }
            if (data.json.preserveOriginalPosition == false)
            {
                data.model.transform.position -= objectOrigin * relativeScale;
            }
        }

        #region Utility Operations

        private static float CalculateRelativeScaleForDimension(Vector3 currentDimensions, Vector3 desiredDimensions)
        {
            var maxDesiredDimension = GetObjectScalarDimension(desiredDimensions);
            switch (maxDesiredDimension)
            {
                case ScalingDimension.x:
                    return GetRelativeScale(currentDimensions.x, desiredDimensions.x);

                case ScalingDimension.y:
                    return GetRelativeScale(currentDimensions.y, desiredDimensions.y);

                case ScalingDimension.z:
                    return GetRelativeScale(currentDimensions.z, desiredDimensions.z);

                case ScalingDimension.none:
                default:
                    //If scalar is none, default to 1m on the x axis.
                    return GetRelativeScale(currentDimensions.x, 1);
            }
        }

        private static Vector3 GetObjectBounds(Renderer[] renderers)
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var objRenderer in renderers)
            {
                bounds.Encapsulate(objRenderer.bounds);
            }
            return bounds.size;
        }

        private static Bounds GetObjectBounds(MeshFilter[] meshFilters)
        {
            var totalBounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (var mFilter in meshFilters)
            {
                var mMesh = mFilter.sharedMesh;
                totalBounds.Encapsulate(mMesh.bounds);
            }
            return totalBounds;
        }

        private static void LoadDimensionsVectorFromDB(ModelData data)
        {
            data.loadedData.dbDimensionsVector = ParseDimension(data.json.scale);
            //data.Debug(data.loadedData.dbDimensionsVector);

        }

        private static float GetRelativeScale(float currentDimension, float desiredDimension)
        {
            return (desiredDimension) / currentDimension;
        }

        private static ScalingDimension GetObjectScalarDimension(Vector3 dimensions)
        {
            var maxDimension = Math.Max(dimensions.z, Math.Max(dimensions.x, dimensions.y));

            if (maxDimension == 0)
            {
                return ScalingDimension.none;
            }

            if (maxDimension == dimensions.x)
            {
                return ScalingDimension.x;
            }
            else if (maxDimension == dimensions.y)
            {
                return ScalingDimension.y;
            }
            else if (maxDimension == dimensions.z)
            {
                return ScalingDimension.z;
            }

            return ScalingDimension.none;
        }

        private enum ScalingDimension
        {
            x,
            y,
            z,
            none
        }

        private static Vector3 ParseDimension(Dictionary<string, float> dictionary)
        {
            Vector3 dimensions = Vector3.one;
            dictionary.TryGetValue("width", out dimensions.x);
            dictionary.TryGetValue("height", out dimensions.y);
            dictionary.TryGetValue("length", out dimensions.z);

            return dimensions;
        }

        #endregion Utility Operations
    }
}