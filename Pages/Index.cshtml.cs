using GuessTheLanguage.Models;
using GuessTheLanguage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuessTheLanguage.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ILanguageGameService _gameService;
    private const string SessionKey = "LanguageGameState";

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
    public List<GuessResult> PreviousGuesses = new List<GuessResult>();
    public bool IsCorrectGuess { get; set; }

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
    public IActionResult OnPostGuess(int selectedLanguageId)
    {
        CurrentGame = _gameService.GetTodaysGame();
        Languages = _gameService.GetAllLanguages();
        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);
        TargetLanguage = CurrentGame.Language;
        TargetSentence = CurrentGame.TargetSentence;
        PreviousGuesses = _gameService.GetGuesses(CurrentGame.Id)
            .Select(g =>
            {
                var result = _gameService.CompareGuess(g.LanguageId, CurrentGame.LanguageId);
                result.GuessNumber = g.GuessNumber;
                return result;
            }).ToList();

        if (IsCorrectGuess || GameCompleted)
        {
            return Partial("_GamePartial", this);
        }

        if (selectedLanguageId > 0)
        {
            var guessedLanguage = Languages.FirstOrDefault(l => l.Id == selectedLanguageId);
            if (guessedLanguage != null)
            {
                try
                {
                    var result = _gameService.CompareGuess(selectedLanguageId, CurrentGame.LanguageId);
                    _gameService.RecordGuess(CurrentGame.Id, selectedLanguageId);

                    PreviousGuesses = _gameService.GetGuesses(CurrentGame.Id)
                        .Select(g =>
                        {
                            var r = _gameService.CompareGuess(g.LanguageId, CurrentGame.LanguageId);
                            r.GuessNumber = g.GuessNumber;
                            return r;
                        }).ToList();

                    GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id);
                }
                catch (ArgumentNullException ex)
                {
                    ModelState.AddModelError("", $"Error processing guess: {ex.Message}");
                }
            }
            else
            {
                ModelState.AddModelError("", "Language not found");
            }
        }
        else
        {
            ModelState.AddModelError("", "Please select a valid language from the list");
        }

        return Partial("_GamePartial", this);
    }
    public string GetCellClass(bool match)
    {
        return match ? "bg-success text-white" : "bg-danger text-white";
    }
    public string GetCellClass(MatchResult match)
    {
        return match switch
        {
            MatchResult.FullMatch => "bg-success text-white",
            MatchResult.PartialMatch => "bg-warning",
            _ => "bg-danger text-white"
        };
    }
}