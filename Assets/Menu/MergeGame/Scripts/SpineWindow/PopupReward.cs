 
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.MergeGame
{
    [Serializable]
    public class PopupReward
    {
        [SerializeField] private GameObject popup;
        [SerializeField] private Image iconResource;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI summRewardText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI continueText;
        [SerializeField] private TextMeshProUGUI costText;
        private Menu.MergeGame.ResourceItem resourceItem;
        private SpineWindow spineWindow;
        public const int MIN = 60;
 
        public void Init(SpineWindow spineWindow)
        { 
            this.spineWindow = spineWindow;
            closeButton.onClick.AddListener(Close);
            continueButton.onClick.AddListener(Continue);
            ShowCostSpine();
        }

        private void Continue()
        {
            Close();
        spineWindow.StartSpine();
        }

        public void Open ( ResourceItem resourceItem,int xReward)
        {
            popup.gameObject.SetActive(true);

            iconResource.sprite = resourceItem.icon.sprite;
            if (resourceItem.ResourceType == typeof(DefaultResource))
            {
                int count = MergeGameController.instance.GetCurrentResourceRatePerSeconds();
                int sum = (count * MIN * xReward);// количество собираемых ресурсов в секунду умноженное на 600 минут например
                rewardText.text = $"{count} X{xReward} min";
                summRewardText.text = (count * 60 * xReward) + "";
                resourceItem.AddResource(sum);
            }
            else
            {
                rewardText.text = $"X{xReward}";//особый ресурс просто выдаётся количеством
                resourceItem.AddResource(xReward);
                summRewardText.text = "";
            }
         
        }

        private void ShowCostSpine()
        {
            var bonusResource = MergeGameController.instance.resourceSystem.GetResource<BonusResource>();
            costText.text = $"{MergeGameController.instance.Config.MergeSpineRewardsConfig.cost_spine}/{bonusResource.count}";
        }

        public void Close()
        {
            popup.gameObject.SetActive(false);
        }

        internal void ShowStateContinueButton(SpineWindow.cash cashState)
        {
            if(cashState == SpineWindow.cash.isResource)
            {
                continueButton.image.sprite = spineWindow.greenSprite;
                continueText.text = "Restart";
            }
            else
            {
                continueButton.image.sprite = spineWindow.redSprite;
                continueText.text = "Close";
            }
        }
    }
}