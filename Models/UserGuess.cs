namespace GuessTheLanguage.Models;

public class UserGuess
{
    public int Id { get; set; }
    public int GameSessionId { get; set; }
    public GameSession GameSession { get; set; }
    public int LanguageId { get; set; }
    public Language Language { get; set; }
    public int GuessNumber { get; set; }
}
