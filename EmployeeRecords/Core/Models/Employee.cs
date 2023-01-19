namespace EmployeeRecords.Core.Models;

public class Employee
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Department Department { get; set; }

    public int DepartmentId { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Address { get; set; }
}