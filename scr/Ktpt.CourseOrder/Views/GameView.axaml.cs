using Avalonia.Controls;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.Views;

public partial class GameView : UserControl
{
    public int GameId { get; set; }
    private decimal shownPrice;

    public GameView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadGameAsync();
    }

    private async Task LoadGameAsync()
    {
        try
        {
            SaveButton.IsEnabled = false;
            using var db = Db.Create();
            Game? game = await db.Games
                .AsNoTracking()
                .SingleOrDefaultAsync(g => g.GameId == GameId);
            if (game == null)
                throw new AppException("Игра не найдена. Вернитесь в каталог.");
            NameText.Text = game.GameName;
            shownPrice = game.BasePrice;
            PriceText.Text = $"Цена: {shownPrice:N2} руб.";
            StockText.Text = $"Остаток на складе: {game.AvailableStock} шт.";
            SaveButton.IsEnabled = game.AvailableStock > 0;
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    private async void SaveClick(object? sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        try
        {
            if (!int.TryParse(QuantityBox.Text, out int quantity) || quantity <= 0)
                throw new AppException("Количество должно быть целым числом больше нуля.");
            int orderId = await OrderService.CreateOrderAsync(
                gameId: GameId,
                quantity: quantity,
                currentPrice: shownPrice
            );
            MainWindow.Instance.OpenPage(new CatalogView(), clearHistory: true);
            MainWindow.Instance.ShowMessage($"Заказ №{orderId} успешно оформлен!");
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
            SaveButton.IsEnabled = true;
        }
    }

    private void BackClick(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.OpenPage(new CatalogView());
    }
}   