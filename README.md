# 戰情室儀表板 (Dashboard)

這是一個基於 .NET 8 MVC 架構的現代化戰情室儀表板應用程式。

## 功能特點

- ✨ 現代化、美觀的使用者介面
- 📊 互動式圖表（使用 Chart.js）
- 📈 即時數據視覺化
- 📱 響應式設計，支援各種裝置
- 🔌 支援多種資料來源：
  - 資料庫（Entity Framework Core）
  - REST API
  - Excel 檔案

## 技術棧

- **後端**: .NET 8.0
- **框架**: ASP.NET Core MVC
- **前端**: Bootstrap 5, Bootstrap Icons
- **圖表**: Chart.js
- **資料處理**:
  - Entity Framework Core (資料庫)
  - EPPlus (Excel)
  - HttpClient (API 調用)

## 專案結構

```
Dashboard/
├── Controllers/         # 控制器
│   └── DashboardController.cs
├── Views/              # 視圖
│   ├── Dashboard/
│   │   └── Index.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/            # 靜態資源
│   ├── css/
│   ├── js/
│   └── lib/
│       ├── bootstrap/
│       ├── bootstrap-icons/
│       └── chart.js/
├── Models/             # 資料模型（待建立）
├── Services/           # 服務層（待建立）
└── Program.cs          # 應用程式入口

## 如何運行

### 1. 開發環境運行

```bash
cd /Users/chenyushen/Documents/Dashboard
dotnet run
```

應用程式將在 `https://localhost:5001` 或 `http://localhost:5000` 上運行。

### 2. 監視模式運行（自動重載）

```bash
dotnet watch run
```

### 3. 構建專案

```bash
dotnet build
```

### 4. 發布專案

```bash
dotnet publish -c Release -o ./publish
```

## 主要頁面

- **首頁/戰情室**: `/Dashboard/Index` - 主要儀表板頁面，顯示統計卡片、圖表和最近活動

## API 端點

- `GET /Dashboard/GetChartData` - 獲取折線圖資料
- `GET /Dashboard/GetPieChartData` - 獲取圓餅圖資料

## 自訂資料來源

目前專案使用範例資料。要連接實際資料來源：

### 連接資料庫

1. 在 `appsettings.json` 中添加連接字串：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "您的資料庫連接字串"
  }
}
```

2. 建立資料模型並使用 Entity Framework Core

### 從 API 獲取資料

在 `DashboardController.cs` 中使用 `HttpClient` 調用外部 API。

### 從 Excel 讀取資料

使用 EPPlus 套件讀取 Excel 檔案（已安裝）。

## 待辦事項

- [ ] 建立資料模型（根據實際業務需求）
- [ ] 實作資料服務層
- [ ] 連接實際資料來源（DB/API/Excel）
- [ ] 添加使用者認證
- [ ] 實作即時資料更新（SignalR）
- [ ] 添加更多圖表類型
- [ ] 資料匯出功能

## 已安裝的套件

- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server 支援
- `Microsoft.EntityFrameworkCore.Design` - EF Core 設計工具
- `EPPlus` - Excel 檔案處理
- `Newtonsoft.Json` - JSON 處理

## 自訂樣式

所有自訂 CSS 樣式都在 `wwwroot/css/site.css` 中。

## 注意事項

- 所有靜態資源（Bootstrap Icons, Chart.js）已下載到本地，不依賴 CDN
- 預設語言為繁體中文
- 專案使用 .NET 8.0，請確保已安裝對應版本的 SDK

## 授權

此專案為內部使用。
