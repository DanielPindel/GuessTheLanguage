namespace GuessTheLanguage.Models;

public class GameState
{
    public int CurrentGameSessionId { get; set; }
    public int GuessCount { get; set; }
    public bool GameCompleted { get; set; }
    public bool GameWon {  get; set; }
}
