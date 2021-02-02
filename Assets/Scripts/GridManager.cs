using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using System;

public class GridManager : MonoBehaviour
{
    [Header("Max size of the grid")]
    // Max size of the grid
    [SerializeField] float maxGridWidth = 1000;
    [SerializeField] float maxGridHeight = 595;

    [Header("Input fields for the size of the generated grid")]
    // Input fields / Size of the grid
    [SerializeField] TMP_InputField WidthInputField;
    [SerializeField] TMP_InputField HeightInputField;

    // Currently active buttons
    List<ButtonInfo> activeButtons;
    // Currently blocked buttons
    [SerializeField] List<ButtonInfo> blockedButtons;
    // Current path buttons
    List<ButtonInfo> pathButtons;

    [Header("Percent of the Block Buttons")]
    // Range for random percent of the blocked buttons
    [SerializeField] int minPercent = 8;
    [SerializeField] int maxPercent = 12;

    [Header("Size of the Block Buttons group")]
    // Range for random group of the blocked buttons
    [SerializeField] int minGroupSize = 1;
    [SerializeField] int maxGroupSize = 5;

    // Grid Layout
    GridLayoutGroup gridLayout;
    // Pathfinding object
    Pathfinder pathfinder;

    // Width and Height provided from the inputs
    public float Width { get; private set; }
    public float Height { get; private set; }

    // The start and goal of the path
    public ButtonInfo StartNode { get; private set; }
    public ButtonInfo GoalNode { get; private set; }
 
    private void Start()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        pathfinder = GetComponent<Pathfinder>();

        activeButtons = new List<ButtonInfo>();
        blockedButtons = new List<ButtonInfo>();
        pathButtons = new List<ButtonInfo>();

