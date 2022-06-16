using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Assets.Script
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InitAsDisabledAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MonoSingletonAssetsAttribute: Attribute
    {
        public string path;
        public MonoSingletonAssetsAttribute(string path) => this.path = path;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotEditableAttribute: PropertyAttribute
    {

    }

#if UNITY_EDITOR
    //[CustomPropertyDrawer(typeof(NotEditableAttribute))]
    public class NotEditablePropertyDrawer: PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            UnityEngine.Random.InitState(property.displayName.GetHashCode());
            container.style.backgroundColor = UnityEngine.Random.ColorHSV();

            var handleLable = new PropertyField(property);
            //handleLable.SetEnabled(false);
            container.Add(handleLable);

            return container;
        }

        //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        //{
        //    var rowHeight = position.height;
        //    EditorGUI.LabelField(position, label, new GUIContent(property.stringValue));
        //    var nextRect = position;
        //    nextRect.y += rowHeight;
        //    position.height += rowHeight;
        //    EditorGUI.LabelField(nextRect, "F**k from OnGUI");
        //}

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return base.GetPropertyHeight(property, label) * 2;
        //}
    }
#endif

    public class Singleton<T> where T: new() { 

        private static readonly Lazy<T> lazy = new Lazy<T>(()=>new T());

        public static T Instance
        {
            get => lazy.Value;
        }
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T InitFromAssets(string path)
        {
            var res = Resources.Load(path) as GameObject;
            Debug.Assert(res != null);
            var globalGO = Instantiate(res);
            globalGO.name = typeof(T).Name;
            return globalGO.GetComponent<T>();
        }

        private static T Init()
        {
            var globalGO = new GameObject(typeof(T).Name);
            globalGO.SetActive(false);
            var comp = globalGO.AddComponent<T>();
            comp.enabled = !typeof(T).GetCustomAttributes(typeof(InitAsDisabledAttribute), false).Any();
            globalGO.SetActive(true);
            return comp;
        }

        private static readonly Lazy<T> lazy = new Lazy<T>(() =>
        {
            var assetsAttrs = typeof(T).GetCustomAttributes(typeof(MonoSingletonAssetsAttribute), false);
            T comp;
            if (assetsAttrs.Any())
            {
                var path = (assetsAttrs.First() as MonoSingletonAssetsAttribute).path;
                comp = InitFromAssets(path);
                Debug.Assert(comp != null);
            }
            else
            {
                comp = Init();
            }
            return comp;
        });

        public static T Instance
        {
            get => lazy.Value;
        }

        public static T Load()
        {
            return Instance;
        }

        //protected MonoSingleton() { }
    }

    public class LRU<T>
    {
        private Queue<T> container;
        private int cap;

        public LRU(int cap) {
            this.cap = cap;
            container = new Queue<T>(cap); 
        }

        public bool Cache(T element, out T removedElement)
        {
            bool removed = false;
            removedElement = default(T);
            if (container.Count == cap)
            {
                removedElement = container.Dequeue();
                removed = true;
            }
            container.Enqueue(element);
            return removed;
        }
    }

    public static partial class Utils
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }
    }

    public static class TerrainExtension
    {
        public static float MultiTerrainSampleHeight(Vector3 pos)
        {
            return Terrain.activeTerrains
                .Where(t => t.IsInTerrainChunk(XZPos(pos)))
                .Select(t => t.SampleHeight(pos))
                .DefaultIfEmpty(float.NegativeInfinity)
                .Max();
            //foreach (var t in Terrain.activeTerrains)
            //{
            //    if (IsInTerrainChunk(t, XZPos(pos)))
            //        return t.SampleHeight(pos);
            //}
            //return float.NegativeInfinity;
        }

        public static bool IsInTerrainChunk(this Terrain t, Vector2 xzPos)
        {
            var terrainPos = XZPos(t.GetPosition());
            var terrainSize = XZPos(t.terrainData.size);
            return new Rect(terrainPos, terrainSize).Contains(xzPos);
        }

        public static Vector2 XZPos(Vector3 pos) => new Vector2(pos.x, pos.z);
    }

    public static class AwaitExtension
    {
        private static readonly SendOrPostCallback sendOrPostCallback = (state) => (state as Action)();
        private static readonly SynchronizationContext backgroundCtx = new SynchronizationContext();
        public class UnityAsyncAwaiter : INotifyCompletion
        {
            private readonly SynchronizationContext context;
            private readonly Action complete;
            private readonly Func<bool> isCompleted;
            public bool IsCompleted => isCompleted();

            private void DefaultWaitUntilComplete()
            {
                while (!IsCompleted)
                    Task.Yield();
            }

            public UnityAsyncAwaiter(Func<bool> isCompleted, Action complete = null)
            {
                this.isCompleted = isCompleted;
                this.complete = complete ?? DefaultWaitUntilComplete;
                context = SynchronizationContext.Current ?? backgroundCtx;
            }

            public void OnCompleted(Action continuation)
            {
                context.Post(sendOrPostCallback, continuation);
            }

            public void GetResult()
            {
                complete();
            }
        }

        public static UnityAsyncAwaiter GetAwaiter(this JobHandle handle)
        {
            return new UnityAsyncAwaiter(() => handle.IsCompleted, handle.Complete);
        }



        public static UnityAsyncAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            return new UnityAsyncAwaiter(() => asyncOp.isDone);
        }
    }
}
