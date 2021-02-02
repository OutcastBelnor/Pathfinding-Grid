using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Possible state of the button.
/// </summary>
public enum ButtonStates
{
    Default,
    Blocked,
    Start,
    Goal,
    Path
}

public class ButtonInfo : MonoBehaviour
{
    // Grid
    GridManager grid;
    // Button component
    Button buttonComponent;
    // Button image for changing the color
    Image buttonImage;
    // Text components
    [SerializeField] TextMeshProUGUI buttonText;

    // State
    public ButtonStates CurrentState { get; set; }

    // Coordinates of the button in the grid
    public int[] Coordinates { get; set; }

    private void Awake()
    {
        grid = FindObjectOfType<GridManager>();

        buttonComponent = FindObjectOfType<Button>();
        // Event listener for clicks
        buttonComponent.onClick.AddListener(OnButtonClick);

        // Used to change color of the button
        buttonImage = FindObjectOfType<Image>();

        CurrentState = ButtonStates.Default;

        Coordinates = new int[2];
    }

    /// <summary>
    /// Sets the correct state when clicked.
    /// </summary>
    private void OnButtonClick()
    {
        // If the button is not in any state
        if (CurrentState == ButtonStates.Default)
        {
            // Check which button is needed
            string startOrStop = grid.IsStartOrGoal(this);

            // Set up the button
            if (startOrStop != "")
            {
                if (startOrStop == "start")
                {
                    CurrentState = ButtonStates.Start;
                    buttonImage.color = Color.green;
                }
                else if (startOrStop == "goal")
                {
                    CurrentState = ButtonStates.Goal;
                    buttonImage.color = Color.red;
                }

                // Change the text to coordinates
                buttonText.SetText(this.name);
            }
        }
        // "Unclick" the button
        else if (CurrentState == ButtonStates.Start || CurrentState == ButtonStates.Goal)
        {
            grid.RemoveStartOrStop(this, CurrentState.ToString());

            // Reset the state
            CurrentState = ButtonStates.Default;
            buttonImage.color = Color.white;
            buttonText.SetText(string.Empty);
        }
    }

    /// <summary>
    /// Add distance to the goal or start button after finding a path.
    /// </summary>
    /// <param name="distance"></param>
    public void AddDistanceText(int distance)
    {
        buttonText.SetText(name + "\n" + string.Intern(distance.ToString()));
    }

    /// <summary>
    /// Remove the distance from the text after "unclicking" a button.
    /// </summary>
    public void RemoveDistanceText()
    {
        buttonText.SetText(name);
    }

    /// <summary>
    /// Reset a button.
    /// </summary>
    public void Reset()
    {
        IsClickable(true);
        IsPath(false);

        if (CurrentState != ButtonStates.Default)
        {
            CurrentState = ButtonStates.Default;

            buttonImage.color = Color.white;
            buttonText.SetText(string.Empty);
        }
    }

    /// <summary>
    /// Block or unblock the button.
    /// </summary>
    /// <param name="isClickable"></param>
    public void IsClickable(bool isClickable)
    {
        if (isClickable && CurrentState == ButtonStates.Blocked)
        {
            CurrentState = ButtonStates.Default;
        }
        else if (!isClickable)
        {
            CurrentState = ButtonStates.Blocked;
        }

        // This prevents the button from being clicked
        GetComponent<Button>().interactable = isClickable;
    }

    /// <summary>
    /// Sets up the path button.
    /// </summary>
    /// <param name="isPath"></param>
    /// <param name="distance"></param>
    public void IsPath(bool isPath, int distance = 0)
    {
        if (isPath && CurrentState == ButtonStates.Default)
        {
            CurrentState = ButtonStates.Path;
            buttonImage.color = Color.yellow;

            buttonText.SetText(name + "\n" + string.Intern(distance.ToString()));
        }
        else if (isPath && CurrentState == ButtonStates.Path)
        {
            CurrentState = ButtonStates.Default;
            buttonImage.color = Color.white;

            buttonText.SetText(string.Empty);
        }
    }
}
