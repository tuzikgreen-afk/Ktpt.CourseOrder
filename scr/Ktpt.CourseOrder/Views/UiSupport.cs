using Avalonia.Media;
using Ktpt.CourseOrder.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ktpt.CourseOrder.Views;

public static class UiSupport
{
    // Цвет из App.axaml по имени
    public static IBrush Brush(string key)
    {
        return (IBrush)Avalonia.Application.Current!.Resources[key]!;
    }

    // Текст ошибки для пользователя
    public static string ErrorText(Exception error)
    {
        if (error is AppException)
            return error.Message;

        if (error is SqlException || error is DbUpdateException)
            return "Ошибка базы данных. Проверьте подключение к SQL Server.";

        return "Операция не выполнена: " + error.Message;
    }
}