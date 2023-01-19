namespace EmployeeRecords.Core.Helpers;

public class EmployeeFileQuery : Query
{
    public int EmployeeId { get; set; }

    public EmployeeFileQuery(
        Pagination? pagination = null,
        Ordering? ordering = null)
    : base(pagination, ordering)
    {
    }
}