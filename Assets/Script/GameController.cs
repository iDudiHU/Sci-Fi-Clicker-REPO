using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BreakInfinity;

public class GameController : MonoBehaviour
{
    public Data data;
    [SerializeField] private TMP_Text counterLabel;
    [SerializeField] private TMP_Text clickPowerText;
    [SerializeField] private UpgradesManager upgradesManager;
    [Header("----------------")] 
    public GameObject floor;

    public GameObject walls;

    public GameObject geodome;

    public GameObject commandCenter;

    public GameObject middleArea;

    public GameObject shipVisual;

    public BigDouble ClickPower() => 1 + data.clickUpgradeLevel;
    void Start()
    {
        data = new Data();
        upgradesManager.StartUpgradeManager();

    }

    void Update()
    {
        UpdateCounterLabel();
        Activiser();
        clickPowerText.text = "+" + ClickPower() + "Elon";

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
        counterLabel.text = $"{data.Elon} Elon";
    }
}
