namespace GameCast.Core.Models;

public class UsernameGenerator
{

    private static readonly string[] Adjectives = { "Funny", "Happy", "Brave", "Lazy", "Clever", "Lucky", "Curious", "Mighty", "Quick", "Silent" };
    private static readonly string[] Nouns = { "Sheep", "Lion", "Tiger", "Fox", "Eagle", "Bear", "Wolf", "Horse", "Monkey", "Rabbit" };
    
    private readonly Random _random = new Random();
    
    public string Next()
    {
        // Generate random indices for adjective, noun, and number
        string randomAdjective = Adjectives[_random.Next(Adjectives.Length)];
        string randomNoun = Nouns[_random.Next(Nouns.Length)];
        int randomNumber = _random.Next(10, 100); // Generates a number between 10 and 99

        return randomAdjective + randomNoun + randomNumber;
    }
    
}