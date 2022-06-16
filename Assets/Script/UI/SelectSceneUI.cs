using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditorInternal;
using System.Linq;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
[Serializable]
public class AssetReferenceSceneAsset: AssetReferenceT<SceneAsset>, IKeyEvaluator
{
    public AssetReferenceSceneAsset(string guid) : base(guid) { }
}

public class SelectSceneUI : MonoBehaviour
{
    public const string SCENE_ADDRESSABLES_GROUP = "Scene";
    public AssetReferenceSceneAsset[] sceneList;

    [HideInInspector]
    public string[] sceneNameList;

    private int curSelectSceneIdx = 0;

    public int CurSelectSceneIdx
    {
        get => curSelectSceneIdx;
        private set
        {
            curSelectSceneIdx = value;
            load.interactable = CurSelectScene.Asset != null && !ActiveSceneNotFromSceneAssets(CurSelectScene);
        }
    }

    private bool ActiveSceneNotFromSceneAssets(AssetReferenceSceneAsset curSelectScene)
    {
        var assetGUID = curSelectScene.AssetGUID;
        var avtiveSceneGUID = RuntimeSceneManager.Instance.ActiveSceneGUID;
        return assetGUID == avtiveSceneGUID;
    }

    public TMPro.TMP_Dropdown dropdown;
    public Button load;

    public string CurSelectSceneName { get => sceneNameList[curSelectSceneIdx]; }
    public AssetReferenceSceneAsset CurSelectScene { get => sceneList[curSelectSceneIdx]; }

    void Start()
    {
        if (sceneNameList == null)
        {
            dropdown.ClearOptions();
            load.enabled = false;
        }
        else
        {
            dropdown.options = sceneNameList.Select(name => new TMPro.TMP_Dropdown.OptionData(name)).ToList();
            dropdown.value = 0;
            CurSelectSceneIdx = 0;
        }
    }

    public void SelectScene(int index)
    {
        CurSelectSceneIdx = index;
    }

    public void LoadScene() => RuntimeSceneManager.LoadScene(CurSelectScene);
}

#if UNITY_EDITOR
[CustomEditor(typeof(SelectSceneUI))]
public class SelectSceneUIEditor: Editor
{
    SerializedProperty sceneListProp;
    //ReorderableList sceneNameListCtrl;

    SelectSceneUI TargetUI
    {
        get => target as SelectSceneUI;
    }

    private void OnEnable()
    {
        sceneListProp = serializedObject.FindProperty("sceneList");
        //sceneNameListCtrl = new ReorderableList(serializedObject, sceneListProp, true, true, true, true);
        //sceneNameListCtrl.drawHeaderCallback = rect =>
        //{
        //    EditorGUI.LabelField(rect, sceneListProp.displayName);
        //};
        //sceneNameListCtrl.drawElementCallback = (rect, index, isActive, isFocus) => {
        //    var eleProp = sceneListProp.GetArrayElementAtIndex(index);
        //    EditorGUI.PropertyField(rect, eleProp);
        //};
        //sceneNameListCtrl.onChangedCallback = list =>
        //{
        //    TargetUI.sceneNameList = TargetUI.sceneList.Select(res => res.editorAsset.name).ToArray();
        //};
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var propItor = serializedObject.GetIterator();
        bool goDeep = true;
        var originIndent = EditorGUI.indentLevel;
        while (propItor.NextVisible(goDeep))
        {
            EditorGUI.indentLevel = propItor.depth;
            using (new EditorGUI.DisabledScope(!propItor.editable || propItor.propertyPath == "m_Script"))
            {
                if(SerializedProperty.EqualContents(propItor, sceneListProp))
                {
                    using(var check = new EditorGUI.ChangeCheckScope())
                    {
                        goDeep = EditorGUILayout.PropertyField(propItor);
                        if (check.changed)
                        {
                            var sceneNameList = TargetUI.sceneList.Select(res => res.editorAsset != null ? res.editorAsset.name : "null").ToArray();
                            TargetUI.sceneNameList = sceneNameList;
                        }
                    }
                }
                else
                {
                    goDeep = EditorGUILayout.PropertyField(propItor);
                }

            }
        }
        EditorGUI.indentLevel = originIndent;
        serializedObject.ApplyModifiedProperties();
    }
}

#endif