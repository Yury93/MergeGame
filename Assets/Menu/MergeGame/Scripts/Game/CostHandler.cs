using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Menu.MergeGame
{
    [Serializable]
    public class CostHandler
    {
        [SerializeField] public TextMeshProUGUI costText;
        public int cost; 
        public void SetupCost(MergeItemEntity mergeItemEntity, int level)
        {
            int createCount = 0;
            if (level != 0)
            {
                var item = MergeGameController.instance.playerProgress.createItems.FirstOrDefault(c => c.levelItem == level);
                if(item != null)
                {
                    createCount = item.createCount;
                }
            }
            cost = mergeItemEntity.base_cost + (GetCostMath(mergeItemEntity) * createCount); 
            //cost = mergeItemEntity.base_cost +   createCount ;
            costText.text = cost + "";
        }
        public int GetCostMath(MergeItemEntity mergeItemEntity)
        {
            if (mergeItemEntity.cost_math.IsNullOrEmpty()) return 0;
            var expression = mergeItemEntity.cost_math.Replace(" ", "");
            string[] parts = expression.Split(new char[] { '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
            char[] operators = expression.Where(c => "+-/*".Contains(c)).ToArray();
            int result = int.Parse(parts[0]);
            for (int i = 0; i < operators.Length; i++)
            {
                int operand = int.Parse(parts[i + 1]);

                switch (operators[i])
                {
                    case '+':
                        result += operand;
                        break;
                    case '-':
                        result -= operand;
                        break;
                    case '*':
                        result *= operand;
                        break;
                    case '/':
                        result /= operand;
                        break;
                }
            }
            return result;
        }
    }
}