using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Ktpt.CourseOrder.Services;
using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Views;

public partial class MainWindow : Window
{
    // Ссылка на единственное главное окно
    public static MainWindow Instance { get; private set; } = null!;

    // История открытых страниц для кнопки «Назад»
    private readonly Stack<UserControl> history = new();

    public MainWindow()
    {
        InitializeComponent();
        Instance = this; // Запоминаем экземпляр окна
        OpenPage(new LoginView(), clearHistory: true);
    }

    // Метод открытия новой страницы
    public void OpenPage(UserControl page, bool clearHistory = false)
    {
        if (clearHistory)
        {
            history.Clear();
        }
        else
        {
            UserControl? current = PageHost.Content as UserControl;
            if (current != null)
                history.Push(current);
        }
        ShowPage(page);
    }

    // Внутренний метод отображения страницы
    private void ShowPage(UserControl page)
    {
        PageHost.Content = page;
        Title = "КТПТ — " + page.Tag;
        MessageText.Text = "";
        UserText.Text = Session.Caption;
        OrdersButton.IsVisible = Session.IsStaff;
        BackButton.IsEnabled = history.Count > 0;
    }

    // Показать обычное сообщение
    public void ShowMessage(string text)
    {
        MessageText.Foreground = Brushes.Black;
        MessageText.Text = text;
    }

    // Показать ошибку
    public void ShowError(Exception error)
    {
        System.Diagnostics.Trace.WriteLine(error);
        MessageText.Foreground = Brushes.DarkRed;
        MessageText.Text = UiSupport.ErrorText(error);
    }

    // --- Обработчики кнопок из XAML ---
    private void BackClick(object? sender, RoutedEventArgs e)
    {
        if (history.Count > 0)
            ShowPage(history.Pop());
    }

    private void CatalogClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new CatalogView(), clearHistory: true);
    }

    private void OrdersClick(object? sender, RoutedEventArgs e)
    {
        OpenPage(new OrdersView());
    }

    private void LogoutClick(object? sender, RoutedEventArgs e)
    {
        Session.Logout();
        OpenPage(new LoginView(), clearHistory: true);
    }
}