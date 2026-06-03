using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoalTracker.Shared.Models;

namespace GoalTracker.MainApp.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    public async Task LoadAsync()
    {
        var data = await App.DataService.LoadAsync();
        Categories = new ObservableCollection<Category>(data.Categories);
    }

    public async Task SaveCategoryAsync(Category category)
    {
        var data = await App.DataService.LoadAsync();
        var existing = data.Categories.FirstOrDefault(c => c.Id == category.Id);
        if (existing is null)
            data.Categories.Add(category);
        else
        {
            var idx = data.Categories.IndexOf(existing);
            data.Categories[idx] = category;
        }
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(Category category)
    {
        var data = await App.DataService.LoadAsync();
        data.Categories.RemoveAll(c => c.Id == category.Id);
        // Unlink any goals that used this category
        foreach (var goal in data.Goals.Where(g => g.CategoryId == category.Id))
            goal.CategoryId = null;
        await App.DataService.SaveAsync(data);
        await LoadAsync();
    }
}
