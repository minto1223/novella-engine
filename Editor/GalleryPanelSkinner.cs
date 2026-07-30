using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Novella.Core;
using Novella.UI;

namespace NovellaEditor
{
    /// <summary>
    /// ギャラリー系5画面（CGギャラリー・シーン回想・章選択・BGM回想・エンディングリスト）に
    /// 仮テーマ（サンプルの水色テーマ）のスキンを一括適用するMCP作業用ツール。
    ///
    /// 一覧の行はコントローラーが実行時に生成するため、パネル本体の意匠に加えて
    /// 各コントローラーのシリアライズ済み色フィールドも書き換える。
    /// </summary>
    public static class GalleryPanelSkinner
    {
        private static readonly Color Ink = new Color(0.133f, 0.22f, 0.29f, 1f);
        private static readonly Color UiBlue = new Color(0.239f, 0.416f, 0.541f, 1f);
        private static readonly Color SubBlue = new Color(0.357f, 0.498f, 0.608f, 1f);
        private static readonly Color Accent = new Color(0.302f, 0.639f, 0.851f, 1f);

        // 一覧の行（白カード基調。ホバーでわずかに青被り）
        private static readonly Color Row = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color RowHover = new Color(0.898f, 0.945f, 0.976f, 1f);
        private static readonly Color RowPressed = new Color(0.827f, 0.898f, 0.945f, 1f);
        private static readonly Color RowLocked = new Color(0.929f, 0.945f, 0.957f, 0.75f);
        private static readonly Color RowCleared = new Color(0.925f, 0.969f, 0.949f, 0.95f);
        private static readonly Color Muted = new Color(0.596f, 0.663f, 0.702f, 1f);
        private static readonly Color BadgeLocked = new Color(0.792f, 0.827f, 0.847f, 1f);
        private static readonly Color Cleared = new Color(0.298f, 0.686f, 0.502f, 1f);
        private static readonly Color ThumbPlaceholder = new Color(0.85f, 0.9f, 0.94f, 1f);

        private static readonly string[] Panels =
        {
            "GalleryPanel", "FullViewPanel", "RecollectionPanel",
            "ChapterSelectPanel", "BGMGalleryPanel", "EndingListPanel",
        };

        [MenuItem("Tools/Novella/Apply Sample Theme Gallery Skin")]
        public static void Apply()
        {
            var bg = Load("bg_sky_gradient.png");
            var card = Load("panel_card.png");
            var pill = Load("button_pill.png");
            var pillAccent = Load("button_pill_accent.png");
            var rect = Load("button_rect_white.png");
            if (bg == null || card == null || pill == null)
            {
                Debug.LogError("[GallerySkinner] theme sprites not found (expected under UI/Sprites/Theme)");
                return;
            }

            int done = 0;
            foreach (var name in Panels)
            {
                var panel = FindPanel(name);
                if (panel == null) { Debug.LogWarning($"[GallerySkinner] {name} not found, skipped"); continue; }

                Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "Sample Theme Gallery Skin");

                if (name == "FullViewPanel")
                {
                    // CG拡大表示は画像を邪魔しないよう、白ではなく薄い暗幕のままにする
                    SetImage(panel.GetComponent<Image>(), null, Image.Type.Simple, new Color(0.06f, 0.11f, 0.16f, 0.9f));
                }
                else
                {
                    SetImage(panel.GetComponent<Image>(), bg, Image.Type.Simple, new Color(1, 1, 1, 0.97f));
                }

                FixViewportMask(panel);
                SkinTitles(panel);
                SkinCloseButton(panel, pillAccent);
                SkinScrollBar(panel);
                SkinLayout(panel);
                done++;
            }

            ApplyControllerColors(rect);

            EditorSceneMarkDirty();
            Debug.Log($"[GallerySkinner] Sample theme gallery skin applied to {done} panels.");
        }

