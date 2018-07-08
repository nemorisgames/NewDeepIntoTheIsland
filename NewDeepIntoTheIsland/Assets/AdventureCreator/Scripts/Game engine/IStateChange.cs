namespace AC
{

	/**
	 * An interface that is called when the gameState variable is changed.
	 */
	public interface IStateChange
	{
		
		void OnStateChange (GameState newGameState);

	}

}