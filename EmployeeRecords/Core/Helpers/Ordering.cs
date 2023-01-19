namespace EmployeeRecords.Core.Helpers;

public class Ordering
{
    public string By { get; set; }

    public bool IsAscending { get; set; }

    public Ordering(string by, bool isAscending)
    {
        By = by;
        IsAscending = isAscending;
    }
}