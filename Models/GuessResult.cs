namespace GuessTheLanguage.Models;

public class GuessResult
{
    public bool NameMatch { get; set; }
    public bool FamilyMatch { get; set; }
    public MatchResult WritingSystemsMatch { get; set; }
    public MatchResult SpeakersComparison { get; set; }
    public MatchResult NativeCountriesMatch { get; set; }
}
