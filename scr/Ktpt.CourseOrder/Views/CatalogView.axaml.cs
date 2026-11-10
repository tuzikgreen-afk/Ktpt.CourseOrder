using Avalonia.Controls;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.Views;

public partial class CatalogView : UserControl
{
    private List<Game> _allGames = new();
    private List<Category> _categories = new();

    public CatalogView()
    {
        InitializeComponent();
        SortBox.SelectedIndex = 0;
        Loaded += async (s, e) => await LoadCatalogAsync();
    }

    private async Task LoadCatalogAsync()
    {
        try
        {
            using var db = Db.Create();
            
            // Загружаем игры с категориями и издателями
            _allGames = await db.Games
                .Include(g => g.Category)
                .Include(g => g.Publisher)
                .ToListAsync();
            
            // Загружаем категории для фильтра
            _categories = await db.Categories.ToListAsync();
            
            // Заполняем ComboBox категорий
            var categoryItems = new List<string> { "Все категории" };
            categoryItems.AddRange(_categories.Select(c => c.CategoryName).OrderBy(n => n));
            CategoryBox.ItemsSource = categoryItems;
            CategoryBox.SelectedIndex = 0;
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<Game> query = _allGames;
        
        // 1. Фильтр по категории
        if (CategoryBox.SelectedIndex > 0)
        {
            string categoryName = (string)CategoryBox.SelectedItem!;
            var category = _categories.FirstOrDefault(c => c.CategoryName == categoryName);
            if (category != null)
                query = query.Where(g => g.CategoryId == category.CategoryId);
        }
        
        // 2. Поиск по названию и артикулу
        string searchText = (SearchBox.Text ?? "").Trim().ToLower();
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(g => 
                g.GameName.ToLower().Contains(searchText) || 
                g.GameCode.ToLower().Contains(searchText));
        }
        
        // 3. Сортировка
        query = SortBox.SelectedIndex switch
        {
            0 => query.OrderBy(g => g.GameName),
            1 => query.OrderByDescending(g => g.GameName),
            2 => query.OrderBy(g => g.BasePrice),
            3 => query.OrderByDescending(g => g.BasePrice),
            4 => query.OrderBy(g => g.AvailableStock),
            5 => query.OrderByDescending(g => g.AvailableStock),
            _ => query.OrderBy(g => g.GameName)
        };
        
        var result = query.ToList();
        GamesList.ItemsSource = result;
        CountText.Text = $"Найдено: {result.Count} из {_allGames.Count}";
    }

    // Обработчики событий
    private void CategoryBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ApplyFilters();
    private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) => ApplyFilters();
    private void SortBox_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ApplyFilters();

    private void ResetClick(object? sender, RoutedEventArgs e)
    {
        SearchBox.Text = "";
        CategoryBox.SelectedIndex = 0;
        SortBox.SelectedIndex = 0;
        ApplyFilters();
    }

    private void OrderClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int gameId)
        {
            MainWindow.Instance.OpenPage(new GameView { GameId = gameId });
        }
    }
}