// 장구 체험의 플레이 설정(채 충돌, 진동 세기, 장구·노래 음량)을 한곳에 모은 에셋.
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 채 프리팹과 씬 오브젝트가 같은 값을 읽도록 에셋으로 둔다. Play 중에 바꿔도 바로 반영된다.
    /// 에디터 Play 중 바꾼 값은 에셋에 그대로 남는다. 빌드에서는 실행할 때마다 에셋 값으로 시작한다.
    /// </summary>
    [CreateAssetMenu(menuName = "JangGu Rhythm/Settings", fileName = "JangGuSettings")]
    public class JangGuSettings : ScriptableObject
    {
        [Header("채 충돌")]
        [Tooltip("켜면 채가 장구 Collider에 막혀 표면에서 멈춘다. 끄면 채가 컨트롤러에 붙어 장구를 통과한다.")]
        public bool sticksCollideWithJanggu = true;

        [Header("진동")]
        [Tooltip("전체 진동 세기. 아래 판정별 세기에 곱한다. 0이면 진동하지 않는다.")]
        [Range(0f, 1f)] public float hapticStrength = 1f;

        [Range(0f, 1f)] public float perfectAmplitude = 1f;
        [Range(0f, 1f)] public float goodAmplitude = 0.6f;
        [Range(0f, 1f)] public float missAmplitude = 0.3f;

        [Tooltip("맞는 손으로 쳤지만 판정할 노트가 없을 때의 세기")]
        [Range(0f, 1f)] public float noNoteAmplitude = 0.1f;

        [Tooltip("진동 길이(초)")]
        public float hapticDuration = 0.08f;

        [Header("소리")]
        [Range(0f, 1f)] public float drumVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;

        /// <summary>판정 결과에 맞는 최종 진동 세기. result가 없으면 노트 없이 친 경우로 본다.</summary>
        public float GetHapticAmplitude(JudgeResult? result)
        {
            float amplitude = result switch
            {
                JudgeResult.Perfect => perfectAmplitude,
                JudgeResult.Good => goodAmplitude,
                JudgeResult.Miss => missAmplitude,
                _ => noNoteAmplitude,
            };
            return Mathf.Clamp01(amplitude * hapticStrength);
        }

        public float GetVolume(AudioChannel channel)
        {
            return channel == AudioChannel.Drum ? drumVolume : musicVolume;
        }
    }

    public enum AudioChannel
    {
        Drum,
        Music,
    }
}
