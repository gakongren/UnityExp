using Assets.Script;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[MonoSingletonAssets("LoadingScreen")]
public class LoadScreen : MonoBehaviour
{
    public Slider progress;
    public TMPro.TMP_Text tipsOnComplete;
    public Button bgCloseBtn;

    private float curProgress;
    public float CurProgress
    {
        get => curProgress;
        private set
        {
            progress.value = value;
            curProgress = value;
        }
    }

    private Func<float> progressGetter;
    private Func<bool> ifCompleteGetter;

    public static LoadScreen Instance
    {
        get; set;
    }

    //private void Start()
    //{
    //    Instance = this;
    //}

    private void Awake()
    {
        ifCompleteGetter = DefaultGetIfComplete;
    }

    public void StartLoadOperation(Func<float> progressGetter, Func<bool> ifCompleteGetter = null)
    {
        CurProgress = 0;
        this.progressGetter = progressGetter;
        this.ifCompleteGetter = ifCompleteGetter ?? DefaultGetIfComplete;
        gameObject.SetActive(true);
        tipsOnComplete.gameObject.SetActive(false);
        bgCloseBtn.enabled = false;
    }

    public void StartLoadOperation(AsyncOperation asyncOp)
    {
        StartLoadOperation(() => asyncOp.progress, () => asyncOp.isDone);
    }

    public void StartLoadOperation<T>(AsyncOperationHandle<T> asyncOp)
    {
        StartLoadOperation(() => asyncOp.PercentComplete, () => asyncOp.IsDone);
    }

    bool DefaultGetIfComplete()
    {
        return CurProgress >= 1;
    }

    private void Update()
    {
        CurProgress = progressGetter?.Invoke() ?? 0;
        var loadCompleted = ifCompleteGetter();
        if (loadCompleted)
        {
            OnLoadCompleted();
        }
    }

    private void OnLoadCompleted()
    {
        tipsOnComplete.gameObject.SetActive(true);
        bgCloseBtn.enabled = true;
    }

    public void Click()
    {
        gameObject.SetActive(false);
        progressGetter = null;
        ifCompleteGetter = DefaultGetIfComplete;
    }
}