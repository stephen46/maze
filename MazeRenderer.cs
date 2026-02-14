using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;

namespace _2026_Amazing
{
    public class MazeRenderer : GameWindow
    {
        const float VIEW_SIZE = 32f;

        private Maze maze;
        private Player player;

        private int vao, vbo;
        private int mazeTex, wallTex;

        private Shader rayShader;
        private Shader minimapShader;

        private int gridW, gridH;
        private byte[,] grid;

        private bool[,] visited;
        private Vector3[,] tileColors;
        private float[] instanceData;

        private float fogStart = 3f;
        private float fogEnd = 10f;

        private int minimapVao, minimapQuadVbo, minimapInstanceVbo;

        private const float MinimapMargin = 20f;

        public MazeRenderer(Maze maze)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Size = new Vector2i(1280, 720),
                Title = "Raycaster Maze",
                Vsync = VSyncMode.On
            })
        {
            this.maze = maze;
        }

        private void InitializeTileColors()
        {
            for (int y = 0; y < gridH; y++)
            {
                for (int x = 0; x < gridW; x++)
                {
                    // Walls stay red
                    if (grid[x, y] == 1)
                    {
                        tileColors[x, y] = new Vector3(0.8f, 0.2f, 0.2f);
                    }
                    else
                    {
                        // Floors are now DARK GREY instead of green
                        tileColors[x, y] = new Vector3(0.25f, 0.25f, 0.25f);
                    }
                }
            }
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0f, 0f, 0f, 1f);

            rayShader = new Shader("Raycast.vert", "Raycast.frag");
            minimapShader = new Shader("Minimap.vert", "Minimap.frag");

            grid = MazeToGrid.BuildRaycastGrid(maze);
            gridW = grid.GetLength(0);
            gridH = grid.GetLength(1);

            visited = new bool[gridW, gridH];
            tileColors = new Vector3[gridW, gridH];
            instanceData = new float[gridW * gridH * 6];

            InitializeTileColors();

            float startX = 1f, startY = 1f;
            for (int y = gridH / 2 - 2; y < gridH / 2 + 2; y++)
            {
                for (int x = gridW / 2 - 2; x < gridW / 2 + 2; x++)
                {
                    if (grid[x, y] == 0)
                    {
                        startX = x + 0.5f;
                        startY = y + 0.5f;
                        goto FOUND;
                    }
                }
            }
        FOUND:
            player = new Player(startX, startY, grid);

            byte[] pixels = new byte[gridW * gridH];
            for (int y = 0; y < gridH; y++)
                for (int x = 0; x < gridW; x++)
                    pixels[y * gridW + x] = (grid[x, y] == 1) ? (byte)255 : (byte)0;

            mazeTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, mazeTex);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
                gridW, gridH, 0, PixelFormat.Red, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            wallTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, wallTex);
            const int texSize = 64;
            byte[] wallPixels = new byte[texSize * texSize * 3];
            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    bool dark = ((x / 8) + (y / 8)) % 2 == 0;
                    byte r = dark ? (byte)120 : (byte)200;
                    byte g = dark ? (byte)40 : (byte)80;
                    byte b = dark ? (byte)40 : (byte)80;
                    int idx = (y * texSize + x) * 3;
                    wallPixels[idx] = r;
                    wallPixels[idx + 1] = g;
                    wallPixels[idx + 2] = b;
                }
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
                texSize, texSize, 0, PixelFormat.Rgb, PixelType.UnsignedByte, wallPixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            float[] quad =
            {
        -1f, -1f,   -1f, -1f,
         0.6f, -1f,   1f, -1f,
        -1f,  1f,   -1f,  1f,
         0.6f,  1f,   1f,  1f
    };

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            SetupMinimap();

            CursorState = CursorState.Grabbed;
            WindowState = WindowState.Fullscreen;
        }


        private void SetupMinimap()
        {
            minimapVao = GL.GenVertexArray();
            minimapQuadVbo = GL.GenBuffer();
            minimapInstanceVbo = GL.GenBuffer();

            GL.BindVertexArray(minimapVao);

            float[] quad =
            {
                0,0,
                1,0,
                1,1,
                0,1
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, minimapQuadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, minimapInstanceVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, gridW * gridH * 6 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            int stride = 6 * sizeof(float);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribDivisor(1, 1);

            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribDivisor(2, 1);

            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribDivisor(3, 1);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            var input = KeyboardState;
            if (input.IsKeyDown(Keys.Escape))
                Close();

            float walk = 0.05f;

            var mouse = MouseState;
            player.Rotate(mouse.Delta.X * 0.002f);

            if (input.IsKeyDown(Keys.Up)) player.MoveForward(walk);
            if (input.IsKeyDown(Keys.Down)) player.MoveForward(-walk);
            if (input.IsKeyDown(Keys.Left)) player.Rotate(-0.04f);
            if (input.IsKeyDown(Keys.Right)) player.Rotate(0.04f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawRaycaster();
            DrawMinimap();

            SwapBuffers();
        }

        private void DrawRaycaster()
        {
            rayShader.Use();

            GL.Uniform2(GL.GetUniformLocation(rayShader.Handle, "pPos"), player.X, player.Y);
            GL.Uniform1(GL.GetUniformLocation(rayShader.Handle, "pAng"), player.Angle);
            GL.Uniform2(GL.GetUniformLocation(rayShader.Handle, "mSize"), (float)gridW, (float)gridH);

            GL.Uniform3(GL.GetUniformLocation(rayShader.Handle, "fogColor"), 0.05f, 0.05f, 0.08f);
            GL.Uniform1(GL.GetUniformLocation(rayShader.Handle, "fogStart"), fogStart);
            GL.Uniform1(GL.GetUniformLocation(rayShader.Handle, "fogEnd"), fogEnd);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, mazeTex);
            GL.Uniform1(GL.GetUniformLocation(rayShader.Handle, "mTex"), 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, wallTex);
            GL.Uniform1(GL.GetUniformLocation(rayShader.Handle, "wallTex"), 1);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        private void UpdateMinimapData()
        {
            int index = 0;

            for (int y = 0; y < gridH; y++)
            {
                for (int x = 0; x < gridW; x++)
                {
                    float dx = x - player.X;
                    float dy = y - player.Y;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);

                    if (dist < fogEnd)
                        visited[x, y] = true;

                    instanceData[index++] = x;
                    instanceData[index++] = y;

                    instanceData[index++] = tileColors[x, y].X;
                    instanceData[index++] = tileColors[x, y].Y;
                    instanceData[index++] = tileColors[x, y].Z;

                    instanceData[index++] = visited[x, y] ? 1f : 0f;
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, minimapInstanceVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, instanceData.Length * sizeof(float),
                          instanceData, BufferUsageHint.DynamicDraw);
        }

        private void DrawMinimap()
        {
            UpdateMinimapData();

            minimapShader.Use();

            float panelX = Size.X * 0.80f;
            float panelWidth = Size.X * 0.20f;

            float minimapSize = panelWidth - MinimapMargin * 2f;

            float originX = panelX + MinimapMargin;
            float originY = MinimapMargin;

            float scale = minimapSize / VIEW_SIZE;

            float centerX = originX + minimapSize * 0.5f;
            float centerY = originY + minimapSize * 0.5f;

            // Correct rotation: minimap rotates opposite the player
            float angle = MathF.PI * 0.5f - player.Angle;

            Vector2 row0 = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 row1 = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));

            GL.Uniform2(GL.GetUniformLocation(minimapShader.Handle, "rotRow0"), row0);
            GL.Uniform2(GL.GetUniformLocation(minimapShader.Handle, "rotRow1"), row1);

            GL.Uniform2(GL.GetUniformLocation(minimapShader.Handle, "playerPos"),
                        new Vector2(player.X, player.Y));

            GL.Uniform1(GL.GetUniformLocation(minimapShader.Handle, "scale"), scale);
            GL.Uniform2(GL.GetUniformLocation(minimapShader.Handle, "screenSize"),
                        new Vector2(Size.X, Size.Y));

            GL.Uniform2(GL.GetUniformLocation(minimapShader.Handle, "mapCenter"),
                        new Vector2(centerX, centerY));

            GL.Uniform1(GL.GetUniformLocation(minimapShader.Handle, "fogStart"), fogStart);
            GL.Uniform1(GL.GetUniformLocation(minimapShader.Handle, "fogEnd"), fogEnd);
            GL.Uniform3(GL.GetUniformLocation(minimapShader.Handle, "fogColor"),
                        new Vector3(0.05f, 0.05f, 0.05f));

            int scissorX = (int)originX;
            int scissorY = (int)originY;
            int scissorW = (int)minimapSize;
            int scissorH = (int)minimapSize;

            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(scissorX, scissorY, scissorW, scissorH);

            GL.BindVertexArray(minimapVao);
            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, gridW * gridH);

            GL.Disable(EnableCap.ScissorTest);
        }

    }
}