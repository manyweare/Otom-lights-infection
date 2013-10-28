public static class GameEventManager
{
	public delegate void GameEvent();
	
	public static event GameEvent NextTurn;
	
	public static void TriggerNextTurn()
	{
		if (NextTurn != null)
		{
			NextTurn();
		}
	}
}