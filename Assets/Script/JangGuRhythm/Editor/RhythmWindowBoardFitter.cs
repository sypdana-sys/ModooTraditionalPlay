// 선택한 메시 오브젝트를 RhythmWindow 크기에 맞추고 앞면에 사각 면(Quad)을 붙이는 에디터 메뉴.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FindOurSound.JangGuRhythm.EditorTools
{
    /// <summary>
    /// 선택한 오브젝트(예: 윷가락 asd_4)의 로컬 축 중 RhythmWindow의 오른쪽·위쪽과 가장 가까운 축을 찾아,
    /// 그 두 축의 실제 크기를 RhythmWindow의 월드 폭·높이와 같게 만든다. 두께 축은 그대로 둔다.
    /// 그다음 RhythmWindow를 보는 쪽 면에 같은 크기의 Quad(BoardFace)를 자식으로 붙인다.
    /// 회전·위치는 바꾸지 않는다. Ctrl+Z로 되돌릴 수 있다.
    /// </summary>
    public static class RhythmWindowBoardFitter
    {
        const string WindowObjectName = "RhythmWindow";
        const string FaceObjectName = "BoardFace";
        const string MaterialFolder = "Assets/Prefabs/JangGuRhythm/Materials";
        const string MaterialPath = MaterialFolder + "/RhythmWindowBoard.mat";

        // 면이 판 표면과 겹쳐 깜빡이지 않도록 띄우는 거리(m).
        const float FaceOffset = 0.002f;

        [MenuItem("Tools/JangGu Rhythm/Fit Selected To RhythmWindow And Add Face")]
        public static void FitSelected()
        {
            Transform target = Selection.activeTransform;
            MeshFilter meshFilter = target != null ? target.GetComponent<MeshFilter>() : null;
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogError("[JangGuRhythm] Hierarchy에서 MeshFilter가 있는 오브젝트(예: asd_4)를 선택한 뒤 실행하세요.");
                return;
            }

            GameObject windowObject = GameObject.Find(WindowObjectName);
            RectTransform window = windowObject != null ? windowObject.GetComponent<RectTransform>() : null;
            if (window == null)
            {
                Debug.LogError($"[JangGuRhythm] 씬에서 '{WindowObjectName}' RectTransform을 찾지 못했습니다.");
                return;
            }

            Vector3[] corners = new Vector3[4];
            window.GetWorldCorners(corners);
            Vector3 windowRight = corners[3] - corners[0];
            Vector3 windowUp = corners[1] - corners[0];
            float windowWidth = windowRight.magnitude;
            float windowHeight = windowUp.magnitude;

            int widthAxis = MostAlignedAxis(target, windowRight, -1);
            int heightAxis = MostAlignedAxis(target, windowUp, widthAxis);
            int thicknessAxis = 3 - widthAxis - heightAxis;

            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Undo.RecordObject(target, "Fit To RhythmWindow");

            Vector3 localScale = target.localScale;
            Vector3 lossy = target.lossyScale;
            localScale[widthAxis] *= windowWidth / (meshBounds.size[widthAxis] * Mathf.Abs(lossy[widthAxis]));
            localScale[heightAxis] *= windowHeight / (meshBounds.size[heightAxis] * Mathf.Abs(lossy[heightAxis]));
            target.localScale = localScale;

            AddFace(target, meshBounds, widthAxis, heightAxis, thicknessAxis, window.forward, windowWidth, windowHeight);

            EditorSceneManager.MarkSceneDirty(target.gameObject.scene);
            Debug.Log($"[JangGuRhythm] {target.name}의 폭·높이를 {windowWidth:F3}m × {windowHeight:F3}m로 맞추고 {FaceObjectName}을 붙였습니다. " +
                $"(폭 축 {"XYZ"[widthAxis]}, 높이 축 {"XYZ"[heightAxis]}, 두께 축 {"XYZ"[thicknessAxis]})");
        }

        static int MostAlignedAxis(Transform target, Vector3 worldDirection, int excludedAxis)
        {
            int best = -1;
            float bestDot = -1f;
            for (int i = 0; i < 3; i++)
            {
                if (i == excludedAxis) continue;

                Vector3 localAxis = Vector3.zero;
                localAxis[i] = 1f;
                float dot = Mathf.Abs(Vector3.Dot((target.rotation * localAxis).normalized, worldDirection.normalized));
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>
        /// 면의 축을 부모 축에 그대로 맞춰서(축 순서만 바꿈) 부모의 비균일 스케일 때문에 면이 찌그러지지 않게 한다.
        /// UI는 Canvas의 -forward 쪽에서 보이므로 면도 그쪽 표면에 붙이고 그쪽을 향하게 한다.
        /// </summary>
        static void AddFace(Transform target, Bounds meshBounds, int widthAxis, int heightAxis, int thicknessAxis,
            Vector3 windowForward, float width, float height)
        {
            Transform existing = target.Find(FaceObjectName);
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

            Vector3 thicknessLocal = Vector3.zero;
            thicknessLocal[thicknessAxis] = 1f;
            // 보는 사람 쪽(-windowForward)이 로컬 두께 축의 + 쪽인지 - 쪽인지.
            float viewerSide = Mathf.Sign(Vector3.Dot(target.rotation * thicknessLocal, -windowForward));

            Vector3 upLocal = Vector3.zero;
            upLocal[heightAxis] = 1f;

            GameObject face = new GameObject(FaceObjectName, typeof(MeshFilter), typeof(MeshRenderer));
            Undo.RegisterCreatedObjectUndo(face, "Add BoardFace");
            face.transform.SetParent(target, false);

            // Quad의 보이는 면은 -Z 쪽이므로 +Z를 보는 사람 반대쪽으로 둔다.
            face.transform.localRotation = Quaternion.LookRotation(-viewerSide * thicknessLocal, upLocal);

            Vector3 lossy = target.lossyScale;
            Vector3 position = meshBounds.center;
            position[thicknessAxis] += viewerSide * (meshBounds.extents[thicknessAxis] + FaceOffset / Mathf.Abs(lossy[thicknessAxis]));
            face.transform.localPosition = position;
            face.transform.localScale = new Vector3(
                width / Mathf.Abs(lossy[widthAxis]),
                height / Mathf.Abs(lossy[heightAxis]),
                1f / Mathf.Abs(lossy[thicknessAxis]));

            face.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            face.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial();
        }

        static Material GetOrCreateMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs/JangGuRhythm", "Materials");
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            Color color = new Color(0.93f, 0.89f, 0.8f);
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }
    }
}
