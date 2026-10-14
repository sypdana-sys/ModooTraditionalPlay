// 한 글자씩 줄을 바꿔 TMP 텍스트를 세로쓰기로 표시한다.
using TMPro;
using UnityEngine;

namespace FindOurSound.UI
{
    /// <summary>
    /// Inspector의 text를 글자마다 줄바꿈해 target에 넣는다. 공백은 빈 줄(띄어쓰기 간격)이 된다.
    /// 프리팹에서 text만 바꾸면 세로 글자가 갱신된다.
    /// </summary>
    [ExecuteAlways]
    public class VerticalText : MonoBehaviour
    {
        [SerializeField] string text;
        [SerializeField] TMP_Text target;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                Apply();
            }
        }

        void Awake()
        {
            Apply();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            // OnValidate 안에서 TMP 메시를 직접 갱신하면 경고가 나므로 한 프레임 미룬다.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Apply();
            };
        }
#endif

        void Apply()
        {
            if (target == null) target = GetComponent<TMP_Text>();
            if (target == null || text == null) return;

            var sb = new System.Text.StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0) sb.Append('\n');
                if (text[i] != ' ') sb.Append(text[i]);
            }
            target.text = sb.ToString();
        }
    }
}
