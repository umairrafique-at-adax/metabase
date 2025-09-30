using Metabase.Web.Models;
using MetabaseMigrator.Core.Context;
using MetabaseMigrator.Core.Interfaces;
using MetabaseMigrator.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<List<MetabaseInstance>>(
    builder.Configuration.GetSection("MetabaseInstances"));
builder.Services.AddSingleton<IMigrationServiceFactory, MigrationServiceFactory>();
builder.Services.AddSingleton<MigrationContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=SelectInstances}/{id?}");

app.Run();
