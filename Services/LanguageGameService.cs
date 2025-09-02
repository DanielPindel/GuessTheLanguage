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
}

public class LanguageGameService: ILanguageGameService
{
    private readonly List<Language> _languages;
    private readonly Dictionary<DateTime, GameSession> _gameSessions;

    public LanguageGameService()
    {
        _languages = LoadLanguagesFromJson();
        _gameSessions = new Dictionary<DateTime, GameSession>();
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
            NameMatch = guess.Name == target.Name,
            FamilyMatch = guess.Family == target.Family,
            WritingSystemsMatch = GetMatchResult(guess.WritingSystems, target.WritingSystems),
            SpeakersComparison = GetSpeakersMatch(guess, target),
            NativeCountriesMatch = GetMatchResult(guess.NativeCountries, target.NativeCountries)
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

    }
    public bool IsGameCompleted(int gameSessionId)
    {
        return false;
    }
    private List<Language> LoadLanguagesFromJson()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "languages.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<Language>>(json);
    }
}
