namespace backend.services;

public class UpdateService(MongoDBContext context)
{
    public void CheckContests()
    {
        Console.WriteLine(context.ToString());
    }
}