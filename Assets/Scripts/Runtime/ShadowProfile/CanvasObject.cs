using Cysharp.Threading.Tasks;
using DG.Tweening;
using ShadowProfile;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class CanvasObject : MonoBehaviour
{
    [Inject] private readonly CanvasManager canvasManager;
    [Inject] private readonly PlayerMovement player;
    [Inject] private readonly WalletLoader wallet;
    [Inject] private readonly CoreLogic core;
    [Inject(Id = "missingImg")] private readonly Texture2D missingImg;
    [Inject(Id = "nftImage")] private readonly RawImage nftImage;
    [Inject(Id = "nftName")] private readonly TextMeshProUGUI nftName;
    [Inject(Id = "nftRarity")] private readonly TextMeshProUGUI nftRarity;
    [Inject(Id = "nftDescription")] private readonly TextMeshProUGUI nftDescription;
    [Inject(Id = "nftCollection")] private readonly TextMeshProUGUI nftCollection;
    [Inject(Id = "nftDetailParent")] private readonly CanvasGroup nftDetailParent;

    private int detailsIndex;
    private Vector3 initialScale;
    private Vector3 initialScaleBG;
    private Material matBG;
    private string imgUrl;
    private Renderer renderer;
    private ImageData imageData;
    private float minDistanceToAnimate = 5.0f;
    private float renderDistance = 15.0f;

    private List<Texture2D> mFrames = new List<Texture2D>();
    private List<float> mFrameDelay = new List<float>();
    private int mCurFrame = 0;
    private float mTime = 0.0f;
    private int totalFrames = 0;


    [SerializeField] private Material mat;
    [SerializeField] private Transform canvasBG;
    [SerializeField] private bool lookAtPlayer = false;
    private bool isFlipped = false;
    private void Start()
    {
        matBG = canvasBG.GetComponent<Renderer>().material;
        initialScaleBG = canvasBG.localScale;

        initialScale = this.transform.localScale;
        canvasManager.AddCanvas(this);
        CreateMaterial(missingImg);

        renderer = GetComponent<Renderer>();
    }

    public async UniTask Init(string url, int detailIndex)
    {
        isFlipped = false;
        this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0.0f);
        if (url == null) { return; }

        imgUrl = url;
        detailsIndex = detailIndex;

        await LoadImage(url);
    }

    private async UniTask LoadImage(string img)
    {
        // Validate input
        if (string.IsNullOrEmpty(imgUrl)) { return; }

        // Fetch or retrieve the cached texture
        Texture2D texture = await GetOrFetchImage(img);

        if(texture == null) { return; }

        // Apply the texture to a new material
        ApplyTexture(texture);

        // Adjust scales and aspect ratios
        AdjustObjectScales(texture);

        await Resources.UnloadUnusedAssets();
        AnimateImage().Forget();
    }

    private async UniTask<Texture2D> GetOrFetchImage(string img)
    {
        // Check if the image is already loaded
        if (canvasManager.LoadedImages.TryGetValue(img, out imageData))
        {
            return imageData.FirstFrame;
        }

        // Determine the URL to fetch the image from
        string imageURL;

        if (img.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || img.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            imageURL = Consts.IPFS_URL + "/proxy?source=" + img; // Use the image string as it is a valid URL
        }
        else
        {
            imageURL = Consts.IPFS_URL + img + "?preset=400x400&as_gif=true"; // Prepend with Consts.IPFS_URL as it's not a valid URL _vid_to_gif
        }

        try
        {
            imageData = await WebRequestHandler.FetchImage(imageURL);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(imageURL);
            throw;
        }

        canvasManager.LoadedImages.Add(img, imageData);
        return imageData.FirstFrame;
    }

    private void CreateMaterial(Texture2D texture)
    {
        // Create a new material and assign the texture
        Material newMat = new Material(mat) { mainTexture = texture };

        // Apply the new material to the renderer
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = newMat;

        // Set the background color to black
        matBG.color = Color.black;
    }

    public void ApplyTexture(Texture2D texture)
    {
        renderer.material.mainTexture = texture;

        // Set the background color to white
        matBG.color = GetAverageColor(texture);
    }

    private void AdjustObjectScales(Texture2D texture)
    {
        // Adjust the scale of the current object and its background
        this.transform.localScale = initialScale;
        AdjustScaleToFitAspectRatio(texture, this.transform);

        canvasBG.localScale = initialScaleBG;
        AdjustScaleToFitAspectRatio(texture, canvasBG, true);
    }

    private void AdjustScaleToFitAspectRatio(Texture texture, Transform t, bool isBackground = false)
    {
        float aspectRatio = (float)texture.width / (float)texture.height;

        if (aspectRatio > 1)
        {
            // Landscape texture
            t.localScale = new Vector3(t.localScale.x, t.localScale.y / aspectRatio, t.localScale.z);
        }
        else if (aspectRatio < 1)
        {
            // Portrait texture
            if (isBackground)
            {
                t.localScale = new Vector3(t.localScale.x * aspectRatio, t.localScale.y * aspectRatio, t.localScale.z);
            }
            else
            {
                t.localScale = new Vector3(t.localScale.x * aspectRatio, t.localScale.y, t.localScale.z);
            }
        }
        // If aspectRatio is 1, then the texture is already a square and we don't need to adjust the scale.
    }

    public Color GetAverageColor(Texture2D texture)
    {
        try
        {
            Color[] pixels = texture.GetPixels();
            Color averageColor = Color.black;

        for (int i = 0; i < pixels.Length; i++)
        {
            averageColor += pixels[i];
        }
        averageColor /= pixels.Length;

        return averageColor;
        }
        catch
        {
            return Color.black;
        }
    }

    public Color Brighten(Color color, float percentage)
    {
        float multiplier = 1f + percentage;
        return new Color(Mathf.Clamp01(color.r * multiplier),
                         Mathf.Clamp01(color.g * multiplier),
                         Mathf.Clamp01(color.b * multiplier),
                         color.a);
    }

    public void InitDetail ()
    {
        nftDetailParent.gameObject.SetActive(true);
        nftDetailParent.alpha = 0.0f;
        nftDetailParent.DOFade(1.0f, 0.3f);

        nftName.text = wallet.GetNameByID(detailsIndex);
        nftRarity.text = wallet.GetRarityByID(detailsIndex);
        nftDescription.text = wallet.GetDescriptionByID(detailsIndex);
        nftCollection.text = wallet.GetCollectionByID(detailsIndex);
        nftImage.texture = renderer.material.mainTexture;

        nftImage.rectTransform.sizeDelta = new Vector2(800, 800);
        float aspect = (float)nftImage.texture.width / nftImage.texture.height;
        if (aspect > 1)
        {
            // Reduce the height if aspect ratio is greater than 1
            nftImage.rectTransform.sizeDelta = new Vector2(nftImage.rectTransform.sizeDelta.x, nftImage.rectTransform.sizeDelta.x / aspect);
        }
        else
        {
            // Increase the width for aspect ratio less than or equal to 1
            nftImage.rectTransform.sizeDelta = new Vector2(nftImage.rectTransform.sizeDelta.y * aspect, nftImage.rectTransform.sizeDelta.y);
        }

        if (isFlipped)
        {
            nftImage.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            nftImage.transform.localEulerAngles = Vector3.zero;
        }

        canvasManager.SetUIAnimation(mFrames, mFrameDelay).Forget();
    }

    public void ResetCanvas()
    {
        ApplyTexture(missingImg);
        detailsIndex = -1;
        imgUrl = string.Empty;
        this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0.0f);
        if(imageData == null) { return; }
        imageData.ImgFormat = ImageFormat.ERROR;
        imageData.FirstFrame = null;
        isFlipped = false;

        // Reset animation data
        mFrames.Clear();
        mFrameDelay.Clear();
        mCurFrame = 0;
        mTime = 0.0f;
        totalFrames = 0;

        this.transform.localScale = initialScale;
        canvasBG.localScale = initialScaleBG;
    }


    public void MatchPositions()
    { 
        this.transform.position = canvasBG.position;
    }

    public void MatchRotations()
    {
        this.transform.rotation = canvasBG.rotation;
    }

    public bool HasImage()
    {
        if (string.IsNullOrEmpty(imgUrl) || imageData == null || imageData.FirstFrame == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public async UniTask AnimateImage()
    {
        
        if (imageData != null && imageData.webpAnimation != null)
        {
                totalFrames = imageData.webpAnimation.Count;
        }
    

        if (totalFrames <= 1) 
        {
            return; 
        }

        mFrames.Clear();
        mFrameDelay.Clear();

        if (imageData.FirstFrame == null)
        {
            Debug.Log("Exiting bc Firstframe eq null");
            return;
        }

        if (imageData.ImgFormat == ImageFormat.GIF)
        {
            Debug.Log("Processing animated GIF for " + imgUrl);
            ProcessWEBPAnimation();
        } else if (imageData.ImgFormat == ImageFormat.WEBP)
        {
            ProcessWEBPAnimation();
            this.transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 180.0f);
            isFlipped = true;
        }
    }

    private void ProcessWEBPAnimation()
    {
        int prevTimestamp = 0;
        int delay;
        for (int i = 0; i < imageData.webpAnimation.Count; i++)
        {
            mFrames.Add(imageData.webpAnimation[i].Item1);

            delay = imageData.webpAnimation[i].Item2 - prevTimestamp;
            prevTimestamp = imageData.webpAnimation[i].Item2;
            mFrameDelay.Add(delay / 1000.0f);
        }
        imageData.Data = null;
    }

    private void LookAtPlayer()
    {
        if (!lookAtPlayer) { return; }

        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0;

        // Check if the direction is not zero (to avoid undefined behavior)
        if (directionToPlayer != Vector3.zero)
        {
            // Rotate the object to face the player
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
            transform.localEulerAngles += new Vector3(0, 180, 0);
        }
    }

    void FixedUpdate()
    {
        LookAtPlayer();

        Debug.LogWarning($"mFrames.Count: {mFrames.Count}, mFrameDelay.Count: {mFrameDelay.Count}, imgUrl: {imgUrl}");

        if (core.UIState == UISTATE.LoadingState) { return; }
        if (imageData == null) { return; }

        if (totalFrames <= 1) { return; }
        if (imageData.ImgFormat != ImageFormat.GIF && imageData.ImgFormat != ImageFormat.WEBP) { return; }
        if (imageData.FirstFrame == null) { return; }
        if (mFrames == null) { return; }
        if (Vector3.Distance(this.transform.position, player.transform.position) > minDistanceToAnimate) { return; }

        
        mTime += Time.deltaTime;

        if (mTime >= mFrameDelay[mCurFrame])
        {
            mCurFrame = (mCurFrame + 1) % mFrames.Count;
            mTime = 0.0f;

            ApplyTexture(mFrames[mCurFrame]);
        }
    }
}
