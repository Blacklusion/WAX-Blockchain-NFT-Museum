using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ShadowProfile;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Zenject;
using System;
using System.Linq;


#region JSON Class
public class AccountInfo
{
    [JsonProperty("success")]
    public string Success;
    [JsonProperty("data")]
    public string Data;
}

public class CollectionsResultData
{
    [JsonProperty("success")]
    public string Success;

    [JsonProperty("data")]
    public CollectionData Data;
}

public class CollectionData
{
    [JsonProperty("collections")]
    public List<CollectionHolder> Collections = new();
}

public class CollectionHolder
{
    [JsonProperty("collection")]
    public Collection Coll;
}

public class Collection
{
    [JsonProperty("name")]
    public string Name;
    [JsonProperty("collection_name")]
    public string WalletName;
}

public class ResultData
{
    [JsonProperty("success")]
    public string Success;

    [JsonProperty("data")]
    public List<NFTData> Data = new();
}

public class NFTData
{
    [JsonProperty("data")]
    public NFDataDetails Details;

    [JsonProperty("collection")]
    public Collection Coll;

    [JsonProperty("template")]
    public NFTTemplate Template;
}

public class NFDataDetails
{
    [JsonProperty("img")]
    private string img = string.Empty;

    [JsonProperty("image")]
    private string image = string.Empty;

    [JsonProperty("video")]
    private string video = string.Empty;

    public string Image
    {
        get
        {
            if (!string.IsNullOrEmpty(image))
            {
                return image;
            }
            else if (!string.IsNullOrEmpty(img))
            {
                return img;
            }
            else
            {
                return video;
            }
        }
    }

    [JsonProperty("name")]
    public string Name;

    [JsonProperty("rarity")]
    public string Rarity;

    [JsonProperty("description")]
    public string Description;
}
public class NFTTemplate
{
    [JsonProperty("template_id")]
    public string ID;
}
#endregion
public class WalletLoader : MonoBehaviour
{
    [Inject(Id = "walletAdd")] private readonly TMP_InputField walletAddress;
    [Inject(Id = "walletCollections")] private readonly TMP_Dropdown walletCollections;
    [Inject(Id = "statusLbl")] private readonly TextMeshProUGUI statusLbl;
    [Inject(Id = "loadingLbl")] private readonly TextMeshProUGUI loadingLbl;
    [Inject] private readonly CoreLogic coreLogic;

    private bool reachedEnd = false;
    private int pageNumber = 1;
    private int startIndexUnique = 0;
    private int accountTotal = 0;
    private HashSet<string> uniqueFilter = new();
    private ResultData results = new();
    private CollectionsResultData collectionResults = new();
    public int CurrentPage { get; private set; } = 1;

    public HashSet<string> templateBlackList = new();

    public int TotalCanvasesToLoad
    {
        get { return 93; }
        private set { }
    }

    public Dictionary<int, string> UniqueItems { get; private set; } = new();
    public Dictionary<int, string> AllItems { get; private set; } = new();

    private void AllElementsHashTable()
    {
        int startIndex = AllItems.Count(); // make sure we're clean

        for (int i = startIndex; i < results.Data.Count; i++)
        {
            AllItems.Add(i, results.Data[i].Details.Image);
        }
    }

    public void ResetElements()
    {
        uniqueFilter = new();
        results = new();
        startIndexUnique = 0;
        reachedEnd = false;
        UniqueItems = new();
        AllItems = new();
    }

    public void ResetPages()
    {
        pageNumber = 1;
    }

    public async UniTask FilterUnique()
    {
        for (int i = startIndexUnique; i < results.Data.Count; i++)
        {
            if (UniqueItems.Count >= TotalCanvasesToLoad - 1)
            {
                break;
            }

            NFDataDetails detail = results.Data[i].Details;
            if (!string.IsNullOrEmpty(detail.Image))
            {
                // If the hashtable does not contain the Image as a key, add it to the hashtable
                if (!uniqueFilter.Contains(detail.Image))
                {
                    templateBlackList.Add(results.Data[i].Template.ID);
                    uniqueFilter.Add(detail.Image);
                    // Store the index of the NFDataDetails object in the list
                    UniqueItems.Add(i, detail.Image);
                }
            }
        }

        startIndexUnique = results.Data.Count; // store the index for the next pass, that way we don't need to parse everything again

        if (UniqueItems.Count < TotalCanvasesToLoad - 1) // TODO calculate how many images we need yet to load to fill. If we have more than 100 spots, then request 100 at one time. 
        {
            if (reachedEnd) { return; } // no more stuff to load

            // we can load more, try one more page
            pageNumber++;
            await GetWalletDataFull();
            await FilterUnique();
        }
    }

