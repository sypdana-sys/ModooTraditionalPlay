using System;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 장단보 한 박(note) 정보. beat 단위는 JangGuChart.bpm 기준 박자 수(0부터 시작).
    /// </summary>
    [Serializable]
    public class NoteData
    {
        public float beat;
        public NoteSide side;

        /// <summary>구음(口音) 표기, 예: 콩/덕/기덕/더러러러/더. 노트 위에 표시되는 라벨.</summary>
        public string syllable = "콩";
    }
}
