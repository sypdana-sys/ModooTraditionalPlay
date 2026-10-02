using UnityEngine;
using UnityEngine.UI;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// NoteSpawner.NoteResolved 이벤트를 구독해 점수/콤보/판정 텍스트를 갱신한다.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public NoteSpawner noteSpawner;
        public Text scoreText;
        public Text comboText;
        public Text judgeFeedbackText;

        int score;
        int combo;

        void OnEnable()
        {
            if (noteSpawner != null) noteSpawner.NoteResolved += HandleResolved;
        }

        void OnDisable()
        {
            if (noteSpawner != null) noteSpawner.NoteResolved -= HandleResolved;
        }

        void HandleResolved(JudgeResult result, NoteData data)
        {
            switch (result)
            {
                case JudgeResult.Perfect:
                    score += 100;
                    combo++;
                    break;
                case JudgeResult.Good:
                    score += 50;
                    combo++;
                    break;
                case JudgeResult.Miss:
                    combo = 0;
                    break;
            }

            if (scoreText != null) scoreText.text = $"SCORE {score}";
            if (comboText != null) comboText.text = combo > 0 ? $"COMBO {combo}" : string.Empty;

            if (judgeFeedbackText != null)
            {
                judgeFeedbackText.text = result switch
                {
                    JudgeResult.Perfect => "정확",
                    JudgeResult.Good => "보통",
                    JudgeResult.Miss => "미흡",
                    _ => string.Empty,
                };
            }
        }
    }
}
