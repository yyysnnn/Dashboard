using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class Revenue
{
    [Key]
    public int ID { get; set; }

    [StringLength(8)]
    public string StoreID { get; set; } = string.Empty;

    public Store? Store { get; set; }

    public int Amount { get; set; }
}
