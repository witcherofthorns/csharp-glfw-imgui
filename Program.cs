public static class Program 
{
    private static Game game = null;

    private static void Main() {
        game = new Game();
        game.Run();
    }
}