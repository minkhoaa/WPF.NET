using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace MyStore.WpfApp.Views;

public partial class CategoriesPage : UserControl
{
    private readonly IServiceScope _scope;

    public CategoriesPage()
    {
        InitializeComponent();
        var scopeFactory = App.Services.GetRequiredService<IServiceScopeFactory>();
        _scope = scopeFactory.CreateScope();
        DataContext = _scope.ServiceProvider.GetRequiredService<ViewModels.CategoriesViewModel>();
        System.Windows.Application.Current.Exit += (_, __) => _scope.Dispose();
    }
}
