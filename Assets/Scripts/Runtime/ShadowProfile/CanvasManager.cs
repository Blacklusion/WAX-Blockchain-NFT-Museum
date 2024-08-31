using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

public enum ImageFormat
{
    ERROR = 0,
    GIF = 1,
    DEFAULT = 2,
    WEBP = 3,
}

public class ImageData
{
    public byte[] Data;
    public Texture2D FirstFrame;
    public ImageFormat ImgFormat;
    public List<(Texture2D, int)> webpAnimation;
}
public class CanvasManager : MonoBehaviour
{
    [Inject(Id = "nftDetailParent")] private readonly CanvasGroup nftDetailParent;
    [Inject(Id = "nftImage")] private readonly RawImage nftImage;
    [Inject(Id = "loadingLbl")] private readonly TextMeshProUGUI loadingLbl;


    [Inject] private readonly WalletLoader walletLoader;
    [SerializeField] public int currentRoom = 0;
    public int currentFloor = 0;

    public Dictionary<string, ImageData> LoadedImages = new();

    [SerializeField] private List<CanvasObject> canvases = new();

    public void AddCanvas(CanvasObject cO)
    {
        if (canvases.Contains(cO)) { return; }
        canvases.Add(cO);
    }

    public int GetTotalCanvas()
    {
        return canvases.Count;
    }

    public async UniTask InitCanvases()
    {
        await AsyncInitCanvases();       
    }

    public void ResetCanvases()
    {
        for (int i = 0; i < canvases.Count; i++)
        {  
            canvases[i].ResetCanvas();
        }
        LoadedImages.Clear();
    }

    private async UniTask AsyncInitCanvases()
    {
        var data = walletLoader.GetItems();
        int roomOffset = GetTotalCanvas();

      

        for (int i = 0; i < roomOffset; i++)
        {
            if (data.Count <= i) { break; }
            await canvases[i].Init(data.Values.ElementAt(i), data.Keys.ElementAt(i));

            if (data.Count > 0)
            {
                loadingLbl.text = (i * 100 / data.Count) + "%";
            }
            else
            {
                loadingLbl.text = "0%";
            }
        }

      
        loadingLbl.text = "";
    }

    public async UniTask SetUIAnimation(List<Texture2D> mFrames, List<float> mFrameDelay)
{
    if (mFrames == null || mFrames.Count == 0) { return; } // no animation to load, ignore

    int mCurFrame = 0;

    // Loop until the parent object is inactive
    while (nftDetailParent.gameObject.activeSelf)
    {
        // Set the current frame texture
        nftImage.texture = mFrames[mCurFrame];

        // Wait for the duration of the current frame delay
        Debug.Log("waiting " + (int)(mFrameDelay[mCurFrame] * 10000));
        await UniTask.Delay((int)(mFrameDelay[mCurFrame] * 10000));

        // Move to the next frame, loop back to the start if at the end
        mCurFrame = (mCurFrame + 1) % mFrames.Count;
    }
}
}
