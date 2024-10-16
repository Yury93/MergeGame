using UnityEngine;
using UnityEngine.EventSystems;

namespace Menu.DragItems
{
    public interface IDragItem : IDragHandler, IBeginDragHandler, IEndDragHandler, IUpdater
    {
        public Transform MyTransform { get; }
        public Transform FreeContent { get; }
        public IDropParent Parent { get; } 
        /// <summary>
        /// Задаётся контент по которому будут перемещаться Айтемы
        /// </summary>
        /// <param name="freeContent"></param>
        public void SetFreeContent(Transform freeContent);
        /// <summary>
        /// Задать объект в который дропнут Айтем
        /// </summary>
        /// <param name="dropParent"></param>
        void SetDropParent(IDropParent dropParent); 
    }
    public interface IUpdater
    { 
        public abstract void Updated();
    }
}