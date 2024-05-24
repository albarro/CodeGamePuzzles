using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    private static int PlayerX;
    private static int PlayerY;

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int R = int.Parse(inputs[0]); // number of rows.
        int C = int.Parse(inputs[1]); // number of columns.
        int A = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

        Map map = new Map(C, R);
        bool isReturnPathCal = false;
        Stack<Cell> returnPath = null;
        Stack<Cell> explorationPath = new Stack<Cell>();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            PlayerY = int.Parse(inputs[0]); // row where Rick is located.
            PlayerX = int.Parse(inputs[1]); // column where Rick is located.
            for (int i = 0; i < R; i++)
            {
                string ROW = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                map.refreshRow(i, ROW);
            }

            //Console.Error.WriteLine("Player Pos:{0} {1}", PlayerX, PlayerY);
            //map.printMapa();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            //Console.WriteLine("RIGHT"); // Rick's next move (UP DOWN LEFT or RIGHT).


            if (map.isControlFound && !isReturnPathCal)
            {//We have found the control cell
                map.findShortesthPath(map.ControlCell, map.StartCell, out int costControl, out Stack<Cell> path);
                if (costControl <= A && costControl >= 0)
                { //We found the path
                    isReturnPathCal = true;
                    returnPath = path;
                }
                // else we need to keep exploring
            }

            if (isReturnPathCal) //We have evething just follow the path
            {
                followPath(returnPath, map);
                // End this iteration
                continue;
            }

            if (!explorationPath.Any()) //We need to calculate the exploration path
            {
                explorationPath = map.findExplorationPath(map.getCell(PlayerX, PlayerY));
            }

            if (explorationPath.Any()) //We have a path to explore
            {
                followPath(explorationPath, map);
                // End this iteration
                continue;
            }

            Console.Error.WriteLine("We don't have a possible move at cell {0}", map.getCell(PlayerX, PlayerY));


        }
    }

    private static void followPath(Stack<Cell> returnPath, Map map)
    {
        //Console.Error.WriteLine("Following path");
        //foreach (var cell in returnPath)
        //{
        //    Console.Error.WriteLine(cell);
        //}

        // Check our nextStep
        Cell nextPos = returnPath.Pop();


        // We have arrived to our next step, check for a new one
        if (nextPos.Cord.Equals(new Coordinates(PlayerX, PlayerY)))
        {
            nextPos = returnPath.Pop();
        }

        //Find how to get to the new step
        int nextX = nextPos.Cord.PosX;
        int nextY = nextPos.Cord.PosY;

        //Left
        if (nextY == PlayerY && nextX == PlayerX - 1)
        {
            moveLeft();
        }
        //Right
        else if (nextY == PlayerY && nextX == PlayerX + 1)
        {
            moveRight();
        }
        //Up
        else if (nextX == PlayerX && nextY == PlayerY - 1)
        {
            moveUp();
        }
        //Down
        else if (nextX == PlayerX && nextY == PlayerY + 1)
        {
            moveDown();
        }
        //We are not near, we need to calculate th path to it and follow it
        else
        {
            // Didn't move readd post to path
            returnPath.Push(nextPos);

            map.findShortesthPath(map.getCell(PlayerX, PlayerY), nextPos, out var cost, out var path);
            followPath(path, map);
        }


    }


    public static void moveRight()
    {
        Console.WriteLine("RIGHT");
    }

    public static void moveLeft()
    {
        Console.WriteLine("LEFT");
    }

    public static void moveUp()
    {
        Console.WriteLine("UP");
    }

    public static void moveDown()
    {
        Console.WriteLine("DOWN");
    }
}

class Map
{

    private int Width;
    private int Height;

    //Costs
    private SortedDictionary<string, int> CellCosts;

    private Cell[,] MapArray;
    public Cell getCell(int x, int y)
    {
        return MapArray[x, y];
    }

    public Cell StartCell;
    public Cell ControlCell;
    public bool isControlFound
    {
        get => ControlCell != null;
    }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;

        MapArray = new Cell[width, height];

