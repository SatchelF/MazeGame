using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{


    // Prims Algorithm Steps
    // 1. Start from a Single Cell: Mark a random cell as part of the maze.
    // 2. Frontier Cells: Identify all the neighboring cells of the maze cells that are not yet in the maze. These are called frontier cells.
    // 3. Select and Connect a Frontier Cell: Randomly select a frontier cell, connect it to an adjacent cell already in the maze, and mark it as part of the maze.
    // 4. Repeat: Continue selecting and connecting frontier cells until there are no more frontier cells.

    public class MazeGenerator
    {
        private Cell[,] grid; // did this instead of a jagged array 
        private HashSet<Cell> frontier = new HashSet<Cell>(); // frontier 
        private Random rand = new Random();
        private int width;
        private int height;

        public MazeGenerator(int width, int height) // constructor, and it Makes a grid 
        {
            this.width = width;
            this.height = height;
            InitializeGrid();
        }


        private void InitializeGrid() // sets cords for every cell and creates a cell for every cord 
        {
            grid = new Cell[width, height]; // sets grid to an array of cells, equal to the height and the width. 
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = new Cell(x, y); // cell in grid is equal to a cell with the current cords 
                }
            }
        }

        public void PrintGrid()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write($"({grid[x, y].X}, {grid[x, y].Y}) ");
                }
                Console.WriteLine();
            }
        }

        public void GenerateMaze()
        {
            Cell startingCell = grid[rand.Next(width), rand.Next(height)]; // 1. Start from a Single Cell: Mark a random cell as part of the maze.
            startingCell.Visited = true;
            AddToFrontier(startingCell);

            while (frontier.Count != 0)
            {

                Cell frontierCell = GetRandomFrontierCell();
                Cell? adjacentMazeCell = FindAdjacentMazeCell(frontierCell);

                if (adjacentMazeCell != null)
                {
                    ConnectCells(frontierCell, adjacentMazeCell); // Step 2: Connect the cells
                }

                AddToFrontier(frontierCell); // Step 3: Add new frontier cells around the connected cell

            }

        }

        private Cell GetRandomFrontierCell() // gets random cell from the frontier 
        {
            int index = rand.Next(frontier.Count);
            return frontier.ElementAt(index);
        }

        private Cell? FindAdjacentMazeCell(Cell frontierCell)
        {
            List<Cell> adjacentMazeCells = new List<Cell>();
            CheckAndCollectAdjacentMazeCell(frontierCell.X, frontierCell.Y - 1, adjacentMazeCells); // North
            CheckAndCollectAdjacentMazeCell(frontierCell.X + 1, frontierCell.Y, adjacentMazeCells); // East
            CheckAndCollectAdjacentMazeCell(frontierCell.X, frontierCell.Y + 1, adjacentMazeCells); // South
            CheckAndCollectAdjacentMazeCell(frontierCell.X - 1, frontierCell.Y, adjacentMazeCells); // West

            if (adjacentMazeCells.Count > 0)
            {
                int randomIndex = rand.Next(adjacentMazeCells.Count); // selected a visited cell
                return adjacentMazeCells[randomIndex]; // return that cell
            }
            return null;
        }

        private void CheckAndCollectAdjacentMazeCell(int x, int y, List<Cell> adjacentMazeCells)
        {
            if (IsWithinBounds(x, y) && grid[x, y].Visited) // if its next to it and it has been visited
            {
                adjacentMazeCells.Add(grid[x, y]); // add to our list of adjacent visited cells 
            }
        }


        private void ConnectCells(Cell frontierCell, Cell mazeCell)
        {
            if (frontierCell.X == mazeCell.X)
            {
                if (frontierCell.Y > mazeCell.Y) // Maze cell is north
                {
                    frontierCell.Edges["n"] = mazeCell;
                    mazeCell.Edges["s"] = frontierCell;
                }
                else // Maze cell is south
                {
                    frontierCell.Edges["s"] = mazeCell;
                    mazeCell.Edges["n"] = frontierCell;
                }
            }
            else if (frontierCell.Y == mazeCell.Y)
            {
                if (frontierCell.X > mazeCell.X) // Maze cell is west
                {
                    frontierCell.Edges["w"] = mazeCell;
                    mazeCell.Edges["e"] = frontierCell;
                }
                else // Maze cell is east
                {
                    frontierCell.Edges["e"] = mazeCell;
                    mazeCell.Edges["w"] = frontierCell;
                }
            }

            // After connecting, mark the frontier cell as part of the maze and remove it from the frontier set
            frontierCell.Visited = true;
            frontier.Remove(frontierCell);
        }





        private void AddToFrontier(Cell currentCell)
        {
            CheckAndAddNeighbor(currentCell.X, currentCell.Y - 1); // North
            CheckAndAddNeighbor(currentCell.X + 1, currentCell.Y); // East
            CheckAndAddNeighbor(currentCell.X, currentCell.Y + 1); // South
            CheckAndAddNeighbor(currentCell.X - 1, currentCell.Y); // West

        }



        private void CheckAndAddNeighbor(int x, int y)
        {
            if (IsWithinBounds(x, y) && !grid[x, y].Visited)
            {
                frontier.Add(grid[x, y]);
                // Removed the line that marks the cell as visited here
            }
        }


        private bool IsWithinBounds(int x, int y)
        {
            // true if positive and smaller than the width
            bool isXWithinBounds = x >= 0 && x < width;

            // true if positive and smaller than the height
            bool isYWithinBounds = y >= 0 && y < height;

            // return true if both are true
            if (isXWithinBounds && isYWithinBounds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void PrintMaze()
        {
            // Print the top border
            const char PASSAGE_CHAR = ' ';
            const char WALL_CHAR = '▓';
            Console.WriteLine(new string(WALL_CHAR, width * 2 + 1));

            for (int y = 0; y < height; y++)
            {
                string horizontalWalls = WALL_CHAR.ToString(); // Left border
                string verticalWalls = WALL_CHAR.ToString(); // Left border for the next row

                for (int x = 0; x < width; x++)
                {
                    Cell cell = grid[x, y];
                    // Cell itself is always a passage
                    horizontalWalls += PASSAGE_CHAR;
                    // Check if there's a connection to the East, print passage if true, wall if false
                    horizontalWalls += cell.Edges.ContainsKey("e") && cell.Edges["e"] != null ? PASSAGE_CHAR : WALL_CHAR;

                    // Check if there's a connection to the South, print passage if true, wall if false
                    // Always add a wall character after since it's the space between cells vertically
                    verticalWalls += cell.Edges.ContainsKey("s") && cell.Edges["s"] != null ? PASSAGE_CHAR : WALL_CHAR;
                    verticalWalls += WALL_CHAR;
                }

                Console.WriteLine(horizontalWalls); // Print row of cells and east walls
                if (y == height - 1)
                {
                    // Bottom border on the last row
                    Console.WriteLine(new string(WALL_CHAR, width * 2 + 1));
                }
                else
                {
                    Console.WriteLine(verticalWalls); // Print south walls for all but the last row
                }
            }
        }


    }




}
