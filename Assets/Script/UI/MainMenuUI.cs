// 메인 화면과 장단 선택 화면 전환(장단맞추기 칸 당김 연출)을 담당한다.
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FindOurSound.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        enum Page
        {
            Main,
            Transition,
            Jangdan
        }

        [Header("메인 화면")]
        [SerializeField] CanvasGroup mainPage;
        [SerializeField] Button jangdanButton;
        [SerializeField] Button playingGuideButton;
        [SerializeField] Button garakButton;

        [Header("장단 선택 화면")]
        [SerializeField] CanvasGroup jangdanPage;
        [Tooltip("왼쪽 끝에 고정된 채 오른쪽으로 당겨져 펼쳐지는 장단 패널")]
        [SerializeField] RectTransform jangdanPanel;
        [Tooltip("패널이 다 펼쳐진 뒤 나타나는 요소(도장 버튼, 화살표)")]
        [SerializeField] CanvasGroup jangdanExtras;
        [SerializeField] ScrollRect jangdanScroll;
        [SerializeField] Button backButton;
        [Tooltip("누르면 장단 목록을 한 화면만큼 더 넘긴다.")]
        [SerializeField] Button scrollHintButton;

        [Header("연출")]
        [Tooltip("메인 화면의 장단맞추기 칸 너비. 당김 연출의 시작 너비다.")]
        [SerializeField] float collapsedWidth = 85f;
        [SerializeField] float expandedWidth = 347f;
        [SerializeField] float pullDuration = 0.45f;
        [SerializeField] float fadeDuration = 0.2f;
        [Tooltip("▷를 눌렀을 때 장단 목록이 한 화면만큼 넘어가는 시간")]
        [SerializeField] float scrollDuration = 0.35f;

        [Header("이벤트")]
        public UnityEvent<string> onJangdanSelected;
        public UnityEvent onPlayingGuideClicked;
        public UnityEvent onGarakClicked;

        Page page = Page.Main;
        Coroutine transition;
        Coroutine scrolling;

        void Awake()
        {
            jangdanButton.onClick.AddListener(OpenJangdan);
            backButton.onClick.AddListener(CloseJangdan);
            playingGuideButton.onClick.AddListener(onPlayingGuideClicked.Invoke);
            garakButton.onClick.AddListener(onGarakClicked.Invoke);
            if (scrollHintButton != null) scrollHintButton.onClick.AddListener(ScrollNextPage);

            foreach (var item in jangdanScroll.content.GetComponentsInChildren<Button>(true))
            {
                var label = item.GetComponentInChildren<VerticalText>(true);
                string jangdanName = label != null ? label.Text : item.name;
                item.onClick.AddListener(() => SelectJangdan(jangdanName));
            }

            ShowMainImmediate();
        }

        public void OpenJangdan()
        {
            if (page != Page.Main) return;
            transition = StartCoroutine(OpenRoutine());
        }

        public void CloseJangdan()
        {
            if (page != Page.Jangdan) return;
            transition = StartCoroutine(CloseRoutine());
        }

        void SelectJangdan(string jangdanName)
        {
            if (page != Page.Jangdan) return;
            Debug.Log($"[MainMenuUI] 장단 선택: {jangdanName}");
            onJangdanSelected.Invoke(jangdanName);
        }

        void ScrollNextPage()
        {
            if (page != Page.Jangdan) return;

            // 목록은 오른쪽에서 왼쪽으로 이어지므로 왼쪽(0)으로 넘긴다.
            float viewport = jangdanScroll.viewport.rect.width;
            float overflow = jangdanScroll.content.rect.width - viewport;
            if (overflow <= 0f) return;

            // 끝까지 넘긴 뒤 누르면 처음 장단이 맨 앞에 오도록 되돌린다.
            float current = jangdanScroll.horizontalNormalizedPosition;
            float target = current <= 0.001f ? 1f : Mathf.Max(0f, current - viewport / overflow);

            if (scrolling != null) StopCoroutine(scrolling);
            scrolling = StartCoroutine(ScrollTo(target));
        }

        IEnumerator ScrollTo(float target)
        {
            jangdanScroll.StopMovement();
            float from = jangdanScroll.horizontalNormalizedPosition;
            for (float t = 0f; t < scrollDuration; t += Time.unscaledDeltaTime)
            {
                jangdanScroll.horizontalNormalizedPosition = Mathf.Lerp(from, target, EaseInOut(t / scrollDuration));
                yield return null;
            }
            jangdanScroll.horizontalNormalizedPosition = target;
            scrolling = null;
        }

        IEnumerator OpenRoutine()
        {
            page = Page.Transition;
            SetGroup(mainPage, mainPage.alpha, false);

            // 다른 칸이 흐려지는 동안 장단맞추기 칸 자리에 접힌 패널을 놓는다.
            SetPanelWidth(collapsedWidth);
            SetGroup(jangdanExtras, 0f, false);
            SetGroup(jangdanPage, 1f, false);
            if (scrolling != null) StopCoroutine(scrolling);
            scrolling = null;
            jangdanScroll.horizontalNormalizedPosition = 1f;
            yield return Fade(mainPage, 0f, fadeDuration);

            yield return Pull(collapsedWidth, expandedWidth, pullDuration, true);
            yield return Fade(jangdanExtras, 1f, fadeDuration);

            SetGroup(jangdanPage, 1f, true);
            SetGroup(jangdanExtras, 1f, true);
            page = Page.Jangdan;
            transition = null;
        }

        IEnumerator CloseRoutine()
        {
            page = Page.Transition;
            SetGroup(jangdanPage, 1f, false);
            SetGroup(jangdanExtras, jangdanExtras.alpha, false);

            yield return Fade(jangdanExtras, 0f, fadeDuration);
            yield return Pull(expandedWidth, collapsedWidth, pullDuration, false);
            SetGroup(jangdanPage, 0f, false);
            yield return Fade(mainPage, 1f, fadeDuration);

            SetGroup(mainPage, 1f, true);
            page = Page.Main;
            transition = null;
        }

        IEnumerator Pull(float from, float to, float duration, bool overshoot)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.Clamp01(t / duration);
                SetPanelWidth(Mathf.LerpUnclamped(from, to, overshoot ? EaseOutBack(k) : EaseInOut(k)));
                yield return null;
            }
            SetPanelWidth(to);
        }

        static IEnumerator Fade(CanvasGroup group, float to, float duration)
        {
            float from = group.alpha;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            group.alpha = to;
        }

        void ShowMainImmediate()
        {
            if (transition != null) StopCoroutine(transition);
            transition = null;
            SetGroup(mainPage, 1f, true);
            SetGroup(jangdanPage, 0f, false);
            SetGroup(jangdanExtras, 0f, false);
            SetPanelWidth(collapsedWidth);
            page = Page.Main;
        }

        void SetPanelWidth(float width)
        {
            jangdanPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        static void SetGroup(CanvasGroup group, float alpha, bool interactive)
        {
            group.alpha = alpha;
            group.interactable = interactive;
            group.blocksRaycasts = interactive;
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.2f;
            const float c3 = c1 + 1f;
            float p = x - 1f;
            return 1f + c3 * p * p * p + c1 * p * p;
        }

        static float EaseInOut(float x)
        {
            return x * x * (3f - 2f * x);
        }
    }
}
