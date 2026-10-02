// 연주 시작 전 장구채 잡는 모양 안내를 잠시 보여준 뒤 Conductor를 시작한다.
using System.Collections;
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 반투명 채 모델, 안내 문구 같은 안내 오브젝트를 guideSeconds 동안 켰다가 끄고 Conductor.Play()를 호출한다.
    /// Conductor의 playOnStart를 꺼야 안내 중에 노트가 진행되지 않는다.
    /// </summary>
    public class GripGuide : MonoBehaviour
    {
        public Conductor conductor;

        [Tooltip("안내 중에만 켜 둘 오브젝트(반투명 채 모델, 안내 문구 Canvas 등)")]
        public GameObject[] guideObjects;

        [Tooltip("안내를 보여주는 시간(초)")]
        public float guideSeconds = 4f;

        void Start()
        {
            if (conductor == null)
            {
                Debug.LogError("[JangGuRhythm] GripGuide에 Conductor가 연결되지 않았습니다.");
                return;
            }

            if (conductor.playOnStart)
            {
                Debug.LogWarning("[JangGuRhythm] Conductor.playOnStart가 켜져 있어 안내 중에도 노트가 진행됩니다. Conductor의 Play On Start를 끄세요.");
            }

            StartCoroutine(ShowGuideThenPlay());
        }

        IEnumerator ShowGuideThenPlay()
        {
            SetGuideVisible(true);
            yield return new WaitForSeconds(guideSeconds);
            SetGuideVisible(false);
            conductor.Play();
        }

        void SetGuideVisible(bool visible)
        {
            if (guideObjects == null) return;

            foreach (GameObject guide in guideObjects)
            {
                if (guide != null) guide.SetActive(visible);
            }
        }
    }
}
