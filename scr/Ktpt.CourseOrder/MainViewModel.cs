using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private User? _currentUser;

        // Исходный список всех игр из БД (не трогаем)
        private List<Game> _allGames = new();

        // Список для отображения (после фильтрации/сортировки)
        [ObservableProperty]
        private ObservableCollection<Game> _games = new();

        // Категории для фильтра
        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private Category? _selectedCategory;

        // Поисковый запрос
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Тип сортировки
        [ObservableProperty]
        private string _selectedSort = "По названию (А-Я)";

        // Варианты сортировки
        public List<string> SortOptions { get; } = new()
        {
            "По названию (А-Я)",
            "По названию (Я-А)",
            "По цене (возрастание)",
            "По цене (убывание)",
            "По остатку (возрастание)",
            "По остатку (убывание)"
        };

        public MainViewModel()
        {
        }

        // Метод для инициализации после входа
        public async Task InitializeAsync(User user)
        {
            CurrentUser = user;
            await LoadCatalogAsync();
        }

        private async Task LoadCatalogAsync()
        {
            try
            {
                using var context = new BoardGamesContext();

                // Загружаем игры с категориями и издателями
                _allGames = await context.Games
                    .Include(g => g.Category)
                    .Include(g => g.Publisher)
                    .ToListAsync();

                // Загружаем список категорий для фильтра
                var cats = await context.Categories.ToListAsync();
                Categories = new ObservableCollection<Category>(cats);
                SelectedCategory = null; // "Все категории"

                ApplyFilters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Автоматически вызываются при изменении свойств
        partial void OnSelectedCategoryChanged(Category? value) => ApplyFilters();
        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnSelectedSortChanged(string value) => ApplyFilters();

        [RelayCommand]
        private void ResetFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = null;
            SelectedSort = "По названию (А-Я)";
        }

        private void ApplyFilters()
        {
            IEnumerable<Game> query = _allGames;

            // 1. Фильтр по категории
            if (SelectedCategory != null)
            {
                query = query.Where(g => g.CategoryId == SelectedCategory.CategoryId);
            }

            // 2. Поиск по названию и артикулу
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLower();
                query = query.Where(g =>
                    g.GameName.ToLower().Contains(search) ||
                    g.GameCode.ToLower().Contains(search));
            }

            // 3. Сортировка
            query = SelectedSort switch
            {
                "По названию (А-Я)" => query.OrderBy(g => g.GameName),
                "По названию (Я-А)" => query.OrderByDescending(g => g.GameName),
                "По цене (возрастание)" => query.OrderBy(g => g.BasePrice),
                "По цене (убывание)" => query.OrderByDescending(g => g.BasePrice),
                "По остатку (возрастание)" => query.OrderBy(g => g.AvailableStock),
                "По остатку (убыванию)" => query.OrderByDescending(g => g.AvailableStock),
                _ => query.OrderBy(g => g.GameName)
            };

            Games = new ObservableCollection<Game>(query);
        }
    }
}