using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 챠트를 읽어 노트를 생성하고, 입력 판정(TryHit)과 놓친 노트 처리(자동 Miss)를 전담한다.
    /// </summary>
    public class NoteSpawner : MonoBehaviour
    {
        public Conductor conductor;
        public JangGuChart chart;

        public Note leftNotePrefab;
        public Note rightNotePrefab;

        public RectTransform leftSpawnPoint;
        public RectTransform rightSpawnPoint;
        public RectTransform judgmentPoint;
        public RectTransform noteParent;

        [Tooltip("노트가 스폰되어 판정선에 도달할 때까지 걸리는 시간(초)")]
        public float travelTime = 1.2f;

        public float perfectWindow = 0.05f;
        public float goodWindow = 0.12f;
        public float missWindow = 0.2f;

        public event Action<JudgeResult, NoteData> NoteResolved;

        readonly List<Note> active = new List<Note>();
        int nextNoteIndex;

        void OnEnable()
        {
            if (conductor != null) conductor.OnLoop += HandleLoop;
        }

        void OnDisable()
        {
            if (conductor != null) conductor.OnLoop -= HandleLoop;
        }

        void HandleLoop()
        {
            nextNoteIndex = 0;
        }

        void Update()
        {
            if (chart == null || conductor == null || chart.notes.Count == 0) return;

            float songTime = conductor.SongPositionSeconds;

            while (nextNoteIndex < chart.notes.Count)
            {
                NoteData data = chart.notes[nextNoteIndex];
                float hitTime = conductor.GetHitTime(data);

                if (hitTime - travelTime > songTime) break;

                SpawnNote(data, hitTime);
                nextNoteIndex++;
            }

            for (int i = active.Count - 1; i >= 0; i--)
            {
                Note note = active[i];
                if (note == null)
                {
                    active.RemoveAt(i);
                    continue;
                }

                if (songTime > note.HitTime + missWindow)
                {
                    active.RemoveAt(i);
                    NoteResolved?.Invoke(JudgeResult.Miss, note.Data);
                    Destroy(note.gameObject);
                }
            }
        }

        void SpawnNote(NoteData data, float hitTime)
        {
            Note prefab = data.side == NoteSide.Left ? leftNotePrefab : rightNotePrefab;
            RectTransform spawnPoint = data.side == NoteSide.Left ? leftSpawnPoint : rightSpawnPoint;
            if (prefab == null || spawnPoint == null || judgmentPoint == null) return;

            Note note = Instantiate(prefab, noteParent != null ? noteParent : transform);
            note.Init(conductor, data, hitTime, spawnPoint.anchoredPosition, judgmentPoint.anchoredPosition, travelTime);
            active.Add(note);
        }

        /// <summary>지정한 쪽(Left/Right)에 대해 지금 시점에서 가장 가까운 노트를 판정한다.</summary>
        public bool TryHit(NoteSide side)
        {
            Note best = null;
            float bestAbs = float.MaxValue;

            foreach (Note note in active)
            {
                if (note == null || note.Data.side != side) continue;

                float diff = Mathf.Abs(note.HitTime - conductor.SongPositionSeconds);
                if (diff < bestAbs)
                {
                    bestAbs = diff;
                    best = note;
                }
            }

            if (best == null || bestAbs > missWindow)
            {
                Debug.Log(best == null
                    ? $"[JangGuRhythm] TryHit({side}) 실패: 판정 가능한 활성 노트 없음 (song={conductor.SongPositionSeconds:F3})"
                    : $"[JangGuRhythm] TryHit({side}) 실패: 가장 가까운 노트와의 시간차={bestAbs:F3}s > missWindow={missWindow:F3}s (song={conductor.SongPositionSeconds:F3}, hitTime={best.HitTime:F3})");
                return false;
            }

            JudgeResult result = bestAbs <= perfectWindow ? JudgeResult.Perfect
                : bestAbs <= goodWindow ? JudgeResult.Good
                : JudgeResult.Miss;

            Debug.Log($"[JangGuRhythm] TryHit({side}) 성공: {result} (diff={bestAbs:F3}s)");

            active.Remove(best);
            NoteResolved?.Invoke(result, best.Data);
            Destroy(best.gameObject);
            return true;
        }
    }
}
