namespace GoDeep
{
	class Program
	{
		static DogGame game;

		static void Main (string[] args)
		{
		    game = new DogGame ();
            game.Content.RootDirectory = "Resources\\Content";
		    DogGame.AudioExtension = ".wav";
		    game.Run();
		}
	}
}