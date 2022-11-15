using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace AnythingWorld
{

    /// <summary>
    /// Static class accepts parameter values and packages them in RequestParam classes and returns to user.
    /// Allows us to accept a variable sized array of parameters of different types from the user.
    /// Accepted values are accepted after being passed to Factory by packing them into an object, this is class then reset to base values.
    /// </summary>
    public static class RequestParameters
    {
        internal static bool animateModel = true;
        internal static bool placeOnGrid = false;
        internal static Vector3Param position = new Vector3Param();
        internal static Quaternion rotation = Quaternion.identity;
        internal static Vector3Param scale = new Vector3Param();
        internal static float scaleMultiplier = 1;
        internal static ScaleType scaleType = AnythingWorld.Utilities.ScaleType.SetRealWorld;
        internal static TransformSpace transformSpace = Utilities.TransformSpace.Local;
        internal static Transform parentTransform = null;
        internal static Action onSuccessAction = null;
        internal static Action onFailAction = null;
        internal static Action<CallbackInfo> onSuccessActionCallback = null;
        internal static Action<CallbackInfo> onFailActionCallback = null;
        internal static Type[] behaviours = null;
        internal static Dictionary<AnimationPipeline, Type> qualifiedBehaviours = null;

        /// <summary>
        /// Package user inputs into object and reset Request class.
        /// </summary>
        /// <returns>RequestParamObject holding user parameter inputs.</returns>
        internal static RequestParamObject Fetch()
        {
            var rPO = new RequestParamObject
            {
                placeOnGrid = placeOnGrid,
                animateModel = animateModel,
                position = position,
                rotation = rotation,
                scale = scale,
                scaleMultiplier = scaleMultiplier,
                scaleType = scaleType,
                onSuccessAction = onSuccessAction,
                onSuccessActionCallback = onSuccessActionCallback,
                onFailAction = onFailAction,
                onFailActionCallback = onFailActionCallback,
                behaviours = behaviours,
                qualifiedBehaviours = qualifiedBehaviours,
                parentTransform = parentTransform,
                transformSpace = transformSpace
            };
            Reset();
            return rPO;
        }

        /// <summary>
        /// Specify which transform space input rotation and position will be applied to.
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public static RequestParam TransformSpace(TransformSpace space)
        {
            return new TransformSpaceParam(space);
        }

        /// <summary>
        /// Parent the model to a parent transform.
        /// </summary>
        /// <param name="parentTransform">Transform that model will be parented to.</param>
        public static RequestParam Parent(Transform parentTransform)
        {
            return new ParentTransformParam(parentTransform);
        }

        /// <summary>
        /// Request model with animation system if available.
        /// </summary>
        public static RequestParam IsAnimated(bool value)
        {
            return new IsAnimated(value);
        }

        #region Behaviours

        /// <summary>
        /// Specify an array of behaviour script to be added to model on completion.
        /// </summary>
        /// <param name="behaviourScripts"></param>
        /// <returns></returns>
        public static RequestParam Behaviours(params Type[] behaviourScripts)
        {
            return new BehaviourParam(behaviourScripts);
        }


        public static RequestParam PlaceOnGrid(bool value)
        {
            return new PlaceOnGridParam(value);
        }




        /// <summary>
        /// Specify an array of behaviour scriptsto be added to model on completion.
        /// </summary>
        /// <param name="behaviourScripts"></param>
        /// <returns></returns>
        public static RequestParam Behaviours(params MonoBehaviour[] behaviourScripts)
        {
            return new BehaviourParam(behaviourScripts);
        }

        /// <summary>
        /// Specify a dictionary mapping animation pipeline to behaviours.
        /// Allows user to apply specific behaviours to specific model animation pipelines (Eg. rigged vs. vehicles).
        /// </summary>
        /// <param name="behaviourDictionary"></param>
        /// <returns></returns>
        public static RequestParam Behaviours(Dictionary<AnimationPipeline, Type> behaviourDictionary)
        {
            return new BehaviourParam(behaviourDictionary);
        }
        
        #endregion Behaviours

        #region Rotation

        /// <summary>
        /// Set rotation of created model with Quaternion.
        /// </summary>
        /// <param name="quaternionRotation">Quaternion rotation value to be assigned to model rotation.</param>
        public static RequestParam Rotation(Quaternion quaternionRotation)
        {
            return new RotationParam(quaternionRotation);
        }

        /// <summary>
        /// Set rotation of created model with euler angles.
        /// </summary>
        /// <param name="eulerRotation">Euler rotation to apply to model.</param>
        /// <returns></returns>
        public static RequestParam Rotation(Vector3 eulerRotation)
        {
            return new RotationParam(eulerRotation);
        }

        /// <summary>
        /// Set rotation of created model with euler angles.
        /// </summary>
        /// <param name="x">Euler angle for x axis.</param>
        /// <param name="y">Euler angle for y axis.</param>
        /// <param name="z">Euler angle for z axis.</param>
        public static RequestParam Rotation(int x, int y, int z)
        {
            return new RotationParam(x, y, z);
        }

        #endregion Rotation

        #region Scale

        /// <summary>
        /// Set model scale with vector.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RequestParam Scale(Vector3 value)
        {
            return new ScaleParam(value);
        }

        /// <summary>
        /// Multiply the default or defined scale value by this value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RequestParam ScaleMultiplier(float value)
        {
            return new ScaleMultiplier(value);
        }

        /// <summary>
        /// Set model scale with integers.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static RequestParam Scale(int x, int y, int z)
        {
            return new ScaleParam(x, y, z);
        }

        /// <summary>
        /// Set type of scaling operation to apply to model.
        /// </summary>
        /// <param name="scaleType"></param>
        /// <returns></returns>
        public static RequestParam ScaleType(ScaleType scaleType)
        {
            return new ScaleTypeParam(scaleType);
        }
        #endregion Scale

        #region Position

        /// <summary>
        /// Position of created mode.
        /// </summary>
        /// <param name="value">Vector3 value that will be set to object transform position.</param>
        /// <returns></returns>
        public static RequestParam Position(Vector3 value)
        {
            return new PositionParam(value);
        }

        public static RequestParam Position(int x, int y, int z)
        {
            return new PositionParam(x, y, z);
        }

        #endregion Position

        #region Success Action

        /// <summary>
        /// Action that will be called on successful model creation.
        /// </summary>
        /// <param name="value">Function to be invoked.</param>
        /// <returns></returns>
        public static RequestParam OnSuccessAction(Action value)
        {
            return new OnSuccessActionParam(value);
        }

        public static RequestParam OnSuccessAction(Action<CallbackInfo> value)
        {
            return new OnSuccessActionParam(value);
        }

        #endregion Success Action

        #region Failure Action

        /// <summary>
        /// Action called on failed model creation.
        /// </summary>
        /// <param name="value">Function to be invoked.</param>
        /// <returns></returns>
        public static RequestParam OnFailAction(Action value)
        {
            return new OnFailActionParam(value);
        }

        #endregion Failure Action

        /// <summary>
        /// Reset all fields in this class.
        /// </summary>
        internal static void Reset()
        {
            animateModel = true;
            placeOnGrid = false;
            rotation = default;
            position = new Vector3Param();
            scale = new Vector3Param();
            scaleMultiplier = 1;
            scaleType = Utilities.ScaleType.ScaleRealWorld;
            transformSpace = Utilities.TransformSpace.Local;
            parentTransform = null;
            onSuccessAction = null;
            onFailAction = null;
            onSuccessActionCallback = null;
            onFailActionCallback = null;
            behaviours = null;
            qualifiedBehaviours = null;
            //ResetAllStaticsVariables(typeof(RequestParameters));
        }
       /*
        /// <summary>
        /// Resets static variables in class of given Type.
        /// </summary>
        /// <param name="type"></param>
        private static void ResetAllStaticsVariables(Type type)
        {
            var fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                fieldInfo.SetValue(null, GetDefault(type));
            }
        }
        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
       */
    }

    public class RequestParam
    { }

    public class PlaceOnGridParam : RequestParam
    {
        public PlaceOnGridParam(bool value)
        {
            RequestParameters.placeOnGrid = value;
        }
    }
    public class TransformSpaceParam : RequestParam
    {
        public TransformSpaceParam(TransformSpace space)
        {
            RequestParameters.transformSpace = space;
        }
    }

    public class ParentTransformParam : RequestParam
    {
        public ParentTransformParam(GameObject gameObject)
        {
            RequestParameters.parentTransform = gameObject.transform;
        }

        public ParentTransformParam(Transform transform)
        {
            RequestParameters.parentTransform = transform;
        }
    }

    public class BehaviourParam : RequestParam
    {
        public BehaviourParam(Dictionary<AnimationPipeline, Type> behaviours)
        {
            RequestParameters.qualifiedBehaviours = behaviours;
        }

        public BehaviourParam(Type[] behaviours)
        {
            RequestParameters.behaviours = behaviours;
        }

        public BehaviourParam(MonoBehaviour[] behaviours)
        {
            //Convert array of monobehaviours to their type values
            var typeArray = behaviours.Select(x => x.GetType()).ToArray(); ;
            RequestParameters.behaviours = typeArray;
        }
    }

    /// <summary>
    /// Request model with animation system if available.
    /// </summary>
    public class IsAnimated : RequestParam
    {
        public IsAnimated(bool value)
        {
            RequestParameters.animateModel = value;
        }
    }

    /// <summary>
    /// Rotation of created object.
    /// </summary>
    public class RotationParam : RequestParam
    {
        public RotationParam(Quaternion quaternionRotation)
        {
            RequestParameters.rotation = quaternionRotation;
        }

        public RotationParam(Vector3 eulerRotation)
        {
            RequestParameters.rotation = Quaternion.Euler(eulerRotation);
        }

        /// <summary>
        /// Set rotation using Euler angles.
        /// </summary>
        /// <param name="x">Euler angle for x axis.</param>
        /// <param name="y">Euler angle for y axis.</param>
        /// <param name="z">Euler angle for z axis.</param>
        public RotationParam(int x, int y, int z)
        {
            RequestParameters.rotation = Quaternion.Euler(x, y, z);
        }
    }

    /// <summary>
    /// Position of created object.
    /// </summary>
    public class PositionParam : RequestParam
    {
        public PositionParam(Vector3 value)
        {
            RequestParameters.position = new Vector3Param(value);
        }

        public PositionParam(int x, int y, int z)
        {
            RequestParameters.position = new Vector3Param(new Vector3(x, y, z));
        }
    }

    public class ScaleParam : RequestParam
    {
        public ScaleParam(Vector3 value)
        {
            RequestParameters.scale = new Vector3Param(value);
        }

        public ScaleParam(int x, int y, int z)
        {
            RequestParameters.scale = new Vector3Param(new Vector3(x, y, z));
        }
        public ScaleParam(float x, float y, float z)
        {
            RequestParameters.scale = new Vector3Param(new Vector3(x, y, z));
        }

    }
    public class ScaleMultiplier : RequestParam
    {
        public ScaleMultiplier(float scalar)
        {
            RequestParameters.scaleMultiplier = scalar;
        }
    }

    public class ScaleTypeParam : RequestParam
    {
        public ScaleTypeParam(ScaleType value)
        {
            RequestParameters.scaleType = value;
        }
    }

    /// <summary>
    /// Action called on successful model creation.
    /// </summary>
    public class OnSuccessActionParam : RequestParam
    {
        public OnSuccessActionParam(Action value)
        {
            RequestParameters.onSuccessAction = value;
        }

        /// Returns callback info from model (guid, linked object, message)
        public OnSuccessActionParam(Action<CallbackInfo> value)
        {
            RequestParameters.onSuccessActionCallback = value;
        }
    }

    /// <summary>
    /// Action called on model creation failure.
    /// </summary>
    public class OnFailActionParam : RequestParam
    {
        public OnFailActionParam(Action value)
        {
            RequestParameters.onFailAction = value;
        }

        /// <summary>
        /// Returns callback info from model (guid, linked object, message)
        /// </summary>
        /// <param name="value"></param>
        public OnFailActionParam(Action<CallbackInfo> value)
        {
            RequestParameters.onFailActionCallback = value;
        }
    }
}