        StartCell = null;
        ControlCell = null;
    }

    public void refreshRow(int y, string row)
    {
        char[] line = row.ToCharArray();

        for (int x = 0; x < Width; x++)
        {
            if (MapArray[x, y] == null || MapArray[x, y].isUnkown)
            {
                Cell cell = new Cell(line[x], new Coordinates(x, y));
                MapArray[x, y] = cell;

                if (cell.isControl)
                {
                    ControlCell = cell;
                }
                else if (cell.isStart)
                {
                    StartCell = cell;
                }
            }
        }
    }

    private bool checkValidCell(int x, int y)
    {
        //Console.Error.WriteLine("Check Cell {0} {1}", x, y);
        //Check coords
        if (x > Width || x < 0 || y < 0 || y > Height)
        {
            return false;
        }
        //Check walls
        if (MapArray[x, y].isWall)
        {
            return false;
        }

        //Console.Error.WriteLine("Valid Cell");
        return true;
    }

    private int countUnkowsNearCell(int x, int y){
        int count = 0;

        if(checkValidCell(x+1, y) && MapArray[x+1,y].isUnkown){
            count += 1;
        }
        if(checkValidCell(x-1, y) && MapArray[x-1,y].isUnkown){
            count += 1;
        }
        if(checkValidCell(x, y+1) && MapArray[x,y+1].isUnkown){
            count += 1;
        }
        if(checkValidCell(x, y-1) && MapArray[x,y-1].isUnkown){
            count += 1;
        }

        return count;
    }

    public Stack<Cell> findExplorationPath(Cell startCell)
    {
        Stack<Cell> path = new Stack<Cell>();

        //Console.Error.WriteLine("Get Path from {0} to BestCell we find", startCell);
        Queue<TravelCell> queue = new Queue<TravelCell>();
        TravelCell star = new TravelCell(startCell);
        queue.Enqueue(star);
        star.Cost = countUnkowsNearCell(startCell.Cord.PosX, startCell.Cord.PosY); //use  cost as how interesting is the cell

        bool[,] visited = new bool[Width, Height];
        visited[startCell.Cord.PosX, startCell.Cord.PosY] = true;

        while (queue.Any())
        {
            TravelCell p = queue.Dequeue();
            int x = p.MapCell.Cord.PosX;
            int y = p.MapCell.Cord.PosY;
            //Console.Error.WriteLine("Check cell {0}", p);

            //Found control room we dont want to touch it
            if(p.MapCell.isControl){
                p.Cost = -1;
                continue;
            }
           

            TravelCell nextCell = null;
            // rigth
            if (checkValidCellForPath(x + 1, y, visited))
            {
                nextCell = new TravelCell(MapArray[x + 1, y]);
                nextCell.Cost = countUnkowsNearCell(nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY);
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // left
            if (checkValidCellForPath(x - 1, y, visited))
            {
                nextCell = new TravelCell(MapArray[x - 1, y]);
                nextCell.Cost = countUnkowsNearCell(nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY);
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // up
            if (checkValidCellForPath(x, y - 1, visited))
            {
                nextCell = new TravelCell(MapArray[x, y - 1]);
                nextCell.Cost = countUnkowsNearCell(nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY);
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // down
            if (checkValidCellForPath(x, y + 1, visited))
            {
                nextCell = new TravelCell(MapArray[x, y + 1]);
                nextCell.Cost = countUnkowsNearCell(nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY);
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

             // Destination found
            if (p.Cost >= 1)
            {
                //Console.Error.WriteLine("Path Found Cost={0}", p.Cost);

                //Add final cell to path
                path.Push(p.MapCell);

                //Look parents and create Path
                while (p.ParentCell != null)
                {
                    path.Push(p.ParentCell.MapCell);
                    p = p.ParentCell;
                }

                return path;
            }

        }

        //No path
        return path;
    }

    public void findShortesthPath(Cell startingCell, Cell destinationCell, out int cost, out Stack<Cell> path)
    {
        path = new Stack<Cell>();

        // applying BFS on matrix cells starting from source
        //Console.Error.WriteLine("Get Path from {0} to {1}", startingCell, destinationCell);
        Queue<TravelCell> queue = new Queue<TravelCell>();
        TravelCell star = new TravelCell(startingCell);
        queue.Enqueue(star);
        star.Cost = 0;

        bool[,] visited = new bool[Width, Height];
        visited[startingCell.Cord.PosX, startingCell.Cord.PosY] = true;

        while (queue.Any())
        {
            TravelCell p = queue.Dequeue();
            int x = p.MapCell.Cord.PosX;
            int y = p.MapCell.Cord.PosY;
            //Console.Error.WriteLine("Check cell {0}", p);

            // Destination found
            if (p.MapCell.Equals(destinationCell))
            {
                //Console.Error.WriteLine("Path Found Cost={0}", p.Cost);
                cost = p.Cost;

                //Add final cell to path
                path.Push(p.MapCell);

                //Look parents and create Path
                while (p.ParentCell != null)
                {
                    path.Push(p.ParentCell.MapCell);
                    p = p.ParentCell;
                }

                return;
            }

            TravelCell nextCell = null;
            // rigth
            if (checkValidCellForPath(x + 1, y, visited))
            {
                nextCell = new TravelCell(MapArray[x + 1, y]);
                nextCell.Cost = p.Cost + 1;
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // left
            if (checkValidCellForPath(x - 1, y, visited))
            {
                nextCell = new TravelCell(MapArray[x - 1, y]);
                nextCell.Cost = p.Cost + 1;
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // up
            if (checkValidCellForPath(x, y - 1, visited))
            {
                nextCell = new TravelCell(MapArray[x, y - 1]);
                nextCell.Cost = p.Cost + 1;
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

            // down
            if (checkValidCellForPath(x, y + 1, visited))
            {
                nextCell = new TravelCell(MapArray[x, y + 1]);
                nextCell.Cost = p.Cost + 1;
                queue.Enqueue(nextCell);
                visited[nextCell.MapCell.Cord.PosX, nextCell.MapCell.Cord.PosY] = true;
                nextCell.ParentCell = p;
            }

        }
        //No path
        cost = -1;
        return;
    }

    private bool checkValidCellForPath(int x, int y, bool[,] visited)
    {
        //Check array bounds and basic things first
        if (!checkValidCell(x, y))
        {
            return false;
        }
        //Check for unkowns
        if(MapArray[x,y].isUnkown)
        {
            return false;
        }
        //Check if previously visited
        if (visited[x, y])
        {
            return false;
        }

        //Console.Error.WriteLine("Valid Cell {0} {1}", x, y);
        return true;
    }

    public void printMapa()
    {
        Console.Error.Write("\t");
        for (int xIndex = 0; xIndex < Width; xIndex++)
        {
            Console.Error.Write("x" + xIndex + "\t");
        }
        Console.Error.WriteLine();


        for (int y = 0; y < Height; y++)
        {
            Console.Error.Write("y" + y + "\t");
            for (int x = 0; x < Width; x++)
            {
                Console.Error.Write(MapArray[x, y].Label + "\t");
            }
            Console.Error.WriteLine();
        }
    }
}

// Representation of a map cell
class Cell
{
    public char Label;
    public Coordinates Cord { get; }
    public bool isWall
    {
        get => Label.Equals('#');
    }
    public bool isStart
    {
        get => Label.Equals('T');
    }
    public bool isControl
    {
        get => Label.Equals('C');
    }
    public bool isUnkown
    {
        get => Label.Equals('?');
    }


    public Cell(char label, Coordinates cord)
    {
        Cord = cord;
        Label = label;
    }

    public override string ToString()
    {
        return "[" + Label + " x:" + Cord.PosX + " y:" + Cord.PosY + "]";
    }

    public bool Equals(Cell other)
    {

        return this.Cord.Equals(other.Cord);
    }

}

class TravelCell
{
    public Cell MapCell;
    public TravelCell ParentCell { get; set; }
    public int Cost { get; set; }
    public bool IsVisited { get; set; }

    public TravelCell(Cell mapCell)
    {
        Cost = -1;
        MapCell = mapCell;
        IsVisited = false;
    }

    public override string ToString()
    {
        return "[" + MapCell + " Parent:" + ParentCell + " Cost:" + Cost + " IsVisited:" + IsVisited + "]";
    }

    public bool Equals(TravelCell other)
    {

        return this.MapCell.Equals(other.MapCell);
    }

}


class Coordinates
{

    public int PosX { get; }
    public int PosY { get; }

    public Coordinates(int x, int y)
    {
        PosX = x; PosY = y;
    }

    public double getDistanceToPoint(Coordinates otherPoint)
    {
        return Math.Abs(PosX - otherPoint.PosX) + Math.Abs(PosY - otherPoint.PosY);
    }

    public bool Equals(Coordinates other)
    {
        if (this.PosX == other.PosX && this.PosY == other.PosY)
        {
            return true;
        }
        else
            return false;
    }
}
