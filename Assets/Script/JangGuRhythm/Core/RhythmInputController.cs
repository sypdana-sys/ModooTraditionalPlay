using UnityEngine;
using UnityEngine.InputSystem;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 키보드 입력을 받아 NoteSpawner에 판정을 요청한다. (New Input System 사용)
    /// 왼쪽(원, 북편/왼손) / 오른쪽(사각형, 채편/오른손) 두 개의 입력만 존재하는 프로토타입 입력 구성.
    /// </summary>
    public class RhythmInputController : MonoBehaviour
    {
        public NoteSpawner noteSpawner;

        public Key leftKeyPrimary = Key.LeftArrow;
        public Key leftKeySecondary = Key.A;
        public Key rightKeyPrimary = Key.RightArrow;
        public Key rightKeySecondary = Key.L;

        [Tooltip("넘겨치기(왼손으로 채편/RightHitTrigger_GungChe를 쳐서 넘겨 치는 동작)를 키보드로 테스트하기 위한 키")]
        public Key crossHitKey = Key.O;

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || noteSpawner == null) return;

            if (kb[leftKeyPrimary].wasPressedThisFrame || kb[leftKeySecondary].wasPressedThisFrame)
            {
                noteSpawner.TryHit(NoteSide.Left);
            }

            if (kb[rightKeyPrimary].wasPressedThisFrame || kb[rightKeySecondary].wasPressedThisFrame)
            {
                noteSpawner.TryHit(NoteSide.Right);
            }

            if (kb[crossHitKey].wasPressedThisFrame)
            {
                // 넘겨치기: RightHitTrigger_GungChe와 동일하게 Cross 노트로 판정한다.
                noteSpawner.TryHit(NoteSide.Cross);
            }
        }
    }
}
