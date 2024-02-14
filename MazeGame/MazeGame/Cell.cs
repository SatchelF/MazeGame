using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGame
{

    public class Cell
    {
        public int X { get; } // read only for X
        public int Y { get; } // read only for Y
        public Dictionary<string, Cell> Edges { get; set; } // read and write for edges
        public bool Visited { get; set; } = false; // read and write for visited. They are first all set to false. 

        public Cell(int x, int y) // my constructor
        {
            X = x;
            Y = y;
            Edges = new Dictionary<string, Cell> { // Edges have a direction and a reference to another cell. 
            {"n", null},
            {"s", null},
            {"e", null},
            {"w", null}
        };
        }



    }

}
