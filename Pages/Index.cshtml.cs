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
    public string TargetLanguage { get; set; }

    public void OnGet()
    {
        CurrentGame = _gameService.GetTodaysGame();
        TargetSentence = CurrentGame.TargetSentence;
        Languages = _gameService.GetAllLanguages();
        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);
        TargetLanguage = CurrentGame.Language.Name;
    }
    public IActionResult OnPostGuess(int languageId)
    {
        CurrentGame = _gameService.GetTodaysGame();

        if (_gameService.IsGameCompleted(CurrentGame.Id))
        {
            return Content("<div class='error'>Game completed</div>");
        }
        var result = _gameService.CompareGuess(CurrentGame.LanguageId, languageId);
        _gameService.RecordGuess(CurrentGame.Id, languageId);

        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);

        return Partial("_GuessResult", result);
    }
}
