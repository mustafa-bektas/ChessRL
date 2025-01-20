using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Upgrade
{
    public class UpgradeUI : MonoBehaviour
    {
        public GameObject upgradePanel;

        // Button references
        public Button option1Button; // e.g. +2 HP, +1 DEF
        public Button option2Button; // e.g. +2 ATK

        private KingController _king;
        private TurnManager _turnManager;

        void Start()
        {
            _king = FindAnyObjectByType<KingController>();
            _turnManager = FindAnyObjectByType<TurnManager>();

            // Hide at start
            upgradePanel.SetActive(false);

            // Hook button clicks
            option1Button.onClick.AddListener(SelectOption1);
            option2Button.onClick.AddListener(SelectOption2);
        }

        public void ShowUpgradePanel()
        {
            upgradePanel.SetActive(true);
            // Pause gameplay if you want, or block input
        }

        private void HideUpgradePanel()
        {
            upgradePanel.SetActive(false);
        }

        private void SelectOption1()
        {
            // e.g. +2 HP, +1 DEF
            _king.maxHP += 2;
            _king.def += 1;

            _king.currentHP = Mathf.Min(_king.currentHP, _king.maxHP); // in case new max < current
            Debug.Log("Upgrade chosen: +2 Max HP, +1 DEF");

            DoneUpgrading();
        }

        private void SelectOption2()
        {
            // e.g. +2 ATK
            _king.atk += 2;
            Debug.Log("Upgrade chosen: +2 ATK");

            DoneUpgrading();
        }

        private void DoneUpgrading()
        {
            HideUpgradePanel();
            // Now signal we can proceed to the next floor transition (if that’s the flow)
            // or do whatever logic ends the current floor.
            _turnManager.FloorClearedAndUpgraded();
        }
    }
}
