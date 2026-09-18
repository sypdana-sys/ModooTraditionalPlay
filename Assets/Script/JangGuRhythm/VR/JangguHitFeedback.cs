using UnityEngine;
using UnityEngine.UI;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 어느 트리거를 어느 컨트롤러가 쳤는지 화면에 텍스트로 보여준다.
    /// VR 헤드셋 안에서는 콘솔 로그를 볼 수 없기 때문에 인게임 확인용으로 필요하다.
    /// </summary>
    public class JangguHitFeedback : MonoBehaviour
    {
        public Text hitText;

        static readonly Color CorrectColor = new Color(0.55f, 1f, 0.55f);
        static readonly Color CrossHandColor = new Color(1f, 0.65f, 0.4f);

        public void ShowHit(NoteSide triggerSide, NoteSide controllerSide, bool correctHand)
        {
            if (hitText == null) return;

            string triggerLabel = triggerSide == NoteSide.Left ? "왼쪽(북편)" : "오른쪽(채편)";
            string controllerLabel = controllerSide == NoteSide.Left ? "왼손 컨트롤러" : "오른손 컨트롤러";

            hitText.text = $"{triggerLabel} 트리거 ← {controllerLabel}" + (correctHand ? string.Empty : "  (교차 타격)");
            hitText.color = correctHand ? CorrectColor : CrossHandColor;
        }
    }
}
