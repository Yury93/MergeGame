using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu.DragItems
{
    public interface IDropParent : IDropHandler
    {
        public Transform MyTransform { get; }

        /// <summary>
        /// ������ ������� ������� ���� �� �������
        /// </summary>
        public List<IDragItem> DragItems { get; }

        /// <summary>
        /// ��������� ����� � ������ DragItems 
        /// </summary>
        /// <param name="dragItem"></param>
        void AddDragItem(IDragItem dragItem);

        /// <summary>
        /// ������� ����� �� ������ DragItems, ���� �� ��� ���
        /// </summary>
        /// <param name="dragItem"></param>
        public void RemoveFromListItem(IDragItem dragItem);
    }
}