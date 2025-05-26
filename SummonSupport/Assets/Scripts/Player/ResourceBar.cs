using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using SummonSupportEvents;
using System;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class ResourceBar
{
    public GroupBox groupBox;
    public GroupBox positiveArrowsBox;
    public GroupBox negativeArrowsBox;
    public Label resourceValue;
    Label arrowRightTemplate;
    Label arrowLeftTemplate;

    public ResourceBar(ProgressBar hpProgressBar)
    {
        groupBox = hpProgressBar.Q<GroupBox>("ValueAndArrowsBox");
        positiveArrowsBox = groupBox.Q<GroupBox>("PositiveRegenBox");
        negativeArrowsBox = groupBox.Q<GroupBox>("NegativeRegenBox");
        resourceValue = groupBox.Q<Label>("ResourceValue");
        arrowRightTemplate = positiveArrowsBox.Q<Label>("ArrowRight");
        arrowLeftTemplate = negativeArrowsBox.Q<Label>("ArrowLeft");
        arrowRightTemplate.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            Debug.Log($"Arrow width: {arrowRightTemplate.resolvedStyle.width}, height: {arrowRightTemplate.resolvedStyle.height}");
            Debug.Log("Background Image: " + arrowRightTemplate.style.backgroundImage.value);

            // Erst nach sicherem Zugriff entfernen
            arrowRightTemplate.RemoveFromHierarchy();
        });

        arrowLeftTemplate.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            arrowLeftTemplate.RemoveFromHierarchy();
        });

    }
    Label CreateArrowCopy(Label arrowTemplate)
    {
        var copy = new Label();

        foreach (var styleClass in arrowTemplate.GetClasses())
            copy.AddToClassList(styleClass);

        copy.style.display = DisplayStyle.Flex;
        copy.style.position = Position.Relative;
        copy.style.width = 12;
        copy.style.height = 12;
        copy.style.backgroundColor = new Color(1, 1, 0, 0.5f);

        return copy;
    }
    public void SetArrows(int numberArrows)
    {
        negativeArrowsBox.Clear();
        positiveArrowsBox.Clear();

        if (numberArrows > 0)
        {
            for (int i = 0; i < numberArrows; i++)
            {
                positiveArrowsBox.Add(CreateArrowCopy(arrowRightTemplate));
            }
        }
        else if (numberArrows < 0)
        {
            for (int i = 0; i < -numberArrows; i++)
            {
                negativeArrowsBox.Add(CreateArrowCopy(arrowLeftTemplate));
            }
        }
    }
    public void SetValue(float value)
    {
        resourceValue.text = Mathf.RoundToInt(value).ToString();
    }
}