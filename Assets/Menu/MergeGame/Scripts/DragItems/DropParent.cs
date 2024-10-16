using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu.DragItems
{
    public class DropParent : MonoBehaviour, IDropParent
    {
        protected List<IDragItem> dragItems = new List<IDragItem>();
        public List<IDragItem> DragItems => dragItems;
        Transform IDropParent.MyTransform => transform;

        /// <summary>
        /// Чтобы метод работал - должен быть включен рейкаст на имадже
        /// </summary>
        /// <param name="data"></param>
        public void OnDrop(PointerEventData data)
        {
            if (data.pointerDrag != null)
            {
                var dragItem = data.pointerDrag.GetComponent<IDragItem>();
                AddDragItem(dragItem);
            }
        }
        public virtual void AddDragItem(IDragItem dragItem)
        {
          if (dragItem.Parent != null) dragItem.Parent.RemoveFromListItem(dragItem);
            dragItem.SetDropParent(this);
            if (dragItems.Contains(dragItem) == false)
                dragItems.Add(dragItem);
        }
         
        public void RemoveFromListItem(IDragItem dragItem)
        {
            if (dragItems.Count > 0 && dragItems.Contains(dragItem))
                dragItems.Remove(dragItem); 
        }
    }
}