using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyStore.Library.Data;
using MyStore.Library.Repositories;
using MyStore.WpfApp.Views;

namespace MyStore.WpfApp.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly System.Threading.SemaphoreSlim _gate = new(1,1);

    [ObservableProperty]
    private ObservableCollection<Category> items = new();

    [ObservableProperty]
    private Category? selectedItem;

    public CategoriesViewModel(ICategoryRepository repo, IProductRepository productRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var list = await _repo.GetAllAsync();
            Items = new ObservableCollection<Category>(list.OrderBy(c => c.CategoryId));
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Failed to load categories: {ex.Message}");
        }
        finally
        {
            _gate.Release();
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dlg = new CategoryDialog { Owner = Application.Current.MainWindow };
        if (dlg.ShowDialog() == true)
        {
            await _gate.WaitAsync();
            try
            {
                var entity = new Category { CategoryName = dlg.CategoryName };
                await _repo.AddAsync(entity);
            }
            finally
            {
                _gate.Release();
            }
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (SelectedItem is null) return;
        var dlg = new CategoryDialog { Owner = Application.Current.MainWindow };
        dlg.CategoryName = SelectedItem.CategoryName;
        if (dlg.ShowDialog() == true)
        {
            await _gate.WaitAsync();
            try
            {
                SelectedItem.CategoryName = dlg.CategoryName;
                _repo.Update(SelectedItem);
            }
            finally
            {
                _gate.Release();
            }
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedItem is null) return;
        // Block delete if products exist
        var hasProducts = (await _productRepo.GetAllAsync()).Any(p => p.CategoryId == SelectedItem.CategoryId);
        if (hasProducts)
        {
            MessageBox.Show("Cannot delete: category has related products.");
            return;
        }
        if (MessageBox.Show("Delete selected category?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            await _gate.WaitAsync();
            try
            {
                _repo.Delete(SelectedItem);
            }
            finally
            {
                _gate.Release();
            }
            await RefreshAsync();
        }
    }
}
