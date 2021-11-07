using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BreakInfinity;


public class UpgradesManager : MonoBehaviour
{
	[Header("----------------")]
	public GameController controller;

	public Upgrades clickUpgrade;

	public string clickClikUpgradeName;
	
	public BreakInfinity.BigDouble clickUpgradeBaseCost;
	public BreakInfinity.BigDouble clickUpgradeCostMult;



	public void StartUpgradeManager()
	{
		clickClikUpgradeName = "Flask Per Click";
		clickUpgradeBaseCost = 10;
		clickUpgradeCostMult = 1.5;
		UpdateClickUpgradeUI();
		
	}


	public void UpdateClickUpgradeUI()
	{
		clickUpgrade.LevelText.text = controller.data.clickUpgradeLevel.ToString();
		clickUpgrade.CostText.text = "Cost: " + Cost().ToString("F2") + " Elon";
		clickUpgrade.NameText.text = "+1" + clickClikUpgradeName;
	}
	
	public BigDouble Cost() => clickUpgradeBaseCost * BigDouble.Pow(clickUpgradeCostMult, controller.data.clickUpgradeLevel);

	public void BuyUpgrade()
	{
		if (controller.data.Elon >= Cost())
		{
			controller.data.Elon -= Cost();
			controller.data.clickUpgradeLevel += 1;
		}
		
		UpdateClickUpgradeUI();
	}
}
