using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace WebP.Experiment.Animation
{

    /// <summary>
    /// WebP loader for loading assets, should be override to suit your own needs.
    /// </summary>
    public class WebPLoader
    {

        /// <summary>
        /// The actual function to load file from remote location or project related absolute path
        /// </summary>
        public static async UniTask<byte[]> Load(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("[WebP] Loading path can not be empty");
                return null;
            }
            Debug.Log($"[WebP] Try loading WebP file: {url}");

            byte[] bytes = null;

            if (url.Contains("//") || url.Contains("///"))
            {
                bytes = await LoadAsync(url);
            }
            else
            {
                try
                {
                    bytes = File.ReadAllBytes(url);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WebP] load error: {e.Message}");
                }
            }

            return bytes;
        }

        /// <summary>
        /// Example for async UnityWebRequest
        /// 
        /// This function won't work, just example!!!
        /// You should implement your own loading logic here!
        /// </summary>
        private static async UniTask<byte[]> LoadAsync(string path)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
            {
                // Send the request and wait for a response
               await webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    // Handle errors
                    Debug.LogError($"Error: {webRequest.error}");
                    return null;
                }
                else
                {
                    // Return the downloaded bytes
                    return webRequest.downloadHandler.data;
                }
            }
        }
    }
}