using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC.Samples
{
    // カメラコンポーネントが付いているオブジェクトに付けてください
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class FrustumManager2 : MonoBehaviour
    {
        Camera m_cam = null;

        // 視錘台の前後面の距離（描画範囲）。値はプロジェクトにより調整
        [SerializeField] float m_nearFarDistance;

        // スクリーン用のQuard
        [SerializeField] GameObject m_Screan = null;




        void Start()
        {
            m_cam = GetComponent<Camera>();
        }

        void Update()
        {
            if (m_Screan != null && m_cam != null)
            {
                SetFrustumFromQuad(m_cam, m_Screan);
                // m_cam.nearClipPlane = 1;
            }
        }

        void SetFrustumFromQuad(Camera _cam, GameObject _screenQuad)
        {
            // 射影行列
            float _left;
            float _right;
            float _top;
            float _bottom;
            float _near;
            float _far;

            // _screenQuadのZ回転（ロール）をロック
            _screenQuad.transform.localEulerAngles = new Vector3(_screenQuad.transform.localEulerAngles.x, _screenQuad.transform.localEulerAngles.y, 0);

            // 視錘台の前後面を決める
            _near = GetDistanceFromQuad(_cam, _screenQuad);
            _cam.nearClipPlane = _near/20;
            _far = _near + m_nearFarDistance;
            _cam.farClipPlane = _far;

            // カメラの姿勢をセット
            _cam.transform.forward = m_Screan.transform.forward;

            // 視錘台の上下左右面を決定する点を決める
            {
                Matrix4x4 _worldToCamMat = _cam.worldToCameraMatrix;
                Mesh _screenQuadMesh = _screenQuad.GetComponent<MeshFilter>().sharedMesh;

                // _screenQuadの角の頂点の位置をローカル空間からワールド空間に変換
                Vector3 _screenVertexLeftBottom = _screenQuad.transform.localToWorldMatrix.MultiplyPoint(_screenQuadMesh.vertices[0]);
                Vector3 _screenVertexRight = _screenQuad.transform.localToWorldMatrix.MultiplyPoint(_screenQuadMesh.vertices[1]);
                Vector3 _screenVertexTop = _screenQuad.transform.localToWorldMatrix.MultiplyPoint(_screenQuadMesh.vertices[2]);
                

                // ワールド空間からカメラ空間への変換
                Vector3 _screenQuadleftBottomPoint = _worldToCamMat.MultiplyPoint(_screenVertexLeftBottom);
                Vector3 _screenQuadrightPoint = _worldToCamMat.MultiplyPoint(_screenVertexRight);
                Vector3 _screenQuadtopPoint = _worldToCamMat.MultiplyPoint(_screenVertexTop);

                // 射影行列へ代入
                _left = _screenQuadleftBottomPoint.x;
                _bottom = _screenQuadleftBottomPoint.y;
                _right = _screenQuadrightPoint.x;
                _top = _screenQuadtopPoint.y;
                
            }

            // 射影行列をカメラの視錘台にセット
            // https://docs.unity3d.com/ja/2020.3/ScriptReference/Matrix4x4.Frustum.html
            _cam.projectionMatrix = Matrix4x4.Frustum(_left/20, _right/20, _bottom/20, _top/20, _near/20, _far);
        }

        // カメラと_screenQuadの距離を計算し返す
        float GetDistanceFromQuad(Camera _cam, GameObject _screenQuad)
        {
            Vector3 _qNormal = _screenQuad.transform.forward.normalized;
            Vector3 _qPos = _cam.transform.position - _screenQuad.transform.position;

            float _distance = Mathf.Abs(Vector3.Dot(_qNormal, _qPos));

            return _distance;
        }
    }
}