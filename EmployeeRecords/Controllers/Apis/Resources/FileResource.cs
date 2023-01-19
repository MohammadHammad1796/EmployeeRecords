namespace EmployeeRecords.Controllers.Apis.Resources;

public class FileResource
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public double Size { get; set; }

    public string Path { get; set; }
}