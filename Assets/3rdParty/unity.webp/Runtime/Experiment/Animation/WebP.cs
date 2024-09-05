using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using unity.libwebp.Interop;
using unity.libwebp;
using System;

namespace WebP.Experiment.Animation
{
    /// <summary>
    /// Entry of all the loading logic.
    /// </summary>
    public class WebP
    {

#if USE_FAIRYGUI
        /// <summary>
        /// Async loading webp files, and return WebPRender for render.
        /// This function return NTextures which would work with fairygui: https://en.fairygui.com/
        /// </summary>
        /// <param name="url">Remote urls to download or project related absolute path(based on platform)</param>
        /// <returns>WebPRederer to provide NTexture for rendering</returns>
        public static async Task<WebPRendererWrapper<NTexture>> LoadNTexturesAsync(string url)
        {
            var bytes = await WebPLoader.Load(url);
            if (bytes == null || bytes.Length <= 0) return null;

            var sw = new Stopwatch();
            var textures = await WebPDecoderWrapper.Decode(bytes);
            var nTextures = textures.ConvertAll(ret =>
            {
                var (texture, timestamp) = ret;

                return (new NTexture(texture), timestamp);
            });
            var renderer = new WebPRendererWrapper<NTexture>(nTextures);
            Debug.Log($"[WebP] Decode webp into textures in: {sw.ElapsedMilliseconds}");
            sw.Stop();
            return renderer;
        }
#endif

        /// <summary>
        /// Async loading webp files, and return WebPRender for render.
        /// </summary>
        /// <param name="url">Remote urls to download or project related absolute path(based on platform)</param>
        /// <returns>WebPRederer to provide Texture for rendering</returns>
        public static async UniTask<WebPRendererWrapper<Texture2D>> LoadTexturesAsync(string url)
        {
            byte[] bytes = await WebPLoader.Load(url);
            if (bytes == null || bytes.Length <= 0)
            {
                return null;
            }

            List<(Texture2D, int)> textures = await WebPDecoderWrapper.Decode(bytes);
            WebPRendererWrapper<Texture2D> renderer = new WebPRendererWrapper<Texture2D>(textures);
            return renderer;
        }

        public static async UniTask<WebPRendererWrapper<Texture2D>> LoadTexturesAsync(byte[] bytes)
        {
            Assert.IsNotNull(bytes);

            List<(Texture2D, int)> textures = await WebPDecoderWrapper.Decode(bytes);
            WebPRendererWrapper<Texture2D> renderer = new WebPRendererWrapper<Texture2D>(textures);
            return renderer;
        }

        public static async UniTask<List<(Texture2D, int)>> LoadTexturesAsyncManual(byte[] bytes)
        {
            Assert.IsNotNull(bytes);
            return await WebPDecoderWrapper.Decode(bytes); 
        }

        public static async UniTask<Texture2D> LoadSingleTextureAsync(string url, byte[] data = null)
        {
            byte[] bytes;

            if (data == null)
            {
                bytes = await WebPLoader.Load(url);
            }
            else
            {
                bytes = data;
            }

            if (bytes == null || bytes.Length <= 0)
            {
                return null;
            }

            List<(Texture2D, int)> textures = await WebPDecoderWrapper.Decode(bytes);
            if (textures != null && textures.Count > 0)
            {
                return textures[0].Item1; // Accessing the first Texture2D from the tuple
            }
            else
            {
                return null;
            }
        }

        public static unsafe List<(Texture2D, int)> LoadAnimation(ref byte[] data, bool isUsingSoftwareFlip = false)
        {
            List<(Texture2D, int)> ret = new List<(Texture2D, int)>();

            WebPAnimDecoderOptions option = new WebPAnimDecoderOptions
            {
                use_threads = 0,
                color_mode = WEBP_CSP_MODE.MODE_RGBA,
            };

            option.padding[5] = 1;

            NativeLibwebpdemux.WebPAnimDecoderOptionsInit(&option);
            fixed (byte* p = data)
            {
                WebPData webpdata = new WebPData
                {
                    bytes = p,
                    size = new UIntPtr((uint)data.Length)
                };
                WebPAnimDecoder* dec = NativeLibwebpdemux.WebPAnimDecoderNew(&webpdata, &option);

                WebPAnimInfo anim_info = new WebPAnimInfo();

                NativeLibwebpdemux.WebPAnimDecoderGetInfo(dec, &anim_info);

                Debug.LogWarning($"{anim_info.frame_count} {anim_info.canvas_width}/{anim_info.canvas_height}");

                uint size = anim_info.canvas_width * 4 * anim_info.canvas_height;

                int timestamp = 0;
                int previousTimestamp = 0;

                IntPtr pp = new IntPtr();
                byte** unmanagedPointer = (byte**)&pp;
                for (int i = 0; i < anim_info.frame_count; ++i)
                {
                    int result = NativeLibwebpdemux.WebPAnimDecoderGetNext(dec, unmanagedPointer, &timestamp);
                    Assert.AreEqual(1, result);

                    int lWidth = (int)anim_info.canvas_width;
                    int lHeight = (int)anim_info.canvas_height;
                    bool lMipmaps = false;
                    bool lLinear = false;

                    Texture2D texture = Texture2DExt.CreateWebpTexture2D(lWidth, lHeight, lMipmaps, lLinear);
                    texture.LoadRawTextureData(pp, (int)size);

                    if (isUsingSoftwareFlip)
                    {
                        // Flip texture vertically.
                        Color[] pixels = texture.GetPixels();
                        Color[] pixelsFlipped = new Color[pixels.Length];
                        for (int y = 0; y < anim_info.canvas_height; y++)
                        {
                            Array.Copy(pixels, y * anim_info.canvas_width, pixelsFlipped, (anim_info.canvas_height - y - 1) * anim_info.canvas_width, anim_info.canvas_width);
                        }
                        texture.SetPixels(pixelsFlipped);
                    }

                    texture.Apply();

                    // Calculate the delay between frames.
                    int delay = i == 0 ? 0 : timestamp - previousTimestamp;
                    previousTimestamp = timestamp;

                    ret.Add((texture, delay));
                }
                NativeLibwebpdemux.WebPAnimDecoderReset(dec);
                NativeLibwebpdemux.WebPAnimDecoderDelete(dec);
            }
            return ret;
        }
    }
}