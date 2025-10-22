# 部署說明

## 資料庫設定

請在主機上修改 `appsettings.json` 中的資料庫連線字串：

```json
"ConnectionStrings": {
  "ZuchiDB": "Server=localhost;Database=ZUCHI;User Id=zuchiconnect;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=True"
}
```

## Windows 主機部署步驟

1. Clone 專案
```bash
git clone <repo-url>
cd Dashboard
```

2. 修改 appsettings.json 的資料庫連線字串

3. 發布專案
```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\Dashboard
```

4. 在 IIS 建立網站
   - 應用程式集區：無受控程式碼
   - 實體路徑：C:\inetpub\wwwroot\Dashboard
   - 確保已安裝 ASP.NET Core Runtime

## 功能說明

- **Dashboard (戰情室)**: `/Dashboard/Index` - 營收、來客數、客單價等數據分析
- **Admin (後台管理)**: `/Admin/Index` - 店舖管理、交易記錄、會員管理、營收目標
- **Cashier API**: `/Cashier/Index` - 接收收銀機推送的交易資料（POST）

## 收銀機整合

收銀機需 POST JSON 資料到：`http://your-domain/Cashier/Index`

所有接收的 JSON 檔案會儲存在 `App_Data/Cashier/` 目錄
