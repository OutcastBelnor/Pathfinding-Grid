using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for the button nodes, used for pathfinding.
/// </summary>
public class ButtonNode
{
    /// <summary>
    /// Constructor for root nodes.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public ButtonNode(int x, int y)
    {
        X = x;
        Y = y;

        CombinedDistance = 0;
        DistanceFromStart = 0;
        DistanceToGoal = 0;

        Parent = null;
    }
    /// <summary>
    /// Constructor for child nodes.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="parent">Parent node</param>
    public ButtonNode(int x, int y, ButtonNode parent)
    {
        X = x;
        Y = y;

        CombinedDistance = 0;
        DistanceFromStart = 0;
        DistanceToGoal = 0;

        Parent = parent;
    }

    // Coordinates
    public int X { get; set; }
    public int Y { get; set; }

    // Distance from the start node
    public int DistanceFromStart { get; set; }
    // Distance to the goal node
    public int DistanceToGoal { get; set; }
    // DistanceFromStart + DistanceToGoal (f from the algorithm)
    public int CombinedDistance { get; set; }

    // Parent of the node
    public ButtonNode Parent { get; set; }

    /// <summary>
    /// Custom Equals for easier comparing.
    /// </summary>
    /// <param name="otherNode"></param>
    /// <returns></returns>
    public bool Equals(ButtonNode otherNode)
    {
        return (X == otherNode.X && Y == otherNode.Y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", X, Y);
    }
}

public class Pathfinder : MonoBehaviour
{
    // GridManager
    GridManager gridManager;

    // Possible path nodes, (sometimes called the "frontier")
    List<ButtonNode> openNodes;
    // Almost confirmed nodes
    List<ButtonNode> closedNodes;
    // Candidates for possible path nodes
    List<ButtonNode> nextNodes;

    // The goal node
    ButtonNode goalNode;

    // Start is called before the first frame update
    void Start()
    {
        gridManager = gameObject.GetComponent<GridManager>();

        openNodes = new List<ButtonNode>();
        closedNodes = new List<ButtonNode>();
        nextNodes = new List<ButtonNode>();
    }

    /// <summary>
    /// A* search pathfinding.
    /// </summary>
    /// <returns>Shortest path</returns>
    public List<ButtonNode> FindPath()
    {
        // Reset the lists
        openNodes.Clear();
        closedNodes.Clear();
        nextNodes.Clear();
        
        // Add the start node as the first node to evaluate
        openNodes.Add(new ButtonNode(gridManager.StartNode.Coordinates[0], gridManager.StartNode.Coordinates[1]));
        // Create a goal node from the goal button
        goalNode = new ButtonNode(gridManager.GoalNode.Coordinates[0], gridManager.GoalNode.Coordinates[1]);

        // As long as there is a possible path
        while (openNodes.Count != 0)
        {
            ButtonNode currentNode;
            int newNodeIndex = 0;
            if (openNodes.Count != 1)
            {
                // Get the index of the node with the lowest "f" distance
                int minDistance = 100000;
                for (int i = 0; i < openNodes.Count; i++)
                {
                    currentNode = openNodes[i];

                    if (currentNode.CombinedDistance < minDistance)
                    {
                        minDistance = currentNode.DistanceToGoal;
                        newNodeIndex = i;
                    }
                }
            }
            // Pop it from the list
            currentNode = openNodes[newNodeIndex];
            openNodes.RemoveAt(newNodeIndex);

            // Get neighbour nodes
            FindNextNodes(currentNode);

            // Check if any are possible paths
            for (int i = 0; i < nextNodes.Count; i++)
            {
                ButtonNode newNode = nextNodes[i];
                
                // If the node is actually a goal node
                if (newNode.Equals(goalNode))
                {
                    // Get all the previous nodes from this path
                    List<ButtonNode> pathNodes = new List<ButtonNode>();
                    
                    ButtonNode pathNode = currentNode;
                    while(pathNode.Parent != null)
                    {
                        pathNodes.Add(pathNode);

                        pathNode = pathNode.Parent;
                    }

                    return pathNodes;
                }

                // Calculate the distances needed to determine if this node is any good
                newNode.DistanceFromStart = currentNode.DistanceFromStart + 1;
                newNode.DistanceToGoal = Mathf.Abs(newNode.X - goalNode.X) + Mathf.Abs(newNode.Y - goalNode.Y);
                newNode.CombinedDistance = newNode.DistanceFromStart + newNode.DistanceToGoal;

                // If there is a node with the same coordinates in the "frontier" list
                // and it has a smaller combined distance skip this node
                if(CheckOpenList(newNode))
                {
                    continue;
                }

                // If there isn't a node with the same coordinates in the "closed" list
                // with a smaller combined distance, then add it to the "frontier" list
                if (!CheckClosedList(newNode))
                {
                    openNodes.Add(newNode);
                }
            }

            // Add the used node to the closed list
            closedNodes.Add(currentNode);
        }

        return null;
    }

    /// <summary>
    /// Checks if there is a node with the same coordinates
    /// in the open nodes list.
    /// </summary>
    /// <param name="newNode"></param>
    /// <returns></returns>
    private bool CheckOpenList(ButtonNode newNode)
    {
        for (int i = 0; i < openNodes.Count; i++)
        {
            if (openNodes[i].Equals(newNode))
            {
                if (openNodes[i].CombinedDistance <= newNode.CombinedDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if there is a node with the same coordinates
    /// in the open nodes list.
    /// </summary>
    /// <param name="newNode"></param>
    /// <returns></returns>
    private bool CheckClosedList(ButtonNode newNode)
    {
        for (int i = 0; i < closedNodes.Count; i++)
        {
            if (closedNodes[i].Equals(newNode))
            {
                if (closedNodes[i].CombinedDistance <= newNode.CombinedDistance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Find all the neighbouring nodes, that are possible to move to
    /// from the current node.
    /// </summary>
    /// <param name="currentNode">Currently evaluated node</param>
    private void FindNextNodes(ButtonNode currentNode)
    {
        nextNodes.Clear();

        // Possible directions to move to
        // Assumed that there are only 4 moves possible
        int[,] directions = new int[4, 2] { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

        for (int i = 0; i < 4; i++)
        {
            // Calculate coordinates of the new node
            int newX = currentNode.X + directions[i, 0];
            int newY = currentNode.Y + directions[i, 1];

            // Check if it is within bounds of the grid
            if (newX > 0 && newX <= gridManager.Width && newY > 0 && newY <= gridManager.Height)
            {
                // Check if it not a blocked button
                if (!gridManager.IsBlocked(newX - 1, newY - 1))
                {
                    // Create and add the new node to the list
                    ButtonNode newNode = new ButtonNode(newX, newY, currentNode);
                    nextNodes.Add(newNode);
                }
            }
            else
            {
                continue;
            }
        }
    }
}
