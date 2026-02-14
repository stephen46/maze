using System;
using System.Collections.Generic;
using System.Text;

namespace _2026_Amazing
{
    public static class MazeHelper
    {
        public static void GenerateMaze(Maze maze)
        {
            var visited = new bool[maze.Width, maze.Height];
            Stack<(int x, int y)> stack = new Stack<(int, int)>();
            Random rng = new Random();

            // Start at (0,0)
            stack.Push((0, 0));
            visited[0, 0] = true;

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Peek();
                var neighbors = maze.GetUnvisitedNeighbors(cx, cy, visited);

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                }
                else
                {
                    var (nx, ny) = neighbors[rng.Next(neighbors.Count)];
                    maze.RemoveWallBetween(cx, cy, nx, ny);
                    visited[nx, ny] = true;
                    stack.Push((nx, ny));
                }
            }
        }

        public static string RenderAscii(Maze maze)
        {
            var sb = new StringBuilder();

            for (int y = 0; y < maze.Height; y++)
            {
                // Top row of E-W walls
                for (int x = 0; x < maze.Width; x++)
                {
                    sb.Append(maze.ewWalls[x, y] == WallType.Solid ? "-" :
                              maze.ewWalls[x, y] == WallType.Door ? "D" :
                              maze.ewWalls[x, y] == WallType.Window ? "W" : ".");
                }
                sb.AppendLine();

                // Tiles + N-S walls
                for (int x = 0; x < maze.Width; x++)
                {
                    char tileChar = maze.tiles[x, y] switch
                    {
                        TileGraphic.Empty => ' ',
                        TileGraphic.Floor => '.',
                        TileGraphic.Ceiling => '^',
                        TileGraphic.Special => '*',
                        _ => '?'
                    };

                    sb.Append(tileChar);

                    sb.Append(maze.nsWalls[x + 1, y] switch
                    {
                        WallType.Solid => "|",
                        WallType.Door => "D",
                        WallType.Window => "W",
                        _ => " "
                    });
                }
                sb.AppendLine();
            }

            // Bottom row of E-W walls
            for (int x = 0; x < maze.Width; x++)
            {
                sb.Append(maze.ewWalls[x, maze.Height] == WallType.Solid ? "A" :
                          maze.ewWalls[x, maze.Height] == WallType.Door ? "D" :
                          maze.ewWalls[x, maze.Height] == WallType.Window ? "W" : ".");
            }
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