    public void LoadData()
    {
        pageNumber = 1;
    }

    public async void LoadWalletCollections()
    {
        await GetWalletCollections();
    }

    public async UniTask  GetWalletCollections()
    {
        string result = await WebRequestHandler.GetAsync(Consts.BASE_COLLECTIONS_URL + walletAddress.text);
        collectionResults = JsonConvert.DeserializeObject<CollectionsResultData>(result);

        if (collectionResults.Data.Collections == null || collectionResults.Data.Collections.Count == 0)
        {
            throw new Exception("Wallet not found");
        }

        // Order the Data list by the Name property
        collectionResults.Data.Collections = collectionResults.Data.Collections.OrderBy(c => c.Coll.Name).ToList();

        walletCollections.ClearOptions();

        await UniTask.Delay(10);
        List<TMP_Dropdown.OptionData> options = new();
        options.Add(new TMP_Dropdown.OptionData("---- ALL ----"));

        for (int i = 0; i < collectionResults.Data.Collections.Count; i++)
        {
            options.Add(new TMP_Dropdown.OptionData(collectionResults.Data.Collections[i].Coll.Name));
        }

        walletCollections.AddOptions(options);
        await UniTask.Delay(10);
        walletCollections.value = 0; // makes ALL as default value
    }

    public async UniTask GetWalletCount()
    {
        string result = await WebRequestHandler.GetAsync(Consts.COUNT_URL + walletAddress.text);
        var info = JsonConvert.DeserializeObject<AccountInfo>(result);
        accountTotal = int.Parse(info.Data);
    }

    public async UniTask GetWalletDataFull()
    {
        string url = string.Format(Consts.BASE_URL + "owner={0}&page={1}&limit={2}&order=desc&sort=asset_id&template_blacklist={3}", walletAddress.text, pageNumber, TotalCanvasesToLoad, GetBlackList());

        if (walletCollections.value > 0)
        {
            // -1 because list on dropdown starts at index 1 not 0. 
            url = string.Format(Consts.BASE_URL + "collection_name={0}&owner={1}&page={2}&limit={3}&order=desc&sort=asset_id&template_blacklist={4}", collectionResults.Data.Collections[walletCollections.value - 1].Coll.WalletName, walletAddress.text, pageNumber, TotalCanvasesToLoad, GetBlackList());
        }

        Debug.Log(url);
        string result = await WebRequestHandler.GetAsync(url);

        var res = JsonConvert.DeserializeObject<ResultData>(result);

        if (res.Data.Count == 0)
        {
            reachedEnd = true;
            AllElementsHashTable();
            return;
        }

        results.Data.AddRange(res.Data);
        AllElementsHashTable();
    }

    private string GetBlackList()
    {
        if (templateBlackList.Count == 0) { return string.Empty; }
        string blacklist = string.Empty;

        foreach (var item in templateBlackList)
        {
            blacklist += item + ",";
        }

        return blacklist.Remove(blacklist.Length - 1, 1);
    }

    public async UniTask LoadNextPage()
    {
        if (reachedEnd) { return; } // we can't load more pages, go back if you want
        ResetElements();
        CurrentPage++;
        pageNumber++;
        await GetWalletDataFull();
    }

    public async UniTask LoadPreviousPage()
    {
        ResetElements();
        CurrentPage--;
        pageNumber--;
        pageNumber = Mathf.Clamp(pageNumber, 1, Int32.MaxValue);
        await GetWalletDataFull();
    }

    public async UniTask JumpToPage(int page)
    {
        pageNumber = Mathf.Clamp(page, 1, GetTotalPages());
        CurrentPage = pageNumber;
        ResetElements();
        await GetWalletDataFull();
    }

    public string GetNameByID(int id)
    {
        return results.Data[id].Details.Name;
    }

    public string GetRarityByID(int id)
    {
        return results.Data[id].Details.Rarity;
    }

    public string GetDescriptionByID(int id)
    {
        return results.Data[id].Details.Description;
    }

    public string GetCollectionByID(int id)
    {
        return results.Data[id].Coll.Name;
    }

    public Dictionary<int, string> GetItems()
    {
        if (coreLogic.UniqueOnly)
        {
            return UniqueItems;
        }

        return AllItems;
    }

    public int GetTotalPages()
    {
        return accountTotal / TotalCanvasesToLoad;
    }
}