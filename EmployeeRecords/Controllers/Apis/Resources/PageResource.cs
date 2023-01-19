using System.ComponentModel.DataAnnotations;

namespace EmployeeRecords.Controllers.Apis.Resources;

public class PageResource
{
    [Range(1, int.MaxValue, ErrorMessage = "{0} should be at least {1}")]
    public int Number { get; set; }

    [Range(1, 100, ErrorMessage = "{0} should be between {1} and {2}")]
    public int Size { get; set; }
}