using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class Store
{
    [Key]
    [StringLength(8)]
    public string ID { get; set; } = string.Empty;

    [StringLength(64)]
    public string Name { get; set; } = string.Empty;

    [StringLength(8)]
    public string Area { get; set; } = string.Empty;

    [StringLength(1)]
    public string Brand { get; set; } = string.Empty;

    [StringLength(1)]
    public string Spot { get; set; } = string.Empty;

    public static string ToBrandName(string brand)
    {
        return brand switch
        {
            "A" => "築崎燒串",
            "B" => "築崎鍋物",
            _ => brand
        };
    }

    public static string ToSpotName(string spot)
    {
        return spot switch
        {
            "A" => "街邊",
            "B" => "賣場",
            "C" => "學區",
            "D" => "社區",
            _ => spot
        };
    }
}
