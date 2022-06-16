using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTitle : MonoBehaviour
{
    public UIManager UIManager
    {
        get => UIManager.Instance;
    }
    // Start is called before the first frame update
    async void Start()
    {
        await UIManager.AttachUIAsync("SelectScene");
        LoadScreen.Instance = (await UIManager.AttachUIAsync("LoadingScreen", false)).GetComponent<LoadScreen>();
    }
}
