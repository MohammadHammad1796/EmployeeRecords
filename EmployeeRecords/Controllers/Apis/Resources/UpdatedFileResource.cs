namespace EmployeeRecords.Controllers.Apis.Resources;

public class UpdatedFileResource
{
    public string Path { get; }

    public double Size { get; set; }

    public UpdatedFileResource(string path, double size)
    {
        Path = path;
        Size = size;
    }
}