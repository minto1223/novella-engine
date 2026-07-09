using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Novella.Core
{
    public static class UIInputUtil
    {
        private static readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        /// <summary>現在のポインタ位置がボタン等の操作可能なUI要素の上にあるか判定する</summary>
        public static bool IsPointerOverInteractableUI()
        {
            if (EventSystem.current == null) return false;
            var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, _raycastResults);
            foreach (var result in _raycastResults)
            {
                if (result.gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }
            return false;
        }
    }
}
