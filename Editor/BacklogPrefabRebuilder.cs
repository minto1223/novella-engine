using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Novella.Editor
{
    public static class BacklogPrefabRebuilder
    {
        [MenuItem("Novella/Rebuild Backlog Search Bar")]
        public static void RebuildSearchBar()
        {
            BuildSearchBar();
        }

        [MenuItem("Novella/Rebuild Backlog Prefab")]
        public static void Rebuild()
        {
            // --- 1. BacklogEntry Prefab を再構築 ---
            string prefabPath = "Assets/Novella/Prefabs/BacklogEntry.prefab";
            
            // 既存Prefabのフォントを取得
            TMP_FontAsset font = null;
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                var tmpText = existing.GetComponentInChildren<TMP_Text>();
                if (tmpText != null) font = tmpText.font;
            }
            
            // 新しいGameObjectを構築
            var root = new GameObject("BacklogEntry", typeof(RectTransform));
            var rootRT = root.GetComponent<RectTransform>();
            
            // ルート: HorizontalLayoutGroup（左：テキスト列、右：ジャンプボタン）
            var rootHLG = root.AddComponent<HorizontalLayoutGroup>();
            rootHLG.padding = new RectOffset(36, 20, 20, 20);
            rootHLG.spacing = 12;
            rootHLG.childAlignment = TextAnchor.UpperRight;
            rootHLG.childControlWidth = true;
            rootHLG.childControlHeight = true;
            rootHLG.childForceExpandWidth = false;
            rootHLG.childForceExpandHeight = false;

            // ContentSizeFitter（高さ自動）
            var rootCSF = root.AddComponent<ContentSizeFitter>();
            rootCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // カード背景（半透明の暗い背景）
            var rootImg = root.AddComponent<Image>();
            rootImg.color = new Color(0.02f, 0.02f, 0.06f, 0.55f);

            // --- TextColumn: キャラ名 + セリフを縦に並べる ---
            var textCol = new GameObject("TextColumn", typeof(RectTransform));
            textCol.transform.SetParent(root.transform, false);

            var textColVLG = textCol.AddComponent<VerticalLayoutGroup>();
            textColVLG.padding = new RectOffset(0, 0, 0, 0);
            textColVLG.spacing = 6;
            textColVLG.childAlignment = TextAnchor.UpperLeft;
            textColVLG.childControlWidth = true;
            textColVLG.childControlHeight = true;
            textColVLG.childForceExpandWidth = true;
            textColVLG.childForceExpandHeight = false;

            var textColLE = textCol.AddComponent<LayoutElement>();
            textColLE.flexibleWidth = 1; // テキスト列が残りスペースを埋める

            // キャラ名テキスト
            var charName = new GameObject("CharNameText", typeof(RectTransform));
            charName.transform.SetParent(textCol.transform, false);
            var charTMP = charName.AddComponent<TextMeshProUGUI>();
            charTMP.text = "キャラ名";
            charTMP.fontSize = 42;
            charTMP.fontStyle = FontStyles.Bold;
            charTMP.color = new Color(1f, 0.45f, 0.35f, 1f); // 赤オレンジ（参考画像風）
            charTMP.textWrappingMode = TextWrappingModes.NoWrap;
            charTMP.overflowMode = TextOverflowModes.Overflow;
            charTMP.horizontalAlignment = HorizontalAlignmentOptions.Left;
            charTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            if (font != null) charTMP.font = font;

            // セリフ本体
            var dialogue = new GameObject("DialogueText", typeof(RectTransform));
            dialogue.transform.SetParent(textCol.transform, false);
            var dlgTMP = dialogue.AddComponent<TextMeshProUGUI>();
            dlgTMP.text = "セリフテキスト";
            dlgTMP.fontSize = 44;
            dlgTMP.color = new Color(1f, 1f, 1f, 0.95f);
            dlgTMP.textWrappingMode = TextWrappingModes.Normal;
            dlgTMP.overflowMode = TextOverflowModes.Overflow;
            dlgTMP.horizontalAlignment = HorizontalAlignmentOptions.Left;
            dlgTMP.verticalAlignment = VerticalAlignmentOptions.Top;
            dlgTMP.margin = new Vector4(12, 0, 0, 0); // 左インデント
            if (font != null) dlgTMP.font = font;

            // --- JumpButton: カード右上に配置、常に表示 ---
            var jumpBtn = new GameObject("JumpButton", typeof(RectTransform));
            jumpBtn.transform.SetParent(root.transform, false);

            var jumpBtnImg = jumpBtn.AddComponent<Image>();
            jumpBtnImg.color = new Color(0.3f, 0.5f, 0.8f, 0.7f); // 青系の背景で目立たせる

            var jumpBtnComp = jumpBtn.AddComponent<Button>();
            var jumpBtnColors = jumpBtnComp.colors;
            jumpBtnColors.normalColor = new Color(0.3f, 0.5f, 0.8f, 0.7f);
            jumpBtnColors.highlightedColor = new Color(0.4f, 0.65f, 1f, 0.9f);
            jumpBtnColors.pressedColor = new Color(0.2f, 0.4f, 0.7f, 1f);
            jumpBtnComp.colors = jumpBtnColors;

            var jumpBtnLE = jumpBtn.AddComponent<LayoutElement>();
            jumpBtnLE.preferredWidth = 44;
            jumpBtnLE.preferredHeight = 44;
            jumpBtnLE.minWidth = 44;
            jumpBtnLE.minHeight = 44;
            jumpBtnLE.flexibleWidth = 0;

            // ジャンプボタンのテキスト（ASCII矢印で確実に表示）
            var jumpText = new GameObject("JumpArrow", typeof(RectTransform));
            jumpText.transform.SetParent(jumpBtn.transform, false);
            var jumpTMP = jumpText.AddComponent<TextMeshProUGUI>();
            jumpTMP.text = ">"; // ASCIIで確実に表示される
            jumpTMP.fontSize = 30;
            jumpTMP.fontStyle = FontStyles.Bold;
            jumpTMP.color = new Color(1f, 1f, 1f, 0.95f);
            jumpTMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
            jumpTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            jumpTMP.textWrappingMode = TextWrappingModes.NoWrap;
            if (font != null) jumpTMP.font = font;

            var jumpTextRT = jumpText.GetComponent<RectTransform>();
            jumpTextRT.anchorMin = Vector2.zero;
            jumpTextRT.anchorMax = Vector2.one;
            jumpTextRT.offsetMin = Vector2.zero;
            jumpTextRT.offsetMax = Vector2.zero;
            
            // Prefab保存
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            
            // --- 2. シーン内のEntryContainerのSpacingを更新 ---
            UpdateSceneEntryContainer();
            
            Debug.Log("[BacklogPrefabRebuilder] Prefab rebuilt and scene updated.");
            AssetDatabase.Refresh();
        }
        
        private static void UpdateSceneEntryContainer()
        {
            // シーン内のBacklogUIControllerを見つけてEntryContainerのSpacingを更新
            var controllers = Object.FindObjectsByType<Novella.UI.BacklogUIController>(FindObjectsSortMode.None);
            foreach (var ctrl in controllers)
            {
                // ScrollRectを探す
                var scrollRect = ctrl.GetComponentInChildren<ScrollRect>(true);
                if (scrollRect == null) continue;
                
                Transform content = scrollRect.content;
                if (content == null)
                {
                    // Viewportの子を探す
                    var viewport = scrollRect.transform.Find("Viewport");
                    if (viewport != null && viewport.childCount > 0)
                        content = viewport.GetChild(0);
                }
                if (content == null) continue;
                
                var vlg = content.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                
                vlg.spacing = 24; // エントリ間の均等間隔
                vlg.padding = new RectOffset(60, 60, 30, 30); // 左右に余白
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;
                
                // ContentSizeFitter
                var csf = content.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = content.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                UnityEditor.EditorUtility.SetDirty(content.gameObject);
                Debug.Log($"[BacklogPrefabRebuilder] Updated EntryContainer spacing on {ctrl.gameObject.name}");
            }
        }

        private static void BuildSearchBar()
        {
            var canvas = GameObject.Find("NovellaCanvas");
            if (canvas == null) { Debug.LogError("[Novella] NovellaCanvas が見つかりません。"); return; }

            var panelTr = canvas.transform.Find("BacklogPanel");
            if (panelTr == null) { Debug.LogError("[Novella] BacklogPanel が見つかりません。"); return; }

            // 既存のSearchBarがあれば削除
            var existingSearch = panelTr.Find("SearchBar");
            if (existingSearch != null)
                Object.DestroyImmediate(existingSearch.gameObject);

            // フォント取得
            TMP_FontAsset font = null;
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/font_1_kokugl_1.asset");
            if (fontAsset != null) font = fontAsset;

            // BacklogPanelにVLGがなければ追加（SearchBar + ScrollView を縦に並べる）
            var panelVLG = panelTr.GetComponent<VerticalLayoutGroup>();
            if (panelVLG == null)
            {
                panelVLG = panelTr.gameObject.AddComponent<VerticalLayoutGroup>();
                panelVLG.padding = new RectOffset(0, 0, 0, 0);
                panelVLG.spacing = 0;
                panelVLG.childControlWidth = true;
                panelVLG.childControlHeight = true;
                panelVLG.childForceExpandWidth = true;
                panelVLG.childForceExpandHeight = false;
            }

            // ScrollViewにLayoutElement（flexibleHeight）を設定
            var scrollView = panelTr.Find("BacklogScrollView");
            if (scrollView != null)
            {
                var scrollLE = scrollView.GetComponent<LayoutElement>();
                if (scrollLE == null) scrollLE = scrollView.gameObject.AddComponent<LayoutElement>();
                scrollLE.flexibleHeight = 1;
                scrollLE.flexibleWidth = 1;
            }

            // --- SearchBar ---
            var searchBar = new GameObject("SearchBar", typeof(RectTransform));
            searchBar.transform.SetParent(panelTr, false);
            searchBar.transform.SetAsFirstSibling(); // パネル最上部に配置

            var searchBarLE = searchBar.AddComponent<LayoutElement>();
            searchBarLE.preferredHeight = 60;
            searchBarLE.flexibleHeight = 0;

            var searchBarHLG = searchBar.AddComponent<HorizontalLayoutGroup>();
            searchBarHLG.padding = new RectOffset(60, 60, 8, 8);
            searchBarHLG.spacing = 10;
            searchBarHLG.childAlignment = TextAnchor.MiddleLeft;
            searchBarHLG.childControlWidth = true;
            searchBarHLG.childControlHeight = true;
            searchBarHLG.childForceExpandWidth = false;
            searchBarHLG.childForceExpandHeight = true;

            var searchBarBg = searchBar.AddComponent<Image>();
            searchBarBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            // 検索アイコンラベル
            var iconGO = new GameObject("SearchIcon", typeof(RectTransform));
            iconGO.transform.SetParent(searchBar.transform, false);
            var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
            iconTMP.text = "Q";
            iconTMP.fontSize = 28;
            iconTMP.color = new Color(0.6f, 0.6f, 0.7f, 1f);
            iconTMP.horizontalAlignment = HorizontalAlignmentOptions.Center;
            iconTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            if (font != null) iconTMP.font = font;
            var iconLE = iconGO.AddComponent<LayoutElement>();
            iconLE.preferredWidth = 40;

            // InputField (TMP)
            var inputGO = new GameObject("SearchInput", typeof(RectTransform));
            inputGO.transform.SetParent(searchBar.transform, false);

            var inputBg = inputGO.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            var inputLE = inputGO.AddComponent<LayoutElement>();
            inputLE.flexibleWidth = 1;

            // Text Area
            var textArea = new GameObject("Text Area", typeof(RectTransform));
            textArea.transform.SetParent(inputGO.transform, false);
            var textAreaRT = textArea.GetComponent<RectTransform>();
            textAreaRT.anchorMin = Vector2.zero;
            textAreaRT.anchorMax = Vector2.one;
            textAreaRT.offsetMin = new Vector2(10, 0);
            textAreaRT.offsetMax = new Vector2(-10, 0);
            textArea.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderGO = new GameObject("Placeholder", typeof(RectTransform));
            placeholderGO.transform.SetParent(textArea.transform, false);
            var placeholderRT = placeholderGO.GetComponent<RectTransform>();
            placeholderRT.anchorMin = Vector2.zero;
            placeholderRT.anchorMax = Vector2.one;
            placeholderRT.sizeDelta = Vector2.zero;
            var placeholderTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholderTMP.text = "検索...";
            placeholderTMP.fontSize = 26;
            placeholderTMP.fontStyle = FontStyles.Italic;
            placeholderTMP.color = new Color(0.5f, 0.5f, 0.55f, 0.8f);
            placeholderTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            if (font != null) placeholderTMP.font = font;

            // Input Text
            var inputTextGO = new GameObject("Text", typeof(RectTransform));
            inputTextGO.transform.SetParent(textArea.transform, false);
            var inputTextRT = inputTextGO.GetComponent<RectTransform>();
            inputTextRT.anchorMin = Vector2.zero;
            inputTextRT.anchorMax = Vector2.one;
            inputTextRT.sizeDelta = Vector2.zero;
            var inputTextTMP = inputTextGO.AddComponent<TextMeshProUGUI>();
            inputTextTMP.text = "";
            inputTextTMP.fontSize = 26;
            inputTextTMP.color = Color.white;
            inputTextTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            if (font != null) inputTextTMP.font = font;

            // TMP_InputField 設定
            var inputField = inputGO.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRT;
            inputField.textComponent = inputTextTMP;
            inputField.placeholder = placeholderTMP;
            inputField.fontAsset = font;
            inputField.pointSize = 26;

            // --- BacklogUIControllerに_searchInput を配線 ---
            var novellaManager = GameObject.Find("NovellaManager");
            var backlogUI = novellaManager != null ? novellaManager.GetComponent<Novella.UI.BacklogUIController>() : null;
            if (backlogUI != null)
            {
                var so = new SerializedObject(backlogUI);
                so.FindProperty("_searchInput").objectReferenceValue = inputField;
                so.ApplyModifiedProperties();
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[BacklogPrefabRebuilder] Search bar built and wired.");
        }
    }
}
