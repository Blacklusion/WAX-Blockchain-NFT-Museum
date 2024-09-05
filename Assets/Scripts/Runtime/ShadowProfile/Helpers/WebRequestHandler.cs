using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ShadowProfile
{
    public static class WebRequestHandler
    {
        public static async UniTask<string> SendRequestAsync(string url, string method, string jsonData,
            string contentType = "application/json")
        {
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                if (method == UnityWebRequest.kHttpVerbPOST || method == UnityWebRequest.kHttpVerbPUT)
                {
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", contentType);
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + request.error);
                    return null;
                }
                else
                {
                    return request.downloadHandler.text;
                }
            }
        }

        public static async UniTask<string> GetAsync(string url)
        {
            return await SendRequestAsync(url, UnityWebRequest.kHttpVerbGET, null);
        }

        public static async UniTask<string> PostAsync(string url, string jsonData)
        {
            return await SendRequestAsync(url, UnityWebRequest.kHttpVerbPOST, jsonData);
        }

        public static async UniTask<string> SendCustomRequestAsync(string url, string method, string jsonData)
        {
            return await SendRequestAsync(url, method, jsonData);
        }

        public static async UniTask<ImageData> FetchImage(string url)
        {
            ImageData image = new ImageData
                { Data = null, FirstFrame = null, ImgFormat = ImageFormat.ERROR, webpAnimation = null };

            try
            {
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    await www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        return image;
                    }

                    byte[] imageData = www.downloadHandler.data;
                    var processedData = await ProcessImageData(imageData, url);
                    Texture2D texture = processedData[0].Item1;

                    image.ImgFormat = GetImageFormat(ref imageData);
                    image.Data = imageData;
                    image.FirstFrame = texture;
                    image.webpAnimation = processedData;
                    return image;
                }
            }
            catch (Exception ex)
            {
                return image;
            }
        }

        private static async UniTask<List<(Texture2D, int)>> ProcessImageData(byte[] imageData, string url)
        {
            ImageFormat format = GetImageFormat(ref imageData);
            List<(Texture2D, int)> list = new();
            switch (format)
            {
                case ImageFormat.GIF:
                    list = await ProcessGIF(imageData, url);
                    break;
                case ImageFormat.WEBP:
                    list = await ProcessWEBP(imageData, url);
                    break;
                default:
                    list.Add((ProcessStandardFormat(ref imageData), 0));
                    break;
            }

            return list;
        }

        public static ImageFormat GetImageFormat(ref byte[] imageData)
        {
            // Check for GIF
            if (imageData.Length >= 3 && imageData[0] == 'G' && imageData[1] == 'I' && imageData[2] == 'F')
                return ImageFormat.GIF;

            // Check for WEBP
            if (imageData.Length >= 12 &&
                imageData[0] == 'R' && imageData[1] == 'I' && imageData[2] == 'F' && imageData[3] == 'F' &&
                imageData[8] == 'W' && imageData[9] == 'E' && imageData[10] == 'B' && imageData[11] == 'P')
                return ImageFormat.WEBP;

            return ImageFormat.WEBP;
        }

        private static async UniTask<List<(Texture2D, int)>> ProcessGIF(byte[] imageData, string url)
        {
            List<(Texture2D, int)> returnList = new();

            try
            {
                using (var decoder = new MG.GIF.Decoder(ref imageData))
                {
                    MG.GIF.Image img;

                    while ((img = decoder.NextImage()) != null)
                    {
                        Texture2D texture = img.CreateTexture();
                        int delay = img.Delay;
                        returnList.Add((texture, delay));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing GIF: {ex.Message}");
            }

            return returnList;
        }

        private static async UniTask<List<(Texture2D, int)>> ProcessWEBP(byte[] imageData, string url)
        {
            // check if animated
            // Look for 'ANMF' chunk to determine if it's animated
            for (int i = 12; i < imageData.Length - 4; i++)
            {
                if (Encoding.ASCII.GetString(imageData, i, 4) == "ANMF")
                {
                    // return animated processing
                    return WebP.Experiment.Animation.WebP.LoadAnimation(ref imageData);
                }
            }

            List<(Texture2D, int)> returnList = new();
            Texture2D texture = await WebP.Experiment.Animation.WebP.LoadSingleTextureAsync(string.Empty, imageData);
            returnList.Add((texture, -1));

            return returnList;
        }

        private static Texture2D ProcessStandardFormat(ref byte[] imageData)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            return texture;
        }
    }
}