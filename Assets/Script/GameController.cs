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

    void Start()
    {
        data = new Data();

    }

    void Update()
    {
        UpdateCounterLabel();

    }

    public void GenerateButtonPressed()
    {
        data.Elon += 1;
    }

    void UpdateCounterLabel()
    {
        print(counterLabel.text);
        counterLabel.text = $"{data.Elon} Elon";
    }
}
