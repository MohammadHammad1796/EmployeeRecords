namespace EmployeeRecords.Core.Helpers;

public class Query
{
    public string? SearchQuery { get; set; }

    public Ordering? Ordering { get; set; }

    public Pagination? Pagination { get; set; }

    public Query(
        Pagination? pagination = null,
        Ordering? ordering = null)
    {
        Pagination = pagination;
        Ordering = ordering;
    }
}