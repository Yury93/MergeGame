
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MergeGame
{
    public class MergeGameController : MonoBehaviour, ISavedProgress  
    {
        public static MergeGameController instance;
        public enum GameState { init, play }
        private GameState gameState = GameState.init;
        [SerializeField] private Button createItemButton;
        [SerializeField] private Image iconRune;
        [SerializeField] private TextMeshProUGUI levelRuneText;
        [SerializeField] private CostHandler costHandler; 
        [SerializeField] private DestroyerMergeItem destroyerItems;
        [field: SerializeField] public ResourceSystem resourceSystem { get; private set; }
        [field:SerializeField] public MergeItemCreater itemCreater { get; private set; } 
        [field: SerializeField] public PlayerProgress playerProgress { get; private set; }
        [field: SerializeField] public WindowService WindowService { get; private set; }
        [field: SerializeField] public MergeItemConfig Config { get; private set; }

        public const string PLAYER_PROGRESS_KEY = "PlayerProgress";
        public int CurrentLevel => playerProgress.currentLevel;
        public int MaximumLevel => playerProgress.maximumLevel;
 
        private void Awake()
        {
            Init();
        }
        private void Start()
        {
            StartGame();
        }
        private void Init()
        {
            instance = this;
            //if (GamerPrefs.HasKey(PLAYER_PROGRESS_KEY))
            //{
            //    var json = GamerPrefs.GetString(PLAYER_PROGRESS_KEY);
            //    playerProgress = JsonConvert.DeserializeObject<PlayerProgress>(json);
  
            //}
            //else
            //{
            //    playerProgress = new PlayerProgress(); 
            //} 
            itemCreater.CreateCells();
            itemCreater.MergeCells.ForEach(c => c.onMerge += OnMerge);
            destroyerItems.onDestoy += OnDestroyItem;
            createItemButton.onClick.AddListener(OnClickCreateItem);
            itemCreater.onDragProcess += OnDragProcess;
            resourceSystem.Init(this);
            WindowService.Init(this);

        } 
        private void OnDragProcess(bool drag)
        {
            if (drag)
            {
                createItemButton.gameObject.SetActive(false);
                destroyerItems.gameObject.SetActive(true);
            }
            else
            {
                createItemButton.gameObject.SetActive(true);
                destroyerItems.gameObject.SetActive(false);
            }
        } 
        private void OnDestroyItem(MergeItem item)
        {
            item.Parent.RemoveFromListItem(item); 
            Destroy(item.gameObject);
            createItemButton.gameObject.SetActive(true);
            destroyerItems.gameObject.SetActive(false);
        } 
        private void StartGame()
        {
            gameState = GameState.play;
            itemCreater.StartCreateItem(playerProgress);
            UpdateLevel();
            int level = CurrentLevel;
            var entity = Config.MergeItemEntities.FirstOrDefault(i => i.level == level);
            iconRune.sprite = entity.sprite;

            levelRuneText.text = level + "";
            costHandler.SetupCost(entity, level);


        }
        private void OnClickCreateItem()
        {
            
            List<MergeCell> freeCells = itemCreater.GetFreeMergeCells();
            if (freeCells.Count == 0)
            {
                Debug.LogError("no found free cell / show popup");
            }
            else
            {
                 UpdateLevel();
                int level = CurrentLevel;
                var entity = Config.MergeItemEntities.FirstOrDefault(i => i.level == level); 
                var defaultResource = resourceSystem.GetResource<DefaultResource>();
                BuyItem (costHandler, level, freeCells,defaultResource);
                if (defaultResource .HasResource(costHandler.cost))
                {  
                    iconRune.sprite = entity.sprite;
                    levelRuneText.text = level + ""; 
                } 
            }
        }
        public void BuyItem (CostHandler costHandler,int level,List<MergeCell> freeCells,IResource resource)  
        {
         
            var entity = Config.MergeItemEntities.FirstOrDefault(i => i.level == level);
            var createClickData = playerProgress.createItems.FirstOrDefault(i => i.levelItem == level);
            if (createClickData == null)
            {
                createClickData = new ClickCreateItem() { levelItem = level };
                playerProgress.createItems.Add(createClickData);
            }

            costHandler.SetupCost(entity, level);
       
            if (resource.HasResource(costHandler.cost))
            {
                createClickData.createCount++;
                resource.TakeResource(costHandler.cost);
                costHandler.SetupCost(entity, level);
             
                var mergeItem = itemCreater.SetupItemToFreeCell(freeCells, level);
            }
            else
            {
                Debug.LogError($"no resource {costHandler.cost} / show popup");
            }
        }
        public MergeItem BuyFreeItemOfShop(int level)
        {
            var freeCells = MergeGameController.instance.itemCreater.GetFreeMergeCells();
            if (freeCells.Count == 0)
            {
                Debug.LogError("no free cells");
                return null;
            }
            else
            {
                var mergeItem = itemCreater.SetupItemToFreeCell(freeCells, level);
                return mergeItem;
            }
        }
        private void OnMerge(MergeItem itemInCell, MergeItem dropItem)
        {
            //Debug.Log($"Анимация мержа itemIn: {itemInCell.Power}/ dropItem: {dropItem.Power}");
            itemInCell.PowerUp(); 
            dropItem.Parent.RemoveFromListItem(dropItem);
            Destroy(dropItem.gameObject);
            var itemEntity = Config.MergeItemEntities.FirstOrDefault(m => m.level == itemInCell.Level);
            if (itemEntity != null)
                itemInCell.Images.ForEach(i => i.sprite = itemEntity.sprite);

            createItemButton.gameObject.SetActive(true);
            destroyerItems.gameObject.SetActive(false);

            UpdateLevel();
            int level = CurrentLevel;
            var entity = Config.MergeItemEntities.FirstOrDefault(i => i.level == level);
            iconRune.sprite = entity.sprite;
            levelRuneText.text = level + "";
        }
        private void Update()
        {
            if (gameState == GameState.init) itemCreater.RefreshItemCreatedTime();

            if (Time.time > itemCreater.TimeSpawnItem)
            {
                SpawnOnTimeItems();

            }
            for (int i = 0; i < itemCreater.MergeCells.Count; i++)
            {
                RefreshUpdaterItems(i);
            }
            WindowService.ShopWindow.Updated();
          
        } 
        private void RefreshUpdaterItems(int i)
        {
            for (int k = 0; k < itemCreater.MergeCells[i].DragItems.Count; k++)
            {
                itemCreater.MergeCells[i].DragItems[k].Updated();
            }
            resourceSystem.Updated();
        } 
        private void SpawnOnTimeItems()
        {
            List<MergeCell> freeCells = itemCreater.GetFreeMergeCells();
            if (freeCells.Count == 0) itemCreater.RefreshItemCreatedTime();
            else
            {
                UpdateLevel();
                int level = CurrentLevel;

                itemCreater.SetupItemToFreeCell(freeCells, level);
                itemCreater.RefreshItemCreatedTime();
            }
        }

        private void UpdateLevel()
        {
            List<MergeCell> fillItems = itemCreater.GetFillMergeCells();
            if (fillItems.Count > 0)
            {
                int maxLevelItem = fillItems.Max(c => c.LevelMergeItem);
                maxLevelItem = Mathf.Clamp(maxLevelItem, 1, maxLevelItem);
                SaveMaxLevel(maxLevelItem);
                int level = maxLevelItem - Config.MergeGame.deductible_level_item;
                level = Mathf.Clamp(level, 1, level);
                SaveCurrentLevel(level);
          
            } 
        }

        private void SaveMaxLevel(int maxLevelItem)
        {
            if (playerProgress.maximumLevel < maxLevelItem)
            {
                playerProgress.maximumLevel = maxLevelItem;
            }
        }

        private void SaveCurrentLevel(int level)
        {
             
            if (playerProgress.currentLevel < level)
            {
                playerProgress.currentLevel = level;
            }
        }

        public void Save(PlayerProgress progress)
        {
            List<MergeItemData> data = new List<MergeItemData>();
            foreach (var cell in itemCreater.MergeCells)
            {
                if(cell.DragItems.Count > 0)
                {
                   var id = cell.Id; 
                   var mergeItem = cell.ConvertToMergeItem(cell.DragItems[0]);

                    data.Add(new MergeItemData() {IdCell = id,
                        Level = mergeItem.Level, 
                        AccumulatedResource = (int)mergeItem.AccumulatedResource});
                }
            }
            progress.mergeItems = data;
         
            //var json = JsonConvert.SerializeObject(playerProgress);
            //GamerPrefs.Set(PLAYER_PROGRESS_KEY, json);
        }
        public int GetCurrentResourceRatePerSeconds()
        {
            int resourceRate = 0;
            for (int i = 0; i < MergeGameController.instance.itemCreater.MergeCells.Count; i++)
            {
                if (MergeGameController.instance.itemCreater.MergeCells[i].DragItems.Count > 0)
                {
                    var mergeCell = MergeGameController.instance.itemCreater.MergeCells[i];
                    var mergeItem = mergeCell.ConvertToMergeItem(mergeCell.DragItems[0]);
                    resourceRate += mergeItem.ItemEntity.energy_gen;
                }
            }
            return resourceRate;
        }
        private void OnDestroy()
        {
            Save(playerProgress);
        } 
    }
}