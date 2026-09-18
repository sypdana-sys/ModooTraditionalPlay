using System.Collections.Generic;
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 하나의 장단(리듬 패턴)을 담는 데이터 에셋. 인스펙터에서 직접 노트를 추가/수정할 수 있다.
    /// </summary>
    [CreateAssetMenu(menuName = "JangGuRhythm/Jangdan Chart", fileName = "NewJangdanChart")]
    public class JangGuChart : ScriptableObject
    {
        public string jangdanName = "기본 장단";
        public float bpm = 92f;

        [Tooltip("게임 시작 후 첫 노트가 판정선에 도달하기까지의 대기 시간(초)")]
        public float leadInSeconds = 1.5f;

        [Tooltip("마지막 노트 이후 다시 처음으로 돌아가기 전 여백 박자 수")]
        public float tailBeats = 2f;

        public bool loop = true;

        public List<NoteData> notes = new List<NoteData>();
    }
}
