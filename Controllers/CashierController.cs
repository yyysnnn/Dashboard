using Microsoft.AspNetCore.Mvc;
using Dashboard.Models;
using System.Text;

namespace Dashboard.Controllers;

public class CashierController : Controller
{
    private readonly ILogger<CashierController> _logger;
    private readonly ZuchiDB _db;
    private readonly IWebHostEnvironment _env;

    public CashierController(ILogger<CashierController> logger, ZuchiDB db, IWebHostEnvironment env)
    {
        _logger = logger;
        _db = db;
        _env = env;
    }

    private void SaveTransaction(JsoCashierData? data)
    {
        if (data == null) return;
        if (data.store == null) return;
        if (data.order == null) return;

        var store = _db.Stores.Where(x => x.Name == data.store.name).FirstOrDefault();
        if (store == null)
        {
            _logger.LogWarning($"找不到店舖: {data.store.name}");
            return;
        }

        var t = new Transaction();
        t.StoreID = store.ID;
        if (DateTime.TryParse(data.order.time, out DateTime time)) t.Time = time;
        t.Amount = Convert.ToInt32(data.order.total);

        // 儲存交易項目
        var items = new List<TransactionItem>();
        foreach (var item in data.order.items)
        {
            var ti = new TransactionItem();
            ti.Master = t;
            ti.StoreID = t.StoreID;
            ti.Time = t.Time;

            if (!string.IsNullOrEmpty(item.name))
            {
                string[] parts = item.name.Split('-');
                if (parts.Length >= 2)
                {
                    ti.ProductClass = parts[0].Trim();
                    ti.Product = parts[1].Trim();
                }
                else
                {
                    ti.ProductClass = "";
                    ti.Product = item.name;
                }
            }

            ti.Qty = item.quantity;

            items.Add(ti);
        }

        // 計算來客數和消費人數
        if (store.Brand == "A")
        {
            foreach (var item in items)
            {
                if (item.Product != null && (item.Product.Contains("平日午餐") || item.Product.Contains("晚餐/假日")))
                {
                    t.NumOfCustomers++;
                    t.NumOfConsumers++;
                }
                else if (item.Product == "兒童299")
                {
                    t.NumOfCustomers++;
                }
            }
        }
        else if (store.Brand == "B")
        {
            foreach (var item in items)
            {
                if (item.Product != null && item.Product.Contains("湯底"))
                {
                    t.NumOfCustomers++;
                    t.NumOfConsumers++;
                }
                else if (item.Product != null && (item.Product.Contains("兒童") || item.Product.Contains("幼童")))
                {
                    t.NumOfCustomers++;
                }
                else if (item.Product != null && item.Product.Contains("【雙人】"))
                {
                    t.NumOfCustomers += 2;
                    t.NumOfConsumers += 2;
                }
                else if (item.Product != null && item.Product.Contains("【三人】"))
                {
                    t.NumOfCustomers += 3;
                    t.NumOfConsumers += 3;
                }
            }
        }

        _db.Transactions.Add(t);

        // 儲存交易項目
        foreach (var item in items)
        {
            _db.TransactionItems.Add(item);
        }

        _db.SaveChanges();
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromBody] JsoCashierRecord rec)
    {
        try
        {
            // 啟用 Request Body 重複讀取
            Request.EnableBuffering();

            // 讀取原始 JSON 資料
            Request.Body.Position = 0;
            using StreamReader reader = new StreamReader(Request.Body, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            // 儲存 JSON 檔案
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    string cashierDir = Path.Combine(_env.ContentRootPath, "App_Data", "Cashier");

                    // 確保目錄存在
                    if (!Directory.Exists(cashierDir))
                    {
                        Directory.CreateDirectory(cashierDir);
                    }

                    string fileName = rec.data?.order?.time?.Substring(0, 19).Replace(":", "") + ".json";
                    string filePath = Path.Combine(cashierDir, fileName);

                    await System.IO.File.WriteAllTextAsync(filePath, body, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "儲存 JSON 檔案失敗，使用時間戳記");

                    string cashierDir = Path.Combine(_env.ContentRootPath, "App_Data", "Cashier");
                    if (!Directory.Exists(cashierDir))
                    {
                        Directory.CreateDirectory(cashierDir);
                    }

                    string fileName = DateTime.Now.ToString("_yyyyMMddHHmmss") + ".json";
                    string filePath = Path.Combine(cashierDir, fileName);
                    await System.IO.File.WriteAllTextAsync(filePath, body, Encoding.UTF8);
                }
            }

            // 儲存交易資料到資料庫
            if (rec != null && rec.data != null)
            {
                SaveTransaction(rec.data);
                _logger.LogInformation($"成功接收並儲存交易資料: {rec.data.store?.name} - {rec.data.order?.time}");
            }

            return StatusCode(200, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理 Cashier 資料時發生錯誤");
            return StatusCode(500, "Internal Server Error");
        }
    }

    public IActionResult Log()
    {
        var transactions = _db.Transactions
            .OrderByDescending(t => t.Time)
            .Take(100)
            .ToList();

        return View(transactions);
    }
}

// JSON 資料模型
public class JsoStore
{
    public string name { get; set; } = string.Empty;
    public string uuid { get; set; } = string.Empty;
    public string no { get; set; } = string.Empty;
}

public class JsoOrderItemOption
{
    public string sku { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public double price { get; set; }
}

public class JsoOrderItem
{
    public string uuid { get; set; } = string.Empty;
    public string sku { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public int quantity { get; set; }
    public double price { get; set; }
    public List<JsoOrderItemOption> options { get; set; } = new List<JsoOrderItemOption>();
}

public class JsoOrder
{
    public string time { get; set; } = string.Empty;
    public string serveType { get; set; } = string.Empty;
    public string uuid { get; set; } = string.Empty;
    public string no { get; set; } = string.Empty;
    public double subtotal { get; set; }
    public double discount { get; set; }
    public double tax { get; set; }
    public double total { get; set; }
    public List<JsoOrderItem> items { get; set; } = new List<JsoOrderItem>();
}

public class JsoCashierData
{
    public JsoStore? store { get; set; }
    public JsoOrder? order { get; set; }
}

public class JsoCashierRecord
{
    public string code { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public JsoCashierData? data { get; set; }
}
