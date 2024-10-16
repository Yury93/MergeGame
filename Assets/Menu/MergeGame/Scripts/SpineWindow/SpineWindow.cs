
using Menu;
using Menu.DragItems;
using Menu.MergeGame;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public partial class SpineWindow : MiniWindow
{
    public enum cash{isResource,noResources}
    public cash cashState { get; set; }
    [SerializeField] private PopupReward popupReward;
    [SerializeField] private float speed = 1.1f;
    [SerializeField] private UnityEngine.UI.Button openButton, closeButton;
    [SerializeField] private UnityEngine.UI.Slider slider;
    [SerializeField] private Button startSpineButton;
    [SerializeField] private Vector2 sliderRange = new Vector2(0.98f, 0.02f);
    [SerializeField] private TextMeshProUGUI costText;
    [field:SerializeField] public Sprite greenSprite, redSprite;
    public bool isStartSpine;
    private static System.Random random = new System.Random();
    public void Init(MergeGameController gameController)
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        startSpineButton.onClick.AddListener(ClickStartSpineButton);
        window.gameObject.SetActive(false);
        popupReward.Init(this);
        ShowCostSpine();
    }
    public void RefreshStateContinueButton()
    {
      var spineConfig =  MergeGameController.instance.Config.MergeSpineRewardsConfig;
        var bonusResource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
        if(bonusResource.HasResource(spineConfig.cost_spine))
        {
            cashState = SpineWindow.cash.isResource;
        }
        else
        {
            cashState = SpineWindow.cash.noResources;
        }
        popupReward.ShowStateContinueButton(cashState);
    
        ShowStateStartSpineButton();
    }
    private void ShowCostSpine()
    {
        var bonusResource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
        costText.text = $"{MergeGameController.instance.Config.MergeSpineRewardsConfig.cost_spine}/{bonusResource.count}";
    }
    private void ShowStateStartSpineButton()
    {
        if (cashState == SpineWindow.cash.isResource)
        {
            startSpineButton.image.sprite = greenSprite;
        }
        else
        {
            startSpineButton.image.sprite = redSprite;
        }
    }

    private void ClickStartSpineButton()
    {
        if (isStartSpine == false)
        {
            StartSpine();
        }
        else
        {
            StopSpine();
        }
    }

    private void StopSpine()
    {
        isStartSpine = false;
         var reward = GetReward();
        var split = reward.reward.Split(':');
        if (split[0] == "def_reward")
        {
            var defaultResource = MergeGameController.instance.resourceSystem.GetResource<DefaultResource>();
            popupReward.Open(defaultResource, split[1].ToInt());

        }
        else
        {
            var defaultResource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
            popupReward.Open(defaultResource, split[1].ToInt());
        }
    }
    private MergeSpineReward GetReward() 
    {
        List<RewardItemStruct> rarityStruct = new List<RewardItemStruct>();
        foreach (var item in MergeGameController.instance.Config.MergeSpineRewardsConfig.merge_spine_rewards)
        { 
            rarityStruct.Add(new RewardItemStruct() { spineReward = item, weight = item.chance }); 
        } 
        var reward = Methods.GetRandomByWeight(rarityStruct, (a) => a.weight, random).spineReward;

        return reward;
    }

    public void StartSpine()
    {
        if (cashState == cash.noResources) return;

        var bonusResource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
        var cost = MergeGameController.instance.Config.MergeSpineRewardsConfig.cost_spine;
        bonusResource.TakeResource(cost);

        isStartSpine = true;
        StartCoroutine(CorStartSpine());
        RefreshStateContinueButton();
    }
    IEnumerator CorStartSpine()
    {
        bool forwardMove = true;
        while (isStartSpine)
        {
            if (forwardMove)
            {
                slider.value += Time.deltaTime * speed;
                if (slider.value > sliderRange.x) forwardMove = false;
            }
            else
            {
                slider.value -= Time.deltaTime * speed;
                if (slider.value < sliderRange.y) forwardMove = true;
            }

            yield return null;
        }
    }

    public override void Open()
    {
        base.Open();
        RefreshStateContinueButton();
    }
    public override void Close()
    {
        base.Close();
    }


    [Serializable]
    public class RewardItemStruct
    { 
        public MergeSpineReward spineReward;
        public int weight;
    }
}
