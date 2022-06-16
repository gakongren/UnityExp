using Assets.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public Canvas uiRoot;

    private Dictionary<string, AsyncOperationHandle<GameObject>> resHandleCache = new Dictionary<string, AsyncOperationHandle<GameObject>>();

    private Dictionary<string, int> resHandleInUseCount = new Dictionary<string, int>();

    public static Transform UIRootTranform
    {
        get => Instance.uiRoot.transform;
    }

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(uiRoot.gameObject);
    }

    internal int GetResInUseCount(string uiResPath)
    {
        resHandleInUseCount.TryGetValue(uiResPath, out int count);
        return count;
    }

    internal void ReleaseUI(string uiResPath, bool releaseIfNotInUse = false)
    {
        Debug.Assert(uiResPath != null);
        if(resHandleCache.TryGetValue(uiResPath, out var handle))
        {
            if(resHandleInUseCount.TryGetValue(uiResPath, out int count) && count > 0)
            {
                resHandleInUseCount[uiResPath] = --count;
                if(releaseIfNotInUse && count == 0)
                {
                    ReleaseFromCache(uiResPath);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Release operation for UI: {uiResPath} is invalid. In-use count of res[{handle.DebugName}] is {count}."
                );
            }
        }
        else
        {
            throw new InvalidOperationException(
                $"Release operation for UI: {uiResPath} is invalid since res[{handle.DebugName}] is not loaded."
            );
        }
    }

    // Update is called once per frame
    public async Task<GameObject> AttachUIAsync(string uiResPath, bool active=true)
    {
        var op = GetFromCache(uiResPath);
        await op.Task;
        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            var prefab = op.Result;
            var go = Instantiate(prefab, uiRoot.transform);
            bool trackerIsFound = go.TryGetComponent<UIPathTracker>(out var tracker);
            if (!trackerIsFound)
                tracker = go.AddComponent<UIPathTracker>();
            tracker.resPath = uiResPath;
            go.SetActive(active);
            return go;
        }
        else
        {
            Debug.LogError(op.OperationException);
            return null;
        }
    }

    private AsyncOperationHandle<GameObject> GetFromCache(string uiResPath)
    {
        bool cached = resHandleCache.TryGetValue(uiResPath, out var handle);
        if (!cached)
        {
            handle = Addressables.LoadAssetAsync<GameObject>(uiResPath);
            resHandleCache[uiResPath] = handle;
        }
        resHandleInUseCount.TryGetValue(uiResPath, out int count);
        resHandleInUseCount[uiResPath] = ++count;

        return handle;
    }

    private void ReleaseFromCache(string uiResPath)
    {
        bool cached = resHandleCache.TryGetValue(uiResPath, out var handle);
        Debug.Assert(cached);
        resHandleCache.Remove(uiResPath);
        Addressables.Release(handle);
    }
}
