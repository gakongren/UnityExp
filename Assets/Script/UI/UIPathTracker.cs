using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Assets.Script
{
    [DisallowMultipleComponent]
    public class UIPathTracker: MonoBehaviour
    {
        [NotEditable]
        public string resPath;
        private void OnDestroy()
        {
            UIManager.Instance?.ReleaseUI(resPath);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UIPathTracker))]
    public class UIPathTrackerEditor : Editor
    {
        SerializedProperty resPath;
        VisualElement container;
        private void OnEnable()
        {
            resPath = serializedObject.FindProperty("resPath");
        }

        public override VisualElement CreateInspectorGUI()
        {
            container = new VisualElement();
            Random.InitState(target.name.GetHashCode());
            container.style.backgroundColor = Random.ColorHSV();

            var handleLable = new PropertyField(resPath);
            container.Add(handleLable);

            var imguiContainer = new IMGUIContainer(OnResCountGUI);
            container.Add(imguiContainer);

            return container;
        }

        private void OnResCountGUI()
        {
            EditorGUILayout.LabelField($"Res In-use Count", $"{resPath.stringValue}: {GetResInUseCount()}");
        }

        private int GetResInUseCount()
        {
            return UIManager.Instance.GetResInUseCount(resPath.stringValue);
        }
    }
#endif
}