        Width = 0;
        Height = 0;
    }

    /// <summary>
    /// Generates the grid with the size provided in the input fields.
    /// </summary>
    public void GenerateGrid()
    {
        // Need both the Height and Width to be able to generate the grid
        if (WidthInputField.text != "" && HeightInputField.text != "")
        {
            // Get the size of the grid from the inputs
            Width = float.Parse(WidthInputField.text);
            Height = float.Parse(HeightInputField.text);

            // Check if the input size is correct
            if (Width < 2 || Height < 2)
            {
                Debug.Log("The minimum grid size is 4x4.");
                return;
            }
            else if (Width > 50 || Height > 50)
            {
                Debug.Log("The maximum grid size is 50x50.");
                return;
            }

            // Adjust the size of the buttons
            ScaleButtonSize();

            // Copy the list for further resetting the no longer needed buttons
            List<ButtonInfo> buttonsToReset = new List<ButtonInfo>(activeButtons.ToArray());
            // Clear the list of the old buttons
            activeButtons.Clear();

            // Generate the grid by getting the buttons with the right coordinates
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    // Active, reset, add and remove the button from the lists
                    ButtonInfo currentButton = ButtonPool.sharedInstance.GetPooledButton(j, i);
                    currentButton.gameObject.SetActive(true);
                    currentButton.Reset();
                    activeButtons.Add(currentButton);
                    buttonsToReset.Remove(currentButton);
                }
            }

            // Generate the blocked buttons
            BlockButtons();

            // Reset the start and goal node
            StartNode = null;
            GoalNode = null;

            // Reset the no longer used buttons
            ResetRemainingActiveButtons(buttonsToReset);
        }
    }

    /// <summary>
    /// Scales the buttons to always fit the grid and occupy the most space.
    /// </summary>
    private void ScaleButtonSize()
    {
        // Calculate the maximum Width and Height of the button
        float buttonWidth = (maxGridWidth - Width) / Width; ;
        float buttonHeight = (maxGridHeight - Height) / Height;
        // Choose smaller length to fit into the grid
        float buttonSize = Mathf.Min(buttonWidth, buttonHeight);

        // Scale the button size
        gridLayout.cellSize = new Vector2(buttonSize, buttonSize);
        
        // Set the column count to the Width provided in the input
        // Since we are generating row by row you only need to set the column count,
        // because the scaling of the buttons will the rest of the work
        gridLayout.constraintCount = (int)Width;
    }

    /// <summary>
    /// Resets the buttons that are no longer used in the grid.
    /// </summary>
    /// <param name="buttonsToReset">List of buttons that need resetting after generating a new grid.</param>
    private void ResetRemainingActiveButtons(List<ButtonInfo> buttonsToReset)
    {
        for (int i = 0; i < buttonsToReset.Count; i++)
        {
            ButtonInfo currentButton = buttonsToReset[i];
            currentButton.Reset();
            currentButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Randomly makes group of buttons not clickable.
    /// The amount of blocked buttons is randomized between 8 and 12% of the total buttons in the current grid.
    /// The blocks to be blocked are grouped in the groups of randomized size.
    /// Each next blocked button is a neighbour of the last and there are no repeats.
    /// </summary>
    private void BlockButtons()
    {
        blockedButtons.Clear();

        // Calculate the amount of buttons to become blocks
        var blockedButtonsAmount = (int)Mathf.Round(activeButtons.Count * ((float)Random.Range(minPercent, maxPercent) / 100f));

        // This loop will add groups of blocked buttons until the limit (blockedButtonsAmount) is reached
        while(blockedButtonsAmount > 0)
        {
            // Randomized group size
            int currentGroupSize = Random.Range(minGroupSize, Mathf.Min(maxGroupSize, blockedButtonsAmount));

            // Coordinates of the first blocked button
            int randomXCoordinate = Random.Range(1, (int)Width);
            int randomYCoordinate = Random.Range(1, (int)Height);
            // Gets the first blocked button
            ButtonInfo currentButton = ButtonPool.sharedInstance.GetPooledButton(randomXCoordinate, randomYCoordinate);
            // If the button is already blocked or the button doesn't exist at the specified coords, repeat
            if (blockedButtons.Contains(currentButton) || currentButton is null)
            {
                continue;
            }
            currentButton.IsClickable(false);
            blockedButtons.Add(currentButton);
            
            // Next buttons will be neighbouring with previous
            for (int i = 1; i < currentGroupSize; i++)
            {
                // Randomizes the direction in which the group expands
                int randomDirection = Random.Range(0, 8);
                switch(randomDirection)
                {
                    case 0:
                        randomYCoordinate += 1;
                        break;
                    case 1:
                        randomXCoordinate += 1;
                        randomYCoordinate += 1;
                        break;
                    case 2:
                        randomXCoordinate += 1;
                        break;
                    case 3:
                        randomXCoordinate += 1;
                        randomYCoordinate -= 1;
                        break;
                    case 4:
                        randomYCoordinate -= 1;
                        break;
                    case 5:
                        randomXCoordinate -= 1;
                        randomYCoordinate -= 1;
                        break;
                    case 6:
                        randomXCoordinate -= 1;
                        break;
                    case 7:
                        randomXCoordinate -= 1;
                        randomYCoordinate += 1;
                        break;
                }

                // Get the next blocked button
                currentButton = ButtonPool.sharedInstance.GetPooledButton(randomXCoordinate, randomYCoordinate);
                // If the button is already blocked or the button doesn't exist at the specified coords, repeat
                if (blockedButtons.Contains(currentButton) || currentButton is null)
                {
                    i--;
                    continue;
                }
                currentButton.IsClickable(false);
                blockedButtons.Add(currentButton);
            }

            // Update the amount of buttons to be blocked
            blockedButtonsAmount -= currentGroupSize;
        }
    }

    /// <summary>
    /// Checks if there is a blocked button with provided coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsBlocked(int x, int y)
    {
        ButtonInfo buttonToCheck = ButtonPool.sharedInstance.GetPooledButton(x, y);
        if (buttonToCheck != null && !blockedButtons.Contains(buttonToCheck))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if there is a start and goal button.
    /// Used to determine what the clicked button should be.
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public string IsStartOrGoal(ButtonInfo button)
    {
        if (StartNode is null)
        {
            StartNode = button;
            CheckIfPathReady();
            return "start";
        }
        else if (GoalNode is null)
        {
            GoalNode = button;
            CheckIfPathReady();
            return "goal";
        }

        return "";
    }

    /// <summary>
    /// Checks if the path is ready to be found.
    /// </summary>
    private void CheckIfPathReady()
    {
        if (StartNode != null && GoalNode != null)
        {
            // Pathfinding
            List<ButtonNode> pathNodes = pathfinder.FindPath();

            // If the path is found
            if (!(pathNodes is null))
            {
                // Set up the path buttons
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    ButtonInfo pathButton = ButtonPool.sharedInstance.GetPooledButton(pathNodes[i].X - 1, pathNodes[i].Y - 1);
                    pathButton.IsPath(true, pathNodes[i].DistanceFromStart);
                    pathButtons.Add(pathButton);
                }

                // Adjust the text of the start and goal buttons
                StartNode.AddDistanceText(0);
                int goalNodeDistance = pathNodes[0].DistanceFromStart + 1;
                GoalNode.AddDistanceText(goalNodeDistance);
            }
            else
            {
                Debug.Log("Could not find a path to the goal.");
            }
        }
    }

    /// <summary>
    /// After clicking on the same button for second time,
    /// it is resetted.
    /// </summary>
    /// <param name="button"></param>
    /// <param name="startOrStop"></param>
    public void RemoveStartOrStop(ButtonInfo button, string startOrStop)
    {
        if (startOrStop == "Start")
        {
            StartNode = null;
            // Adjust the text of the second button if not null
            if (!(GoalNode is null))
            {
                GoalNode.RemoveDistanceText();
            }
        }
        else if (startOrStop == "Goal")
        {
            GoalNode = null;
            // Adjust the text of the second button if not null
            if (!(StartNode is null))
            {
                StartNode.RemoveDistanceText();
            }
        }
        
        // Reset the path buttons
        for (int i = 0; i < pathButtons.Count; i++)
        {
            pathButtons[i].Reset();
        }

        pathButtons.Clear();
    }
}
