using Menu.MergeGame;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneShopItem : MonoBehaviour
{
    public enum State { locked, open, noResources, free}
    [SerializeField] private Image runeImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private CostHandler costHandler;
    [SerializeField] private Button buyButton;
    [SerializeField] private Image costIcon;
    [SerializeField] private State state = State.locked;
    [SerializeField] private Menu.MergeGame.ResourceItem resource;
    [Space(20)]
    [SerializeField] private Sprite redButtonSprite, greenButtonSprite, grayButtonSprite,yellowButtonSprite; 
    public MergeItemEntity Entity { get; private set; }
     
    public RuneShopConfig ShopConfig => MergeGameController.instance.Config.Shop;
    public PlayerProgress PlayerProgress => MergeGameController.instance.playerProgress;
    public TextMeshProUGUI costText => costHandler.costText;

    public Action onBuyItem;
    public Action onBuyFreeItem;
    public void Init(MergeItemEntity entity)
    {
       this.Entity = entity;
        buyButton.onClick.AddListener(Buy);
        var createClickData = PlayerProgress.createItems.FirstOrDefault(i => i.levelItem == entity.level);
        if (createClickData == null)
        {
            createClickData = new ClickCreateItem() { levelItem = entity.level };
            PlayerProgress.createItems.Add(createClickData);
        }  
        costHandler.SetupCost(entity, entity.level);
        runeImage.sprite = entity.sprite;
        levelText.text = entity.level + "";
    }
    private void Buy()
    {
        if(state == State.free)
        { 
            var freeItem = MergeGameController.instance.BuyFreeItemOfShop(Entity.level);
            if (freeItem)
            {
                SetFreeRune(false);
                onBuyFreeItem?.Invoke();
            }
            return;
        }

        if (state == State.locked)
        {
            Debug.LogError("locked");
        }
        if(state == RuneShopItem.State.noResources)
        {
            Debug.LogError("no resources");
        }
        if (state == RuneShopItem.State.open)
        {
            var freeCells = MergeGameController.instance.itemCreater.GetFreeMergeCells();
            if (freeCells.Count == 0)
            {
                Debug.LogError("no free cells"); 
            }
            else
            { 
                MergeGameController.instance.BuyItem(costHandler, Entity.level, freeCells, resource);
                onBuyItem?.Invoke();
               // var createClickData = PlayerProgress.createItems.FirstOrDefault(i => i.levelItem == entity.level);
               // createClickData.createCount++;
                costHandler.SetupCost(Entity, Entity.level);
            }
        }
    }
    public void RefreshState()
    {
      
        costHandler.SetupCost(Entity, Entity.level);
        resource = MergeGameController.instance.resourceSystem.GetResource<DefaultResource>();
        int minOpenLevel, minBonusLevel;
        GetOpennedLevels(out minOpenLevel, out minBonusLevel);
        if (minOpenLevel <= Entity.level && Entity.level <= PlayerProgress.maximumLevel)
        {
            if (state != State.free ) state = State.open;
            if (Entity.level >= minBonusLevel + 1)
            {
                resource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
            } 
            if ( !resource.HasResource(costHandler.cost))
            {
                if (state != State.free)
                    state = RuneShopItem.State.noResources;
            }
        }
        else
        {
            if (state != State.free) state = State.locked;
        }
        ShowState();

    } 
   
    public void SetFreeRune(bool free)
    {
        if (free) state = RuneShopItem.State.free;
        else state = RuneShopItem.State.open;

        RefreshState();
    }
    private void GetOpennedLevels(out int minOpenLevel, out int minBonusLevel)
    {
        minOpenLevel = PlayerProgress.maximumLevel - ShopConfig.max_count_items;
        minBonusLevel = PlayerProgress.maximumLevel - ShopConfig.GetBonusResourceCount();
        if (minOpenLevel < 0) minOpenLevel = RuneShop.MIN_LEVEL_OPEN_ITEM;  
        if (minBonusLevel < 0) minBonusLevel = RuneShop.MIN_LEVEL_OPEN_ITEM;  
    } 
    private void ShowState()
    { 
        costIcon.sprite =  resource.icon.sprite;
        costIcon.enabled = true;
        costText.text = costHandler.cost + "";
        if(state == RuneShopItem.State.open)
        {
            buyButton.image.sprite = greenButtonSprite;
        }
        if (state == RuneShopItem.State.locked)
        {
            buyButton.image.sprite = grayButtonSprite;
        }
        if (state == RuneShopItem.State.noResources)
        {
            buyButton.image.sprite  = redButtonSprite;
        }
        if (state == RuneShopItem.State.free)
        {
            buyButton.image.sprite = yellowButtonSprite;
            costText.text = "free item (non translate)";
            costIcon.enabled = false;
        }
    }
}
