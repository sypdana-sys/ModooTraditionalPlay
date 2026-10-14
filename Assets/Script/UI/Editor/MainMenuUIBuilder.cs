// 한지·서책 느낌의 메인 메뉴 UI 텍스처, 폰트 에셋, 프리팹을 만들고 현재 씬에 배치한다.
using System.IO;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace FindOurSound.UI.EditorTools
{
    /// <summary>
    /// 메뉴 한 번으로 메인 메뉴 UI 프리팹 세트를 (재)생성하고 활성 씬에 인스턴스를 놓는다.
    /// 카드·장단 항목·도장 버튼은 각각 프리팹이며, 메인 메뉴 프리팹 안에 중첩 프리팹으로 들어간다.
    /// </summary>
    public static class MainMenuUIBuilder
    {
        const string UIFolder = "Assets/UI/MainMenu";
        const string FontFolder = UIFolder + "/Fonts";
        const string TextureFolder = UIFolder + "/Textures";
        const string PrefabFolder = "Assets/Prefabs/MainMenu";

        const string HanjiPath = TextureFolder + "/Hanji.png";
        const string FramePath = TextureFolder + "/BookFrame.png";
        const string SealPath = TextureFolder + "/Seal.png";
        const string ArrowPath = TextureFolder + "/Arrow.png";

        const string BrushFontPath = FontFolder + "/YiSunShin-Bold.ttf";
        const string BodyFontPath = FontFolder + "/YiSunShin-Regular.ttf";
        const string BrushFontAssetPath = FontFolder + "/YiSunShin-Bold SDF.asset";
        const string BodyFontAssetPath = FontFolder + "/YiSunShin-Regular SDF.asset";

        const string CardPrefabPath = PrefabFolder + "/HanjiCard.prefab";
        const string ItemPrefabPath = PrefabFolder + "/JangdanItem.prefab";
        const string SealPrefabPath = PrefabFolder + "/SealButton.prefab";
        const string MainMenuPrefabPath = PrefabFolder + "/MainMenuUI.prefab";

        const string RootName = "MainMenuUI";
        const string UIRayName = "UI Ray Interactor";
        const string InputActionsPath = "Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/XRI Default Input Actions.inputactions";

        // 캔버스는 시안(400x856)과 같은 단위를 쓴다.
        static readonly Vector2 CanvasSize = new Vector2(400f, 856f);
        const float CanvasWorldScale = 0.0012f;
        const float PanelHeight = 396f;
        const float PanelBleed = 12f; // 장단 패널 왼쪽 테두리를 화면 밖으로 숨기는 길이

        static readonly string[] Jangdans = { "자진모리", "세마치", "두마치", "굿거리", "휘모리", "중모리", "중중모리", "엇모리", "진양조" };
        const string ExtraCharacters = "장단맞추기연주법과부호가락정하뒤로";

        static readonly Color Ink = new Color(0.12f, 0.10f, 0.08f, 1f);
        static readonly Color Paper = new Color(0.93f, 0.89f, 0.80f, 1f);
        static readonly Color CardPaper = new Color(0.96f, 0.93f, 0.86f, 1f);
        static readonly Color Vermilion = new Color(0.70f, 0.13f, 0.10f, 1f);

        [MenuItem("Tools/Main Menu UI/Build Prefabs And Place In Scene")]
        public static void Build()
        {
            EnsureFolder(TextureFolder);
            EnsureFolder(PrefabFolder);

            WriteTexture(HanjiPath, PaintHanji(512, 1024), Vector4.zero);
            WriteTexture(FramePath, PaintBookFrame(128), new Vector4(24, 24, 24, 24));
            WriteTexture(SealPath, PaintSeal(160), Vector4.zero);
            WriteTexture(ArrowPath, PaintArrow(64), Vector4.zero);

            var hanji = AssetDatabase.LoadAssetAtPath<Sprite>(HanjiPath);
            var frame = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
            var seal = AssetDatabase.LoadAssetAtPath<Sprite>(SealPath);
            var arrow = AssetDatabase.LoadAssetAtPath<Sprite>(ArrowPath);

            var brushFont = LoadOrCreateFontAsset(BrushFontPath, BrushFontAssetPath);
            var bodyFont = LoadOrCreateFontAsset(BodyFontPath, BodyFontAssetPath);
            if (brushFont == null || bodyFont == null) return;

            var card = BuildCardPrefab(hanji, frame, brushFont);
            var item = BuildItemPrefab(bodyFont);
            var sealButton = BuildSealPrefab(seal, brushFont);
            var mainMenu = BuildMainMenuPrefab(hanji, frame, arrow, brushFont, card, item, sealButton);

            PlaceInScene(mainMenu);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MainMenuUIBuilder] 프리팹 생성 완료: {MainMenuPrefabPath}");
        }

        /// <summary>
        /// 양손 컨트롤러에 UI 전용 레이를 달고 EventSystem을 XR UI 입력으로 바꾼다.
        /// 트리거(XRI 기본 액션의 UI Press)로 클릭한다. 상호작용 레이어가 비어 있어 물체를 잡지 않으므로 장구채와 겹치지 않는다.
        /// </summary>
        [MenuItem("Tools/Main Menu UI/Setup Controller UI Rays")]
        public static void SetupControllerRays()
        {
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin == null)
            {
                Debug.LogError("[MainMenuUIBuilder] 씬에 XR Origin이 없습니다.");
                return;
            }

            var references = AssetDatabase.LoadAllAssetsAtPath(InputActionsPath);
            bool ok = AddUIRay(origin.transform, "Left Controller", "XRI Left Interaction", references);
            ok &= AddUIRay(origin.transform, "Right Controller", "XRI Right Interaction", references);
            if (!ok) return;

            var eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                Undo.RegisterCreatedObjectUndo(eventSystem.gameObject, "Create EventSystem");
            }

            var oldModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (oldModule != null) Undo.DestroyObjectImmediate(oldModule);
            var module = eventSystem.GetComponent<XRUIInputModule>();
            if (module == null) module = Undo.AddComponent<XRUIInputModule>(eventSystem.gameObject);
            module.enableXRInput = true;
            module.enableMouseInput = true; // 에디터에서 마우스로도 확인할 수 있게 둔다.

            EditorSceneManager.MarkSceneDirty(origin.gameObject.scene);
            Debug.Log("[MainMenuUIBuilder] 컨트롤러 UI 레이 설정 완료(트리거로 클릭).");
        }

        static bool AddUIRay(Transform origin, string controllerName, string actionMap, Object[] references)
        {
            var controller = FindDeep(origin, controllerName);
            if (controller == null)
            {
                Debug.LogError($"[MainMenuUIBuilder] {controllerName}를 찾을 수 없습니다.");
                return false;
            }

            var press = FindReference(references, actionMap + "/UI Press");
            var pressValue = FindReference(references, actionMap + "/UI Press Value");
            var scroll = FindReference(references, actionMap + "/UI Scroll");
            if (press == null || pressValue == null)
            {
                Debug.LogError($"[MainMenuUIBuilder] 입력 액션을 찾을 수 없습니다: {actionMap}/UI Press ({InputActionsPath})");
                return false;
            }

            var existing = controller.Find(UIRayName);
            if (existing != null) Undo.DestroyObjectImmediate(existing.gameObject);

            var go = new GameObject(UIRayName, typeof(LineRenderer));
            Undo.RegisterCreatedObjectUndo(go, "Add UI Ray");
            go.transform.SetParent(controller, false);

            var ray = go.AddComponent<XRRayInteractor>();
            ray.interactionLayers = 0;
            ray.enableUIInteraction = true;
            ray.blockUIOnInteractableSelection = false;
            ray.maxRaycastDistance = 10f;
            ray.uiPressInput.inputSourceMode = XRInputButtonReader.InputSourceMode.InputActionReference;
            ray.uiPressInput.inputActionReferencePerformed = press;
            ray.uiPressInput.inputActionReferenceValue = pressValue;
            if (scroll != null)
            {
                ray.uiScrollInput.inputSourceMode = XRInputValueReader.InputSourceMode.InputActionReference;
                ray.uiScrollInput.inputActionReference = scroll;
            }

            var line = go.GetComponent<LineRenderer>();
            line.widthMultiplier = 0.004f;
            line.numCapVertices = 4;
            line.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

            // UI를 가리킬 때만 먹색 선이 보이고, 그 밖에는 투명하다(장구 연주 중 시야를 가리지 않게).
            var visual = go.AddComponent<XRInteractorLineVisual>();
            visual.lineWidth = 0.004f;
            visual.validColorGradient = Gradient(new Color(0.12f, 0.10f, 0.08f, 0.9f), new Color(0.12f, 0.10f, 0.08f, 0.3f));
            visual.invalidColorGradient = Gradient(Color.clear, Color.clear);
            visual.blockedColorGradient = Gradient(Color.clear, Color.clear);
            visual.stopLineAtFirstRaycastHit = true;
            return true;
        }

        static InputActionReference FindReference(Object[] references, string name)
        {
            foreach (var asset in references)
            {
                if (asset is InputActionReference reference && reference.action != null
                    && $"{reference.action.actionMap.name}/{reference.action.name}" == name)
                    return reference;
            }
            return null;
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t;
            }
            return null;
        }

        static Gradient Gradient(Color start, Color end)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
                new[] { new GradientAlphaKey(start.a, 0f), new GradientAlphaKey(end.a, 1f) });
            return gradient;
        }

        // ───────────────────────── 프리팹 ─────────────────────────

        static GameObject BuildCardPrefab(Sprite hanji, Sprite frame, TMP_FontAsset font)
        {
            var root = NewUI("HanjiCard", null);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(85f, PanelHeight);

            var paper = root.AddComponent<Image>();
            paper.sprite = hanji;
            paper.color = CardPaper;
            var button = root.AddComponent<Button>();
            button.targetGraphic = paper;
            button.colors = PaperColors();
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            AddFrame(root.transform, frame, Ink, 2f);
            var label = AddVerticalLabel(root.transform, "Label", "장단맞추기", font, 46f, Ink);
            Stretch(label.rectTransform, 4f, 16f, 4f, 16f);

            return SavePrefab(root, CardPrefabPath);
        }

        static GameObject BuildItemPrefab(TMP_FontAsset font)
        {
            var root = NewUI("JangdanItem", null);
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(61f, PanelHeight);

            // 평소에는 투명하고, 가리키면 먹물이 살짝 번진 듯 어두워진다.
            var hit = root.AddComponent<Image>();
            hit.color = Color.white;
            var button = root.AddComponent<Button>();
            button.targetGraphic = hit;
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(0.55f, 0.45f, 0.30f, 0.18f);
            colors.pressedColor = new Color(0.45f, 0.35f, 0.22f, 0.32f);
            colors.selectedColor = colors.normalColor;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var layout = root.AddComponent<LayoutElement>();
            layout.preferredWidth = 61f;
            layout.flexibleWidth = 0f;

            // 옛 책의 계선(세로 줄). 오른쪽에서 왼쪽으로 읽으므로 각 칸의 왼쪽에 둔다.
            var divider = AddLine(root.transform, "Divider", new Color(Ink.r, Ink.g, Ink.b, 0.55f), 1.5f);
            AnchorVerticalLine(divider, 0f, 0f);

            var label = AddVerticalLabel(root.transform, "Label", "자진모리", font, 36f, Ink);
            Stretch(label.rectTransform, 2f, 20f, 2f, 20f);

            return SavePrefab(root, ItemPrefabPath);
        }

        static GameObject BuildSealPrefab(Sprite seal, TMP_FontAsset font)
        {
            var root = NewUI("SealButton", null);
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(68f, 68f);
            rt.localRotation = Quaternion.Euler(0f, 0f, 5f); // 손으로 찍은 듯 살짝 기울인다.

            var image = root.AddComponent<Image>();
            image.sprite = seal;
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = ColorBlock.defaultColorBlock;
            colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            // 음각 도장: 붉은 인주 바탕에 글자가 종이색으로 파여 보인다.
            var label = AddVerticalLabel(root.transform, "Label", "뒤로", font, 24f, Paper);
            Stretch(label.rectTransform, 6f, 4f, 6f, 6f);
            label.lineSpacing = 4f;

            return SavePrefab(root, SealPrefabPath);
        }

        static GameObject BuildMainMenuPrefab(Sprite hanji, Sprite frame, Sprite arrow, TMP_FontAsset brushFont,
            GameObject cardPrefab, GameObject itemPrefab, GameObject sealPrefab)
        {
            var root = new GameObject(RootName, typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = CanvasSize;
            rootRect.localScale = Vector3.one * CanvasWorldScale;

            var background = NewUI("Background", root.transform).AddComponent<Image>();
            background.sprite = hanji;
            background.color = Paper;
            background.raycastTarget = false;
            Stretch(background.rectTransform);

            var clip = NewUI("Viewport", root.transform);
            clip.AddComponent<RectMask2D>();
            Stretch(clip.GetComponent<RectTransform>());

            // 장단 페이지는 메인 페이지 뒤에 둔다. 메인의 장단맞추기 칸이 흐려지면 접힌 패널이 드러난다.
            var jangdanPage = NewUI("JangdanPage", clip.transform);
            Stretch(jangdanPage.GetComponent<RectTransform>());
            var jangdanGroup = jangdanPage.AddComponent<CanvasGroup>();
            // 실행 시 MainMenuUI가 상태를 정한다. 편집할 때는 Alpha를 1로 올려 확인한다.
            jangdanGroup.alpha = 0f;

            var panel = NewUI("JangdanPanel", jangdanPage.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0f, 0.5f);
            panelRect.pivot = new Vector2(0f, 0.5f);
            panelRect.anchoredPosition = new Vector2(-PanelBleed, 0f);
            panelRect.sizeDelta = new Vector2(347f + PanelBleed, PanelHeight);
            panel.AddComponent<RectMask2D>();

            var panelPaper = NewUI("Paper", panel.transform).AddComponent<Image>();
            panelPaper.sprite = hanji;
            panelPaper.color = CardPaper;
            Stretch(panelPaper.rectTransform);

            const float headerWidth = 52f;
            var scroll = BuildJangdanList(panel.transform, headerWidth, itemPrefab);

            var header = NewUI("Header", panel.transform);
            var headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(1f, 0f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(1f, 0.5f);
            headerRect.sizeDelta = new Vector2(headerWidth, 0f);
            headerRect.anchoredPosition = Vector2.zero;
            var lineColor = new Color(Ink.r, Ink.g, Ink.b, 0.8f);
            AnchorVerticalLine(AddLine(header.transform, "DoubleLine_Outer", lineColor, 1.5f), 0f, 0f);
            AnchorVerticalLine(AddLine(header.transform, "DoubleLine_Inner", lineColor, 1.5f), 0f, 4f);
            var headerLabel = AddVerticalLabel(header.transform, "Label", "장단", brushFont, 42f, Ink);
            Stretch(headerLabel.rectTransform, 6f, 16f, 2f, 16f);

            AddFrame(panel.transform, frame, Ink, 2f);

            var extras = NewUI("Extras", jangdanPage.transform);
            Stretch(extras.GetComponent<RectTransform>());
            var extrasGroup = extras.AddComponent<CanvasGroup>();

            var hint = NewUI("ScrollHint", extras.transform);
            var hintRect = hint.GetComponent<RectTransform>();
            hintRect.anchorMin = hintRect.anchorMax = new Vector2(0f, 0.5f);
            hintRect.pivot = new Vector2(0.5f, 0.5f);
            hintRect.anchoredPosition = new Vector2(384f, 0f);
            hintRect.sizeDelta = new Vector2(32f, 48f); // 레이로 누르기 쉽게 세로로 조금 넉넉히 둔다.
            var hintImage = hint.AddComponent<Image>();
            hintImage.sprite = arrow;
            hintImage.preserveAspect = true;
            hintImage.color = Color.white;
            var hintButton = hint.AddComponent<Button>();
            hintButton.targetGraphic = hintImage;
            hintButton.navigation = new Navigation { mode = Navigation.Mode.None };
            // 다른 버튼처럼 가리키면 옅어지고, 누르면 인주색으로 바뀐다.
            var hintColors = ColorBlock.defaultColorBlock;
            hintColors.normalColor = Ink;
            hintColors.highlightedColor = new Color(0.40f, 0.30f, 0.20f, 1f);
            hintColors.pressedColor = Vermilion;
            hintColors.selectedColor = Ink;
            hintColors.fadeDuration = 0.08f;
            hintButton.colors = hintColors;

            var sealButton = InstantiateNested(sealPrefab, extras.transform, "BackSeal");
            var sealRect = sealButton.GetComponent<RectTransform>();
            sealRect.anchorMin = sealRect.anchorMax = new Vector2(0f, 0f);
            sealRect.anchoredPosition = new Vector2(85f, 115f);

            var mainPage = NewUI("MainPage", clip.transform);
            Stretch(mainPage.GetComponent<RectTransform>());
            var mainGroup = mainPage.AddComponent<CanvasGroup>();
            var jangdanCard = PlaceCard(cardPrefab, mainPage.transform, "Card_Jangdan", "장단맞추기", 2f, Ink);
            var playingCard = PlaceCard(cardPrefab, mainPage.transform, "Card_PlayingGuide", "연주법과 부호", 163f, Vermilion);
            var garakCard = PlaceCard(cardPrefab, mainPage.transform, "Card_Garak", "가락정하기", 313f, Ink);

            // 화면 전체 테두리(사주쌍변)는 맨 위에 그리고 입력은 막지 않는다.
            var border = AddFrame(root.transform, frame, Ink, 1.5f);
            border.name = "Border";

            var menu = root.AddComponent<MainMenuUI>();
            var so = new SerializedObject(menu);
            so.FindProperty("mainPage").objectReferenceValue = mainGroup;
            so.FindProperty("jangdanButton").objectReferenceValue = jangdanCard;
            so.FindProperty("playingGuideButton").objectReferenceValue = playingCard;
            so.FindProperty("garakButton").objectReferenceValue = garakCard;
            so.FindProperty("jangdanPage").objectReferenceValue = jangdanGroup;
            so.FindProperty("jangdanPanel").objectReferenceValue = panelRect;
            so.FindProperty("jangdanExtras").objectReferenceValue = extrasGroup;
            so.FindProperty("jangdanScroll").objectReferenceValue = scroll;
            so.FindProperty("backButton").objectReferenceValue = sealButton.GetComponent<Button>();
            so.FindProperty("scrollHintButton").objectReferenceValue = hintButton;
            so.FindProperty("collapsedWidth").floatValue = 85f + PanelBleed;
            so.FindProperty("expandedWidth").floatValue = 347f + PanelBleed;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(root, MainMenuPrefabPath);
        }

        static ScrollRect BuildJangdanList(Transform panel, float headerWidth, GameObject itemPrefab)
        {
            var list = NewUI("List", panel);
            Stretch(list.GetComponent<RectTransform>(), 0f, 0f, headerWidth, 0f);
            var scroll = list.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 20f;

            var viewport = NewUI("ListViewport", list.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            // 빈 영역도 드래그되도록 투명 레이캐스트 대상을 둔다.
            viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            // 오른쪽에서 왼쪽으로 세로쓰기 책처럼 이어진다.
            var content = NewUI("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(1f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(1f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(61f * Jangdans.Length, 0f);
            var layout = content.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.reverseArrangement = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            content.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (var jangdan in Jangdans)
            {
                var item = InstantiateNested(itemPrefab, content.transform, "Item_" + jangdan);
                SetVerticalText(item.GetComponentInChildren<VerticalText>(), jangdan);
            }

            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontalNormalizedPosition = 1f;
            return scroll;
        }

        static Button PlaceCard(GameObject cardPrefab, Transform parent, string name, string text, float left, Color frameColor)
        {
            var card = InstantiateNested(cardPrefab, parent, name);
            var rt = card.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(left, 0f);
            SetVerticalText(card.GetComponentInChildren<VerticalText>(), text);
            card.transform.Find("Frame").GetComponent<Image>().color = frameColor;
            return card.GetComponent<Button>();
        }

        static void PlaceInScene(GameObject prefab)
        {
            var scene = EditorSceneManager.GetActiveScene();
            // 이미 놓인 인스턴스는 위치와 오버라이드를 그대로 두고, 프리팹 변경만 반영되게 한다.
            foreach (var existing in scene.GetRootGameObjects())
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(existing) == prefab)
                {
                    Selection.activeGameObject = existing;
                    return;
                }
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var cam = Camera.main;
            if (cam != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                forward.Normalize();
                Vector3 position = cam.transform.position + forward * 1.2f;
                if (position.y < 1f) position.y = 1.4f;
                instance.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
            }
            else
            {
                instance.transform.position = new Vector3(0f, 1.4f, 1.2f);
            }

            var canvas = instance.GetComponent<Canvas>();
            if (cam != null) canvas.worldCamera = cam;

            Undo.RegisterCreatedObjectUndo(instance, "Place Main Menu UI");
            Selection.activeGameObject = instance;
            EditorSceneManager.MarkSceneDirty(scene);
        }

        // ───────────────────────── UI 헬퍼 ─────────────────────────

        static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject InstantiateNested(GameObject prefab, Transform parent, string name)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            return go;
        }

        static void Stretch(RectTransform rt, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        static Image AddFrame(Transform parent, Sprite frame, Color color, float pixelsPerUnitMultiplier)
        {
            var image = NewUI("Frame", parent).AddComponent<Image>();
            image.sprite = frame;
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            image.color = color;
            image.raycastTarget = false;
            Stretch(image.rectTransform);
            return image;
        }

        static RectTransform AddLine(Transform parent, string name, Color color, float width)
        {
            var image = NewUI(name, parent).AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(width, 0f);
            return image.rectTransform;
        }

        static void AnchorVerticalLine(RectTransform line, float anchorX, float offsetX)
        {
            line.anchorMin = new Vector2(anchorX, 0f);
            line.anchorMax = new Vector2(anchorX, 1f);
            line.pivot = new Vector2(0f, 0.5f);
            line.anchoredPosition = new Vector2(offsetX, 0f);
            line.sizeDelta = new Vector2(line.sizeDelta.x, -12f);
        }

        static TextMeshProUGUI AddVerticalLabel(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color)
        {
            var go = NewUI(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.lineSpacing = -18f;
            tmp.raycastTarget = false;

            var vertical = go.AddComponent<VerticalText>();
            var so = new SerializedObject(vertical);
            so.FindProperty("target").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();
            SetVerticalText(vertical, text);
            return tmp;
        }

        static void SetVerticalText(VerticalText vertical, string text)
        {
            var so = new SerializedObject(vertical);
            so.FindProperty("text").stringValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            vertical.Text = text;
        }

        static ColorBlock PaperColors()
        {
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.94f, 0.90f, 0.80f, 1f);
            colors.pressedColor = new Color(0.82f, 0.76f, 0.64f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ───────────────────────── 폰트 ─────────────────────────

        static TMP_FontAsset LoadOrCreateFontAsset(string fontPath, string assetPath)
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (fontAsset == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
                if (font == null)
                {
                    Debug.LogError($"[MainMenuUIBuilder] 폰트 파일이 없습니다: {fontPath}");
                    return null;
                }

                fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, false);
                fontAsset.name = Path.GetFileNameWithoutExtension(assetPath);
                AssetDatabase.CreateAsset(fontAsset, assetPath);
                fontAsset.atlasTextures[0].name = fontAsset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            // 화면에 쓰는 글자는 미리 아틀라스에 넣어 둔다. 그 밖의 글자는 동적으로 추가된다.
            fontAsset.TryAddCharacters(string.Concat(Jangdans) + ExtraCharacters);
            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }

        // ───────────────────────── 텍스처 생성 ─────────────────────────

        static void WriteTexture(string path, Texture2D texture, Vector4 border)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        static Texture2D NewTexture(int width, int height)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        /// <summary>닥나무 섬유가 비치는 누런 한지.</summary>
        static Texture2D PaintHanji(int width, int height)
        {
            var random = new System.Random(1170);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float grain = Fbm(x / 6f, y / 6f, 4, 11) - 0.5f;
                    float cloud = Fbm(x / 140f, y / 140f, 3, 23) - 0.5f;
                    float edge = Mathf.Min(Mathf.Min(x, width - 1 - x) / (float)width, Mathf.Min(y, height - 1 - y) / (float)height);
                    float age = 1f - 0.10f * (1f - SmoothStep(0f, 1f, edge / 0.18f));

                    float v = (1f + grain * 0.07f + cloud * 0.10f) * age;
                    pixels[y * width + x] = new Color(v, v * (0.995f + cloud * 0.02f), v * (0.985f + cloud * 0.04f), 1f);
                }
            }

            // 섬유: 짧고 휘어진 결을 밝게 또는 어둡게 겹친다.
            for (int i = 0; i < 1400; i++)
            {
                float px = (float)random.NextDouble() * width;
                float py = (float)random.NextDouble() * height;
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float bend = ((float)random.NextDouble() - 0.5f) * 0.12f;
                int length = 8 + random.Next(50);
                bool light = random.NextDouble() < 0.6;
                float strength = 0.04f + (float)random.NextDouble() * 0.08f;
                Color tone = light ? new Color(1.08f, 1.08f, 1.06f) : new Color(0.78f, 0.72f, 0.62f);

                for (int s = 0; s < length; s++)
                {
                    angle += bend;
                    px += Mathf.Cos(angle);
                    py += Mathf.Sin(angle);
                    BlendPixel(pixels, width, height, (int)px, (int)py, tone, strength);
                }
            }

            // 껍질 티: 작고 진한 점.
            for (int i = 0; i < 160; i++)
            {
                int px = random.Next(width);
                int py = random.Next(height);
                var tone = new Color(0.45f, 0.38f, 0.30f);
                BlendPixel(pixels, width, height, px, py, tone, 0.35f);
                if (random.NextDouble() < 0.4) BlendPixel(pixels, width, height, px + 1, py, tone, 0.2f);
            }

            var texture = NewTexture(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>옛 서책의 사주쌍변(굵은 바깥 선 + 가는 안쪽 선). 9-slice용, 가운데는 투명하다.</summary>
        static Texture2D PaintBookFrame(int size)
        {
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                    // 선을 따라 굵기가 조금씩 흔들리게 해 붓으로 그은 느낌을 낸다.
                    float wobble = (Fbm((x + y) / 9f, (x - y) / 9f, 2, 31) - 0.5f) * 1.6f;
                    float outer = Band(d + wobble, 2f, 7f);
                    float inner = Band(d - wobble * 0.5f, 12f, 14f);
                    float ink = Mathf.Max(outer, inner);
                    ink *= 0.80f + 0.20f * Fbm(x / 3f, y / 3f, 2, 37);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(ink));
                }
            }

            var texture = NewTexture(size, size);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>인주가 고르지 않게 묻은 네모난 붉은 도장(가운데 글자는 TMP로 얹는다).</summary>
        static Texture2D PaintSeal(int size)
        {
            var pixels = new Color[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float jitter = (Fbm(x / 7f, y / 7f, 3, 41) - 0.5f) * 6f;
                    float box = RoundedBoxDistance(x - half, y - half, half - 8f, 12f) + jitter;
                    float coverage = 1f - SmoothStep(-1f, 1f, box);

                    // 안쪽 테두리 홈(음각 선).
                    float frameGroove = Band(-box, 9f, 12.5f);
                    coverage *= 1f - frameGroove;

                    // 인주가 덜 묻은 자리.
                    float blotch = Fbm(x / 11f, y / 11f, 4, 53);
                    coverage *= Mathf.Lerp(1f, 0.35f, SmoothStep(0.62f, 0.78f, blotch));
                    float speck = Fbm(x / 2.2f, y / 2.2f, 2, 59);
                    coverage *= speck > 0.72f ? 0.4f : 1f;

                    var c = Vermilion * (0.92f + 0.12f * Fbm(x / 20f, y / 20f, 2, 61));
                    c.a = Mathf.Clamp01(coverage);
                    pixels[y * size + x] = c;
                }
            }

            var texture = NewTexture(size, size);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>오른쪽을 가리키는 먹선 삼각형(▷).</summary>
        static Texture2D PaintArrow(int size)
        {
            var a = new Vector2(size * 0.22f, size * 0.12f);
            var b = new Vector2(size * 0.22f, size * 0.88f);
            var c = new Vector2(size * 0.86f, size * 0.5f);
            const float thickness = 5f;

            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float d = Mathf.Min(SegmentDistance(p, a, b), Mathf.Min(SegmentDistance(p, b, c), SegmentDistance(p, c, a)));
                    float alpha = 1f - SmoothStep(thickness * 0.5f - 0.75f, thickness * 0.5f + 0.75f, d);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            var texture = NewTexture(size, size);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        static void BlendPixel(Color[] pixels, int width, int height, int x, int y, Color tone, float strength)
        {
            if (x < 0 || y < 0 || x >= width || y >= height) return;
            int i = y * width + x;
            var src = pixels[i];
            pixels[i] = new Color(
                Mathf.Lerp(src.r, src.r * tone.r, strength * 4f),
                Mathf.Lerp(src.g, src.g * tone.g, strength * 4f),
                Mathf.Lerp(src.b, src.b * tone.b, strength * 4f),
                1f);
        }

        // Mathf.SmoothStep은 값 보간이므로 셰이더식 smoothstep(경계 사이 0→1)을 따로 둔다.
        static float SmoothStep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        static float Band(float d, float from, float to)
        {
            return SmoothStep(from - 0.8f, from + 0.8f, d) * (1f - SmoothStep(to - 0.8f, to + 0.8f, d));
        }

        static float RoundedBoxDistance(float px, float py, float halfSize, float radius)
        {
            float qx = Mathf.Abs(px) - halfSize + radius;
            float qy = Mathf.Abs(py) - halfSize + radius;
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        static float Fbm(float x, float y, int octaves, int seed)
        {
            float sum = 0f;
            float amplitude = 0.5f;
            float total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += ValueNoise(x, y, seed + i * 101) * amplitude;
                total += amplitude;
                x *= 2f;
                y *= 2f;
                amplitude *= 0.5f;
            }
            return sum / total;
        }

        static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float fx = x - x0;
            float fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float a = Hash(x0, y0, seed);
            float b = Hash(x0 + 1, y0, seed);
            float c = Hash(x0, y0 + 1, seed);
            float d = Hash(x0 + 1, y0 + 1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144269504);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
