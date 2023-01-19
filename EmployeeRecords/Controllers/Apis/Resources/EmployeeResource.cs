using EmployeeRecords.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class EmployeeResource
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Department Department { get; set; }

    [Display(Name = "Date of birth")]
    public DateOnly DateOfBirth { get; set; }

    public string Address { get; set; }
}