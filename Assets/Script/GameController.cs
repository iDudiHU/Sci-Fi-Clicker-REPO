using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BreakInfinity;

public class GameController : MonoBehaviour
{
    public static GameController instance; 
    public void Awake() => instance = this;
    public Data data;
    [SerializeField] private TMP_Text counterLabel;
    [SerializeField] private TMP_Text productionPerSecond;
    [SerializeField] private TMP_Text clickPowerText;
    [Header("----------------")] 
    public GameObject floor;
    public GameObject walls;
    public GameObject geodome;
    public GameObject commandCenter;
    public GameObject middleArea;
    public GameObject shipVisual;

    public BigDouble ClickPower()
    {
        BigDouble total = 1;
        for (int i = 0; i < data.clickUpgradeLevel.Count; i++)
        {
            total += UpgradesManager.instance.clickUpgradesBasePower[i] * data.clickUpgradeLevel[i];
        }

        return total;
    }
    
    public BigDouble ProductionPower()
    {
        BigDouble total = 0;
        for (int i = 0; i < data.productionUpgradeLevel.Count; i++)
        {
            total += UpgradesManager.instance.productionUpgradesBasePower[i] * data.productionUpgradeLevel[i];
        }

        return total;
    }

    void Start()
    {
        data = new Data();
        UpgradesManager.instance.StartUpgradeManager();

    }

    void Update()
    {
        UpdateCounterLabel();
        Activiser();

        data.Elon += ProductionPower() * Time.deltaTime;

    }

    public void Activiser()
    {
        if (data.Elon >= 10)
        {
            floor.SetActive(true);
        }
        if (data.Elon >= 30)
        {
            walls.SetActive(true);
        }
        if (data.Elon >= 60)
        {
            geodome.SetActive(true);
        }
        if (data.Elon >= 100)
        {
            commandCenter.SetActive(true);
        }
        if (data.Elon >= 150)
        {
            middleArea.SetActive(true);
        }
        if (data.Elon >= 200)
        {
            shipVisual.SetActive(true);
        }
    }
    public void GenerateButtonPressed()
    {
        data.Elon += ClickPower();
    }

    void UpdateCounterLabel()
    {
        counterLabel.text = $"{data.Elon:F2} Elon";
        productionPerSecond.text = $"{ProductionPower():F2}/s";
        clickPowerText.text = "+" + ClickPower() + "Elon";
    }
}
