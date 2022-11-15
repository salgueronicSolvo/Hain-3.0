using AnythingWorld.Utilities;
using AnythingWorld.Utilities.Data;
using AnythingWorld.Utilities.Networking;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AnythingWorld.Networking.Editor
{
    public static class CollectionProcessor
    {
        public delegate void SearchCompleteDelegate(CollectionResult[] results);
        private static SearchCompleteDelegate searchDelegate;

        public delegate void RefreshCompleteDelegate(CollectionResult result);
        private static RefreshCompleteDelegate refreshDelegate;

        public delegate void NameFetchDelegate(string[] results);
        private static NameFetchDelegate nameDelegate;

        public static void AddToCollection(SearchResult searchResult, string collectionName, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(AddToCollectionCoroutine(collectionName, searchResult), parent);
        }

        public static void RemoveFromCollection(SearchResult searchResult, string collectionName, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(RemoveFromCollectionCoroutine(collectionName, searchResult), parent);
        }

        public static void CreateNewCollection(string collectionName, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(CreateCollectionCoroutine(collectionName), parent);
        }

        public static void DeleteCollection(string collectionName, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(DeleteCollectionCoroutine(collectionName), parent);
        }

        public static void GetCollectionNames(NameFetchDelegate nameFetchDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionNamesCoroutine(nameFetchDelegate), parent);
        }

        public static void GetCollections(SearchCompleteDelegate searchCompleteDelegate, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionsCoroutine(searchCompleteDelegate), parent);
        }

        public static void GetCollection(RefreshCompleteDelegate refreshCompleteDelegate, CollectionResult collection, object parent)
        {
            CoroutineExtension.StartEditorCoroutine(GetUserCollectionCoroutine(refreshCompleteDelegate, collection), parent);
        }

        private static IEnumerator GetUserCollectionNamesCoroutine(NameFetchDelegate delegateFunc)
        {
            nameDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.UserCollectionsUri(true);
            www = UnityWebRequest.Get(apiCall);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }

            var result = www.downloadHandler.text;
            Dictionary<string, List<string>> resultsDictionary;
            try
            {
                resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not fetch the search results: {e}");
                resultsDictionary = new Dictionary<string, List<string>>();
            }

            var collectionNames = resultsDictionary.Keys.ToArray();
            www.Dispose();
            nameDelegate(collectionNames);
            nameDelegate -= delegateFunc;
        }

        private static IEnumerator GetUserCollectionsCoroutine(SearchCompleteDelegate delegateFunc)
        {
            searchDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.UserCollectionsUri(false);
            www = UnityWebRequest.Get(apiCall);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }

            var result = www.downloadHandler.text;

            Dictionary<string, List<ModelJson>> resultsDictionary = new Dictionary<string, List<ModelJson>>();
            try
            {
                resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<ModelJson>>>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not fetch the search results: {e}");
                resultsDictionary = new Dictionary<string, List<ModelJson>>();
            }
            var collectionResults = new CollectionResult[resultsDictionary.Count];
            for (int i = 0; i < collectionResults.Length; i++)
            {
                KeyValuePair<string, List<ModelJson>> kvp = resultsDictionary.ElementAt(i);
                collectionResults[i] = new CollectionResult(kvp.Key, kvp.Value);
                yield return ThumbnailRequester.LoadThumbnailsBatch(collectionResults[i].Results.ToArray());
            }

            www.Dispose();
            //Turn JSON into AWThing data format.
            searchDelegate(collectionResults);

            //Unsubscribe search delegate
            searchDelegate -= delegateFunc;
        }

        private static IEnumerator GetUserCollectionCoroutine(RefreshCompleteDelegate delegateFunc, CollectionResult collection)
        {
            refreshDelegate += delegateFunc;

            UnityWebRequest www;
            var apiCall = NetworkConfig.UserCollectionsUri(false);
            www = UnityWebRequest.Get(apiCall);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }

            var result = www.downloadHandler.text;

            Dictionary<string, List<ModelJson>> resultsDictionary = new Dictionary<string, List<ModelJson>>();
            try
            {
                resultsDictionary = JsonConvert.DeserializeObject<Dictionary<string, List<ModelJson>>>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not fetch the search results: {e}");
                resultsDictionary = new Dictionary<string, List<ModelJson>>();
            }

            KeyValuePair<string, List<ModelJson>> kvp = resultsDictionary.FirstOrDefault(x => x.Key == collection.Name);
            CollectionResult collectionResult = new CollectionResult(kvp.Key, kvp.Value);
            yield return ThumbnailRequester.LoadThumbnailsBatch(collectionResult.Results.ToArray());

            www.Dispose();
            //Turn JSON into AWThing data format.
            refreshDelegate(collectionResult);

            //Unsubscribe search delegate
            refreshDelegate -= delegateFunc;
        }

        private static IEnumerator CreateCollectionCoroutine(string collectionName)
        {
            UnityWebRequest www;
            var apiCall = NetworkConfig.AddCollectionUri(collectionName);
            www = UnityWebRequest.Post(apiCall, "");
            www.timeout = 5;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                    
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            www.Dispose();
        }

        private static IEnumerator DeleteCollectionCoroutine(string collectionName)
        {
            UnityWebRequest www;
            var apiCall = NetworkConfig.RemoveCollectionUri(collectionName);
            www = UnityWebRequest.Post(apiCall, "");
            www.timeout = 5;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            www.Dispose();
        }

        private static IEnumerator AddToCollectionCoroutine(string collectionName, SearchResult searchResult)
        {
            var nameSplit = searchResult.data.name.Split('#');

            UnityWebRequest www;
            var apiCall = NetworkConfig.AddToCollectionUri(collectionName, nameSplit[0], nameSplit[1]);
            www = UnityWebRequest.Post(apiCall, "");
            www.timeout = 5;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            www.Dispose();
        }

        private static IEnumerator RemoveFromCollectionCoroutine(string collectionName, SearchResult searchResult)
        {
            var nameSplit = searchResult.data.name.Split('#');

            UnityWebRequest www;
            var apiCall = NetworkConfig.RemoveFromCollectionUri(collectionName, nameSplit[0], nameSplit[1]);
            www = UnityWebRequest.Post(apiCall, "");
            www.timeout = 5;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success) { }
            else
            {
                try
                {
                    //If supported error format process
                    var error = new NetworkErrorMessage(www.downloadHandler.text);
                    if (error.code == "Unrepeatable action") { }
                    else
                    {
                        Debug.Log($"{error.code}: {error.message}");
                    }
                }
                catch
                {
                    //Else just debug as not handled by server properly
                    Debug.Log($"Couldn't parse error: {www.downloadHandler.text}");
                }
            }
            www.Dispose();
        }
    }
}
