using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnythingWorld.Utilities.Networking;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace AnythingWorld.Utilities 
{
    public class NetworkConfig
    { 
        private static string Api => AnythingSettings.APIKey;
        private static string AppName => AnythingSettings.AppName;
       
        public static string ApiUrlStem
        {
            get
            {
                return AW_API_STEM;
            }
        }
        private const string AW_API_STEM = "https://api.anything.world";
        public static string GetNameEnpointUri(string modelName)
        {
            return $"{AW_API_STEM}/anything?key={Api}&app={Encode(AppName)}&name={Encode(modelName)}";
        }
        public static string SearchUri(string searchTerm, string sortingType)
        {
            return $"{AW_API_STEM}/anything?key={Api}&search={Encode(searchTerm)}{sortingType}";
        }
        public static string VoteUri(string voteType, string name, string id)
        {
            return $"{AW_API_STEM}/vote?key={Api}&type={voteType}&name={name}&guid={id}"; 
        }
        public static string MyLikesUri()
        {
            return $"{AW_API_STEM}/voted?key={Api}";
        }

        #region Collections
        public static string UserCollectionsUri(bool namesOnly)
        {
            return $"{AW_API_STEM}/user-collections?key={Api}&onlyName={namesOnly.ToString().ToLower()}";
        }
        public static string AddCollectionUri(string collection)
        {
            return $"{AW_API_STEM}/add-collection?key={Api}&collection={Encode(collection)}";
        }
        public static string RemoveCollectionUri(string collection)
        {
            return $"{AW_API_STEM}/remove-collection?key={Api}&collection={Encode(collection)}";
        }
        public static string AddToCollectionUri(string collection, string name, string id)
        {
            return $"{AW_API_STEM}/add-to-collection?key={Api}&collection={Encode(collection)}&name={name}&guid={id}";
        }
        public static string RemoveFromCollectionUri(string collection, string name, string id)
        {
            return $"{AW_API_STEM}/remove-from-collection?key={Api}&collection={Encode(collection)}&name={name}&guid={id}";
        }
        #endregion Collections

        private static string Encode(string str)
        {
            return UrlEncoder.Encode(str);
        }

    }
}
