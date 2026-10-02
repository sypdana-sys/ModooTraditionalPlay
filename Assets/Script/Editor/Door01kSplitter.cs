// SM_Door01k 메시를 틀(기둥·경첩), 왼쪽·오른쪽 문짝, 중앙 돌로 나눠 프리팹을 만드는 에디터 메뉴.
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FindOurSound.EditorTools
{
    /// <summary>
    /// 원본 메시는 서로 떨어진 조각(문살, 가로대, 기둥, 경첩, 돌 등)의 묶음이다.
    /// 붙어 있는 삼각형끼리 한 조각으로 묶은 뒤, 조각 위치로 부분을 나눈다. 기준 위치는 메시 원점(두 문짝 사이 틈)이다.
    /// - 돌: 바닥에 붙은 낮은 조각
    /// - 틀: 안쪽 가장자리까지 원점에서 충분히 먼 바깥 기둥과 경첩. 고정되어 움직이지 않는다.
    /// - 문짝: 나머지를 폭 방향 부호로 왼쪽(-)·오른쪽(+)으로 나눈다. 좌우 이름은 메시 로컬 축 기준이다.
    /// 문짝 메시는 경첩 쪽 바깥 모서리를 원점으로 옮겨서, 문짝 오브젝트를 Y축으로 돌리면 문이 열린다.
    /// 원본 에셋은 수정하지 않고 Assets/Prefabs/Door01kSplit에 새 메시와 프리팹을 만든다.
    /// </summary>
    public static class Door01kSplitter
    {
        const string SourceMeshPath = "Assets/Naganeupseong/Resource/Meshes/Door/SM_Door01k.fbx";
        const string SourceMeshName = "SM_Door01k";
        const string MaterialPath = "Assets/Naganeupseong/Resource/Materials/M_Door01k.mat";

        const string OutputFolder = "Assets/Prefabs/Door01kSplit";
        const string MeshFolder = OutputFolder + "/Meshes";

        // 기준값은 원본 전체 폭(187.2cm)에 대한 비율이다. 가져오기 단위(cm/m)와 상관없이 같게 판정한다.
        // 원본 측정값: 돌 높이 약 6cm. 안쪽 가장자리 기준 경첩 77.6cm 이상, 문짝 쪽 문틀 기둥 72cm 이하.
        const float SourceWidthCm = 187.2f;
        const float StoneMaxHeightCm = 10f;
        const float FrameInnerEdgeCm = 77f;

        // 좌표가 같은 정점은 UV·노멀 경계로 나뉜 같은 점으로 보고 한 조각으로 묶는다.
        const float WeldPrecision = 10000f;

        enum Part
        {
            Frame,
            LeftDoor,
            RightDoor,
            Stone,
        }

        [MenuItem("Tools/Door/Split SM_Door01k (Frame, Doors, Stone)")]
        public static void Split()
        {
            Mesh source = LoadSourceMesh();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (source == null) return;
            if (!source.isReadable)
            {
                Debug.LogError($"[Door01kSplitter] {SourceMeshPath}의 Read/Write가 꺼져 있습니다. 모델 Import 설정에서 켜고 다시 실행하세요.");
                return;
            }

            Bounds bounds = source.bounds;
            // 폭 방향은 수평 축 중 더 긴 쪽이다. 두께 방향은 나머지 수평 축이다.
            int widthAxis = bounds.size.x >= bounds.size.z ? 0 : 2;
            int depthAxis = widthAxis == 0 ? 2 : 0;

            Vector3[] vertices = source.vertices;
            List<int>[] trianglesBySubmesh = ReadTriangles(source);
            int[] componentOfVertex = FindComponents(vertices, trianglesBySubmesh);
            Dictionary<int, Bounds> componentBounds = MeasureComponents(vertices, componentOfVertex);

            Dictionary<int, Part> partOfComponent = new Dictionary<int, Part>();
            foreach (KeyValuePair<int, Bounds> pair in componentBounds)
            {
                partOfComponent[pair.Key] = Classify(pair.Value, bounds, widthAxis);
            }

            EnsureFolder(MeshFolder);

            GameObject assembly = new GameObject("Door01k_Split");
            foreach (Part part in System.Enum.GetValues(typeof(Part)))
            {
                Vector3 pivot = GetPivot(part, vertices, componentOfVertex, partOfComponent, widthAxis, depthAxis);
                Mesh partMesh = BuildPartMesh(source, part, pivot, trianglesBySubmesh, componentOfVertex, partOfComponent);
                if (partMesh == null)
                {
                    Debug.LogWarning($"[Door01kSplitter] {part}에 해당하는 조각이 없어 건너뜁니다.");
                    continue;
                }

                partMesh = SaveMesh(partMesh, $"{MeshFolder}/SM_Door01k_{part}.asset");

                GameObject partPrefab = SavePartPrefab(part, partMesh, material);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(partPrefab, assembly.transform);
                instance.transform.localPosition = pivot;
            }

            PrefabUtility.SaveAsPrefabAsset(assembly, $"{OutputFolder}/Door01k_Split.prefab");
            Object.DestroyImmediate(assembly);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Door01kSplitter] {OutputFolder}에 틀·문짝·돌 프리팹과 조립 프리팹(Door01k_Split)을 만들었습니다.");
        }

        /// <summary>이미 있으면 내용만 덮어써서 GUID를 유지한다. 씬에 놓은 프리팹 참조가 끊기지 않게 하기 위해서다.</summary>
        static Mesh SaveMesh(Mesh mesh, string path)
        {
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        static Mesh LoadSourceMesh()
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(SourceMeshPath))
            {
                if (asset is Mesh mesh && mesh.name == SourceMeshName) return mesh;
            }

            Debug.LogError($"[Door01kSplitter] {SourceMeshPath}에서 '{SourceMeshName}' 메시를 찾지 못했습니다.");
            return null;
        }

        static Part Classify(Bounds piece, Bounds whole, int widthAxis)
        {
            float unitPerCm = whole.size[widthAxis] / SourceWidthCm;
            if (piece.max.y - whole.min.y <= StoneMaxHeightCm * unitPerCm) return Part.Stone;

            float min = piece.min[widthAxis];
            float max = piece.max[widthAxis];
            bool crossesCenter = min < 0f && max > 0f;
            float innerEdge = crossesCenter ? 0f : Mathf.Min(Mathf.Abs(min), Mathf.Abs(max));
            if (innerEdge >= FrameInnerEdgeCm * unitPerCm) return Part.Frame;

            return piece.center[widthAxis] < 0f ? Part.LeftDoor : Part.RightDoor;
        }

        static List<int>[] ReadTriangles(Mesh mesh)
        {
            List<int>[] result = new List<int>[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                result[i] = new List<int>(mesh.GetTriangles(i));
            }
            return result;
        }

        static int[] FindComponents(Vector3[] vertices, List<int>[] trianglesBySubmesh)
        {
            int[] parent = new int[vertices.Length];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;

            int Find(int a)
            {
                while (parent[a] != a)
                {
                    parent[a] = parent[parent[a]];
                    a = parent[a];
                }
                return a;
            }

            void Union(int a, int b)
            {
                a = Find(a);
                b = Find(b);
                if (a != b) parent[a] = b;
            }

            foreach (List<int> triangles in trianglesBySubmesh)
            {
                for (int t = 0; t < triangles.Count; t += 3)
                {
                    Union(triangles[t], triangles[t + 1]);
                    Union(triangles[t], triangles[t + 2]);
                }
            }

            Dictionary<Vector3Int, int> firstAtPosition = new Dictionary<Vector3Int, int>();
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3Int key = Vector3Int.RoundToInt(vertices[i] * WeldPrecision);
                if (firstAtPosition.TryGetValue(key, out int other)) Union(i, other);
                else firstAtPosition[key] = i;
            }

            int[] component = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++) component[i] = Find(i);
            return component;
        }

        static Dictionary<int, Bounds> MeasureComponents(Vector3[] vertices, int[] componentOfVertex)
        {
            Dictionary<int, Bounds> result = new Dictionary<int, Bounds>();
            for (int i = 0; i < vertices.Length; i++)
            {
                int c = componentOfVertex[i];
                if (result.TryGetValue(c, out Bounds b))
                {
                    b.Encapsulate(vertices[i]);
                    result[c] = b;
                }
                else
                {
                    result[c] = new Bounds(vertices[i], Vector3.zero);
                }
            }
            return result;
        }

        /// <summary>문짝은 경첩 쪽 바깥 모서리(두께 중앙, 바닥 높이), 나머지는 원본 원점을 기준점으로 쓴다.</summary>
        static Vector3 GetPivot(Part part, Vector3[] vertices, int[] componentOfVertex, Dictionary<int, Part> partOfComponent,
            int widthAxis, int depthAxis)
        {
            if (part != Part.LeftDoor && part != Part.RightDoor) return Vector3.zero;

            bool hasBounds = false;
            Bounds doorBounds = default;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (partOfComponent[componentOfVertex[i]] != part) continue;
                if (hasBounds) doorBounds.Encapsulate(vertices[i]);
                else
                {
                    doorBounds = new Bounds(vertices[i], Vector3.zero);
                    hasBounds = true;
                }
            }
            if (!hasBounds) return Vector3.zero;

            Vector3 pivot = Vector3.zero;
            pivot[widthAxis] = part == Part.LeftDoor ? doorBounds.min[widthAxis] : doorBounds.max[widthAxis];
            pivot[depthAxis] = doorBounds.center[depthAxis];
            pivot.y = doorBounds.min.y;
            return pivot;
        }

        static Mesh BuildPartMesh(Mesh source, Part part, Vector3 pivot, List<int>[] trianglesBySubmesh,
            int[] componentOfVertex, Dictionary<int, Part> partOfComponent)
        {
            Vector3[] vertices = source.vertices;
            Vector3[] normals = source.normals;
            Vector4[] tangents = source.tangents;
            Vector2[] uv = source.uv;
            Vector2[] uv2 = source.uv2;
            Color[] colors = source.colors;

            Dictionary<int, int> remap = new Dictionary<int, int>();
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Vector4> newTangents = new List<Vector4>();
            List<Vector2> newUv = new List<Vector2>();
            List<Vector2> newUv2 = new List<Vector2>();
            List<Color> newColors = new List<Color>();
            List<int>[] newTriangles = new List<int>[trianglesBySubmesh.Length];

            int Map(int oldIndex)
            {
                if (remap.TryGetValue(oldIndex, out int mapped)) return mapped;

                mapped = newVertices.Count;
                remap[oldIndex] = mapped;
                newVertices.Add(vertices[oldIndex] - pivot);
                if (normals.Length > 0) newNormals.Add(normals[oldIndex]);
                if (tangents.Length > 0) newTangents.Add(tangents[oldIndex]);
                if (uv.Length > 0) newUv.Add(uv[oldIndex]);
                if (uv2.Length > 0) newUv2.Add(uv2[oldIndex]);
                if (colors.Length > 0) newColors.Add(colors[oldIndex]);
                return mapped;
            }

            int triangleCount = 0;
            for (int s = 0; s < trianglesBySubmesh.Length; s++)
            {
                newTriangles[s] = new List<int>();
                List<int> triangles = trianglesBySubmesh[s];
                for (int t = 0; t < triangles.Count; t += 3)
                {
                    if (partOfComponent[componentOfVertex[triangles[t]]] != part) continue;

                    newTriangles[s].Add(Map(triangles[t]));
                    newTriangles[s].Add(Map(triangles[t + 1]));
                    newTriangles[s].Add(Map(triangles[t + 2]));
                    triangleCount++;
                }
            }
            if (triangleCount == 0) return null;

            Mesh mesh = new Mesh { name = $"SM_Door01k_{part}" };
            if (newVertices.Count > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(newVertices);
            if (newNormals.Count > 0) mesh.SetNormals(newNormals);
            if (newTangents.Count > 0) mesh.SetTangents(newTangents);
            if (newUv.Count > 0) mesh.SetUVs(0, newUv);
            if (newUv2.Count > 0) mesh.SetUVs(1, newUv2);
            if (newColors.Count > 0) mesh.SetColors(newColors);

            mesh.subMeshCount = newTriangles.Length;
            for (int s = 0; s < newTriangles.Length; s++) mesh.SetTriangles(newTriangles[s], s);

            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// 틀은 문 사이 통로를 막지 않도록 메시 그대로의 MeshCollider를, 문짝과 돌은 메시 크기의 BoxCollider를 쓴다.
        /// </summary>
        static GameObject SavePartPrefab(Part part, Mesh mesh, Material material)
        {
            GameObject go = new GameObject($"Door01k_{part}");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = go.AddComponent<MeshRenderer>();
            Material[] materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++) materials[i] = material;
            renderer.sharedMaterials = materials;

            if (part == Part.Frame)
            {
                go.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
            else
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = mesh.bounds.center;
                box.size = mesh.bounds.size;
            }

            string path = $"{OutputFolder}/Door01k_{part}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
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
