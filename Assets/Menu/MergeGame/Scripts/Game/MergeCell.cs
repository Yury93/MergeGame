using Menu.DragItems; 
using System; 
using UnityEngine; 

namespace Menu.MergeGame 
{
    public class MergeCell : DropParent
    {
        [field: SerializeField] public int Id { get; set; }
        public Action<MergeItem, MergeItem> onMerge;

        public int LevelMergeItem
        {
            get
            {
                if(dragItems.Count > 0)
                {
                    var mergeitem = ConvertToMergeItem(dragItems[0]);
                    return mergeitem.Level;
                }
                return 0;
            }
        }
        public override void AddDragItem(IDragItem dragItem)
        {
            MergeItem dropItem = ConvertToMergeItem(dragItem);
            if(dragItems.Count > 0)
            {
                MergeItem itemInCell = ConvertToMergeItem(dragItems[0]);
                if (itemInCell.IsDragProcess) { Debug.LogError($"IsDragProcess failture"); return; }
                if (itemInCell.Level == dropItem.Level)
                {
                    Merge(itemInCell, dropItem);
                }
                else
                {
                    SwapCells(itemInCell, dropItem); 
                }
            }
            else
            {
                base.AddDragItem(dragItem); 
            } 
        }

        private void SwapCells(MergeItem itemInCell, MergeItem dropItem)
        {
            var dropPaarent = dropItem.Parent; 
            RemoveFromListItem(itemInCell); 
            base.AddDragItem(dropItem); 
            dropPaarent.AddDragItem(itemInCell); 
            itemInCell.MyTransform.SetParent(dropPaarent.MyTransform);
            itemInCell.MyTransform.localPosition = Vector3.zero; 
        }

        private void Merge(MergeItem itemInCell, MergeItem dropItem)
        { 
                onMerge?.Invoke(itemInCell, dropItem);    
        }

        public MergeItem ConvertToMergeItem(IDragItem dragItem)
        {
            MergeItem mergeItem = dragItem as MergeItem;
            return mergeItem;
        }
    }
}
