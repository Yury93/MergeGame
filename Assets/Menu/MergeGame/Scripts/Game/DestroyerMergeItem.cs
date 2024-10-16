using Menu.DragItems;
using System; 

namespace Menu.MergeGame
{
    public class DestroyerMergeItem : DropParent
    {
        public Action<MergeItem> onDestoy;
        public override void AddDragItem(IDragItem dragItem)
        {
            var mergeItem = dragItem as MergeItem;
            onDestoy?.Invoke(mergeItem);
        }
    }
}