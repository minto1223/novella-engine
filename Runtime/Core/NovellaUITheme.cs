using TMPro;
using UnityEngine;

namespace Novella.Core
{
    /// <summary>
    /// UIの見た目を一括で変更できるテーマ設定。
    /// Assets > Create > Novella > UI Theme で作成し、NovellaEngine にアタッチする。
    /// </summary>
    [CreateAssetMenu(fileName = "NovellaUITheme", menuName = "Novella/UI Theme")]
    public class NovellaUITheme : ScriptableObject
    {
        [Header("Font")]
        public TMP_FontAsset Font;

        [Header("Button Styles")]
        [Tooltip("メインメニュー等のプライマリボタン用スタイル（未設定なら従来のフラット色を適用）")]
        public NovellaButtonStyle PrimaryButtonStyle;
        [Tooltip("HUD等のアイコンボタン用スタイル（未設定なら従来のフラット色を適用）")]
        public NovellaButtonStyle IconButtonStyle;
        [Tooltip("終了・リセット等の危険操作ボタン用スタイル（未設定ならPrimaryにフォールバック）")]
        public NovellaButtonStyle DangerButtonStyle;

        [Header("Message Window")]
        [Tooltip("メッセージウィンドウの背景画像（未設定なら色のみ）")]
        public Sprite MessageWindowImage;
        public Color MessageWindowBackground = new Color(0f, 0f, 0.1f, 0.8f);
        public Color MessageTextColor = Color.white;
        public Color ReadTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        public Color CharacterNameColor = new Color(1f, 0.9f, 0.5f, 1f);
        [Range(16, 48)] public float FontSize = 24f;
        [Range(10, 100)] public float TextSpeed = 40f;

        [Header("Choice Buttons")]
        [Tooltip("選択肢ボタンの背景画像（未設定なら色のみ）")]
        public Sprite ChoiceButtonImage;
        public Color ChoiceButtonColor = new Color(0.2f, 0.3f, 0.5f, 0.9f);
        public Color ChoiceButtonHoverColor = new Color(0.3f, 0.4f, 0.7f, 1f);
        public Color ChoiceTextColor = Color.white;

        [Header("HUD / Menu")]
        [Tooltip("HUDボタンの背景画像（未設定なら色のみ）")]
        public Sprite HUDButtonImage;
        public Color HUDButtonColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        public Color HUDTextColor = Color.white;
        [Tooltip("メニューパネルの背景画像（未設定なら色のみ）")]
        public Sprite MenuPanelImage;
        public Color MenuPanelColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        [Header("Backlog")]
        [Tooltip("バックログパネルの背景画像（未設定なら色のみ）")]
        public Sprite BacklogPanelImage;
        public Color BacklogBackground = new Color(0f, 0f, 0.05f, 0.9f);
        public Color BacklogEntryColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        [Header("Save / Load")]
        [Tooltip("セーブ/ロードパネルの背景画像（未設定なら色のみ）")]
        public Sprite SavePanelImage;

        [Header("Settings")]
        [Tooltip("設定パネルの背景画像（未設定なら色のみ）")]
        public Sprite SettingsPanelImage;
        public Color SettingsTabActiveColor = new Color(0.25f, 0.45f, 0.75f, 1f);
        public Color SettingsTabInactiveColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        public Color SettingsTabActiveTextColor = Color.white;
        public Color SettingsTabInactiveTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("Confirm Dialog")]
        [Tooltip("確認ダイアログの背景画像（未設定なら色のみ）")]
        public Sprite ConfirmDialogPanelImage;
        public Color ConfirmDialogBackground = new Color(0.05f, 0.05f, 0.08f, 0.95f);
        public Color ConfirmDialogTextColor = Color.white;
        public Color ConfirmDialogYesButtonColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        public Color ConfirmDialogNoButtonColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        public Color ConfirmDialogButtonTextColor = Color.white;

        [Header("Title Display")]
        [Range(32, 96)] public float TitleFontSize = 64f;
        public Color TitleTextColor = Color.white;

        [Header("Title Screen")]
        [Tooltip("タイトル画面の背景画像")]
        public Sprite TitleBackgroundImage;
        [Tooltip("タイトル画面のロゴ画像（画面上部に表示）")]
        public Sprite TitleLogoImage;
        [Tooltip("タイトル画面のBGM（Resources/Audio/BGM/ 内のクリップ名）")]
        public string TitleBGM;
        [Tooltip("タイトル画面のボタン色")]
        public Color TitleButtonColor = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        [Tooltip("タイトル画面のボタンテキスト色")]
        public Color TitleButtonTextColor = Color.white;
        [Tooltip("タイトル画面のボタン背景画像（未設定なら色のみ）")]
        public Sprite TitleButtonImage;
    }
}
