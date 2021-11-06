using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using BreakInfinity;

public class GameController : MonoBehaviour
{
    public Data data;
    public Button generateButton;
    public Label counterLabel;

    void Start()
    {
        data = new Data();
        var root = GetComponent<UIDocument>().rootVisualElement;
        

        generateButton = root.Q<Button>("generate-button");
        counterLabel = root.Q<Label>("counter-text");

        generateButton.RegisterCallback<ClickEvent>(ev => GenerateButtonPressed());
    }

    void Update()
    {
        UpdateCounterLabel();

    }

    void GenerateButtonPressed()
    {
        data.Elon += 1;
    }

    void UpdateCounterLabel()
    {
        print(counterLabel.text);
        counterLabel.text = $"{data.Elon} Elon";
    }
}
