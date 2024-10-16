using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menu.DragItems
{
    public class DragItem : MonoBehaviour, IDragItem
    {
        [SerializeField] private Button button;
        [SerializeField] private IDropParent parent;
        private Transform freeContent;
        public Transform MyTransform => transform;
        public Transform FreeContent => freeContent; 
        public IDropParent Parent => parent;
  
        public void SetDropParent(IDropParent dropParent) => parent = dropParent; 
        public void SetFreeContent(Transform freeContent) => this.freeContent = freeContent;

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            parent = MyTransform.parent.GetComponent<IDropParent>();
            button.image.raycastTarget = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            MyTransform.position = eventData.position;
            MyTransform.SetParent(FreeContent);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            button.image.raycastTarget = true;
            SetupParentPosition();
        }

        private void SetupParentPosition()
        {
            MyTransform.SetParent(parent.MyTransform);
            transform.localPosition = Vector3.zero;
        }

        public virtual void Updated()
        {
           
        }
    }
}