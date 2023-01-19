namespace EmployeeRecords.Controllers.Apis.Resources;

public class CreatedFileResource : CreatedResourceMeta<Guid>
{
    public string Path { get; }

    public double Size { get; set; }

    public CreatedFileResource(string path, double size, Guid id)
        : base(id)
    {
        Path = path;
        Size = size;
    }
}