using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace HuaHaiLiKanHua
{
    public class TextAnimation : MonoBehaviour
    {
        TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }
        private void Start()
        {
            LogWordAnimation();
        }

        /// <summary>
        /// 打印文字动画
        /// </summary>
        public void LogWordAnimation()
        {
            LMotion.String.Create128Bytes("", "<color=red>Zero</color> Allocation <i>Text</i> Tween! <b>Foooooo!!</b>", 5f)
      .WithRichText()//启用富文本
      .WithScrambleChars(ScrambleMode.Lowercase)//用 a-z 的小写字母当乱码
      .BindToText(text);//把算出来的字符串，每帧塞进这个 Text 里

        }

        /// <summary>
        /// 数值改变动画
        /// </summary>
        public void ValueChangeAnimation()
        {
            LMotion.Create(0f, 100000f, 2f)
        .BindToText(text, "{0:N2}"); //带有逗号，保留两位小数
        }

        /// <summary>
        /// 控制单个字符串动画
        /// </summary>
        public void SingleWordAnimation()
        {
            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                LMotion.Create(Color.white, Color.red, 1f)
                    .WithDelay(i * 0.1f)
                    .WithEase(Ease.OutQuad)
                    .BindToTMPCharColor(text, i);

                LMotion.Punch.Create(Vector3.zero, Vector3.up * 30f, 1f)
                    .WithDelay(i * 0.1f)
                    .WithEase(Ease.OutQuad)
                    .BindToTMPCharPosition(text, i);
            }
        }
    }

}

