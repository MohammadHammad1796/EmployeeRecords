namespace EmployeeRecords.Controllers.Apis.Resources;

public class UpdatedFileResource
{
    public string Path { get; }

    public UpdatedFileResource(string path)
    {
        Path = path;
    }
}