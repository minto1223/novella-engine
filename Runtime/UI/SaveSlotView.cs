using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Novella.UI
{
    /// <summary>
    /// セーブスロット1コマの UI 参照をまとめるコンポーネント。
    /// SavePanelBuilder がプレファブ生成時に配線し、SaveUIController が表示更新に使う。
    /// </summary>
    public class SaveSlotView : MonoBehaviour
    {
        public TMP_Text SlotNumberText;
        public TMP_Text DateText;
        public TMP_Text TitleText;
        public TMP_Text DialogueText;
        public GameObject NoDataOverlay;   // データなし時に表示するオーバーレイ
        public Image ThumbnailImage;       // サムネイル（将来スクリーンショット対応）
    }
}
