using System;

namespace _2026_Amazing
{
    public static class MazeToGrid
    {
        // Produces a grid where:
        // 0 = floor
        // 1 = wall
        public static byte[,] BuildRaycastGrid(Maze maze)
        {
            int W = maze.Width;
            int H = maze.Height;

            // Raycast grid is (2*W+1) x (2*H+1)
            int GW = W * 2 + 1;
            int GH = H * 2 + 1;

            byte[,] grid = new byte[GW, GH];

            // Fill everything as wall
            for (int y = 0; y < GH; y++)
                for (int x = 0; x < GW; x++)
                    grid[x, y] = 1;

            // Carve rooms + passages
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int gx = x * 2 + 1;
                    int gy = y * 2 + 1;

                    // Room
                    grid[gx, gy] = 0;

                    // East passage
                    if (maze.nsWalls[x + 1, y] == WallType.None)
                        grid[gx + 1, gy] = 0;

                    // South passage
                    if (maze.ewWalls[x, y + 1] == WallType.None)
                        grid[gx, gy + 1] = 0;
                }
            }

            return grid;
        }
    }
}
