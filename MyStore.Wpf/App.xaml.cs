using System;
using System.Configuration;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyStore.Library.DataAccess;
using MyStore.Library.Repositories;

namespace MyStore.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var connString = ConfigurationManager.ConnectionStrings["Default"]?.ConnectionString
                         ?? throw new InvalidOperationException("Missing connection string 'Default'");

        var options = new DbContextOptionsBuilder<MyStoreContext>()
            .UseSqlServer(connString)
            .Options;

        var context = new MyStoreContext(options);
        var productRepository = new ProductRepository(context);
        var categoryRepository = new CategoryRepository(context);

        var mainWindow = new Views.WindowProductManagement(productRepository, categoryRepository);
        mainWindow.Show();
    }
}
