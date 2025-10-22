using Microsoft.AspNetCore.Mvc;
using Dashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Controllers;

public class DashboardController : Controller
{
    private readonly ILogger<DashboardController> _logger;
    private readonly ZuchiDB _db;

    public DashboardController(ILogger<DashboardController> logger, ZuchiDB db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        // 預設顯示當天-3天到當天的資料
        DateTime today = DateTime.Today;
        DateTime beginDate = today.AddDays(-3);

        ViewBag.BeginDate = Utility.ToHtmlDate(beginDate);
        ViewBag.EndDate = Utility.ToHtmlDate(today);
        return View();
    }

    [HttpGet]
    public IActionResult TestConnection()
    {
        try
        {
            // 測試資料庫連線
            var canConnect = _db.Database.CanConnect();

            if (canConnect)
            {
                // 嘗試讀取一些基本資料
                var storeCount = _db.Stores.Count();
                var transactionCount = _db.Transactions.Count();
                var memberCount = _db.Members.Count();

                return Json(new
                {
                    success = true,
                    message = "資料庫連線成功！",
                    data = new
                    {
                        storeCount = storeCount,
                        transactionCount = transactionCount,
                        memberCount = memberCount,
                        connectionString = "100.124.60.109 (ZUCHI)"
                    }
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "無法連接到資料庫"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "資料庫連線測試失敗");
            return Json(new
            {
                success = false,
                message = "資料庫連線測試失敗",
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpGet]
    public IActionResult DebugUnitPrice(string begin = "2025-10-17", string end = "2025-10-20", string group = "custom")
    {
        try
        {
            DateTime beginDate = Utility.FromHtmlDate(begin);
            DateTime endDate = Utility.FromHtmlDate(end).AddDays(1);

            var transactions = _db.Transactions
                .Where(x => x.Time >= beginDate && x.Time <= endDate)
                .ToList();

            var stores = _db.Stores.ToArray();

            // 檢查 A01 的交易資料
            var a01Transactions = transactions.Where(t => t.StoreID == "A01").ToList();

            var a01Info = new
            {
                storeID = "A01",
                storeName = stores.FirstOrDefault(s => s.ID == "A01")?.Name,
                transactionCount = a01Transactions.Count,
                totalAmount = a01Transactions.Sum(t => t.Amount),
                totalConsumers = a01Transactions.Sum(t => t.NumOfConsumers),
                totalCustomers = a01Transactions.Sum(t => t.NumOfCustomers),
                avgUnitPrice = a01Transactions.Sum(t => t.NumOfConsumers) > 0
                    ? a01Transactions.Sum(t => t.Amount) / a01Transactions.Sum(t => t.NumOfConsumers)
                    : 0,
                transactions = a01Transactions.Select(t => new {
                    t.Time,
                    t.Amount,
                    t.NumOfConsumers,
                    t.NumOfCustomers
                }).Take(5).ToList()
            };

            return Json(new
            {
                success = true,
                dateRange = $"{begin} ~ {end}",
                totalTransactions = transactions.Count,
                a01Info = a01Info,
                allStores = transactions.GroupBy(t => t.StoreID).Select(g => new {
                    storeID = g.Key,
                    storeName = stores.FirstOrDefault(s => s.ID == g.Key)?.Name,
                    count = g.Count(),
                    totalAmount = g.Sum(t => t.Amount),
                    totalConsumers = g.Sum(t => t.NumOfConsumers)
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DebugUnitPrice 失敗");
            return Json(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet]
    public IActionResult DebugTally(string begin = "2025-10-01", string end = "2025-10-03")
    {
        try
        {
            DateTime beginDate = Utility.FromHtmlDate(begin);
            DateTime endDate = Utility.FromHtmlDate(end).AddDays(1);

            // 測試商品統計
            var r4 = TallyProducts(beginDate, endDate, "area");
            var productInfo = new
            {
                groupCount = r4.Count,
                groups = r4.Select(g => new
                {
                    name = g.Key,
                    productCount = g.Value.Count,
                    topProducts = g.Value.Take(5).Select(p => new { p.Key, p.Value }).ToList()
                }).ToList()
            };

            // 測試會員統計
            var r5 = TallyMembers();

            return Json(new
            {
                success = true,
                products = productInfo,
                members = new
                {
                    sexCounts = r5.Item1,
                    ageCounts = r5.Item2
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DebugTally 失敗");
            return Json(new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet]
    public IActionResult DebugData()
    {
        try
        {
            // 檢查資料庫中有哪些資料
            var firstTransaction = _db.Transactions.OrderBy(t => t.Time).FirstOrDefault();
            var lastTransaction = _db.Transactions.OrderByDescending(t => t.Time).FirstOrDefault();
            var transactionCount = _db.Transactions.Count();
            var stores = _db.Stores.Select(s => new { s.ID, s.Name, s.Area, s.Brand }).ToList();
            var revenueCount = _db.Revenues.Count();

            // 檢查商品資料
            var totalItems = _db.TransactionItems.Count();
            var itemsWithProduct = _db.TransactionItems.Where(x => x.Product != null && x.Product != "").Count();
            var sampleProducts = _db.TransactionItems
                .Where(x => x.Product != null && x.Product != "")
                .Select(x => x.Product)
                .Distinct()
                .Take(10)
                .ToList();

            // 檢查會員資料
            var memberCount = _db.Members.Count();
            var membersWithSex = _db.Members.Where(m => m.Sex != null && m.Sex != "").Count();

            return Json(new
            {
                success = true,
                transactionCount = transactionCount,
                firstTransactionDate = firstTransaction?.Time?.ToString("yyyy-MM-dd"),
                lastTransactionDate = lastTransaction?.Time?.ToString("yyyy-MM-dd"),
                stores = stores,
                revenueCount = revenueCount,
                totalItems = totalItems,
                itemsWithProduct = itemsWithProduct,
                sampleProducts = sampleProducts,
                memberCount = memberCount,
                membersWithSex = membersWithSex,
                hint = "請將日期範圍設定在 firstTransactionDate 和 lastTransactionDate 之間"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debug 失敗");
            return Json(new
            {
                success = false,
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    private Tuple<int, int> TallyRevenue(DateTime beginDate, DateTime endDate, List<Transaction> transactions)
    {
        int targetRevenue = 0;
        var plans = _db.Revenues.ToList();
        foreach (var p in plans)
        {
            targetRevenue += p.Amount;
        }

        int days = (endDate - beginDate).Days + 1;
        int totalDays = 0;
        DateTime ym = beginDate;
        while (ym <= endDate)
        {
            totalDays += DateTime.DaysInMonth(ym.Year, ym.Month);
            ym = ym.AddMonths(1);
        }
        targetRevenue = Convert.ToInt32(targetRevenue * (double)days / totalDays);

        int realRevenue = 0;
        foreach (var t in transactions)
        {
            realRevenue += t.Amount;
        }

        return Tuple.Create(targetRevenue, realRevenue);
    }

    private Tuple<int, int> TallyConsumers(DateTime beginDate, DateTime endDate, List<Transaction> transactions)
    {
        int custs = 0;
        int cons = 0;

        foreach (var t in transactions)
        {
            custs += t.NumOfCustomers;
            cons += t.NumOfConsumers;
        }

        return Tuple.Create(custs, cons);
    }

    private List<KeyValuePair<string, List<ValueData>>> TallyUnitPrice(DateTime beginDate, DateTime endDate, string interval, string group,
        List<Transaction> transactions)
    {
        var data = new SortedList<string, List<ValueData>>();

        var tic = new TimeIntervalClassifier(beginDate, endDate, interval);
        var sc = new StoreClassifier(group);
        var stores = _db.Stores.ToArray();

        foreach (var t in transactions)
        {
            if (t.Time == null) continue;

            t.Store = stores.Where(x => x.ID == t.StoreID).FirstOrDefault();

            string name = tic.GetName((DateTime)t.Time);
            if (!data.ContainsKey(name))
            {
                var list = sc.CreateCollections(stores);
                data.Add(name, list);
            }

            var cv = data[name].Where(x => x.Title == sc.GetTitle(t.Store)).FirstOrDefault();
            if (cv != null)
            {
                cv.Value1 += t.Amount;
                cv.Value2 += t.NumOfConsumers;
                cv.Value3 = cv.Value2 > 0 ? cv.Value1 / cv.Value2 : 0;
            }
        }

        var result = data.OrderBy(x => x.Key).ToList();
        return result;
    }

    private SortedList<string, List<KeyValuePair<string, int>>> TallyProducts(DateTime beginDate, DateTime endDate, string group)
    {
        var data = new SortedList<string, Dictionary<string, int>>();

        var sc = new StoreClassifier(group);
        var stores = _db.Stores.ToArray();

        // 使用原始 SQL 查詢或分步驟讀取，避免 NULL 值問題
        List<TransactionItem> items;
        try
        {
            items = _db.TransactionItems
                .Include(x => x.Master)
                .ThenInclude(m => m!.Store)
                .Where(x => x.Master!.Time >= beginDate && x.Master.Time <= endDate)
                .AsEnumerable()  // 改為在記憶體中處理
                .Where(x => !string.IsNullOrEmpty(x.Product))  // 只檢查 Product 不為空
                .ToList();

            _logger.LogInformation("TallyProducts: Found {Count} items in date range", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TallyProducts failed to fetch items");
            // 如果還是失敗，返回空結果
            return new SortedList<string, List<KeyValuePair<string, int>>>();
        }

        foreach (var item in items)
        {
            // 確保商品名稱不是空的
            if (string.IsNullOrEmpty(item.Product))
                continue;

            string name = sc.GetTitle(item.Master?.Store);

            // 跳過空的分組名稱
            if (string.IsNullOrEmpty(name))
                continue;

            if (!data.ContainsKey(name))
            {
                var dic = new Dictionary<string, int>();
                data.Add(name, dic);
            }

            if (data[name].TryGetValue(item.Product, out int count))
            {
                data[name][item.Product] = count + item.Qty;
            }
            else
            {
                data[name].Add(item.Product, item.Qty);
            }
        }

        var result = new SortedList<string, List<KeyValuePair<string, int>>>();
        foreach (string key in data.Keys)
        {
            var list = data[key].OrderBy(x => x.Value).Reverse().ToList();
            result.Add(key, list);
        }

        return result;
    }

    private Tuple<int[], int[]> TallyMembers()
    {
        int[] sexCounts = new int[2];
        int[] ageCounts = new int[6];
        DateTime today = DateTime.Today;

        var members = _db.Members.ToList();
        foreach (var member in members)
        {
            if (member.Sex == "M") sexCounts[0]++;
            else if (member.Sex == "F") sexCounts[1]++;

            int age = today.Year - member.BirthDay.Year;
            if (age < 20) ageCounts[0]++;
            else if (age < 30) ageCounts[1]++;
            else if (age < 40) ageCounts[2]++;
            else if (age < 50) ageCounts[3]++;
            else if (age < 60) ageCounts[4]++;
            else ageCounts[5]++;
        }

        return Tuple.Create(sexCounts, ageCounts);
    }

    private Tuple<double[], double[]> TallyCarbonEmissions(List<Transaction> transactions)
    {
        int t4Open = 0;
        int t6Open = 0;
        int t4Custs = 0;
        int t6Custs = 0;

        foreach (var t in transactions)
        {
            if (t.NumOfCustomers <= 4)
            {
                t4Open++;
                t4Custs += t.NumOfCustomers;
            }
            else if (t.NumOfCustomers <= 6)
            {
                t6Open++;
                t6Custs += t.NumOfCustomers;
            }
            else if (t.NumOfCustomers <= 8)
            {
                t4Open += 2;
                t4Custs += t.NumOfCustomers;
            }
            else
            {
                t6Open += t.NumOfCustomers / 6;
                t6Custs += (t.NumOfCustomers / 6) * 6;
                if (t.NumOfCustomers % 6 > 4)
                {
                    t6Open++;
                    t6Custs += (t.NumOfCustomers % 6);
                }
                else
                {
                    t4Open++;
                    t4Custs += (t.NumOfCustomers % 6);
                }
            }
        }

        double[] secs = new double[3];
        secs[0] = 30 * t4Open * 120;
        secs[1] = t4Custs > 0 ? secs[0] / t4Custs : 0;
        secs[2] = t4Open > 0 ? (double)t4Custs / (t4Open * 4) : 0;

        double[] dces = new double[3];
        dces[0] = 50 * t6Open * 120;
        dces[1] = t6Custs > 0 ? dces[0] / t6Custs : 0;
        dces[2] = t6Open > 0 ? (double)t6Custs / (t6Open * 6) : 0;

        return Tuple.Create(secs, dces);
    }

    [HttpGet]
    public IActionResult Tally(string begin, string end, string interval, string group)
    {
        var jso = new TallyResultJSO();

        try
        {
            DateTime beginDate = Utility.FromHtmlDate(begin);
            DateTime endDate = Utility.FromHtmlDate(end).AddDays(1);

            if (beginDate > endDate)
            {
                jso.success = false;
                jso.message = "開始日期不能大於結束日期";
            }
            else
            {
                if (interval == "day" && (endDate - beginDate).Days > 7) endDate = beginDate.AddDays(7);
                else if (interval == "week" && (endDate - beginDate).Days > 56) endDate = beginDate.AddDays(56);
                else if (interval == "month" && (endDate - beginDate).Days > 365) endDate = beginDate.AddDays(365);

                var transactions = _db.Transactions
                    .Where(x => x.Time >= beginDate && x.Time <= endDate)
                    .ToList();

                var r1 = TallyRevenue(beginDate, endDate, transactions);
                var r2 = TallyConsumers(beginDate, endDate, transactions);
                var r3 = TallyUnitPrice(beginDate, endDate, interval, group, transactions);
                var r4 = TallyProducts(beginDate, endDate, group);
                var r5 = TallyMembers();
                var r6 = TallyCarbonEmissions(transactions);

                _logger.LogInformation("Products count: {Count}", r4.Count);
                _logger.LogInformation("Members - Sex counts: {SexCounts}, Age counts: {AgeCounts}",
                    string.Join(",", r5.Item1), string.Join(",", r5.Item2));

                jso.success = true;
                jso.SetRevenue(r1.Item1, r1.Item2);
                jso.SetConsumers(r2.Item1, r2.Item2);
                jso.SetUnitPrices(r3);
                jso.SetProducts(r4);
                jso.SetMemberCounts(r5);
                jso.SetCarbonEmissions(r6);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Tally method");
            jso.success = false;
            jso.message = ex.Message;
        }

        return Json(jso);
    }
}

public class TimeIntervalClassifier
{
    private DateTime BeginDate;
    private DateTime EndDate;
    private string Interval;

    public TimeIntervalClassifier(DateTime beginDate, DateTime endDate, string interval)
    {
        BeginDate = beginDate;
        EndDate = endDate;
        Interval = interval;
    }

    public string GetName(DateTime date)
    {
        string name = "";
        switch (Interval)
        {
            case "day":
                name = date.ToString("M");
                break;
            case "week":
                name = string.Format("第{0}週", (date - BeginDate).Days / 7 + 1);
                break;
            case "month":
                name = date.ToString("Y");
                break;
        }
        return name;
    }
}

public class ValueData
{
    public string Title { get; set; } = string.Empty;
    public double Value1 { get; set; }
    public double Value2 { get; set; }
    public double Value3 { get; set; }
}

public class StoreClassifier
{
    private string Group;

    public StoreClassifier(string group)
    {
        Group = group;
    }

    public string GetTitle(Store? store)
    {
        if (store == null) return "";

        string title = "";
        switch (Group)
        {
            case "area":
                title = store.Area;
                break;
            case "brand":
                title = Store.ToBrandName(store.Brand);
                break;
            case "spot":
                title = Store.ToSpotName(store.Spot);
                break;
            case "custom":
                // 自選：顯示各店店名
                title = store.Name;
                break;
            default:
                title = store.Name;
                break;
        }
        return title;
    }

    public List<ValueData> CreateCollections(ICollection<Store> stores)
    {
        var list = new List<ValueData>();

        foreach (var store in stores)
        {
            string title = GetTitle(store);
            if (!list.Any(x => x.Title == title))
            {
                list.Add(new ValueData { Title = title });
            }
        }

        return list;
    }
}

public class TallyResultJSO
{
    public class GroupData
    {
        public string title { get; set; } = string.Empty;
        public int value { get; set; }
    }

    public class IntervalData
    {
        public string name { get; set; } = string.Empty;
        public List<GroupData> values { get; set; } = new List<GroupData>();
    }

    public bool success { get; set; }
    public string message { get; set; } = string.Empty;

    public string targetRev { get; set; } = string.Empty;
    public string realRev { get; set; } = string.Empty;
    public string revRate { get; set; } = string.Empty;
    public bool revAchieve { get; set; }

    public string customers { get; set; } = string.Empty;
    public string consumers { get; set; } = string.Empty;
    public string consumerRate { get; set; } = string.Empty;

    public int avgUnitPrice { get; set; }
    public List<IntervalData> unitPrices { get; set; } = new List<IntervalData>();

    public List<IntervalData> products { get; set; } = new List<IntervalData>();

    public int[] sexCounts { get; set; } = new int[2];
    public int[] ageCounts { get; set; } = new int[6];

    public string totalSCE { get; set; } = string.Empty;
    public string avgSCE { get; set; } = string.Empty;
    public string useRateSCE { get; set; } = string.Empty;
    public string totalDCE { get; set; } = string.Empty;
    public string avgDCE { get; set; } = string.Empty;
    public string useRateDCE { get; set; } = string.Empty;

    public void SetRevenue(int target, int real)
    {
        targetRev = target.ToString("N0");
        realRev = real.ToString("N0");
        double rate = ((double)real / target) * 100;
        revRate = rate.ToString("N2") + "%";
        revAchieve = rate >= 100.0;
    }

    public void SetConsumers(int numOfCustomers, int numOfConsumers)
    {
        customers = numOfCustomers.ToString("N0");
        consumers = numOfConsumers.ToString("N0");
        if (numOfCustomers > 0)
        {
            consumerRate = (((double)numOfConsumers / numOfCustomers) * 100).ToString("N2") + "%";
        }
        else
        {
            consumerRate = "0.00%";
        }
    }

    public void SetUnitPrices(List<KeyValuePair<string, List<ValueData>>> buffer)
    {
        double price = 0;
        double times = 0;

        foreach (var pair in buffer)
        {
            var iv = new IntervalData();
            iv.name = pair.Key;
            foreach (var v in pair.Value)
            {
                var g = new GroupData();
                g.title = v.Title;
                // 防止 Value3 太大或無限大造成溢位
                if (double.IsInfinity(v.Value3) || double.IsNaN(v.Value3) || v.Value3 > int.MaxValue || v.Value3 < int.MinValue)
                {
                    g.value = 0;
                }
                else
                {
                    g.value = Convert.ToInt32(v.Value3);
                }
                iv.values.Add(g);

                price += v.Value1;
                times += v.Value2;
            }

            unitPrices.Add(iv);
        }

        avgUnitPrice = times > 0 ? Convert.ToInt32(price / times) : 0;
    }

    public void SetProducts(SortedList<string, List<KeyValuePair<string, int>>> buffer)
    {
        foreach (var pair in buffer)
        {
            var iv = new IntervalData();
            iv.name = pair.Key;

            foreach (var p in pair.Value)
            {
                var g = new GroupData();
                g.title = p.Key;
                g.value = p.Value;
                iv.values.Add(g);
            }

            products.Add(iv);
        }
    }

    public void SetMemberCounts(Tuple<int[], int[]> buffer)
    {
        sexCounts = buffer.Item1;
        ageCounts = buffer.Item2;
    }

    public void SetCarbonEmissions(Tuple<double[], double[]> buffer)
    {
        totalSCE = buffer.Item1[0].ToString("N0");
        avgSCE = buffer.Item1[1].ToString("N0");
        useRateSCE = (buffer.Item1[2] * 100).ToString("N2") + "%";
        totalDCE = buffer.Item2[0].ToString("N0");
        avgDCE = buffer.Item2[1].ToString("N0");
        useRateDCE = (buffer.Item2[2] * 100).ToString("N2") + "%";
    }
}
