using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// ボタンの1状態分（通常/ホバー/押下/無効のいずれか）の見た目設定。
    /// </summary>
    [System.Serializable]
    public class ButtonStateStyle
    {
        [Tooltip("この状態専用のボタン画像（未設定なら共通のBackground Sprite、それも無ければ色のみで描画）")]
        public Sprite Sprite;

        [Tooltip("画像使用時の乗算色。白=画像そのままの色（画像がある時はBackgroundColorの代わりに使われる）")]
        public Color SpriteTint = Color.white;

        [Tooltip("画像を使わない時の背景色")]
        public Color BackgroundColor = new Color(0.118f, 0.137f, 0.251f, 1f);
        public Color BorderColor = new Color(0.165f, 0.188f, 0.314f, 1f);
        public Color TextColor = new Color(0.925f, 0.902f, 0.847f, 1f);

        [Tooltip("四隅の角括弧装飾を表示する")]
        public bool ShowCorners = false;

        [Tooltip("光が横切るエフェクト（sheen）を再生する")]
        public bool PlaySheen = false;

        [Tooltip("ボタンのスケール（押下時の縮み等）")]
        [Range(0.8f, 1.2f)]
        public float Scale = 1f;

        [Tooltip("この状態に入った時に鳴らすSE（未設定なら無音）")]
        public AudioClip EnterSe;
    }

    /// <summary>
    /// ボタンの4状態（通常/ホバー/押下/無効）の見た目を1アセットに集約したスタイル定義。
    /// Assets > Create > Novella > Button Style で作成し、NovellaUITheme または
    /// ボタン個別に割り当てる。未設定の場合は従来のフラット色適用にフォールバックする。
    /// </summary>
    [CreateAssetMenu(fileName = "NovellaButtonStyle", menuName = "Novella/Button Style")]
    public class NovellaButtonStyle : ScriptableObject
    {
        [Header("状態別スタイル")]
        public ButtonStateStyle Normal = new ButtonStateStyle();

        public ButtonStateStyle Hover = new ButtonStateStyle
        {
            BackgroundColor = new Color(0.157f, 0.184f, 0.322f, 1f),
            BorderColor = new Color(0.788f, 0.639f, 0.373f, 1f),
            TextColor = new Color(0.902f, 0.788f, 0.541f, 1f),
            ShowCorners = true,
            PlaySheen = true,
        };

        public ButtonStateStyle Pressed = new ButtonStateStyle
        {
            BackgroundColor = new Color(0.078f, 0.090f, 0.165f, 1f),
            BorderColor = new Color(0.788f, 0.639f, 0.373f, 1f),
            TextColor = new Color(0.902f, 0.788f, 0.541f, 1f),
            Scale = 0.98f,
        };

        public ButtonStateStyle Disabled = new ButtonStateStyle
        {
            BackgroundColor = new Color(0.078f, 0.090f, 0.165f, 1f),
            BorderColor = new Color(0.137f, 0.153f, 0.247f, 1f),
            TextColor = new Color(0.337f, 0.357f, 0.471f, 1f),
        };

        [Header("共通設定")]
        [Tooltip("状態間のトゥイーン遷移時間（秒）。0で即時切替")]
        [Range(0f, 1f)]
        public float TransitionDuration = 0.25f;

        [Tooltip("角括弧装飾の色")]
        public Color CornerColor = new Color(0.788f, 0.639f, 0.373f, 1f);

        [Tooltip("枠線を表示する。画像に枠が描き込んであるデザインではOFFにする")]
        public bool ShowBorder = true;

        [Tooltip("ボタン背景のスプライト（9スライス推奨。全状態共通。状態別Spriteが設定されている状態ではそちらが優先）")]
        public Sprite BackgroundSprite;
    }
}
