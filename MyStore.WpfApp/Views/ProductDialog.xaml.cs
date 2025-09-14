using System;
using System.Globalization;
using System.Windows;
using MyStore.Library.Data;

namespace MyStore.WpfApp.Views;

public partial class ProductDialog : Window
{
    public ProductDialog()
    {
        InitializeComponent();
    }

    public string ProductName
    {
        get => NameBox.Text.Trim();
        set => NameBox.Text = value;
    }

    public Category? SelectedCategory
    {
        get => CategoryBox.SelectedItem as Category;
        set => CategoryBox.SelectedItem = value;
    }

    public short? UnitsInStock
    {
        get
        {
            if (short.TryParse(StockBox.Text, out var val) && val >= 0) return val;
            return null;
        }
        set => StockBox.Text = value?.ToString() ?? string.Empty;
    }

    public decimal? UnitPrice
    {
        get
        {
            if (decimal.TryParse(PriceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) && val >= 0) return val;
            return null;
        }
        set => PriceBox.Text = value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public void LoadCategories(System.Collections.IEnumerable categories)
    {
        CategoryBox.ItemsSource = categories;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            MessageBox.Show("Name is required.");
            return;
        }
        if (ProductName.Length > 40)
        {
            MessageBox.Show("Name must be <= 40 characters.");
            return;
        }
        if (SelectedCategory is null)
        {
            MessageBox.Show("Please select a category.");
            return;
        }
        if (UnitsInStock is null || UnitsInStock < 0)
        {
            MessageBox.Show("Stock must be >= 0.");
            return;
        }
        if (UnitPrice is null || UnitPrice < 0)
        {
            MessageBox.Show("Price must be >= 0.");
            return;
        }
        DialogResult = true;
    }
}

