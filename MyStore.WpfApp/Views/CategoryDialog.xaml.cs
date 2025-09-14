using System.Windows;

namespace MyStore.WpfApp.Views;

public partial class CategoryDialog : Window
{
    public string CategoryName
    {
        get => NameBox.Text.Trim();
        set => NameBox.Text = value;
    }

    public CategoryDialog()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            MessageBox.Show("Name is required.");
            return;
        }
        if (CategoryName.Length > 15)
        {
            MessageBox.Show("Name must be <= 15 characters.");
            return;
        }
        DialogResult = true;
    }
}

