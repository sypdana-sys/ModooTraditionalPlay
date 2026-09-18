using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 장구 좌/우에 배치되는 트리거 콜라이더. 어떤 컨트롤러가 쳤는지 ControllerSideMarker로 판별해서
    /// 피드백을 표시하고, NoteSpawner(있다면)에 판정을 전달한다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class JangguHitTrigger : MonoBehaviour
    {
        [Tooltip("이 트리거가 어느 노트 풀을 판정할지 (Left/Right/Cross). NoteSpawner.TryHit에 그대로 전달된다.")]
        public NoteSide side;

        [Tooltip("체크하면 아래 requiredHand로 지정한 손만 인정한다. 끄면 side와 같은 쪽 손만 인정한다. " +
            "넘겨치기(RightHitTrigger_GungChe)처럼 판정할 노트 풀(side=Cross)과 실제로 쳐야 하는 손(왼손)이 다를 때 켠다.")]
        public bool overrideRequiredHand;
        public NoteSide requiredHand;

        public NoteSpawner noteSpawner;
        public JangguHitFeedback feedback;

        [Tooltip("연속 타격 사이 최소 간격(초). 콜라이더가 여러 프레임 겹칠 때 중복 판정을 막는다.")]
        public float hitCooldown = 0.15f;

        float lastHitTime = -999f;

        void OnTriggerEnter(Collider other)
        {
            if (Time.time - lastHitTime < hitCooldown) return;

            ControllerSideMarker marker = other.GetComponentInParent<ControllerSideMarker>();
            if (marker == null) return;

            lastHitTime = Time.time;

            NoteSide expectedHand = overrideRequiredHand ? requiredHand : side;
            bool correctHand = marker.side == expectedHand;
            feedback?.ShowHit(side, marker.side, correctHand);

            Debug.Log($"[JangGuRhythm] {name} 트리거 충돌: other={other.name}, controllerSide={marker.side}, correctHand={correctHand}");

            if (correctHand)
            {
                noteSpawner?.TryHit(side);
            }
        }
    }
}
