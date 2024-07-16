namespace backend.utils;

public class MathUtils
{
    // assuming normal distribution
    public const double EloStd = 283;

    public const int CountMonteCarloSimulations = 100000;

    private static readonly Random Rand = new Random();

    public static double BoxMullerTransform(double mean, double dev)
    {
        double u1 = 1.0 - Rand.NextDouble();
        double u2 = 1.0 - Rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                               Math.Sin(2.0 * Math.PI * u2);
        double randNormal = mean + dev * randStdNormal;

        return randNormal;
    }
}