using Microsoft.EntityFrameworkCore;
using Dashboard.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // 用於 API 調用

// 註冊 DbContext
builder.Services.AddDbContext<ZuchiDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ZuchiDB")));

var app = builder.Build();

// 只在生產環境支援子路徑部署 (如 /dashboard)
if (!app.Environment.IsDevelopment())
{
    app.UsePathBase("/dashboard");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 在生產環境不使用 HTTPS 重定向 (根據您的需求)
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}"); // 預設路由設為 Dashboard

app.Run();
