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

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly System.Threading.SemaphoreSlim _gate = new(1,1);

    [ObservableProperty]
    private ObservableCollection<Product> items = new();

    [ObservableProperty]
    private Product? selectedItem;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    partial void OnSelectedCategoryChanged(Category? value)
    {
        _ = RefreshAsync();
    }

    public ProductsViewModel(IProductRepository repo, ICategoryRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _ = RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await _gate.WaitAsync();
        try
        {
            var cats = await _categoryRepo.GetAllAsync();
            Categories = new ObservableCollection<Category>(cats.OrderBy(c => c.CategoryName));

            var list = await _repo.GetAllWithCategoryAsync();
            if (SelectedCategory != null)
            {
                list = list.Where(p => p.CategoryId == SelectedCategory.CategoryId).ToList();
            }
            Items = new ObservableCollection<Product>(list);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Failed to load products: {ex.Message}");
        }
        finally
        {
            _gate.Release();
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var dlg = new ProductDialog { Owner = Application.Current.MainWindow };
        dlg.LoadCategories(Categories);
        if (SelectedCategory != null)
        {
            dlg.SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedCategory.CategoryId);
        }
        if (dlg.ShowDialog() == true)
        {
            await _gate.WaitAsync();
            try
            {
                var entity = new Product
                {
                    ProductName = dlg.ProductName,
                    CategoryId = dlg.SelectedCategory!.CategoryId,
                    UnitsInStock = dlg.UnitsInStock,
                    UnitPrice = dlg.UnitPrice
                };
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
        var dlg = new ProductDialog { Owner = Application.Current.MainWindow };
        dlg.LoadCategories(Categories);
        dlg.ProductName = SelectedItem.ProductName;
        dlg.SelectedCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedItem.CategoryId);
        dlg.UnitsInStock = SelectedItem.UnitsInStock;
        dlg.UnitPrice = SelectedItem.UnitPrice;
        if (dlg.ShowDialog() == true)
        {
            await _gate.WaitAsync();
            try
            {
                SelectedItem.ProductName = dlg.ProductName;
                SelectedItem.CategoryId = dlg.SelectedCategory!.CategoryId;
                SelectedItem.UnitsInStock = dlg.UnitsInStock;
                SelectedItem.UnitPrice = dlg.UnitPrice;
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
        if (MessageBox.Show("Delete selected product?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
