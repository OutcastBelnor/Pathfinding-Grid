using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPool : MonoBehaviour
{
    public static ButtonPool sharedInstance;
    [SerializeField] GameObject[,] pooledButtons;
    [SerializeField] GameObject buttonPrefab;
    
    // The amount of the objects to pool is dependent on the maximum size of the grid
    [SerializeField] int maximumButtonRow = 50;
    [SerializeField] int maximumButtonColumn = 50;

    void Awake()
    {
        sharedInstance = this;
    }

    /// <summary>
    /// At the start of the program, all of the buttons are instantiated and disactivated.
    /// </summary>
    void Start()
    {
        // Object pool
        pooledButtons = new GameObject[maximumButtonRow, maximumButtonColumn];
        GameObject button;
        for (int i = 0; i < maximumButtonColumn; i++)
        {
            for (int j = 0; j < maximumButtonRow; j++)
            {
                // Instantiate the button
                button = Instantiate(buttonPrefab, transform);
                button.name = string.Intern(string.Format("({0}, {1})", j + 1, i + 1));

                // Set the coordinates of the button
                ButtonInfo currentButtonInfo = button.GetComponent<ButtonInfo>();
                currentButtonInfo.Coordinates[0] = j + 1;
                currentButtonInfo.Coordinates[1] = i + 1;
                
                // Disactive
                button.SetActive(false);
                pooledButtons[j, i] = button;
            }
        }
    }

    /// <summary>
    /// Get next inactive button.
    /// </summary>
    /// <returns></returns>
    public GameObject GetPooledButton()
    {
        for (int i = 0; i < maximumButtonRow; i++)
        {
            for (int j = 0; j < maximumButtonColumn; j++)
            {
                if (!pooledButtons[i, j].activeInHierarchy)
                {
                    return pooledButtons[i, j];
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Get the button with specified coordinates.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns></returns>
    public ButtonInfo GetPooledButton(int x, int y)
    {
        try
        {
            return pooledButtons[x, y].GetComponent<ButtonInfo>();
        }
        catch (Exception exception)
        {
            return null;
        }
    }
}
