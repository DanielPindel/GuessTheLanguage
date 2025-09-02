namespace GuessTheLanguage.Models;

public class GuessResult
{
    public bool NameMatch { get; set; }
    public bool FamilyMatch { get; set; }
    public MatchResult WritingSystemsMatch { get; set; }
    public SpeakersMatch SpeakersComparison { get; set; }
    public MatchResult NativeCountriesMatch { get; set; }
}

public class SpeakersMatch
{
    public MatchResult Match { get; set; }
    public string Direction { get; set; } = "";
}