using Microsoft.EntityFrameworkCore;
using SistemaBodega.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Agrega el DbContext generado desde la base de datos
builder.Services.AddDbContext<SistemaBodegaContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Agrega IHttpContextAccessor para acceder a sesión desde las vistas (_Layout.cshtml)
builder.Services.AddHttpContextAccessor();

// 🔹 Agrega servicios MVC y Razor
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

// 🔹 Agrega soporte para sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 🔸 Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Activar sesiones antes de Authorization
app.UseSession();

app.UseAuthorization();

// 🔹 Rutas MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



