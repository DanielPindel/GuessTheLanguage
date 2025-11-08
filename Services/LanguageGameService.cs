using System.Text.Json;
using GuessTheLanguage.Models;

namespace GuessTheLanguage.Services;

public interface ILanguageGameService
{
    GameSession GetTodaysGame();
    GuessResult CompareGuess(int guessedLanguageId, int targetLanguageId);
    List<Language> GetAllLanguages();
    bool IsGameCompleted(int gameSessionId);
    void RecordGuess(int gameSessionId, int languageId);
    int GetGuessCount(int gameSessionId);
    public List<UserGuess> GetGuesses(int gameSessionId);
}

public class LanguageGameService: ILanguageGameService
{
    private readonly List<Language> _languages;
    private readonly Dictionary<DateTime, GameSession> _gameSessions;
    private readonly Dictionary<int, List<UserGuess>> _userGuesses;

    public LanguageGameService()
    {
        _languages = LoadLanguagesFromJson();
        _gameSessions = new Dictionary<DateTime, GameSession>();
        _userGuesses = new Dictionary<int, List<UserGuess>>();
    }

    public GameSession GetTodaysGame()
    {
        var today = DateTime.Today;
        
        if (_gameSessions.TryGetValue(today, out var session))
        {
            return session;
        }

        int seed = today.Year * 10000 + today.Month * 100 + today.Day;
        var random = new Random(seed);
        var targetLanguage = _languages[random.Next(_languages.Count)];
        var targetSentence = targetLanguage.Sentences[random.Next(targetLanguage.Sentences.Length)];

        var newSession = new GameSession()
        {
            Id = seed,
            Date = today,
            LanguageId = targetLanguage.Id,
            Language = targetLanguage,
            TargetSentence = targetSentence,
            isCompleted = false
        };

        _gameSessions[today] = newSession;
        return newSession;
    }

    public GuessResult CompareGuess(int guessedLanguageId, int targetLanguageId)
    {
        var target = _languages.First(l => l.Id == targetLanguageId);
        var guess = _languages.First(l => l.Id ==  guessedLanguageId);

        return new GuessResult()
        {
            GuessedLanguage = guess,
            TargetLanguage = target,
            NameMatch = guess.Name == target.Name,
            FamilyMatch = guess.Family == target.Family,
            WritingSystemsMatch = GetMatchResult(guess.WritingSystems, target.WritingSystems),
            SpeakersComparison = GetSpeakersMatch(guess, target),
            ContinentsMatch = GetMatchResult(guess.Continents, target.Continents),
            CountriesMatch = GetMatchResult(guess.Countries, target.Countries)
        };
    }
    public List<Language> GetAllLanguages()
    {
        return _languages.OrderBy(l => l.Name).ToList();
    }
    private MatchResult GetMatchResult(string[] guess, string[] target)
    {
        guess ??= Array.Empty<string>();
        target ??= Array.Empty<string>();

        var guessSet = new HashSet<string>(guess, StringComparer.OrdinalIgnoreCase);
        var targetSet = new HashSet<string>(target, StringComparer.OrdinalIgnoreCase);

        if (guessSet.SetEquals(targetSet))
        {
            return MatchResult.FullMatch;
        }
        else if (guessSet.Overlaps(targetSet))
        {
            return MatchResult.PartialMatch;
        }
        return MatchResult.NoMatch;
    }

    private SpeakersMatch GetSpeakersMatch(Language guess, Language target)
    {
        int guessSpeakers = guess.Speakers;
        int targetSpeakers = target.Speakers;

        return new SpeakersMatch()
        {
            Match = guess.Speakers == target.Speakers ? MatchResult.FullMatch : MatchResult.NoMatch,
            Direction = guessSpeakers > targetSpeakers ? "↓" : guessSpeakers < targetSpeakers ? "↑" : ""
        };
    }
    public void RecordGuess(int gameSessionId, int languageId)
    {
        if (!_userGuesses.ContainsKey(gameSessionId))
        {
            _userGuesses[gameSessionId] = new List<UserGuess>();
        }

        var guessCount = _userGuesses[gameSessionId].Count + 1;

        _userGuesses[gameSessionId].Add(new UserGuess()
        {
            GameSessionId = gameSessionId,
            LanguageId = languageId,
            GuessNumber = guessCount
        });
    }
    public bool IsGameCompleted(int gameSessionId)
    {
        if (!_userGuesses.ContainsKey(gameSessionId))
        {
            return false;
        }

        var guesses = _userGuesses[gameSessionId];

        return guesses.Count >= 6 || guesses.Any(g => g.LanguageId == GetTodaysGame().LanguageId);
    }
    public int GetGuessCount(int gameSessionId)
    {
        return _userGuesses.ContainsKey(gameSessionId) ? _userGuesses[gameSessionId].Count : 0;
    }
    public List<UserGuess> GetGuesses(int gameSessionId)
    {
        return _userGuesses.ContainsKey(gameSessionId) ? _userGuesses[gameSessionId] : new List<UserGuess>();
    }
    private List<Language> LoadLanguagesFromJson()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "languages.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Language>>(json);
    }
}
