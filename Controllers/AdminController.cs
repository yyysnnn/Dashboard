using Microsoft.AspNetCore.Mvc;
using Dashboard.Models;

namespace Dashboard.Controllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly ZuchiDB _db;

    public AdminController(ILogger<AdminController> logger, ZuchiDB db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    // API: 取得會員總覽資料
    [HttpGet]
    public IActionResult GetMembersOverview(int page = 1, int pageSize = 10)
    {
        try
        {
            var totalMembers = _db.Members.Count();

            // 分頁取得會員資料，按加入時間排序（假設用 PhoneNumber 當作排序，實際應該要有 CreateDate 欄位）
            var members = _db.Members
                .OrderByDescending(m => m.PhoneNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    phoneNumber = m.PhoneNumber,
                    name = m.Name,
                    sex = m.Sex == "M" ? "男" : m.Sex == "F" ? "女" : "未知",
                    birthDay = m.BirthDay.ToString("yyyy-MM-dd"),
                    age = DateTime.Today.Year - m.BirthDay.Year
                })
                .ToList();

            return Json(new
            {
                success = true,
                total = totalMembers,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalMembers / pageSize),
                members = members
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得會員總覽失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // API: 取得最近交易資料
    [HttpGet]
    public IActionResult GetRecentTransactions(int count = 50, int? year = null, int? month = null, string? storeId = null)
    {
        try
        {
            var stores = _db.Stores.ToDictionary(s => s.ID, s => s.Name);

            var query = _db.Transactions.Where(t => t.Time != null);

            // 如果指定年月，則按月份篩選
            if (year.HasValue && month.HasValue)
            {
                var startDate = new DateTime(year.Value, month.Value, 1);
                var endDate = startDate.AddMonths(1);
                query = query.Where(t => t.Time >= startDate && t.Time < endDate);
            }

            // 如果指定店舖，則按店舖篩選
            if (!string.IsNullOrEmpty(storeId))
            {
                query = query.Where(t => t.StoreID == storeId);
            }

            // 如果有指定年月或店舖，則不限制筆數，否則限制筆數
            var orderedQuery = query.OrderByDescending(t => t.Time);
            var limitedQuery = (year.HasValue && month.HasValue) || !string.IsNullOrEmpty(storeId)
                ? orderedQuery
                : orderedQuery.Take(count);

            var transactions = limitedQuery
                .ToList()
                .Select(t => new
                {
                    id = t.ID,
                    time = t.Time,
                    storeID = t.StoreID,
                    storeName = t.StoreID != null && stores.ContainsKey(t.StoreID)
                        ? stores[t.StoreID]
                        : "未知店舖",
                    amount = t.Amount,
                    numOfCustomers = t.NumOfCustomers,
                    numOfConsumers = t.NumOfConsumers
                })
                .ToList();

            return Json(new
            {
                success = true,
                transactions = transactions,
                total = transactions.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得最近交易失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // API: 取得統計數據
    [HttpGet]
    public IActionResult GetStatistics(string revenueMonth = "current")
    {
        try
        {
            var totalStores = _db.Stores.Count();
            var totalMembers = _db.Members.Count();

            // 今日交易
            var today = DateTime.Today;
            var todayTransactions = _db.Transactions
                .Where(t => t.Time >= today && t.Time < today.AddDays(1))
                .Count();

            // 營收計算
            int revenue = 0;
            string revenueLabel = "";

            if (revenueMonth == "last")
            {
                // 上月營收
                var firstDayOfLastMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
                var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
                revenue = _db.Transactions
                    .Where(t => t.Time >= firstDayOfLastMonth && t.Time < firstDayOfThisMonth)
                    .Sum(t => (int?)t.Amount) ?? 0;
                revenueLabel = "上月營收";
            }
            else
            {
                // 本月營收
                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                revenue = _db.Transactions
                    .Where(t => t.Time >= firstDayOfMonth && t.Time < today.AddDays(1))
                    .Sum(t => (int?)t.Amount) ?? 0;
                revenueLabel = "本月營收";
            }

            return Json(new
            {
                success = true,
                totalStores = totalStores,
                totalMembers = totalMembers,
                todayTransactions = todayTransactions,
                revenue = revenue,
                revenueLabel = revenueLabel
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得統計數據失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // 店舖管理
    public IActionResult Stores()
    {
        return View();
    }

    // API: 取得所有店舖
    [HttpGet]
    public IActionResult GetStores()
    {
        try
        {
            var stores = _db.Stores
                .OrderBy(s => s.ID)
                .Select(s => new
                {
                    id = s.ID,
                    name = s.Name,
                    area = s.Area,
                    brand = s.Brand,
                    brandName = Store.ToBrandName(s.Brand),
                    spot = s.Spot,
                    spotName = Store.ToSpotName(s.Spot)
                })
                .ToList();

            return Json(new
            {
                success = true,
                stores = stores
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得店舖資料失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // API: 新增店舖
    [HttpPost]
    public IActionResult AddStore([FromBody] StoreAddModel model)
    {
        try
        {
            // 檢查店舖代碼是否已存在
            if (_db.Stores.Any(s => s.ID == model.Id))
            {
                return Json(new
                {
                    success = false,
                    message = "店舖代碼已存在"
                });
            }

            var store = new Store
            {
                ID = model.Id,
                Name = model.Name,
                Area = model.Area,
                Brand = model.Brand,
                Spot = model.Spot
            };

            _db.Stores.Add(store);
            _db.SaveChanges();

            // 同時為新店舖建立營收目標
            var revenue = new Revenue
            {
                StoreID = model.Id,
                Amount = 0
            };
            _db.Revenues.Add(revenue);
            _db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "新增成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增店舖失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // 交易管理
    public IActionResult Transactions()
    {
        return View();
    }

    // 會員管理
    public IActionResult Members()
    {
        return View();
    }

    // 營收目標管理
    public IActionResult Revenues()
    {
        return View();
    }

    // API: 取得所有營收目標
    [HttpGet]
    public IActionResult GetRevenues()
    {
        try
        {
            var stores = _db.Stores.ToDictionary(s => s.ID, s => s.Name);

            var revenues = _db.Revenues
                .OrderBy(r => r.StoreID)
                .ToList()
                .Select(r => new
                {
                    id = r.ID,
                    storeID = r.StoreID,
                    storeName = r.StoreID != null && stores.ContainsKey(r.StoreID)
                        ? stores[r.StoreID]
                        : "未知店舖",
                    amount = r.Amount
                })
                .ToList();

            return Json(new
            {
                success = true,
                revenues = revenues
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得營收目標失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // API: 更新營收目標
    [HttpPost]
    public IActionResult UpdateRevenue([FromBody] RevenueUpdateModel model)
    {
        try
        {
            var revenue = _db.Revenues.FirstOrDefault(r => r.ID == model.Id);
            if (revenue == null)
            {
                return Json(new
                {
                    success = false,
                    message = "找不到該營收目標"
                });
            }

            revenue.Amount = model.Amount;
            _db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "更新成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新營收目標失敗");
            return Json(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}

public class RevenueUpdateModel
{
    public int Id { get; set; }
    public int Amount { get; set; }
}

public class StoreAddModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Spot { get; set; } = string.Empty;
}
