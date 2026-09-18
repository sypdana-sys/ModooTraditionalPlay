using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// XR 컨트롤러(Left Controller / Right Controller) 오브젝트에 붙여서
    /// "이 콜라이더 계층은 왼쪽/오른쪽 컨트롤러 소속"임을 표시한다.
    /// JangguHitTrigger가 OnTriggerEnter 시 GetComponentInParent로 이 마커를 찾아 어느 손인지 판별한다.
    /// </summary>
    public class ControllerSideMarker : MonoBehaviour
    {
        public NoteSide side;
    }
}
