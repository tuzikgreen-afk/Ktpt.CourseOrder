using Avalonia.Controls;
using Avalonia.Interactivity;
using Ktpt.CourseOrder.Models;
using Ktpt.CourseOrder.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.Views;

public partial class OrderDetailsView : UserControl
{
    public int OrderId { get; set; }
    private int? itemToDelete;

    public OrderDetailsView()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadOrderAsync();
    }

    private async Task LoadOrderAsync()
    {
        try
        {
            ConfirmPanel.IsVisible = false;
            Order? order = await OrderService.GetOrderAsync(OrderId);
            if (order == null)
            {
                MainWindow.Instance.OpenPage(new OrdersView(), clearHistory: true);
                MainWindow.Instance.ShowMessage("Заказ удалён, места возвращены.");
                return;
            }
            HeaderText.Text = $"Заказ №{order.OrderId}";
            ClientText.Text = "Клиент: " + order.Client.FullName;
            DateText.Text = "Дата: " + order.OrderDate.ToString("dd.MM.yyyy");
            TotalText.Text = $"Итого: {order.OrderItems.Sum(i => i.Total):N2} руб.";
            ItemsList.ItemsSource = order.OrderItems.ToList();
            AdminPanel.IsVisible = Session.IsAdmin;
            DateBox.SelectedDate = order.OrderDate.ToDateTime(TimeOnly.MinValue);
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    private async void ReloadClick(object? sender, RoutedEventArgs e) => await LoadOrderAsync();

    private async void SaveDateClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DateBox.SelectedDate == null)
                throw new AppException("Выберите дату.");
            DateOnly date = DateOnly.FromDateTime(DateBox.SelectedDate.Value);
            await OrderService.ChangeDateAsync(OrderId, date);
            await LoadOrderAsync();
            MainWindow.Instance.ShowMessage("Дата сохранена. Цены позиций не изменились.");
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }

    private void DeleteOrderClick(object? sender, RoutedEventArgs e)
    {
        itemToDelete = null;
        ConfirmText.Text = "Удалить весь заказ? Все его места вернутся в потоки.";
        ConfirmPanel.IsVisible = true;
    }

    private void DeleteItemClick(object? sender, RoutedEventArgs e)
    {
        OrderItem? item = ItemsList.SelectedItem as OrderItem;
        if (item == null)
        {
            MainWindow.Instance.ShowMessage("Сначала выберите позицию в списке.");
            return;
        }
        itemToDelete = item.OrderItemId;
        ConfirmText.Text = "Удалить позицию? Её места вернутся в поток. " +
            "Если позиция последняя, удалится и весь заказ.";
        ConfirmPanel.IsVisible = true;
    }

    private void CancelClick(object? sender, RoutedEventArgs e)
    {
        ConfirmPanel.IsVisible = false;
    }

    private async void ConfirmClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await OrderService.DeleteAsync(OrderId, itemToDelete);
            MainWindow.Instance.ShowMessage("Удаление выполнено, места возвращены.");
            await LoadOrderAsync();
        }
        catch (Exception ex)
        {
            MainWindow.Instance.ShowError(ex);
        }
    }
}