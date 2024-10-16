 
using Menu.DragItems;
using System;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MergeGame
{
    public class RuneShop : MiniWindow, IUpdater
    {
        [SerializeField] private RuneShopItem shopItemPrefab;
        [SerializeField] private Transform contentItems;
        [SerializeField] protected Button openButton;
        [SerializeField] protected Button closeButton;
        [SerializeField] private List<RuneShopItem> shopItems = new List<RuneShopItem>();
 
        private bool isSetFreeShopItem;
        private MergeGameController gameController;
        public const int MIN_LEVEL_OPEN_ITEM = 1;
        private static System.Random random = new System.Random();
        public RuneShopConfig ShopConfig => MergeGameController.instance.Config.Shop;
        public PlayerProgress PlayerProgress => MergeGameController.instance.playerProgress;
        public void Init(MergeGameController gameController)
        {
            this.gameController = gameController; 
            window.gameObject.SetActive(false);
            openButton.onClick.AddListener(Open);
            closeButton.onClick.AddListener(Close);
            CreateItems();
            shopItems.ForEach(b => b.onBuyItem += OnBuyRune);
            shopItems.ForEach(b => b.onBuyFreeItem += OnBuyFreeRune);
            MergeGameController.instance.resourceSystem.onChangeResources += OnChangeResources;
      
        } 
        public void UpdateTimer()
        {
            PlayerProgress.timerFreeShopItem = Time.time + (ShopConfig.free_rune_time_min*60);
        }
        public void Updated()
        {
            int minDeductible = ShopConfig.min_free_rune;
            int maxDeductible = ShopConfig.max_free_rune;
            int minFreeLevel = PlayerProgress.maximumLevel - minDeductible;
            int maxFreeLevel = PlayerProgress.maximumLevel - maxDeductible;
            if (minFreeLevel > 0 && maxFreeLevel > 0)
            {
                if (Time.time >= PlayerProgress.timerFreeShopItem && isSetFreeShopItem == false)
                {
                    SetFreeShopItem();
                }
            }
        }
        private void OnBuyFreeRune()
        {
            UpdateTimer();
            isSetFreeShopItem = false;
        }
        [ContextMenu("setFree")]
        private void SetFreeShopItem()
        {
            Debug.LogError("Setup new free item in shop");
            isSetFreeShopItem = true;
            int minDeductible = ShopConfig.min_free_rune;
            int maxDeductible = ShopConfig.max_free_rune;
            int maximumLevel = MergeGameController.instance.playerProgress.maximumLevel;

            int minFreeLevel = PlayerProgress.maximumLevel - minDeductible;
            int maxFreeLevel = PlayerProgress.maximumLevel - maxDeductible;

            RuneShopItem minShopItem = shopItems.FirstOrDefault(i => i.Entity.level == minFreeLevel);
            RuneShopItem maxShopItem = shopItems.FirstOrDefault(i => i.Entity.level == maxFreeLevel);
            if (minShopItem && maxShopItem)
            {
                int level = GetIntRandomLevel(minShopItem, maxShopItem);
                if (minShopItem.Entity.level == level)
                {
                    Debug.LogError("chance = 10");
                    //minShopItem.DebugMark();
                    minShopItem.SetFreeRune(true);
                }
                if (maxShopItem.Entity.level == level)
                {
                    Debug.LogError("chance = 1");
                    //maxShopItem.DebugMark();
                    maxShopItem.SetFreeRune(true);
                }
            } 
        }
        
        private int GetIntRandomLevel(RuneShopItem minItem, RuneShopItem maxItem)
        {
            List<RuneItemStruct> rarityStruct = new List<RuneItemStruct>();
            rarityStruct.Add(new RuneItemStruct() { level = maxItem.Entity.level, weight = 1 });
            rarityStruct.Add(new RuneItemStruct() { level = minItem.Entity.level, weight = 10 }); 
            var level = Methods.GetRandomByWeight(rarityStruct, (a) => a.weight, random).level;
         
            return level;
        }


        private void OnChangeResources()
        {
            if(IsOpen)
            {
                RefreshRunes();
            }
        }

        public override void Open()
        {
            base.Open();
            RefreshRunes();
        }
        public override void Close()
        {
            base.Close();
        }
        private void CreateItems()
        {
            foreach (var entity in MergeGameController.instance.Config.MergeItemEntities)
            {
                var item = Instantiate(shopItemPrefab, contentItems);
                item.Init(entity);
                shopItems.Add(item);
            }
        } 
        private void OnBuyRune()
        {
            RefreshRunes();
        } 
        private void RefreshRunes()
        {
            shopItems.ForEach(i => i.RefreshState());
        } 
    }
    public class RuneItemStruct
    {
        public int level;
        public int weight;
    }
}