        /// <summary>非アクティブなパネルも含め、全ルートCanvas直下から名前で探す。</summary>
        private static Transform FindPanel(string name)
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas) continue;
                var found = canvas.transform.Find(name);
                if (found != null) return found;
            }
            return null;
        }

        private static void SkinTitles(Transform panel)
        {
            foreach (var txt in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                // 見出し（"〜Title"）は太字インク、その他の固定ラベルはサブ色
                if (txt.name.EndsWith("Title"))
                {
                    txt.color = Ink;
                    txt.fontStyle = FontStyles.Bold;
                    txt.characterSpacing = 8f;
                }
                else if (txt.name == "NowPlayingLabel")
                {
                    txt.color = SubBlue;
                }
            }
        }

        private static void SkinCloseButton(Transform panel, Sprite pillAccent)
        {
            foreach (var btn in panel.GetComponentsInChildren<Button>(true))
            {
                if (!btn.name.EndsWith("CloseButton")) continue;
                SetImage(btn.GetComponent<Image>(), pillAccent, Image.Type.Sliced, Color.white);
                var txt = btn.GetComponentInChildren<TMP_Text>(true);
                if (txt != null) { txt.color = Color.white; txt.fontStyle = FontStyles.Bold; }
            }
        }

        /// <summary>
        /// Viewport の Mask をアルファ0の Image と組み合わせているとステンシルが書かれず、
        /// 一覧の中身が丸ごと不可視になる。他パネルと同じ RectMask2D に差し替える。
        /// （EndingListPanel が該当。ビルダー側も修正済みだが既存シーンはここで直す）
        /// </summary>
        private static void FixViewportMask(Transform panel)
        {
            foreach (var mask in panel.GetComponentsInChildren<Mask>(true))
            {
                var go = mask.gameObject;
                Undo.DestroyObjectImmediate(mask);
                var img = go.GetComponent<Image>();
                if (img != null && img.color.a <= 0f) Undo.DestroyObjectImmediate(img);
                if (go.GetComponent<RectMask2D>() == null) Undo.AddComponent<RectMask2D>(go);
                Debug.Log($"[GallerySkinner] {panel.name}/{go.name}: Mask -> RectMask2D (list content was fully clipped)");
            }
        }

        /// <summary>
        /// 縦積みレイアウトを中央寄せの列にする。
        /// 既定は childForceExpandWidth=true で閉じるボタンが画面全幅の帯になってしまうため、
        /// ボタンだけ固定幅にし、見出しと一覧は flexibleWidth で列幅いっぱいに広げる。
        /// </summary>
        private static void SkinLayout(Transform panel)
        {
            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) return; // FullViewPanel はアンカー配置なので対象外

            vlg.padding = new RectOffset(180, 180, 48, 48);
            vlg.spacing = 16f;
            vlg.childForceExpandWidth = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            foreach (Transform child in panel)
            {
                var le = child.GetComponent<LayoutElement>();
                if (le == null) continue;
                if (child.name.EndsWith("CloseButton"))
                {
                    le.preferredWidth = 280f;
                    le.preferredHeight = 54f;
                    le.flexibleWidth = -1f;
                }
                else
                {
                    le.flexibleWidth = 1f;
                }
            }

            // 一覧の行はコントローラーが sizeDelta で高さを決めているが、
            // childControlHeight=true だと中の文字1行分（34px程度）まで潰れてしまう
            foreach (var sr in panel.GetComponentsInChildren<ScrollRect>(true))
            {
                if (sr.content == null) continue;
                var contentVlg = sr.content.GetComponent<VerticalLayoutGroup>();
                if (contentVlg == null) continue;
                contentVlg.childControlHeight = false;
                contentVlg.spacing = 10f;
            }
        }

        private static void SkinScrollBar(Transform panel)
        {
            foreach (var sb in panel.GetComponentsInChildren<Scrollbar>(true))
            {
                var bgImg = sb.GetComponent<Image>();
                if (bgImg != null) bgImg.color = new Color(1f, 1f, 1f, 0.45f);
                if (sb.targetGraphic is Image handle) handle.color = new Color(Accent.r, Accent.g, Accent.b, 0.65f);
            }
        }

        /// <summary>実行時に行を生成するコントローラーの色フィールドをテーマ準拠に書き換える。</summary>
        private static void ApplyControllerColors(Sprite rowSprite)
        {
            var recollection = Object.FindFirstObjectByType<SceneRecollectionUIController>(FindObjectsInactive.Include);
            SetFields(recollection, new (string, object)[]
            {
                ("_itemSprite", rowSprite),
                ("_unlockedColor", Row), ("_lockedColor", RowLocked),
                ("_unlockedTextColor", Ink), ("_lockedTextColor", Muted),
                ("_hoverColor", RowHover), ("_pressedColor", RowPressed),
                ("_thumbPlaceholderColor", ThumbPlaceholder), ("_emptyTextColor", Muted),
            });

            var chapter = Object.FindFirstObjectByType<ChapterSelectUIController>(FindObjectsInactive.Include);
            SetFields(chapter, new (string, object)[]
            {
                ("_itemSprite", rowSprite),
                ("_cardUnlocked", Row), ("_cardLocked", RowLocked), ("_cardCleared", RowCleared),
                ("_badgeColor", Accent), ("_badgeClearedColor", Cleared), ("_badgeLockedColor", BadgeLocked),
                ("_cardHoverColor", RowHover), ("_cardPressedColor", RowPressed),
                ("_titleTextColor", Ink), ("_lockedTextColor", Muted),
                ("_progressTextColor", SubBlue), ("_emptyTextColor", Muted),
                ("_replayColor", new Color(1f, 1f, 1f, 0.75f)),
                ("_replayHoverColor", RowHover), ("_replayPressedColor", RowPressed),
                ("_replayTextColor", UiBlue),
            });

            var bgm = Object.FindFirstObjectByType<BGMGalleryUIController>(FindObjectsInactive.Include);
            SetFields(bgm, new (string, object)[]
            {
                ("_itemSprite", rowSprite),
                ("_itemColor", Row), ("_itemHighlightColor", RowHover), ("_itemPressedColor", RowPressed),
                ("_itemTextColor", Ink), ("_iconColor", Accent), ("_emptyTextColor", Muted),
            });

            var ending = Object.FindFirstObjectByType<EndingListUIController>(FindObjectsInactive.Include);
            SetFields(ending, new (string, object)[]
            {
                ("_itemSprite", rowSprite),
                ("_unlockedColor", RowCleared), ("_lockedColor", RowLocked),
                ("_unlockedTextColor", Ink), ("_lockedTextColor", Muted),
                ("_progressTextColor", SubBlue), ("_emptyTextColor", Muted),
            });
        }

        private static void SetFields(Object target, (string name, object value)[] fields)
        {
            if (target == null) return;
            var so = new SerializedObject(target);
            foreach (var (name, value) in fields)
            {
                var prop = so.FindProperty(name);
                if (prop == null) { Debug.LogWarning($"[GallerySkinner] {target.GetType().Name}.{name} not found"); continue; }
                if (value is Color c) prop.colorValue = c;
                else prop.objectReferenceValue = value as Object;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        private static Sprite Load(string file) => ThemeAssetLocator.Sprite(file);

        private static void SetImage(Image img, Sprite sprite, Image.Type type, Color color)
        {
            if (img == null) return;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = type;
            }
            img.color = color;
        }

        private static void EditorSceneMarkDirty()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
