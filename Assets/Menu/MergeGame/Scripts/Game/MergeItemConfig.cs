using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menu.MergeGame
{
    [CreateAssetMenu(fileName = "MergeItemConfig", menuName = "MiniGameConfigs/MergeGame/MergeItemConfig")]
    public class MergeItemConfig : ScriptableObject
    {
        [SerializeField] public MergeGameConfig MergeGame;
        [SerializeField] public List<MergeItemEntity> MergeItemEntities;
        [SerializeField] public RuneShopConfig Shop;
        [SerializeField] public MergeSpineRewardsConfig MergeSpineRewardsConfig;
    }
    [Serializable]
    public class MergeSpineRewardsConfig
    {
        public List<MergeSpineReward> merge_spine_rewards;
        public int cost_spine;
    }
    [Serializable]
    public class MergeSpineReward
    {
        public string reward;
        public int chance;
    }
    [Serializable]
    public class RuneShopConfig
    {
        public int max_count_items; 
        public int bonus_count_items;
        [Space(20)]
        public int free_rune_time_min;
        public int min_free_rune;
        public int max_free_rune;
        public int GetBonusResourceCount()
        {
            bonus_count_items = Mathf.Clamp(bonus_count_items,0, max_count_items );
            return bonus_count_items ;
        }
    }
    [Serializable]
    public class MergeGameConfig
    {
        public float time_claim_resource;
        public float time_spawn_items;
        public int cell_count;
        public int start_item_count;
        public int deductible_level_item;//вычитаемое 
    
    }
    [Serializable]
    public class MergeItemEntity
    {
        public int id;
        public string name;
        public Sprite sprite;//заменить на стринг и превратить в свойство
        public int level;
        public int base_cost;
        public string cost_math;
        public int energy_gen; 
    }
    //Конфиг по рунам:
    //    id номер name ключ локи 
    //    base_cost базовая цена  
    //    cost_math это формула увеличения цены за руну
    //    (то как увеличивается цена под центральной кнопкой и в магазине)   
    //    energy_gen генерация ресурса в секунду
}