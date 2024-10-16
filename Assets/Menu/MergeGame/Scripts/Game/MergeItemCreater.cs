using Menu.DragItems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Menu.MergeGame
{
    [Serializable]
    public class MergeItemCreater : ILoaderProgress
    {
        [SerializeField] private MergeCell mergeCellPrefab;
        [SerializeField] private  MergeItem  mergeItemPrefab; 
        [SerializeField] private Transform rightContent, leftContent  ,fieldContent;
        public List<MergeItemData> mergeItemData;
    
        private List<MergeCell> mergeCells = new List<MergeCell>(); 
        public List<MergeCell> MergeCells => mergeCells;
        public float CreateItemTime { get; private set; }
        private MergeItemConfig mergeItemConfig => MergeGameController.instance.Config;
        public int CellCount => mergeItemConfig.MergeGame.cell_count;
        public int StartItemCount => mergeItemConfig.MergeGame.start_item_count;
        public float TimeSpawnItem => mergeItemConfig.MergeGame.time_spawn_items + CreateItemTime;
      
        public Action<bool> onDragProcess;
 
       
        public MergeItem SetupItemToFreeCell(List<MergeCell> freeCells,int level)
        {
            if (freeCells.Count > 0)
            {
                return NewItem(freeCells[0], level); 
            }
            return null;
        }

        public void RefreshItemCreatedTime() => 
            CreateItemTime = Time.time;
        public List<MergeCell> GetFreeMergeCells() => 
            mergeCells.Where(c => c.DragItems.Count == 0).ToList();
        public List<MergeCell> GetFillMergeCells() =>
           mergeCells.Where(c => c.DragItems.Count > 0).ToList();
        public void CreateCells()
        {
            int contentLimit = CellCount / 2;
            for (int i = 0; i < CellCount; i++)
            {
                MergeCell mergeCell = null;
                if (i > contentLimit -1)
                    mergeCell = NewCell(rightContent); 
                else
                    mergeCell = NewCell(leftContent); 

                mergeCell.Id = i;
            }
        }
        public void StartCreateItem(PlayerProgress playerProgress)
        { 
            if (playerProgress.mergeItems.Count == 0)
            {
                for (int i = 0; i < mergeCells.Count; i++)
                {
                    if (i < StartItemCount)
                    {
                        NewItem(mergeCells[i], 1);
                    }
                }
            }
            else
            {
                Load(playerProgress);
            }
        } 
        private MergeItem NewItem(MergeCell mergeCell, int level)
        {
            MergeItem item =  CreateMergeItem(mergeCell);
           
            MergeItemEntity entity = mergeItemConfig.MergeItemEntities.First(s => s.level == level);
            item.Init(entity,level);
            mergeCell.AddDragItem(item);
            item.onBeginDrag += OnBeginDrag;
            item.onEndDrag += OnEndDrag;
            return item;
        }

        private void OnEndDrag()
        {
            onDragProcess?.Invoke(false);
        }

        private void OnBeginDrag()
        {
            onDragProcess?.Invoke(true);
        }

        private MergeCell NewCell(Transform content)
        {
            var mergeCell =  CreateMergeCell(content);
            mergeCells.Add(mergeCell);
            return mergeCell;
        }


        private MergeCell CreateMergeCell(Transform content)
        {
            return GameObject.Instantiate(mergeCellPrefab, content);
        }
        private MergeItem CreateMergeItem(MergeCell mergeCell)
        {
            var dragItem = GameObject.Instantiate(mergeItemPrefab, Vector3.zero, Quaternion.identity, mergeCell.transform);
            dragItem.transform.localPosition = Vector2.zero;
            dragItem.SetFreeContent(fieldContent);
            return dragItem;
        }

        public void Load(PlayerProgress progress)
        {
            mergeItemData = progress.mergeItems;
            for (int i = 0; i < mergeItemData.Count; i++)
            { 
               var item = NewItem(mergeCells[mergeItemData[i].IdCell], mergeItemData[i].Level);
                item.AccumulatedResource = mergeItemData[i].AccumulatedResource;
            }
        }
    }
}