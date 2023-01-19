namespace EmployeeRecords.Controllers.Apis.Resources;

public class ItemsWithTotalResource<TResource> where TResource : class
{
    public int Total { get; }

    public IEnumerable<TResource> Items { get; }

    public ItemsWithTotalResource(int total, IEnumerable<TResource> resources)
    {
        Total = total;
        Items = resources;
    }
}