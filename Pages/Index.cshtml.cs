using GuessTheLanguage.Models;
using GuessTheLanguage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuessTheLanguage.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ILanguageGameService _gameService;

    public IndexModel(ILogger<IndexModel> logger, ILanguageGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    public GameSession CurrentGame {  get; set; }
    public string TargetSentence { get; set; }
    public List<Language> Languages { get; set; }
    public bool GameCompleted { get; set; }
    public Language TargetLanguage { get; set; }
    public List<UserGuess> PreviousGuesses { get; set; } = new List<UserGuess>();

    public void OnGet()
    {
        CurrentGame = _gameService.GetTodaysGame();
        TargetSentence = CurrentGame.TargetSentence;
        Languages = _gameService.GetAllLanguages();
        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);
        TargetLanguage = CurrentGame.Language;
        var PreviousGuesses = _gameService.GetGuesses(CurrentGame.Id);
        foreach (var guess in PreviousGuesses)
        {
            var result = _gameService.CompareGuess(guess.LanguageId, CurrentGame.LanguageId);
            result.GuessNumber = guess.GuessNumber;
            result.GameCompleted = GameCompleted;
            PreviousGuesses.Add(guess);
        }
    }
    public IActionResult OnPostGuess(int languageId)
    {
        CurrentGame = _gameService.GetTodaysGame();

        var result = _gameService.CompareGuess(CurrentGame.LanguageId, languageId);
        _gameService.RecordGuess(CurrentGame.Id, languageId);

        result.GuessNumber = _gameService.GetGuessCount(CurrentGame.Id);
        result.GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);

        return Partial("_GamePartial", result);
    }
}
