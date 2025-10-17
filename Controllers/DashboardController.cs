using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.BeginDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
        ViewBag.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
        return View();
    }

    // API endpoint for getting chart data
    [HttpGet]
    public IActionResult GetChartData()
    {
        // 暫時返回示例數據，之後可以連接實際資料來源
        var data = new
        {
            labels = new[] { "一月", "二月", "三月", "四月", "五月", "六月" },
            datasets = new[]
            {
                new
                {
                    label = "銷售額",
                    data = new[] { 12500, 15200, 18300, 22100, 19800, 24500 },
                    backgroundColor = "rgba(78, 115, 223, 0.2)",
                    borderColor = "rgba(78, 115, 223, 1)",
                    borderWidth = 2
                }
            }
        };

        return Json(data);
    }

    [HttpGet]
    public IActionResult GetPieChartData()
    {
        var data = new
        {
            labels = new[] { "產品A", "產品B", "產品C", "產品D", "其他" },
            datasets = new[]
            {
                new
                {
                    data = new[] { 35, 25, 20, 15, 5 },
                    backgroundColor = new[]
                    {
                        "rgba(78, 115, 223, 0.8)",
                        "rgba(28, 200, 138, 0.8)",
                        "rgba(54, 185, 204, 0.8)",
                        "rgba(246, 194, 62, 0.8)",
                        "rgba(231, 74, 59, 0.8)"
                    }
                }
            }
        };

        return Json(data);
    }

    [HttpGet]
    public IActionResult Tally(string begin, string end, string interval, string group)
    {
        // 模擬數據 - 之後可以連接真實資料庫
        // 根據不同的篩選條件調整假資料
        var random = new Random();

        // 基礎數據
        var baseRevenue = 500000;
        var actualRevenue = 567890 + random.Next(-50000, 100000);
        var revenueRate = Math.Round((double)actualRevenue / baseRevenue * 100, 1);

        var totalCustomers = 1234 + random.Next(-200, 300);
        var totalConsumers = (int)(totalCustomers * 0.8) + random.Next(-50, 50);
        var consumerRate = Math.Round((double)totalConsumers / totalCustomers * 100, 1);

        var avgPrice = actualRevenue / totalConsumers;

        // 分店名稱（固定顯示在客單價圖表的橫軸）
        string[] storeNames = new[] {
            "築崎燒串松竹店",
            "築崎鍋物太平殿",
            "築崎鍋物北屯旗艦殿",
            "築崎鍋物豐原殿"
        };

        // 根據 interval 生成不同的時間標籤（用於數據系列名稱）
        string[] timeLabels = interval switch
        {
            "day" => new[] { "今日", "昨日", "前日" },
            "week" => new[] { "本週", "上週", "上上週" },
            "month" => new[] { "本月", "上月", "上上月" },
            _ => new[] { "本週", "上週", "上上週" }
        };

        var data = new
        {
            success = true,
            message = "Success",
            targetRev = baseRevenue,
            realRev = actualRevenue,
            revRate = $"{revenueRate}%",
            revAchieve = revenueRate >= 100,
            customers = totalCustomers,
            consumers = totalConsumers,
            consumerRate = $"{consumerRate}%",
            avgUnitPrice = avgPrice,
            unitPrices = timeLabels.Select(timeName => new
            {
                name = timeName,
                values = storeNames.Select(storeName => new
                {
                    title = storeName,
                    value = random.Next(400, 800)
                }).ToArray()
            }).ToArray(),
            totalSCE = 65000 + random.Next(-5000, 10000),
            totalDCE = 28000 + random.Next(-3000, 5000),
            avgSCE = 220 + random.Next(-20, 40),
            avgDCE = 135 + random.Next(-15, 30),
            useRateSCE = $"{random.Next(85, 105)}%",
            useRateDCE = $"{random.Next(110, 135)}%",
            products = new[]
            {
                new
                {
                    name = "築崎燒串松竹店",
                    values = new[]
                    {
                        new { title = "美式咖啡", value = 350 + random.Next(-50, 100) },
                        new { title = "拿鐵咖啡", value = 420 + random.Next(-50, 100) },
                        new { title = "珍珠奶茶", value = 380 + random.Next(-50, 100) },
                        new { title = "綠茶", value = 280 + random.Next(-50, 100) },
                        new { title = "柳橙汁", value = 150 + random.Next(-30, 60) },
                        new { title = "可樂", value = 120 + random.Next(-20, 40) }
                    }
                },
                new
                {
                    name = "餐點類",
                    values = new[]
                    {
                        new { title = "法式吐司", value = 320 + random.Next(-50, 80) },
                        new { title = "炸雞套餐", value = 580 + random.Next(-80, 120) },
                        new { title = "牛肉麵", value = 450 + random.Next(-60, 100) },
                        new { title = "義大利麵", value = 390 + random.Next(-50, 90) },
                        new { title = "鐵板牛排", value = 680 + random.Next(-100, 150) },
                        new { title = "三明治", value = 230 + random.Next(-40, 60) }
                    }
                },
                new
                {
                    name = "甜點類",
                    values = new[]
                    {
                        new { title = "提拉米蘇", value = 180 + random.Next(-30, 50) },
                        new { title = "起司蛋糕", value = 200 + random.Next(-30, 60) },
                        new { title = "巧克力布朗尼", value = 150 + random.Next(-25, 45) },
                        new { title = "水果塔", value = 160 + random.Next(-25, 50) },
                        new { title = "馬卡龍", value = 120 + random.Next(-20, 40) },
                        new { title = "鬆餅", value = 140 + random.Next(-25, 45) }
                    }
                },
                new
                {
                    name = "輕食類",
                    values = new[]
                    {
                        new { title = "凱薩沙拉", value = 180 + random.Next(-30, 50) },
                        new { title = "田園沙拉", value = 160 + random.Next(-25, 45) },
                        new { title = "雞肉捲", value = 150 + random.Next(-25, 40) },
                        new { title = "吐司套餐", value = 100 + random.Next(-15, 30) }
                    }
                }
            },
            sexCounts = new[] {
                totalCustomers * 47 / 100,  // 男性約 47%
                totalCustomers * 53 / 100   // 女性約 53%
            },
            ageCounts = new[] {
                totalCustomers * 8 / 100,   // ~20歲
                totalCustomers * 32 / 100,  // 20~30歲
                totalCustomers * 28 / 100,  // 30~40歲
                totalCustomers * 18 / 100,  // 40~50歲
                totalCustomers * 10 / 100,  // 50~60歲
                totalCustomers * 4 / 100    // 60歲~
            }
        };

        return Json(data);
    }
}
