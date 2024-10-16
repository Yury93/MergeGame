
using Menu.DragItems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Menu.MergeGame
{
    public partial class ResourceSystem : MonoBehaviour, ISavedProgress, IUpdater
    { 
        private MergeGameController gameController;
        private float claimTimeResource;
        [field:SerializeField] public List<ResourceItem> resourceItems { get; private set; }
        private DefaultResource defaultResource;
        PlayerProgress PlayerProgress => gameController.playerProgress;
        public const int CHECK_RESOURCES_TIME = 1;
        public Action onChangeResources;
        public void Init(MergeGameController gameController)
        {
            this.gameController = gameController;
            foreach (var item in resourceItems)
            {
                item.Load(PlayerProgress);
                item.onChangeResource += OnChangeResource;
            }
            defaultResource = GetResource<DefaultResource>();
        }

        private void OnChangeResource(ResourceItem item)
        {
            Save(PlayerProgress);
            onChangeResources.Invoke();
        }

        public void Updated() =>
            ClaimDefaultResource();
        private void ClaimDefaultResource()
        {
            if (Time.time > claimTimeResource)
            {
                for (int i = 0; i < gameController.itemCreater.MergeCells.Count; i++)
                {
                    if (gameController.itemCreater.MergeCells[i].DragItems.Count > 0)
                    {
                        var mergeCell = gameController.itemCreater.MergeCells[i];
                        var mergeItem = mergeCell.ConvertToMergeItem(mergeCell.DragItems[0]);
                       
                        defaultResource.AddResource((int)mergeItem.ClaimResource()); 
                    }
                }
                claimTimeResource = Time.time + MergeGameController.instance.Config.MergeGame.time_claim_resource;
            }
        }
        public T GetResource<T>()
        {
            return resourceItems.OfType<T>().FirstOrDefault();
        }
        public void Save(PlayerProgress progress)
        {
            foreach (var item in resourceItems)
            {
                var resourceData = progress.resources.FirstOrDefault(r => r.ResourceType == item.ResourceType);
                if(resourceData == null)
                {
                    resourceData = new ResourceData();
                    resourceData.ResourceType = item.ResourceType;
                    progress.resources.Add(resourceData);
                }
                resourceData.Count = item.count;
            } 
        }
    }
}