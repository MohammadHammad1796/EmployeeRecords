namespace EmployeeRecords.Core.Helpers;

public class FileCriteria
{
    public int MaxSizeInMb { get; set; }

    public List<string> AllowedContentTypes { get; set; }

    public FileCriteria() => AllowedContentTypes = new List<string>();
}