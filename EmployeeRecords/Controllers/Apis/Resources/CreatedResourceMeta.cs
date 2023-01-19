namespace EmployeeRecords.Controllers.Apis.Resources;

public class CreatedResourceMeta<TKey>
{
    public TKey Id { get; }

    public CreatedResourceMeta(TKey id) => Id = id;
}