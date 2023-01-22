namespace EmployeeRecords.Controllers.Apis.Resources;

public class CreatedFileResource : CreatedResourceMeta<Guid>
{
    public string Path { get; }

    public CreatedFileResource(string path, Guid id)
        : base(id)
    {
        Path = path;
    }
}