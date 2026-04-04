using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Text goldLabel;
        [SerializeField] private Text baseHpLabel;
        [SerializeField] private Text roundLabel;

        public void SetGold(int value)
        {
            if (goldLabel != null)
            {
                goldLabel.text = $"Gold: {value}";
            }
        }

        public void SetBaseHp(int value)
        {
            if (baseHpLabel != null)
            {
                baseHpLabel.text = $"Base HP: {value}";
            }
        }

        public void SetRound(int value)
        {
            if (roundLabel != null)
            {
                roundLabel.text = $"Round: {value}";
            }
        }

        public void Bind(Text gold, Text baseHp, Text round)
        {
            goldLabel = gold;
            baseHpLabel = baseHp;
            roundLabel = round;
        }
    }
}

