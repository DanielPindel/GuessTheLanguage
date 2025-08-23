namespace GuessTheLanguage.Models;

public class GameSession
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int LanguageId { get; set; }
    public Language Language { get; set; }
}
