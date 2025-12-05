using GuessTheLanguage.Models;
using GuessTheLanguage.Services;
using GuessTheLanguage.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GuessTheLanguage.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    protected readonly ILanguageGameService _gameService;
    private const string SessionKey = "DailyGameState";

    public IndexModel(ILogger<IndexModel> logger, ILanguageGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    public GameSession CurrentGame {  get; set; }
    public Language TargetLanguage { get; set; }
    public string TargetSentence { get; set; }
    public List<Language> Languages { get; set; }
    public bool GameCompleted { get; set; }
    public List<GuessResult> PreviousGuesses = new List<GuessResult>();
    public bool IsCorrectGuess { get; set; }
    public bool HasGivenUp { get; set; }

    public void OnGet()
    {
        LoadGameState();

        CurrentGame = _gameService.GetTodaysGame();
        TargetSentence = CurrentGame.TargetSentence;
        Languages = _gameService.GetAllLanguages();
        TargetLanguage = CurrentGame.Language;

        if (PreviousGuesses.Count == 0)
        {
            var serviceGuesses = _gameService.GetGuesses(CurrentGame.Id);
            foreach (var guess in serviceGuesses)
            {
                var result = _gameService.CompareGuess(guess.LanguageId, CurrentGame.LanguageId);
                result.GuessNumber = guess.GuessNumber;
                result.GameCompleted = false;
                PreviousGuesses.Add(result);
            }
        }

        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id) || PreviousGuesses.Count >= 6 || IsCorrectGuess;

        foreach (var guess in PreviousGuesses)
        {
            guess.GameCompleted = GameCompleted;
        }

        SaveGameState();
    }
    public IActionResult OnPost([FromForm] int selectedLanguageId)
    {
        LoadGameState();

        CurrentGame = _gameService.GetTodaysGame();
        Languages = _gameService.GetAllLanguages();

        GameCompleted = _gameService.IsGameCompleted(CurrentGame.Id) || PreviousGuesses.Count >= 6 || IsCorrectGuess;

        if (GameCompleted)
        {
            SaveGameState();
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
                    result.GuessNumber = PreviousGuesses.Count + 1;
                    result.GameCompleted = false;

                    PreviousGuesses.Add(result);

                    _gameService.RecordGuess(CurrentGame.Id, selectedLanguageId);

                    IsCorrectGuess = result.NameMatch;

                    GameCompleted = IsCorrectGuess || PreviousGuesses.Count >= 6;

                    foreach (var guess in PreviousGuesses)
                    {
                        guess.GameCompleted = GameCompleted;
                    }

                    SaveGameState();
                }
                catch (ArgumentNullException ex)
                {
                    _logger.LogError(ex, "Error processing guess");
                    ModelState.AddModelError("", "Error processing your guess. Please try again.");
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
    private void SaveGameState()
    {
        try
        {
            var state = new GameState
            {
                TargetLanguage = TargetLanguage,
                TargetSentence = TargetSentence,
                Guesses = PreviousGuesses ?? new List<GuessResult>(),
                IsCorrectGuess = IsCorrectGuess,
                LastPlayDate = DateTime.Today
            };

            HttpContext.Session.Set(SessionKey, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving game state");
        }
    }
    private void LoadGameState()
    {
        try
        {
            var state = HttpContext.Session.Get<GameState>(SessionKey);
            var today = DateTime.Today;

            if (state == null || state.LastPlayDate < today)
            {
                ResetGameState();
                return;
            }

            TargetLanguage = state.TargetLanguage;
            TargetSentence = state.TargetSentence;
            PreviousGuesses = state.Guesses ?? new List<GuessResult>();
            IsCorrectGuess = state.IsCorrectGuess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading game state");
            ResetGameState();
        }
    }
    private void ResetGameState()
    {
        CurrentGame = _gameService.GetTodaysGame();
        TargetLanguage = CurrentGame.Language;
        TargetSentence = CurrentGame.TargetSentence;
        PreviousGuesses = new List<GuessResult>();
        IsCorrectGuess = false;
        GameCompleted = false;

        SaveGameState();
    }
    public IActionResult OnPostGiveUp()
    {
        LoadGameState();
        if (!IsCorrectGuess)
        {
            HasGivenUp = true;
            IsCorrectGuess = true;
            var _ = TargetLanguage;
            SaveGameState();
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
    public string FormatSpeakers(int speakers)
    {
        return speakers.ToString("N0").Replace(",", " ");
    }
}

public class GameState
{
    public Language TargetLanguage { get; set; }
    public string TargetSentence { get; set; }
    public List<GuessResult> Guesses { get; set; } = new();
    public bool IsCorrectGuess { get; set; }
    public bool HasGivenUp { get; set; }
    public DateTime LastPlayDate { get; set; }
}