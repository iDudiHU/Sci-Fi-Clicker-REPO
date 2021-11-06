using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using BreakInfinity;

public class GameManager : MonoBehaviour
{
    public Data data;
    public Button generateButton;
    public Label counterLabel;


    private void Awake()
	{
        var root = GetComponent<UIDocument>().rootVisualElement;

        generateButton = root.Q<Button>("generate-button");
        counterLabel = root.Q<Label>("counter-text");

        generateButton.RegisterCallback<ClickEvent>(ev => GenerateButtonPressed());
    }
	void Start()
    {
        
    }

    void Update()
    {
        UpdateCounterLabel();

    }

    void GenerateButtonPressed()
    {
        data.Elon += 1;
    }

    public void UpdateCounterLabel()
    {
        counterLabel.text = $"{data.Elon} Elon";
    }
}
