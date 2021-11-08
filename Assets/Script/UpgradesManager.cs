using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BreakInfinity;
using UnityEngine.UI;


public class UpgradesManager : MonoBehaviour
{
	public static UpgradesManager instance; 
	public void Awake() => instance = this;

	[Header("----------------")]
	

	public List<Upgrades> clickUpgrades;
	public Upgrades clickUpgradePrefab;
	public ScrollRect clickUpgradesScroll;
	public GameObject clickUpgradesPanel;

	public string[] clickUpgradeNames;
	
	public BigDouble[] clickUpgradeBaseCost;
	public BigDouble[] clickUpgradeCostMult;
	public BigDouble[] clickUpgradesBasePower;



	public void StartUpgradeManager()
	{
		clickUpgradeNames = new[] {"Click Power +1", "Click Power +5", "Click Power +10"};
		clickUpgradeBaseCost = new BigDouble[] {10, 50, 100};
		clickUpgradeCostMult = new BigDouble[] {1.25, 1.35, 1.55};
		clickUpgradesBasePower = new BigDouble[] {1, 5, 10};

		for (int i = 0; i < GameController.instance.data.clickUpgradeLevel.Count; i++)
		{
			Upgrades upgrade = Instantiate(clickUpgradePrefab, clickUpgradesPanel.transform);
			upgrade.UpgradeID = i;
			clickUpgrades.Add(upgrade);
		}

		clickUpgradesScroll.normalizedPosition = new Vector2(0, 0);
		
		UpdateClickUpgradeUI();
		Methods.UpgradeCheck(ref GameController.instance.data.clickUpgradeLevel, 3);
	}
	public void UpdateClickUpgradeUI(int UpgradeID = -1)
	{
		if (UpgradeID == -1)
		{
			for (int i = 0; i < clickUpgrades.Count; i++) UpdateUI(i);
		}
		else UpdateUI(UpgradeID);

			void UpdateUI(int ID)
		{
			clickUpgrades[ID].LevelText.text = GameController.instance.data.clickUpgradeLevel[ID].ToString();
			clickUpgrades[ID].CostText.text = $"Cost: {ClickUpgradeCost(ID):F2} Elon";
			clickUpgrades[ID].NameText.text = clickUpgradeNames[ID];
		}

	}
	public BigDouble ClickUpgradeCost(int UpgradeID) => clickUpgradeBaseCost[UpgradeID] * BigDouble.Pow(clickUpgradeCostMult[UpgradeID], GameController.instance.data.clickUpgradeLevel[UpgradeID]);
	
	public void BuyUpgrade(int UpgradeID)
	{
		if (GameController.instance.data.Elon >= ClickUpgradeCost(UpgradeID))
		{
			GameController.instance.data.Elon -= ClickUpgradeCost(UpgradeID);
			GameController.instance.data.clickUpgradeLevel[UpgradeID] += 1;
		}
		
		UpdateClickUpgradeUI(UpgradeID);
	}
}
