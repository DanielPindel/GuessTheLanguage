namespace GuessTheLanguage.Models;

public class Language
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Family { get; set; }
    public string[] WritingSystems { get; set; }
    public int Speakers { get; set; }
    public string[] NativeCountries { get; set; }
    public string[] Sentences { get; set; }
}
