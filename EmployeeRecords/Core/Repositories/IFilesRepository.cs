namespace EmployeeRecords.Core.Repositories;

public interface IFilesRepository
{
    Task<string> SaveAsync(IFormFile file, string path);

    void Delete(string fileNameWithPath);
}