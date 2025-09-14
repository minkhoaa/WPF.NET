using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.Data;
using MyStore.Library.Repositories;

namespace MyStore.WpfApp;

public partial class App : Application
{
    private IHost? _host;
    public static System.IServiceProvider Services => ((App)Current)._host!.Services;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                var cs = ctx.Configuration.GetConnectionString("Default")!;
                services.AddDbContextFactory<MyStoreContext>(opt => opt.UseSqlServer(cs));

                services.AddScoped<ICategoryRepository, CategoryRepository>();
                services.AddScoped<IProductRepository, ProductRepository>();

                services.AddTransient<MainWindow>();
                services.AddTransient<Views.CategoriesPage>();
                services.AddTransient<Views.ProductsPage>();

                services.AddTransient<ViewModels.CategoriesViewModel>();
                services.AddTransient<ViewModels.ProductsViewModel>();
            })
            .Build();

        _host.Start();
        var main = _host.Services.GetRequiredService<MainWindow>();
        main.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null) await _host.StopAsync();
        base.OnExit(e);
    }
}


