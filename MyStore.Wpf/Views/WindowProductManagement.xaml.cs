using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MyStore.Library.DataAccess;
using MyStore.Library.Repositories;

namespace MyStore.Wpf.Views;

public partial class WindowProductManagement : Window
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    private ObservableCollection<Product> _products = new ObservableCollection<Product>();
    private List<Category> _categories = new List<Category>();

    public WindowProductManagement(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        InitializeComponent();
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;

        Loaded += async (_, __) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        _categories = await _categoryRepository.GetAllOrderedAsync();
        cboCategory.ItemsSource = _categories;
    }

    private async Task LoadProductsAsync()
    {
        var list = await _productRepository.GetAllWithCategoryAsync();
        _products = new ObservableCollection<Product>(list);
        dgProducts.ItemsSource = _products;
    }

    private Product? SelectedProduct => dgProducts.SelectedItem as Product;

    private void FillForm(Product? p)
    {
        if (p == null)
        {
            txtProductId.Text = string.Empty;
            txtProductName.Text = string.Empty;
            cboCategory.SelectedIndex = -1;
            txtUnitsInStock.Text = string.Empty;
            txtUnitPrice.Text = string.Empty;
            return;
        }

        txtProductId.Text = p.ProductId.ToString();
        txtProductName.Text = p.ProductName;
        cboCategory.SelectedValue = p.CategoryId;
        txtUnitsInStock.Text = p.UnitsInStock?.ToString() ?? string.Empty;
        txtUnitPrice.Text = p.UnitPrice?.ToString() ?? string.Empty;
    }

    private (bool ok, string message, Product entity)? ReadFormForInsert()
    {
        var name = txtProductName.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return (false, "ProductName is required", null);
        if (!(cboCategory.SelectedValue is int categoryId)) return (false, "Category is required", null);

        short? units = null;
        if (!string.IsNullOrWhiteSpace(txtUnitsInStock.Text))
        {
            if (!short.TryParse(txtUnitsInStock.Text, out var u) || u < 0) return (false, "UnitsInStock must be >= 0", null);
            units = u;
        }

        decimal? price = null;
        if (!string.IsNullOrWhiteSpace(txtUnitPrice.Text))
        {
            if (!decimal.TryParse(txtUnitPrice.Text, out var p) || p < 0) return (false, "UnitPrice must be >= 0", null);
            price = p;
        }

        var entity = new Product
        {
            ProductName = name,
            CategoryId = categoryId,
            UnitsInStock = units,
            UnitPrice = price
        };
        return (true, null, entity);
    }

    private (bool ok, string message)? ReadFormForUpdate(Product entity)
    {
        var name = txtProductName.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return (false, "ProductName is required");
        if (!(cboCategory.SelectedValue is int categoryId)) return (false, "Category is required");

        if (!string.IsNullOrWhiteSpace(txtUnitsInStock.Text))
        {
            if (!short.TryParse(txtUnitsInStock.Text, out var u) || u < 0) return (false, "UnitsInStock must be >= 0");
            entity.UnitsInStock = u;
        }
        else entity.UnitsInStock = null;

        if (!string.IsNullOrWhiteSpace(txtUnitPrice.Text))
        {
            if (!decimal.TryParse(txtUnitPrice.Text, out var p) || p < 0) return (false, "UnitPrice must be >= 0");
            entity.UnitPrice = p;
        }
        else entity.UnitPrice = null;

        entity.ProductName = name!;
        entity.CategoryId = categoryId;
        return (true, null);
    }

    private async void btnLoad_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            FillForm(SelectedProduct);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Load failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void btnInsert_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = ReadFormForInsert();
            if (result.HasValue && !result.Value.ok && result.Value.message != null)
            {
                MessageBox.Show(result.Value.message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = result.HasValue ? result.Value.entity : null;
            if (product == null)
            {
                MessageBox.Show("Invalid input.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            await LoadProductsAsync();
            dgProducts.SelectedItem = _products.LastOrDefault(p => p.ProductName == product.ProductName && p.UnitPrice == product.UnitPrice);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Insert failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var current = await _productRepository.GetByIdAsync(SelectedProduct.ProductId);
            if (current == null)
            {
                MessageBox.Show("Product not found.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var check = ReadFormForUpdate(current);
            if (!check.Value.ok)
            {
                MessageBox.Show(check.Value.message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productRepository.Update(current);
            await _productRepository.SaveAsync();

            var id = current.ProductId;
            await LoadProductsAsync();
            dgProducts.SelectedItem = _products.FirstOrDefault(p => p.ProductId == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Update failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product.", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show($"Delete product #{SelectedProduct.ProductId}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            var current = await _productRepository.GetByIdAsync(SelectedProduct.ProductId);
            if (current == null)
            {
                MessageBox.Show("Product not found.", "Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productRepository.Delete(current);
            await _productRepository.SaveAsync();

            await LoadProductsAsync();
            FillForm(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void dgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FillForm(SelectedProduct);
    }
}
