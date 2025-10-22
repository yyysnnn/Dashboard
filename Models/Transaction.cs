using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class Transaction
{
    [Key]
    public int ID { get; set; }

    [StringLength(8)]
    public string StoreID { get; set; } = string.Empty;

    public Store? Store { get; set; }

    public DateTime? Time { get; set; }

    public int NumOfCustomers { get; set; }

    public int NumOfConsumers { get; set; }

    public int Amount { get; set; }

    public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
}

public class TransactionItem
{
    [Key]
    public int ID { get; set; }

    public Transaction? Master { get; set; }

    public int MasterID { get; set; }

    [StringLength(8)]
    public string? StoreID { get; set; }

    public DateTime? Time { get; set; }

    [StringLength(128)]
    public string? ProductClass { get; set; }

    [StringLength(128)]
    public string? Product { get; set; }

    public int Qty { get; set; }
}
