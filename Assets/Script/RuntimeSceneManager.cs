using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class RuntimeSceneManager : ScriptableObject
{
    private string activeSceneGUID;
    public string ActiveSceneGUID {
        get => activeSceneGUID;
        private set
        {
            activeSceneGUID = value;
            Debug.Log($"ActiveSceneGUID: {activeSceneGUID}");
        }
    }

    private static Lazy<RuntimeSceneManager> inst = new Lazy<RuntimeSceneManager>(() => CreateInstance<RuntimeSceneManager>());

    public static RuntimeSceneManager Instance { get => inst.Value; }

    public static AsyncOperationHandle<SceneInstance> LoadScene(AssetReference sceneRes)
    {
        //var op = Addressables.LoadSceneAsync(sceneRes);
        var op = sceneRes.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single, true);
        SetActiveSceneGuidOnCompleteAsync(sceneRes.AssetGUID, op);
        LoadScreen.Instance.StartLoadOperation(op);
        return op;
    }

    private static async void SetActiveSceneGuidOnCompleteAsync(string assetGUID, AsyncOperationHandle<SceneInstance> op)
    {
        await op.Task;
        Instance.ActiveSceneGUID = assetGUID;
    }

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        Application.quitting += Instance.Reset;
    }

    private void Reset()
    {
        ActiveSceneGUID = string.Empty;
    }
}
