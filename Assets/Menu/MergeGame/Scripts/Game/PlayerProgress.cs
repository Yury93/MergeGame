using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Menu.MergeGame.ResourceSystem;

namespace Menu.MergeGame
{
    [Serializable]
    public class PlayerProgress
    {
        public List<MergeItemData> mergeItems = new List<MergeItemData>();
        public List<ResourceData> resources = new List <ResourceData>();
        public List<ClickCreateItem> createItems = new List<ClickCreateItem>();
        public int maximumLevel;
        public int currentLevel;
        public float timerFreeShopItem;
    }
    [Serializable]
    public class ClickCreateItem
    {
        public int levelItem;
        public int createCount;
    }

    [Serializable]
    public class MergeItemData
    {
        public int IdCell;
        public int Level;
        public int AccumulatedResource;
    }
    [Serializable]
    public class ResourceData
    {
        public Type ResourceType;
        public int Count; 
    }
    interface ISavedProgress
    {
        void Save(PlayerProgress progress); 
    }
    interface ILoaderProgress
    {
        void Load(PlayerProgress progress);
    }
}