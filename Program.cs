using _2026_Amazing;

class Program
{
    static void Main()
    {
        var maze = new Maze(64, 64);
        MazeHelper.GenerateMaze(maze);

        using var window = new MazeRenderer(maze);
        window.Run();
    }
}
