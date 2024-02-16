using System.Collections.Generic;

public class Cell
{
    public int X { get; }
    public int Y { get; }
    public Dictionary<string, Cell> Edges { get; set; }
    public bool Visited { get; set; } = false; // Used in maze generation
    public bool PlayerVisited { get; set; } = false; // Track if the player has visited the cell
    public bool HasBeenScored { get; set; } = false; // Track if the cell has been scored

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
        Edges = new Dictionary<string, Cell> {
            {"n", null},
            {"s", null},
            {"e", null},
            {"w", null}
        };
    }
}
