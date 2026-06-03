using GoalTracker.MainApp.Dialogs;
using GoalTracker.MainApp.ViewModels;
using GoalTracker.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace GoalTracker.MainApp.Pages;

public sealed partial class CategoriesPage : Page
{
    private readonly CategoriesViewModel _vm = new();

    public CategoriesPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await _vm.LoadAsync();
        CategoriesList.ItemsSource = _vm.Categories;
    }

    private async void AddCategory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EditCategoryDialog(new Category()) { XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await _vm.SaveCategoryAsync(dialog.Category);
        await RefreshAsync();
    }

    private async void EditCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Category cat })
        {
            var dialog = new EditCategoryDialog(cat) { XamlRoot = XamlRoot };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                await _vm.SaveCategoryAsync(dialog.Category);
            await RefreshAsync();
        }
    }

    private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Category cat })
        {
            await _vm.DeleteCategoryCommand.ExecuteAsync(cat);
            await RefreshAsync();
        }
    }
}
