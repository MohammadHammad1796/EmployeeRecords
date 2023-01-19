using EmployeeRecords.Controllers.Apis.Resources;
using EmployeeRecords.Core.Helpers;

namespace EmployeeRecords.Custom.Extensions;

public static class DataTableQueryResourceExtensions
{
    public static Query ToQuery(this DataTableQueryResource resource)
    {
        var pagination = new Pagination(resource.Page.Number, resource.Page.Size);
        var ordering = new Ordering(resource.Sort.By, resource.Sort.IsAscending);
        return new Query(pagination, ordering);
    }

    public static EmployeeFileQuery ToFileQuery(this DataTableQueryResource resource)
    {
        var pagination = new Pagination(resource.Page.Number, resource.Page.Size);
        var ordering = new Ordering(resource.Sort.By, resource.Sort.IsAscending);
        return new EmployeeFileQuery(pagination, ordering);
    }
}