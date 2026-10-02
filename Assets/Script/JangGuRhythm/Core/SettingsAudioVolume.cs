// 같은 오브젝트의 AudioSource 음량을 JangGuSettings의 장구/노래 음량에 맞춘다.
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 장구 타격음 AudioSource에는 Channel=Drum, 노래 AudioSource에는 Channel=Music으로 붙인다.
    /// AudioSource에 원래 설정된 음량을 기준으로 설정 음량을 곱하고, 설정값이 바뀔 때만 다시 적용한다.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SettingsAudioVolume : MonoBehaviour
    {
        [SerializeField] JangGuSettings settings;
        [SerializeField] AudioChannel channel = AudioChannel.Drum;

        AudioSource source;
        float baseVolume;
        float appliedSetting = -1f;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            baseVolume = source.volume;

            if (settings == null)
            {
                Debug.LogError($"[JangGuRhythm] {name}: SettingsAudioVolume에 JangGuSettings가 연결되지 않았습니다.");
                enabled = false;
            }
        }

        void Update()
        {
            float setting = settings.GetVolume(channel);
            if (Mathf.Approximately(setting, appliedSetting)) return;

            appliedSetting = setting;
            source.volume = baseVolume * setting;
        }
    }
}
