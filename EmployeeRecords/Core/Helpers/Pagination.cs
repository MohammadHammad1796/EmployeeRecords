namespace EmployeeRecords.Core.Helpers;

public class Pagination
{
    public int Number { get; set; }

    public int Size { get; set; }

    public Pagination(int number, int size)
    {
        Number = number;
        Size = size;
    }
}