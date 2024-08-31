using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoadingScript : MonoBehaviour
{
    public TextMeshProUGUI LoadingLbl;
    public int Rate;
    private bool isRunning = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        LoadingScreen().Forget();
    }

    private async UniTask LoadingScreen()
    {
        if (isRunning) { return; }
        isRunning = true;
        while (this.transform.parent.gameObject.activeSelf)
        {
            for (int i = 0; i < 4; i++) // Adjust the number 4 if you want more or fewer dots
            {
                LoadingLbl.text = "Loading " + new string('•', i).Replace("•", "<color=#4EC6E1>•</color> ");
                await UniTask.Delay(Rate);

                if (!this.transform.parent.gameObject.activeSelf) break; // Break the loop if loading is no longer needed
            }
        }
        if (!this.gameObject.activeSelf) { isRunning = false; return; }
    }
}
