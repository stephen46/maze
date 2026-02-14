using System;
using System.Collections.Generic;

namespace _2026_Amazing
{
    public enum TileGraphic { Empty, Floor, Ceiling, Special }
    public enum WallType { None, Solid, Door, Window }

    public class Maze
    {
        internal readonly TileGraphic[,] tiles;
        internal readonly WallType[,] nsWalls;
        internal readonly WallType[,] ewWalls;
        public int Width { get; }
        public int Height { get; }

        public Maze(int width, int height)
        {
            Width = width; Height = height;
            tiles = new TileGraphic[width, height];
            nsWalls = new WallType[width + 1, height];
            ewWalls = new WallType[width, height + 1];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tiles[x, y] = TileGraphic.Special; // Default all as walls

            for (int x = 0; x <= width; x++)
                for (int y = 0; y < height; y++) nsWalls[x, y] = WallType.Solid;
            for (int x = 0; x < width; x++)
                for (int y = 0; y <= height; y++) ewWalls[x, y] = WallType.Solid;
        }

        public List<(int x, int y)> GetUnvisitedNeighbors(int x, int y, bool[,] visited)
        {
            var neighbors = new List<(int, int)>();
            if (x > 0 && !visited[x - 1, y]) neighbors.Add((x - 1, y));
            if (x < Width - 1 && !visited[x + 1, y]) neighbors.Add((x + 1, y));
            if (y > 0 && !visited[x, y - 1]) neighbors.Add((x, y - 1));
            if (y < Height - 1 && !visited[x, y + 1]) neighbors.Add((x, y + 1));
            return neighbors;
        }

        public void RemoveWallBetween(int x1, int y1, int x2, int y2)
        {
            tiles[x1, y1] = TileGraphic.Floor;
            tiles[x2, y2] = TileGraphic.Floor;
            if (x1 == x2) ewWalls[x1, Math.Max(y1, y2)] = WallType.None;
            else if (y1 == y2) nsWalls[Math.Max(x1, x2), y1] = WallType.None;
        }
    }
}
