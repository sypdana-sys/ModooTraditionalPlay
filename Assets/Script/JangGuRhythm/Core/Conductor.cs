using System;
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 현재 재생 위치(초)를 관리하는 박자 기준 시계. NoteSpawner가 이 값을 기준으로 노트를 생성/이동시킨다.
    /// </summary>
    public class Conductor : MonoBehaviour
    {
        public JangGuChart chart;

        [Tooltip("끄면 Play()가 호출될 때까지 시계를 멈춘다. GripGuide처럼 연주 전 안내를 보여줄 때 끈다.")]
        public bool playOnStart = true;

        public bool IsPlaying { get; private set; }
        public float SongPositionSeconds { get; private set; }
        public float SecPerBeat => chart != null && chart.bpm > 0f ? 60f / chart.bpm : 0.5f;

        public event Action OnLoop;

        float loopLength = -1f;

        void Start()
        {
            SongPositionSeconds = 0f;
            RecalculateLoopLength();
            IsPlaying = playOnStart;
        }

        /// <summary>처음부터 시계를 시작한다. playOnStart를 끈 경우 안내가 끝난 뒤 호출한다.</summary>
        public void Play()
        {
            SongPositionSeconds = 0f;
            IsPlaying = true;
        }

        void RecalculateLoopLength()
        {
            if (chart == null || chart.notes.Count == 0)
            {
                loopLength = 4f;
                return;
            }

            float lastBeat = chart.notes[chart.notes.Count - 1].beat;
            loopLength = chart.leadInSeconds + (lastBeat + chart.tailBeats) * SecPerBeat;
        }

        void Update()
        {
            if (chart == null || !IsPlaying) return;

            SongPositionSeconds += Time.deltaTime;

            if (chart.loop && loopLength > 0f && SongPositionSeconds >= loopLength)
            {
                SongPositionSeconds -= loopLength;
                OnLoop?.Invoke();
            }
        }

        /// <summary>노트의 절대 판정 시각(초)을 계산한다.</summary>
        public float GetHitTime(NoteData note)
        {
            return chart.leadInSeconds + note.beat * SecPerBeat;
        }
    }
}
