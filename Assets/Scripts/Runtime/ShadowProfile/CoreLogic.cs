using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using TMPro;
using System;

public enum UISTATE
{
    WalletInput,
    WalletConfig,
    LoadingState,
    InGameMenu,
    DetailedView
}

public class CoreLogic : MonoBehaviour
{
    [Inject] private readonly CanvasManager canvasManager;
    [Inject] private readonly WalletLoader walletLoader;
    [Inject] private readonly PlayerMovement player;
    [Inject(Id = "walletAdd")] private readonly TMP_InputField walletAddress;

    [Inject(Id = "ignoreRepeated")] private readonly Toggle ignoreRepeated;
    [Inject(Id = "showWalletBtn")] private readonly Button fetchData;
    [Inject(Id = "enterMuseumBtn")] private readonly Button enterMuseum;
    [Inject(Id = "statusLbl")] private readonly TextMeshProUGUI statusLbl;
    [Inject(Id = "pageStatusLbl")] private readonly TextMeshProUGUI pageStatusLbl;

    [Inject(Id = "loadingScreen")] private readonly CanvasGroup loadingScreen;

    [Inject(Id = "walletWindow")] private readonly CanvasGroup walletWindow;
    [Inject(Id = "collectionsWindow")] private readonly CanvasGroup collectionsWindow;
    [Inject(Id = "mainUIBG")] private readonly CanvasGroup mainUIBG;

    [Inject(Id = "inGameSidebar")] private readonly CanvasGroup sideBarMenu;
    [Inject(Id = "nextPageBtn")] private readonly Button nextPageBtn;
    [Inject(Id = "prevPageBtn")] private readonly Button prevPageBtn;
    [Inject(Id = "jumpPageBtn")] private readonly Button jumpPageBtn;
    [Inject(Id = "swapCollectionBtn")] private readonly Button swapCollectionBtn;
    [Inject(Id = "swapWalletBtn")] private readonly Button swapWalletBtn;
    [Inject(Id = "pagesLbl")] private readonly TextMeshProUGUI pageLbl;
    [Inject(Id = "jumpPageLbl")] private readonly TMP_InputField jumpPageLbl;

    public UISTATE UIState = UISTATE.WalletInput;

    public bool UniqueOnly { get { return ignoreRepeated.isOn; } }

    private void Start()
    {
        fetchData.onClick.AddListener(() => LoadWallet());
        enterMuseum.onClick.AddListener(() => EnterMuseum());
        nextPageBtn.onClick.AddListener(() => LoadNextPage());
        prevPageBtn.onClick.AddListener(() => LoadPrevPage());
        jumpPageBtn.onClick.AddListener(() => LoadSelectedPage());
        swapCollectionBtn.onClick.AddListener(() => SwapCollections());
        swapWalletBtn.onClick.AddListener(() => SwapWallet());

        LoadWalletByURL();
    }

    private void Update()
    {
        ToggleSidebar();
    }

    private void LoadNextPage()
    {
        if (UIState != UISTATE.InGameMenu) { return; } 
        GetNextPage().Forget();
    }

    private void LoadPrevPage()
    {
        if (UIState != UISTATE.InGameMenu) { return; } 
        GetPrevPage().Forget();
    }

    private void LoadSelectedPage()
    {
        if (UIState != UISTATE.InGameMenu) return;
    
        int maxPage = walletLoader.GetTotalPages();
        
        if (!int.TryParse(jumpPageLbl.text, out int selectedPage) || selectedPage < 1 || selectedPage > maxPage)
        {
            pageStatusLbl.text = "This page doesn't exist :(";
            return;
        }
    
        // Check if the user is already on the selected page
        if (selectedPage == walletLoader.CurrentPage)
        {
            pageStatusLbl.text = "You are already on this page";
            return;
        }
    
        JumpToPage().Forget();
    }

