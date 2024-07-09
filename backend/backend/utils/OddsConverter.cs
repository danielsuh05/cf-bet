namespace backend.utils;

public class OddsConverter
{
    public static int GetAmericanOddsFromProbability(double probability)
    {
        if (probability > 0.5)
        {
            return (int)Math.Round(probability * 100.0 / (1.0 - probability / 100.0) * -1.0);
        }

        return (int)Math.Round(100.0 / probability - 100.0);
    }
}