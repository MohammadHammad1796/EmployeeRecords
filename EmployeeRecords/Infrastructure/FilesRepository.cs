using EmployeeRecords.Core.Repositories;
using File = System.IO.File;

namespace EmployeeRecords.Infrastructure;

public class FilesRepository : IFilesRepository
{
    private readonly string _rootPath;

    public FilesRepository(IWebHostEnvironment host)
    {
        _rootPath = host.WebRootPath;
    }

    public async Task<string> SaveAsync(IFormFile file, string path)
    {
        var fullPath = Path.Combine(_rootPath, path);
        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var fullFilePath = Path.Combine(fullPath, fileName);

        var stream = new FileStream(fullFilePath, FileMode.Create);
        await using var _ = stream.ConfigureAwait(false);
        await file.CopyToAsync(stream);

        var fileNameWithPath = Path.Combine(path, fileName);
        return fileNameWithPath;
    }

    public void Delete(string fileNameWithPath)
    {
        var fullFilePath = Path.Combine(_rootPath, fileNameWithPath);
        File.Delete(fullFilePath);
    }
}