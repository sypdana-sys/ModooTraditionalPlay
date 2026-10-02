using UnityEngine;
using UnityEngine.UI;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 노트 하나의 시각적 이동만 담당한다. 판정 로직은 NoteSpawner가 전담한다.
    /// 왼쪽 노트(원)/오른쪽 노트(사각형) 프리팹 모두 이 컴포넌트를 사용한다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Note : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] Text syllableLabel;

        public NoteData Data { get; private set; }
        public float HitTime { get; private set; }

        Conductor conductor;
        Vector2 startPos;
        Vector2 endPos;
        float travelTime;

        void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Init(Conductor conductor, NoteData data, float hitTime, Vector2 startPos, Vector2 endPos, float travelTime)
        {
            this.conductor = conductor;
            Data = data;
            HitTime = hitTime;
            this.startPos = startPos;
            this.endPos = endPos;
            this.travelTime = travelTime;

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = startPos;

            if (syllableLabel != null) syllableLabel.text = data.syllable;
        }

        void Update()
        {
            if (conductor == null || travelTime <= 0f) return;

            float t = 1f - (HitTime - conductor.SongPositionSeconds) / travelTime;
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
        }
    }
}
