using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


#if false
using UnityEditor;
[CustomEditor(typeof(Assets.Protal.Portal))]
public class PortalEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
        {
            (target as Assets.Protal.Portal).Test();
        }
    }
}
#endif

namespace Assets.Protal
{
    public class Portal: MonoBehaviour
    {
        public Camera virtualCamera, originCamera;

        public Transform connectTarget;

        private RenderTexture camRT;

        public new MeshRenderer renderer;

        private void Start()
        {
            camRT = new RenderTexture(1920, 1080, 0);
            virtualCamera.targetTexture = camRT;
            var matProps = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(matProps);
            matProps.SetTexture("_PortalTex", camRT);
            renderer.SetPropertyBlock(matProps);
            //SetOriginCamera(Camera.current);
        }

        private void OnDestroy()
        {
            camRT.Release();
            camRT = null;
        }

        private void SetOriginCamera(Camera current)
        {
            originCamera = current;
            virtualCamera.fieldOfView = current.fieldOfView;
            virtualCamera.nearClipPlane = current.nearClipPlane;
            virtualCamera.farClipPlane = current.farClipPlane;
        }

        public bool test;

        private void Update()
        {
            Debug.Assert(originCamera != null);

            RenderPortal();
        }

        public static int renderIteration = 3;

        private static Matrix4x4 CalcWarpTrans(Transform @in, Transform @out)
        {
            Matrix4x4 virtualCamTransformMtx = @out.localToWorldMatrix;
            Matrix4x4 warpTransMtx = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, @out.lossyScale);
            return virtualCamTransformMtx * warpTransMtx * @in.worldToLocalMatrix;
        }

        private Matrix4x4 WarpTrans
        {
            get => CalcWarpTrans(transform, connectTarget);
        }

        private void RenderPortal()
        {
            for (int i = renderIteration; i > 0; i--)
            {
                SetVirtualCamTransform(transform, connectTarget, i);
                var newMatrix = originCamera.CalculateObliqueMatrix(GetPortalNearClip(connectTarget, virtualCamera));
                virtualCamera.projectionMatrix = newMatrix;
                virtualCamera.Render();
            }
        }

        private void SetVirtualCamTransform(Transform inTransform, Transform outTransform, int iteration)
        {
            var trans = CalcWarpTrans(inTransform, outTransform);
            for (int i = 1; i < iteration; i++)
                trans *= trans;

            var pos = trans.MultiplyPoint3x4(originCamera.transform.position);
            var lookAt = trans.MultiplyVector(originCamera.transform.forward);
            var up = trans.MultiplyVector(originCamera.transform.up);

            virtualCamera.transform.position = pos;
            virtualCamera.transform.rotation = Quaternion.LookRotation(lookAt, up);
        }

        private static Vector4 GetPortalNearClip(Transform outProtal, Camera virtualCamera)
        {
            Plane p = new Plane(outProtal.forward, outProtal.position);
            Vector4 clipPlane = new Vector4(p.normal.x, p.normal.y, p.normal.z, p.distance);
            return virtualCamera.worldToCameraMatrix.inverse.transpose * clipPlane;
        }


        private void OnDrawGizmos()
        {
            DrawCamGizmos(originCamera, Color.red);
            DrawCamGizmos(virtualCamera, Color.green);
        }

        private void DrawCamGizmos(Camera cam, Color color)
        {
            var m = Gizmos.matrix;
            var _color = Gizmos.color;

            Gizmos.color = color;
            Matrix4x4 nagtiveZ = Matrix4x4.identity;
            nagtiveZ.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, 0), new Vector3(1, 1, -1));
            Matrix4x4 matrixCam = cam.cameraToWorldMatrix * nagtiveZ;
            Gizmos.matrix = matrixCam;
            // center是平截头的顶端，即摄像机的位置。相对于自己是zero.
            Vector3 center = Vector3.zero;

            Gizmos.DrawRay(center, 4 * Vector3.forward);
            Gizmos.DrawFrustum(center, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);

            Gizmos.matrix = m;
            Gizmos.color = _color;
        }
    }
}
