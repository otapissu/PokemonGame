using UnityEngine;
using TMPro;

public class EggUIService
{
    public void UpdateAll(EggEnhanceController c)
    {
        if (c.currentInstance == null)
        {
            return;
        }

        c.enhanceLevelText.text = "강화 단계 : +" + c.currentInstance.enhanceLevel;

        int next = c.currentInstance.enhanceLevel + 1;

        if (next <= c.maxLevel)
        {
            float baseRate = EggDataService.GetSuccessRate(c, next);
            float bonus = GeneralBagPanelController.Instance != null ? GeneralBagPanelController.Instance.GetSuccessRateBonus() : 0f;

            if (bonus > 0f)
            {
                c.successRateText.text =
                    "강화확률 : " + (baseRate * 100).ToString("0") + "% + (" + (bonus * 100).ToString("0") + "%)";
            }
            else
            {
                c.successRateText.text =
                    "강화확률 : " + (baseRate * 100).ToString("0") + "%";
            }

            float d = EggDataService.GetDestroyRate(c, next) * 100;

            if (d > 0)
            {
                c.destroyRateText.text = "파괴확률 : " + d.ToString("0") + "%";
            }
            else
            {
                c.destroyRateText.text = "";
            }

            c.costText.text =
                "강화비용 : " + EggDataService.GetEnhanceCost(c, next).ToString("N0");
        }
        else
        {
            c.successRateText.text = "MAX";
            c.destroyRateText.text = "";
            c.costText.text = "";
        }

        c.sellPriceText.text =
            "판매 금액 : " + EggDataService.GetSellPrice(c).ToString("N0");
    }

    public void Clear(EggEnhanceController c)
    {
        c.enhanceLevelText.text = "";
        c.successRateText.text = "";
        c.destroyRateText.text = "";
        c.costText.text = "";
        c.sellPriceText.text = "";
    }

    public void UpdateGold(EggEnhanceController c)
    {
        c.goldText.text = "소지금 : " + c.gold.ToString("N0");

        if (c.gold >= 10_000_000L && TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TryShowMillionTutorial();
        }
    }
}