    private void ToggleSidebar()
    {
        if (UIState != UISTATE.InGameMenu) { return; }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIState = UISTATE.LoadingState;

            if (sideBarMenu.gameObject.activeSelf) // close it
            {
                player.LockMouse();
                sideBarMenu.DOFade(0.0f, 0.3f).OnComplete(() =>
                {
                    sideBarMenu.gameObject.SetActive(false);
                    UIState = UISTATE.InGameMenu;
                });
            }
            else
            {
                sideBarMenu.gameObject.SetActive(true);
                sideBarMenu.alpha = 0.0f;
                player.UnlockMouse();
                sideBarMenu.DOFade(1.0f, 0.3f).OnComplete(() =>
                {
                    UIState = UISTATE.InGameMenu;
                });
            }
        }
    }

    private async UniTask GetPrevPage()
    {
        UIState = UISTATE.LoadingState;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1.0f, 0.3f);
        canvasManager.ResetCanvases();
        await walletLoader.LoadPreviousPage();
        if (ignoreRepeated.isOn)
        {
            await walletLoader.FilterUnique();
        }
        await canvasManager.InitCanvases();
        SetLblText();
        UIState = UISTATE.InGameMenu;
        loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() => { loadingScreen.gameObject.SetActive(false); });
    }

    private async UniTask GetNextPage()
    {
        UIState = UISTATE.LoadingState;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1.0f, 0.3f);

        canvasManager.ResetCanvases();
        await walletLoader.LoadNextPage();
        if (ignoreRepeated.isOn)
        {
            await walletLoader.FilterUnique();
        }
        await canvasManager.InitCanvases();
        SetLblText();
        UIState = UISTATE.InGameMenu;

        loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() => { loadingScreen.gameObject.SetActive(false); });
    }

    private async UniTask JumpToPage()
    {
        UIState = UISTATE.LoadingState;
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1.0f, 0.3f);

        canvasManager.ResetCanvases();
        await walletLoader.JumpToPage(int.Parse(jumpPageLbl.text));
        await canvasManager.InitCanvases();
        SetLblText();
        jumpPageLbl.text = string.Empty;
        UIState = UISTATE.InGameMenu;
        loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() => { loadingScreen.gameObject.SetActive(false); });
    }

    private async UniTask LoadAssets()
    {
        if (ignoreRepeated.isOn)
        {
            jumpPageBtn.gameObject.SetActive(false);
            pageLbl.gameObject.SetActive(false);
            jumpPageLbl.gameObject.SetActive(false);
        }
        else
        {
            jumpPageBtn.gameObject.SetActive(true);
            pageLbl.gameObject.SetActive(true);
            jumpPageLbl.gameObject.SetActive(true);
        }

        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1.0f, 0.3f);

        UIState = UISTATE.LoadingState;
        await LoadDataInCollections();
        player.LockMouse();
        await canvasManager.InitCanvases();
        SetLblText();
        UIState = UISTATE.InGameMenu;
        loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() => { loadingScreen.gameObject.SetActive(false); });
    }

    private async UniTask LoadWalletCollections()
    {

        if (walletAddress.text.Length > 13 || walletAddress.text.Length == 0)
        {
            statusLbl.text = "Invalid wallet address";
            return;
        }
        try
        {
            loadingScreen.gameObject.SetActive(true);
            loadingScreen.DOFade(1.0f, 0.3f);

            await walletLoader.GetWalletCollections();
            await walletLoader.GetWalletCount();

            // Fade out the wallet window and fade in the collections window
            walletWindow.DOFade(0.0f, 0.3f).OnComplete(() =>
            {
                collectionsWindow.gameObject.SetActive(true);
                collectionsWindow.alpha = 0;
                collectionsWindow.DOFade(1.0f, 0.3f);
                walletWindow.gameObject.SetActive(false);
                UIState = UISTATE.WalletConfig;
            });

            // Fade out the loading screen
            loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() =>
            {
                loadingScreen.gameObject.SetActive(false);
            });
        }
        catch (Exception ex)
        {
            // Fade out the loading screen
            loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() =>
            {
                loadingScreen.gameObject.SetActive(false);
            });
            // Set the status label text to "error" if an exception occurs
            statusLbl.text = "Wallet not found or empty wallet";
            Debug.LogError($"An error occurred while loading wallet collections: {ex.Message}");
        }
    }

    private async UniTask LoadDataInCollections()
    {
        loadingScreen.gameObject.SetActive(true);
        loadingScreen.DOFade(1.0f, 0.3f);
        walletLoader.ResetElements();
        walletLoader.ResetPages();

        collectionsWindow.DOFade(0.0f, 0.3f).OnComplete(() =>
        {
            collectionsWindow.gameObject.SetActive(false);
        });

        await walletLoader.GetWalletDataFull();
        if (ignoreRepeated.isOn)
        {
            await walletLoader.FilterUnique();
        }
        await canvasManager.InitCanvases();

        mainUIBG.DOFade(0.0f, 0.3f).OnComplete(() => { mainUIBG.gameObject.SetActive(false); });
        loadingScreen.DOFade(0.0f, 0.3f).OnComplete(() => { loadingScreen.gameObject.SetActive(false); });
    }

    private void EnterMuseum()
    {
        LoadAssets().Forget();
    }

    private void LoadWallet()
    {
        LoadWalletCollections().Forget();
    }

    private void SwapCollections()
    {
        UIState = UISTATE.WalletConfig;
        canvasManager.ResetCanvases();
        sideBarMenu.DOFade(0.0f, 0.3f).OnComplete(() =>
        {
            sideBarMenu.gameObject.SetActive(false);
            collectionsWindow.gameObject.SetActive(true);
            collectionsWindow.DOFade(1.0f, 0.3f);
            mainUIBG.gameObject.SetActive(true);
            mainUIBG.DOFade(1.0f, 0.3f);
        });
    }

    private void SwapWallet()
    {
        UIState = UISTATE.WalletInput;
        canvasManager.ResetCanvases();
        sideBarMenu.DOFade(0.0f, 0.3f).OnComplete(() =>
        {
            sideBarMenu.gameObject.SetActive(false);
            walletWindow.gameObject.SetActive(true);
            walletWindow.DOFade(1.0f, 0.3f);
            mainUIBG.gameObject.SetActive(true);
            mainUIBG.DOFade(1.0f, 0.3f);
            walletAddress.text = string.Empty;
        });
    }

    private void SetLblText()
    {
        // Hide pagination elements if 'ignoreRepeated' is on
        if (UniqueOnly)
        {
            pageLbl.gameObject.SetActive(false);
            nextPageBtn.gameObject.SetActive(false);
            prevPageBtn.gameObject.SetActive(false);
            jumpPageBtn.gameObject.SetActive(false);
            jumpPageLbl.gameObject.SetActive(false);
            return;
        }
    
        int totalPages = walletLoader.GetTotalPages();
        int currentPage = walletLoader.CurrentPage;
    
        if (totalPages <= 1)
        {
            pageLbl.gameObject.SetActive(false);
            nextPageBtn.gameObject.SetActive(false);
            prevPageBtn.gameObject.SetActive(false);
            jumpPageBtn.gameObject.SetActive(false);
            jumpPageLbl.gameObject.SetActive(false);
        }
        else
        {
            pageLbl.gameObject.SetActive(true);
            jumpPageBtn.gameObject.SetActive(true);
            jumpPageLbl.gameObject.SetActive(true);
    
            prevPageBtn.gameObject.SetActive(currentPage > 1);
            nextPageBtn.gameObject.SetActive(currentPage < totalPages);
    
            pageLbl.text = string.Format("Room {0}/{1}", currentPage.ToString(), totalPages.ToString());
        }
    }

    private void LoadWalletByURL()
    {
        string url = Application.absoluteURL;
        string walletParameter = GetUrlParameter(url, "wallet");

        if (!string.IsNullOrEmpty(walletParameter))
        {
            walletAddress.text = walletParameter;
            LoadWallet();
        }
        else
        {
            walletWindow.DOFade(1.0f, 0.1f);
        }
    }

    private string GetUrlParameter(string url, string paramName)
    {
        try
        {
            var uri = new Uri(url);
            var query = uri.Query;
            var parameters = query.Split('&');

            foreach (var param in parameters)
            {
                var keyValue = param.Split('=');
                if (keyValue.Length == 2 && keyValue[0].TrimStart('?') == paramName)
                {
                    return keyValue[1];
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing URL: " + ex.Message);
        }

        return null;
    }
}