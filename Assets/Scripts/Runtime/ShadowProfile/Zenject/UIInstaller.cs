using UnityEngine;
using Zenject;
using TMPro;
using UnityEngine.UI;

namespace ShadowProfile
{
    public class UIInstaller : MonoInstaller
    {
        [Header("MANAGERS")] [SerializeField] private WalletLoader walletLoader;
        [SerializeField] private CanvasManager canvasManager;
        [SerializeField] private CoreLogic coreLogic;
        [SerializeField] private PlayerMovement player;


        [Header("UI ELEMENTS")] [SerializeField]
        private TMP_InputField walletAddress;

        [SerializeField] private Toggle canShowRepeated;
        [SerializeField] private Button enterMuseumBtn;
        [SerializeField] private Button showWalletBtn;
        [SerializeField] private TMP_Dropdown walletCollections;
        [SerializeField] private CanvasGroup walletWindow;
        [SerializeField] private CanvasGroup collectionsWindow;
        [SerializeField] private CanvasGroup mainUIBG;
        [SerializeField] private CanvasGroup loadingScreen;
        [SerializeField] private Texture2D missingImg;
        [SerializeField] private TextMeshProUGUI statusLbl;
        [SerializeField] private TextMeshProUGUI loadingLbl;


        [Header("UI ELEMENTS - SIDEBAR")] [SerializeField]
        private CanvasGroup inGameUI;

        [SerializeField] private Button nextPageBtn;
        [SerializeField] private Button prevPageBtn;
        [SerializeField] private Button jumpPageBtn;
        [SerializeField] private Button swapCollectionBtn;
        [SerializeField] private Button swapWalletBtn;
        [SerializeField] private TextMeshProUGUI pagesLbl;
        [SerializeField] private TextMeshProUGUI pageStatusLbl;
        [SerializeField] private TMP_InputField jumpPageLbl;

        [Header("UI ELEMENTS - NFT DETAILS")] [SerializeField]
        private CanvasGroup nftDetailParent;

        [SerializeField] private TextMeshProUGUI nftName;
        [SerializeField] private TextMeshProUGUI nftRarity;
        [SerializeField] private TextMeshProUGUI nftDescription;
        [SerializeField] private TextMeshProUGUI interactionLbl;
        [SerializeField] private TextMeshProUGUI nftCollection;
        [SerializeField] private RawImage nftImage;


        public override void InstallBindings()
        {
            // Managers
            Container.Bind<WalletLoader>().FromInstance(walletLoader).AsSingle();
            Container.Bind<CanvasManager>().FromInstance(canvasManager).AsSingle();
            Container.Bind<CoreLogic>().FromInstance(coreLogic).AsSingle();
            Container.Bind<PlayerMovement>().FromInstance(player).AsSingle();

            // UI elements
            Container.Bind<Button>().WithId("showWalletBtn").FromInstance(showWalletBtn);
            Container.Bind<Button>().WithId("enterMuseumBtn").FromInstance(enterMuseumBtn);
            Container.Bind<TMP_InputField>().WithId("walletAdd").FromInstance(walletAddress);
            Container.Bind<TMP_Dropdown>().WithId("walletCollections").FromInstance(walletCollections);
            Container.Bind<Toggle>().WithId("ignoreRepeated").FromInstance(canShowRepeated);
            Container.Bind<CanvasGroup>().WithId("walletWindow").FromInstance(walletWindow);
            Container.Bind<CanvasGroup>().WithId("collectionsWindow").FromInstance(collectionsWindow);
            Container.Bind<CanvasGroup>().WithId("mainUIBG").FromInstance(mainUIBG);
            Container.Bind<CanvasGroup>().WithId("loadingScreen").FromInstance(loadingScreen);
            Container.Bind<Texture2D>().WithId("missingImg").FromInstance(missingImg);
            Container.Bind<TextMeshProUGUI>().WithId("statusLbl").FromInstance(statusLbl);
            Container.Bind<TextMeshProUGUI>().WithId("loadingLbl").FromInstance(loadingLbl);

            // UI elements - Sidebar
            Container.Bind<CanvasGroup>().WithId("inGameSidebar").FromInstance(inGameUI);
            Container.Bind<Button>().WithId("nextPageBtn").FromInstance(nextPageBtn);
            Container.Bind<Button>().WithId("prevPageBtn").FromInstance(prevPageBtn);
            Container.Bind<Button>().WithId("jumpPageBtn").FromInstance(jumpPageBtn);
            Container.Bind<Button>().WithId("swapCollectionBtn").FromInstance(swapCollectionBtn);
            Container.Bind<Button>().WithId("swapWalletBtn").FromInstance(swapWalletBtn);
            Container.Bind<TextMeshProUGUI>().WithId("pagesLbl").FromInstance(pagesLbl);
            Container.Bind<TMP_InputField>().WithId("jumpPageLbl").FromInstance(jumpPageLbl);
            Container.Bind<TextMeshProUGUI>().WithId("pageStatusLbl").FromInstance(pageStatusLbl);

            // UI elements - NFT Detail
            Container.Bind<CanvasGroup>().WithId("nftDetailParent").FromInstance(nftDetailParent);
            Container.Bind<TextMeshProUGUI>().WithId("nftName").FromInstance(nftName);
            Container.Bind<TextMeshProUGUI>().WithId("nftRarity").FromInstance(nftRarity);
            Container.Bind<TextMeshProUGUI>().WithId("nftDescription").FromInstance(nftDescription);
            Container.Bind<TextMeshProUGUI>().WithId("interactionLbl").FromInstance(interactionLbl);
            Container.Bind<TextMeshProUGUI>().WithId("nftCollection").FromInstance(nftCollection);
            Container.Bind<RawImage>().WithId("nftImage").FromInstance(nftImage);
        }
    }
}