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
        Engineer engineer = await _engineers.Find(engineer => engineer._id == id).FirstOrDefaultAsync();
        return engineer;
    
    }

    public async Task AddEngineerAsync(Engineer newEngineer) 
    {
       await _engineers.InsertOneAsync(newEngineer);
        
    }

    public async Task UpdateEngineerAsync(Engineer editedEngineer, string id)
    {
        var update = Builders<Engineer>.Update.Set(e => e.name, editedEngineer.name).Set(e=>e.resumeName,editedEngineer.resumeName).Set(e=>e.role,editedEngineer.role).Set(e => e.email, editedEngineer.email).Set(e=>e.avatar, editedEngineer.avatar).Set(e=>e.skills, editedEngineer.skills).Set(e => e.status, editedEngineer.status).Set(e => e.resume,editedEngineer.resume).Set(e=>e.summary,editedEngineer.summary).Set(e=>e.experience,editedEngineer.experience).Set(e=>e.links,editedEngineer.links);
       await _engineers.UpdateOneAsync(engineer => engineer._id ==id ,update);
    }


    public async Task RemoveEngineerAsync(string id)
    {
        await _engineers.DeleteOneAsync(engineer => engineer._id == id);
    }


}
