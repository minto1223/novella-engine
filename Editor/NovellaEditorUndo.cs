#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Novella.Editor
{
    /// <summary>
    /// ビルダー／パッチャー系Editorツールの変更をUndoスタックに正しく積むためのヘルパー。
    ///
    /// 【なぜ必要か】
    /// SerializedObject.ApplyModifiedProperties() は自動でUndoを記録するが、
    /// new GameObject() / AddComponent() / DestroyImmediate() は記録されない。
    /// この非対称性のせいで、ツール実行後にCtrl+Zを押すと
    /// 「GameObjectは残ったまま参照フィールドだけnullに戻る」半壊状態になっていた。
    ///
    /// 【使い方】
    /// MenuItemなどのエントリポイントで Begin() → 処理 → End() で囲み、
    /// 生成・削除・既存オブジェクトへのAddComponentは本クラス経由で行う。
    /// End() が1回の実行分をまとめて1つのUndoグループに畳むため、
    /// Ctrl+Z一発でツール実行前の状態に丸ごと戻る。
    ///
    /// 【注意】
    /// Begin/End はネストさせないこと。他のビルダーから呼ばれる共有ヘルパー
    /// （ConfirmDialogBuilder.EnsureExists など）では Begin/End を呼ばず、
    /// 呼び出し元のエントリポイント側だけで囲む。
    /// </summary>
    public static class NovellaEditorUndo
    {
        /// <summary>Undoグループを開始し、End() に渡すグループ番号を返す。</summary>
        public static int Begin()
        {
            Undo.IncrementCurrentGroup();
            return Undo.GetCurrentGroup();
        }

        /// <summary>Begin() 以降の全操作を1グループに畳み、Undo履歴に表示する名前を付ける。</summary>
        public static void End(int group, string label)
        {
            Undo.SetCurrentGroupName(label);
            Undo.CollapseUndoOperations(group);
        }

        /// <summary>
        /// 生成したGameObjectをUndo対象として登録する。
        /// 子オブジェクトは親ごと消えるため、階層の**ルートだけ**登録すればよい。
        /// </summary>
        public static GameObject Created(GameObject go)
        {
            if (go != null) Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            return go;
        }

        /// <summary>既存オブジェクトの破棄。Undoで復活する。</summary>
        public static void Destroy(UnityEngine.Object obj)
        {
            if (obj != null) Undo.DestroyObjectImmediate(obj);
        }

        /// <summary>既存GameObjectへのコンポーネント追加。Undoで取り除かれる。</summary>
        public static T AddComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            return Undo.AddComponent<T>(go);
        }

        /// <summary>既存GameObjectに無ければ追加、あればそれを返す（冪等）。</summary>
        public static T EnsureComponent<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            var c = go.GetComponent<T>();
            if (c != null) return c;
            return Undo.AddComponent<T>(go);
        }

        /// <summary>既存オブジェクトのフィールドを直接書き換える前に呼ぶ。</summary>
        public static void Record(UnityEngine.Object obj, string label)
        {
            if (obj != null) Undo.RecordObject(obj, label);
        }

        /// <summary>既存オブジェクトの階層まるごとを記録する（子の並び替え・プロパティ変更を伴う場合）。</summary>
        public static void RecordHierarchy(GameObject go, string label)
        {
            if (go != null) Undo.RegisterFullObjectHierarchyUndo(go, label);
        }
    }
}
#endif
