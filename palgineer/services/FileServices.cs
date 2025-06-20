namespace palgineer.services
{
    public class FileServices
    {
        private readonly string _path="/uploads";

        public async Task<string> saveFileAsync(IFormFile file) { 
        
        if(file==null || file.Length==0) return null ;

        var fileName=Guid.NewGuid() +Path.GetExtension(file.FileName);
            var directory = Path.Combine(Directory.GetCurrentDirectory(), _path);

            if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

            var path = Path.Combine(directory, fileName);

            using (var stream = new FileStream(path, FileMode.Create)) { 
            
                await file.CopyToAsync(stream);
            
            }
            return Path.Combine(_path, fileName).Replace("\\", "/");
        }

    }
}
