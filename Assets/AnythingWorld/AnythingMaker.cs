using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace AnythingWorld
{
    public static class AnythingMaker
    {
        /// <summary>
        /// Request object that's the closest to the search term. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject Make(string name)
        {
            Debug.Log("Called make with string" + name);
            return AnythingWorld.Core.AnythingFactory.RequestModel(name, new Utilities.Data.RequestParamObject());
        }
        public static GameObject Make(string name, params RequestParam[] parameters)
        {
            //Debug.Log("Called make with string" + name);
            //Fetches data from user input and clears request static variables ready for next request.
            var requestParams = RequestParameters.Fetch();
            return AnythingWorld.Core.AnythingFactory.RequestModel(name, requestParams);
        }
    }
}
