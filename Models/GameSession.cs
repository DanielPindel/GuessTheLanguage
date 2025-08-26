namespace GuessTheLanguage.Models;

public class GameSession
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int LanguageId { get; set; }
    public Language Language { get; set; }
    public string TargetSentence { get; set; }
    public bool isCompleted { get; set; }
    public List<UserGuess> Guesses { get; set; } = new List<UserGuess>();
}
