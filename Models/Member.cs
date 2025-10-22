using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class Member
{
    [Key]
    [StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(64)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1)]
    public string Sex { get; set; } = string.Empty;

    public DateTime BirthDay { get; set; }
}
