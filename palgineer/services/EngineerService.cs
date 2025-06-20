namespace palgineer.services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using palgineer.models2;

public class EngineerService
{
    private readonly IMongoCollection<Engineer> _engineers;
    public EngineerService(IOptions<MongoDBSettings> mongoSettings )  {
        var settings = mongoSettings.Value;
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _engineers = database.GetCollection<Engineer>(settings.EngineerCollectionName);



    }

    public async Task<List<Engineer>> GetAllAsync()
    {
        return await _engineers.Find(engineer => true).ToListAsync();
    }

    public async Task<Engineer?> GetByEmail(string email) {
        Engineer newEngineer = await _engineers.Find(eng=>eng.email==email).FirstOrDefaultAsync();
        return newEngineer;
    }

    public async Task<Engineer> GetByIdAsync(string id) { 
        Engineer engineer = await _engineers.Find(engineer => engineer.Id == id).FirstOrDefaultAsync();
        return engineer;
    
    }

    public async Task AddEngineerAsync(Engineer newEngineer) 
    {
       await _engineers.InsertOneAsync(newEngineer);

    }

    public async Task UpdateEngineerAsync(Engineer editedEngineer, string id)
    {
        await _engineers.ReplaceOneAsync(engineer => engineer.Id == id, editedEngineer);
    }


    public async Task RemoveEngineerAsync(string id)
    {
        await _engineers.DeleteOneAsync(engineer => engineer.Id == id);
    }


}
