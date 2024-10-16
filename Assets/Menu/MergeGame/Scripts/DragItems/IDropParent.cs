using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu.DragItems
{
    public interface IDropParent : IDropHandler
    {
        public Transform MyTransform { get; }

        /// <summary>
        /// список айтемов которые есть на объекте
        /// </summary>
        public List<IDragItem> DragItems { get; }

        /// <summary>
        /// Добавляет айтем в список DragItems 
        /// </summary>
        /// <param name="dragItem"></param>
        void AddDragItem(IDragItem dragItem);

        /// <summary>
        /// Удаляет айтем из списка DragItems, если он там был
        /// </summary>
        /// <param name="dragItem"></param>
        public void RemoveFromListItem(IDragItem dragItem);
    }
}