// 기본 도형으로 궁채(GungChe)·열채(YeolChe) 프리팹과 머티리얼을 생성하는 에디터 메뉴.
using UnityEditor;
using UnityEngine;

namespace FindOurSound.JangGuRhythm.EditorTools
{
    /// <summary>
    /// 프리팹 루트(0,0,0)가 손으로 쥐는 지점이고 채는 +Z 방향으로 뻗는다.
    /// 컨트롤러의 StickPivot 아래에 위치 0으로 넣으면 손잡이가 손에 온다.
    /// 타격 Collider는 실제로 장구를 치는 끝부분(궁채 공, 열채 가는 끝)에만 둔다.
    /// 여러 번 실행해도 같은 경로에 덮어써서 재생성된다.
    /// </summary>
    public static class JangGuStickBuilder
    {
        const string StickFolder = "Assets/Prefabs/JangGuRhythm/Sticks";
        const string MaterialFolder = StickFolder + "/Materials";

        const string GungChePrefabPath = StickFolder + "/GungChe.prefab";
        const string YeolChePrefabPath = StickFolder + "/YeolChe.prefab";

        [MenuItem("Tools/JangGu Rhythm/Build Stick Prefabs (GungChe, YeolChe)")]
        public static void Build()
        {
            EnsureFolder(MaterialFolder);

            Material grip = GetOrCreateMaterial("Stick_Grip", new Color(0.08f, 0.08f, 0.08f), 0.35f);
            Material lightWood = GetOrCreateMaterial("Stick_LightWood", new Color(0.88f, 0.78f, 0.58f), 0.3f);
            Material ballWood = GetOrCreateMaterial("Stick_BallWood", new Color(0.62f, 0.45f, 0.3f), 0.4f);
            Material bamboo = GetOrCreateMaterial("Stick_Bamboo", new Color(0.8f, 0.66f, 0.38f), 0.35f);

            BuildGungChe(grip, lightWood, ballWood);
            BuildYeolChe(bamboo);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[JangGuRhythm] 장구채 프리팹을 생성했습니다: {GungChePrefabPath}, {YeolChePrefabPath}");
        }

        /// <summary>궁채: 검은 손잡이 + 가는 나무 자루 + 끝의 나무 공. 왼손용.</summary>
        static void BuildGungChe(Material grip, Material shaft, Material ball)
        {
            GameObject root = new GameObject("GungChe");

            CreateCylinderAlongZ("Grip", root.transform, startZ: -0.05f, endZ: 0.1f, diameter: 0.024f, grip);
            CreateCylinderAlongZ("Shaft", root.transform, startZ: 0.1f, endZ: 0.37f, diameter: 0.009f, shaft);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 0f, 0.385f);
            head.transform.localScale = Vector3.one * 0.038f;
            head.GetComponent<Renderer>().sharedMaterial = ball;

            SavePrefab(root, GungChePrefabPath);
        }

        /// <summary>열채: 넓고 납작한 손잡이 + 가늘고 긴 대나무 막대. 가는 끝으로 친다. 오른손용.</summary>
        static void BuildYeolChe(Material bamboo)
        {
            GameObject root = new GameObject("YeolChe");

            GameObject grip = CreateBoxAlongZ("Grip", root.transform, startZ: -0.05f, endZ: 0.08f, width: 0.02f, thickness: 0.004f, bamboo);
            Object.DestroyImmediate(grip.GetComponent<Collider>());

            GameObject shaft = CreateBoxAlongZ("Shaft", root.transform, startZ: 0.08f, endZ: 0.45f, width: 0.008f, thickness: 0.006f, bamboo);
            // 막대 끝 20%만 타격 범위로 쓴다. 가는 막대가 빠른 스윙에서 빠져나가지 않도록 폭은 2배로 둔다.
            BoxCollider tipCollider = shaft.GetComponent<BoxCollider>();
            tipCollider.center = new Vector3(0f, 0f, 0.4f);
            tipCollider.size = new Vector3(2f, 2f, 0.2f);

            SavePrefab(root, YeolChePrefabPath);
        }

        static void CreateCylinderAlongZ(string name, Transform parent, float startZ, float endZ, float diameter, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, (startZ + endZ) * 0.5f);
            // 기본 Cylinder는 Y축 방향 높이 2이므로 X축으로 90도 눕히고 Y 스케일을 길이의 절반으로 둔다.
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(diameter, (endZ - startZ) * 0.5f, diameter);
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static GameObject CreateBoxAlongZ(string name, Transform parent, float startZ, float endZ, float width, float thickness,
            Material material)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, (startZ + endZ) * 0.5f);
            go.transform.localScale = new Vector3(width, thickness, endZ - startZ);
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        static Material GetOrCreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
