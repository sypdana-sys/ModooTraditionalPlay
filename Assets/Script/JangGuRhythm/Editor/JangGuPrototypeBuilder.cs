using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace FindOurSound.JangGuRhythm.EditorTools
{
    /// <summary>
    /// 메뉴 한 번으로 프로토타입에 필요한 노트 프리팹, 기본 장단 챠트 에셋, 그리고
    /// 드래그 가능한 리듬 게임 창을 현재 활성 씬에 생성한다. 여러 번 실행해도 안전하게(idempotent) 재생성된다.
    /// </summary>
    public static class JangGuPrototypeBuilder
    {
        const string PrefabFolder = "Assets/Prefabs/JangGuRhythm";
        const string NotePrefabFolder = PrefabFolder + "/Notes";
        const string DataFolder = "Assets/Data/JangGuRhythm";

        const string LeftNotePrefabPath = NotePrefabFolder + "/Note_Left_Circle.prefab";
        const string RightNotePrefabPath = NotePrefabFolder + "/Note_Right_Rect.prefab";
        const string ChartAssetPath = DataFolder + "/JangdanChart_Basic.asset";
        const string WindowPrefabPath = PrefabFolder + "/RhythmWindow.prefab";

        const string RootObjectName = "JangGuRhythmPrototype";
        const string CanvasName = "JangGuRhythmCanvas";
        const string JangguObjectName = "JejuShamanicJanggu";
        const string XrOriginObjectName = "XR Origin (XR Rig)";

        static Font koreanFont;

        [MenuItem("Tools/JangGu Rhythm/Build Prototype Scene")]
        public static void Build()
        {
            EnsureFolder(PrefabFolder);
            EnsureFolder(NotePrefabFolder);
            EnsureFolder(DataFolder);

            EnsureKoreanFont();

            Note leftPrefab = BuildNotePrefab(LeftNotePrefabPath, isCircle: true, new Color(0.35f, 0.75f, 0.95f));
            Note rightPrefab = BuildNotePrefab(RightNotePrefabPath, isCircle: false, new Color(0.98f, 0.55f, 0.38f));
            JangGuChart chart = BuildBasicChart();

            GameObject existingRoot = GameObject.Find(RootObjectName);
            if (existingRoot != null) Object.DestroyImmediate(existingRoot);

            GameObject root = new GameObject(RootObjectName);

            Canvas canvas = BuildCanvas(root.transform);
            EnsureEventSystem();

            RectTransform window = BuildWindow(canvas.transform, leftPrefab, rightPrefab, chart, out NoteSpawner spawner);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(window.gameObject, WindowPrefabPath, InteractionMode.AutomatedAction);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = prefabAsset != null ? window.gameObject : root;

            Debug.Log("[JangGuRhythm] 프로토타입 생성 완료. Game 뷰에서 Play를 눌러 ←/A(콩, 왼쪽) · →/L(덕, 오른쪽) 키로 테스트하세요.");
        }

        static void EnsureKoreanFont()
        {
            if (koreanFont != null) return;
            koreanFont = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 24)
                         ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        [MenuItem("Tools/JangGu Rhythm/Convert Canvas To World Space (3D UI)")]
        public static void ConvertCanvasToWorldSpace()
        {
            GameObject canvasGo = GameObject.Find(CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError($"[JangGuRhythm] '{CanvasName}'을(를) 찾을 수 없습니다. 먼저 Build Prototype Scene을 실행하세요.");
                return;
            }

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            GraphicRaycaster oldRaycaster = canvasGo.GetComponent<GraphicRaycaster>();
            if (oldRaycaster != null) Object.DestroyImmediate(oldRaycaster);
            if (canvasGo.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
            }

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1080f, 1920f);
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
            canvasRect.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            GameObject janggu = GameObject.Find(JangguObjectName);
            GameObject xrOrigin = GameObject.Find(XrOriginObjectName);

            if (janggu != null)
            {
                Vector3 playerPos = xrOrigin != null ? xrOrigin.transform.position : janggu.transform.position + Vector3.back * 1.5f;
                Vector3 towardPlayer = playerPos - janggu.transform.position;
                towardPlayer.y = 0f;
                if (towardPlayer.sqrMagnitude < 0.0001f) towardPlayer = Vector3.back;
                towardPlayer.Normalize();

                canvasGo.transform.position = janggu.transform.position + Vector3.up * 0.9f + towardPlayer * 0.4f;

                Vector3 lookDir = playerPos - canvasGo.transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude < 0.0001f) lookDir = towardPlayer;
                canvasGo.transform.rotation = Quaternion.LookRotation(-lookDir.normalized, Vector3.up);
            }
            else
            {
                Debug.LogWarning($"[JangGuRhythm] '{JangguObjectName}'을(를) 찾지 못해 캔버스를 기본 위치에 배치했습니다.");
                canvasGo.transform.position = new Vector3(0f, 1.5f, 1f);
                canvasGo.transform.rotation = Quaternion.identity;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = canvasGo;
            Debug.Log("[JangGuRhythm] 캔버스를 3D(World Space) UI로 전환했습니다.");
        }

        [MenuItem("Tools/JangGu Rhythm/Add Hit Triggers To Janggu")]
        public static void AddHitTriggersToJanggu()
        {
            GameObject janggu = GameObject.Find(JangguObjectName);
            if (janggu == null)
            {
                Debug.LogError($"[JangGuRhythm] 씬에서 '{JangguObjectName}' 오브젝트를 찾을 수 없습니다.");
                return;
            }

            if (!TryGetLocalBounds(janggu.transform, out Bounds localBounds))
            {
                Debug.LogError($"[JangGuRhythm] '{JangguObjectName}'에서 Renderer를 찾을 수 없어 트리거 위치를 계산할 수 없습니다.");
                return;
            }

            Transform existingLeft = janggu.transform.Find("LeftHitTrigger");
            if (existingLeft != null) Object.DestroyImmediate(existingLeft.gameObject);
            Transform existingRight = janggu.transform.Find("RightHitTrigger");
            if (existingRight != null) Object.DestroyImmediate(existingRight.gameObject);

            bool xIsLongAxis = localBounds.extents.x >= localBounds.extents.z;
            float axisExtent = xIsLongAxis ? localBounds.extents.x : localBounds.extents.z;
            float otherExtent = xIsLongAxis ? localBounds.extents.z : localBounds.extents.x;
            float markerRadius = Mathf.Max(0.05f, Mathf.Min(localBounds.extents.y, otherExtent) * 0.7f);

            Vector3 centerA = localBounds.center;
            Vector3 centerB = localBounds.center;
            if (xIsLongAxis)
            {
                centerA.x = localBounds.center.x - axisExtent * 0.85f;
                centerB.x = localBounds.center.x + axisExtent * 0.85f;
            }
            else
            {
                centerA.z = localBounds.center.z - axisExtent * 0.85f;
                centerB.z = localBounds.center.z + axisExtent * 0.85f;
            }

            Vector3 worldA = janggu.transform.TransformPoint(centerA);
            Vector3 worldB = janggu.transform.TransformPoint(centerB);

            GameObject xrOrigin = GameObject.Find(XrOriginObjectName);
            Vector3 playerPos = xrOrigin != null ? xrOrigin.transform.position : janggu.transform.position + Vector3.back * 1.5f;
            Vector3 playerRight = xrOrigin != null ? xrOrigin.transform.right : Vector3.right;

            float sideOfA = Vector3.Dot(worldA - playerPos, playerRight);
            float sideOfB = Vector3.Dot(worldB - playerPos, playerRight);

            Vector3 rightWorldPos = sideOfA >= sideOfB ? worldA : worldB;
            Vector3 leftWorldPos = sideOfA >= sideOfB ? worldB : worldA;

            NoteSpawner spawner = Object.FindFirstObjectByType<NoteSpawner>();
            if (spawner == null)
            {
                Debug.LogWarning("[JangGuRhythm] 씬에서 NoteSpawner를 찾지 못했습니다. 트리거는 생성되지만 리듬 판정과는 연결되지 않습니다.");
            }

            JangguHitFeedback feedback = BuildHitFeedbackLabel(janggu, localBounds);

            CreateHitTrigger("LeftHitTrigger", janggu.transform, leftWorldPos, markerRadius, NoteSide.Left,
                new Color(0.35f, 0.75f, 0.95f, 0.6f), spawner, feedback, isCircle: true);
            CreateHitTrigger("RightHitTrigger", janggu.transform, rightWorldPos, markerRadius, NoteSide.Right,
                new Color(0.98f, 0.55f, 0.38f, 0.6f), spawner, feedback, isCircle: false);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[JangGuRhythm] 장구 좌/우 타격 트리거를 생성했습니다. (Left/Right 배정이 반대라면 각 트리거의 Side 필드를 직접 뒤집으세요)");
        }

        [MenuItem("Tools/JangGu Rhythm/Tag XR Controllers (Left Right)")]
        public static void TagXrControllers()
        {
            GameObject leftController = GameObject.Find("Left Controller");
            GameObject rightController = GameObject.Find("Right Controller");

            if (leftController == null && rightController == null)
            {
                Debug.LogError("[JangGuRhythm] 'Left Controller' / 'Right Controller' 오브젝트를 찾을 수 없습니다. XR Origin (XR Rig)이 씬에 있는지 확인하세요.");
                return;
            }

            if (leftController != null)
            {
                ControllerSideMarker marker = leftController.GetComponent<ControllerSideMarker>() ?? leftController.AddComponent<ControllerSideMarker>();
                marker.side = NoteSide.Left;
            }
            else
            {
                Debug.LogWarning("[JangGuRhythm] 'Left Controller'를 찾지 못했습니다.");
            }

            if (rightController != null)
            {
                ControllerSideMarker marker = rightController.GetComponent<ControllerSideMarker>() ?? rightController.AddComponent<ControllerSideMarker>();
                marker.side = NoteSide.Right;
            }
            else
            {
                Debug.LogWarning("[JangGuRhythm] 'Right Controller'를 찾지 못했습니다.");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[JangGuRhythm] XR 컨트롤러에 ControllerSideMarker를 부착했습니다.");
        }

        static bool TryGetLocalBounds(Transform root, out Bounds localBounds)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                localBounds = default;
                return false;
            }

            Bounds worldBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) worldBounds.Encapsulate(renderers[i].bounds);

            Vector3 c = worldBounds.center;
            Vector3 e = worldBounds.extents;
            Bounds lb = new Bounds(root.InverseTransformPoint(c + new Vector3(-e.x, -e.y, -e.z)), Vector3.zero);
            for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            for (int sz = -1; sz <= 1; sz += 2)
            {
                Vector3 worldCorner = c + new Vector3(sx * e.x, sy * e.y, sz * e.z);
                lb.Encapsulate(root.InverseTransformPoint(worldCorner));
            }

            localBounds = lb;
            return true;
        }

        static JangguHitFeedback BuildHitFeedbackLabel(GameObject janggu, Bounds localBounds)
        {
            EnsureKoreanFont();

            const string feedbackCanvasName = "JangguHitFeedbackCanvas";
            Transform existing = janggu.transform.Find(feedbackCanvasName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            GameObject canvasGo = new GameObject(feedbackCanvasName, typeof(Canvas), typeof(TrackedDeviceGraphicRaycaster));
            canvasGo.transform.SetParent(janggu.transform, false);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(600f, 120f);
            canvasRect.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
            canvasRect.localPosition = new Vector3(localBounds.center.x, localBounds.center.y + localBounds.extents.y + 0.3f, localBounds.center.z);
            canvasRect.localRotation = Quaternion.identity;

            RectTransform textRect = CreateUIObject("HitText", canvasGo.transform);
            StretchFill(textRect);
            Text hitText = textRect.gameObject.AddComponent<Text>();
            ConfigureText(hitText, string.Empty, 36, Color.white, TextAnchor.MiddleCenter);

            JangguHitFeedback feedback = canvasGo.AddComponent<JangguHitFeedback>();
            feedback.hitText = hitText;
            return feedback;
        }

        static void CreateHitTrigger(string name, Transform parent, Vector3 worldPos, float radius, NoteSide side,
            Color color, NoteSpawner spawner, JangguHitFeedback feedback, bool isCircle)
        {
            GameObject go = GameObject.CreatePrimitive(isCircle ? PrimitiveType.Sphere : PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.rotation = parent.rotation;

            Vector3 parentLossyScale = parent.lossyScale;
            Vector3 desiredWorldSize = isCircle
                ? new Vector3(radius * 2f, radius * 2f, radius * 2f)
                : new Vector3(radius * 1.6f, radius * 1.6f, radius * 0.6f);
            go.transform.localScale = new Vector3(
                desiredWorldSize.x / Mathf.Max(0.0001f, parentLossyScale.x),
                desiredWorldSize.y / Mathf.Max(0.0001f, parentLossyScale.y),
                desiredWorldSize.z / Mathf.Max(0.0001f, parentLossyScale.z));

            Collider col = go.GetComponent<Collider>();
            col.isTrigger = true;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            Renderer renderer = go.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Material mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            renderer.sharedMaterial = mat;

            JangguHitTrigger trigger = go.AddComponent<JangguHitTrigger>();
            trigger.side = side;
            trigger.noteSpawner = spawner;
            trigger.feedback = feedback;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        static Note BuildNotePrefab(string path, bool isCircle, Color color)
        {
            GameObject go = new GameObject(isCircle ? "Note_Left_Circle" : "Note_Right_Rect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = isCircle ? new Vector2(64f, 64f) : new Vector2(44f, 64f);

            Image image = go.GetComponent<Image>();
            image.color = color;
            if (isCircle)
            {
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                image.type = Image.Type.Simple;
            }

            RectTransform labelRect = CreateUIObject("Syllable", go.transform);
            StretchFill(labelRect);
            Text label = labelRect.gameObject.AddComponent<Text>();
            label.font = koreanFont;
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.black;
            label.text = isCircle ? "콩" : "덕";
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            Note note = go.AddComponent<Note>();
            SetPrivateField(note, "rectTransform", rt);
            SetPrivateField(note, "syllableLabel", label);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);
            GameObject savedAsset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            return savedAsset.GetComponent<Note>();
        }

        static JangGuChart BuildBasicChart()
        {
            if (AssetDatabase.LoadAssetAtPath<JangGuChart>(ChartAssetPath) != null) AssetDatabase.DeleteAsset(ChartAssetPath);

            JangGuChart chart = ScriptableObject.CreateInstance<JangGuChart>();
            chart.jangdanName = "기본 장단 (콩덕)";
            chart.bpm = 92f;
            chart.leadInSeconds = 1.5f;
            chart.tailBeats = 2f;
            chart.loop = true;

            chart.notes = new List<NoteData>
            {
                new NoteData { beat = 0f, side = NoteSide.Left,  syllable = "콩" },
                new NoteData { beat = 1f, side = NoteSide.Right, syllable = "덕" },
                new NoteData { beat = 2f, side = NoteSide.Left,  syllable = "콩" },
                new NoteData { beat = 3f, side = NoteSide.Right, syllable = "덕" },
                new NoteData { beat = 4f, side = NoteSide.Left,  syllable = "콩" },
                new NoteData { beat = 5f, side = NoteSide.Right, syllable = "덕" },
                new NoteData { beat = 6f, side = NoteSide.Left,  syllable = "콩" },
                new NoteData { beat = 7f, side = NoteSide.Right, syllable = "덕" },
            };

            AssetDatabase.CreateAsset(chart, ChartAssetPath);
            return chart;
        }

        static Canvas BuildCanvas(Transform parent)
        {
            GameObject canvasGo = new GameObject("JangGuRhythmCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(parent, false);

            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        static RectTransform BuildWindow(Transform canvasTransform, Note leftPrefab, Note rightPrefab, JangGuChart chart, out NoteSpawner spawner)
        {
            RectTransform window = CreateUIObject("RhythmWindow", canvasTransform, typeof(Image));
            SetCentered(window, new Vector2(400f, 820f), Vector2.zero);
            Image windowBg = window.GetComponent<Image>();
            windowBg.color = new Color(0.07f, 0.07f, 0.1f, 0.96f);

            // --- 타이틀바 (드래그 핸들) ---
            RectTransform titleBar = CreateUIObject("TitleBar", window, typeof(Image), typeof(DraggableWindow));
            SetTopStretch(titleBar, 56f, 0f);
            titleBar.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.3f, 1f);
            SetPrivateField(titleBar.GetComponent<DraggableWindow>(), "windowRoot", window);

            RectTransform titleLabelRect = CreateUIObject("TitleLabel", titleBar);
            StretchFill(titleLabelRect);
            Text titleLabel = titleLabelRect.gameObject.AddComponent<Text>();
            ConfigureText(titleLabel, "장구 리듬 프로토타입", 24, Color.white, TextAnchor.MiddleCenter);

            // --- 정보 행: 점수 / 콤보 ---
            RectTransform infoRow = CreateUIObject("InfoRow", window);
            SetTopStretch(infoRow, 40f, -56f);

            RectTransform scoreRect = CreateUIObject("ScoreText", infoRow);
            scoreRect.anchorMin = new Vector2(0f, 0f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);
            scoreRect.offsetMin = new Vector2(16f, 0f);
            scoreRect.offsetMax = Vector2.zero;
            Text scoreText = scoreRect.gameObject.AddComponent<Text>();
            ConfigureText(scoreText, "SCORE 0", 20, Color.white, TextAnchor.MiddleLeft);

            RectTransform comboRect = CreateUIObject("ComboText", infoRow);
            comboRect.anchorMin = new Vector2(0.5f, 0f);
            comboRect.anchorMax = new Vector2(1f, 1f);
            comboRect.offsetMin = Vector2.zero;
            comboRect.offsetMax = new Vector2(-16f, 0f);
            Text comboText = comboRect.gameObject.AddComponent<Text>();
            ConfigureText(comboText, string.Empty, 20, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleRight);

            // --- 판정 피드백 텍스트 ---
            RectTransform feedbackRect = CreateUIObject("JudgeFeedbackText", window);
            SetCentered(feedbackRect, new Vector2(360f, 50f), new Vector2(0f, 130f));
            Text feedbackText = feedbackRect.gameObject.AddComponent<Text>();
            ConfigureText(feedbackText, string.Empty, 28, Color.white, TextAnchor.MiddleCenter);

            // --- 노트 트랙 ---
            RectTransform track = CreateUIObject("NoteTrack", window, typeof(Image));
            SetCentered(track, new Vector2(360f, 140f), new Vector2(0f, 20f));
            track.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

            RectTransform leftSpawn = CreateUIObject("LeftSpawnPoint", track);
            SetCentered(leftSpawn, new Vector2(10f, 10f), new Vector2(-170f, 0f));

            RectTransform rightSpawn = CreateUIObject("RightSpawnPoint", track);
            SetCentered(rightSpawn, new Vector2(10f, 10f), new Vector2(170f, 0f));

            RectTransform judgmentLine = CreateUIObject("JudgmentZone", track, typeof(Image));
            SetCentered(judgmentLine, new Vector2(6f, 140f), Vector2.zero);
            judgmentLine.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);

            RectTransform judgmentRing = CreateUIObject("JudgmentRing", track, typeof(Image));
            SetCentered(judgmentRing, new Vector2(74f, 74f), Vector2.zero);
            Image ringImage = judgmentRing.GetComponent<Image>();
            ringImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            ringImage.color = new Color(1f, 1f, 1f, 0.2f);

            // --- 조작 안내 ---
            RectTransform hintRect = CreateUIObject("HintText", window);
            SetBottomStretch(hintRect, 70f, 10f);
            Text hintText = hintRect.gameObject.AddComponent<Text>();
            ConfigureText(hintText, "← / A : 콩 (왼쪽, 북편)      → / L : 덕 (오른쪽, 채편)", 18, new Color(0.8f, 0.8f, 0.8f), TextAnchor.MiddleCenter);

            // --- 로직 오브젝트 ---
            GameObject logicGo = new GameObject("RhythmLogic", typeof(Conductor), typeof(NoteSpawner), typeof(RhythmInputController), typeof(ScoreManager));
            logicGo.transform.SetParent(window, false);

            Conductor conductor = logicGo.GetComponent<Conductor>();
            conductor.chart = chart;

            spawner = logicGo.GetComponent<NoteSpawner>();
            spawner.conductor = conductor;
            spawner.chart = chart;
            spawner.leftNotePrefab = leftPrefab;
            spawner.rightNotePrefab = rightPrefab;
            spawner.leftSpawnPoint = leftSpawn;
            spawner.rightSpawnPoint = rightSpawn;
            spawner.judgmentPoint = judgmentLine;
            spawner.noteParent = track;

            RhythmInputController input = logicGo.GetComponent<RhythmInputController>();
            input.noteSpawner = spawner;

            ScoreManager scoreManager = logicGo.GetComponent<ScoreManager>();
            scoreManager.noteSpawner = spawner;
            scoreManager.scoreText = scoreText;
            scoreManager.comboText = comboText;
            scoreManager.judgeFeedbackText = feedbackText;

            return window;
        }

        static void ConfigureText(Text text, string content, int fontSize, Color color, TextAnchor alignment)
        {
            text.font = koreanFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        static RectTransform CreateUIObject(string name, Transform parent, params System.Type[] extraComponents)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            foreach (System.Type t in extraComponents) go.AddComponent(t);
            return rt;
        }

        static void StretchFill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void SetCentered(RectTransform rt, Vector2 size, Vector2 anchoredPos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        static void SetTopStretch(RectTransform rt, float height, float yOffset)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, yOffset);
        }

        static void SetBottomStretch(RectTransform rt, float height, float yOffset)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = new Vector2(0f, yOffset);
        }

        static void SetPrivateField(Object target, string fieldName, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[JangGuRhythm] 필드를 찾을 수 없음: {fieldName} on {target}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
