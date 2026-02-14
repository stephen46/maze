using System;

namespace _2026_Amazing
{
    public class Player
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Angle { get; private set; }

        private const float Radius = 0.2f;
        private readonly byte[,] grid;

        public Player(float x, float y, byte[,] grid)
        {
            X = x;
            Y = y;
            Angle = 0f;
            this.grid = grid;
        }


        public void Rotate(float delta)
        {
            Angle += delta;
        }

        public void MoveForward(float step)
        {
            float dx = (float)Math.Cos(Angle) * step;
            float dy = (float)Math.Sin(Angle) * step;

            if (!IsWall(X + dx, Y))
                X += dx;

            if (!IsWall(X, Y + dy))
                Y += dy;
        }

        private bool IsWall(float nx, float ny)
        {
            float[] xs = { nx - Radius, nx + Radius };
            float[] ys = { ny - Radius, ny + Radius };

            foreach (float cx in xs)
            {
                foreach (float cy in ys)
                {
                    int gx = (int)Math.Floor(cx);
                    int gy = (int)Math.Floor(cy);

                    if (gx < 0 || gy < 0 || gx >= grid.GetLength(0) || gy >= grid.GetLength(1))
                        return true;

                    if (grid[gx, gy] == 1)
                        return true;
                }
            }

            return false;
        }
    }
}
