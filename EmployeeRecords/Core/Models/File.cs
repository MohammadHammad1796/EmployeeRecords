namespace EmployeeRecords.Core.Models;

public class File
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public double Size { get; set; }

    public string Path { get; set; }

    public int EmployeeId { get; set; }
}