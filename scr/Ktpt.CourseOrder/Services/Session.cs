using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.Services;

public static class Session
{
    public static User? CurrentUser { get; private set; }
    public static string RoleName => CurrentUser?.Role?.RoleName ?? "";
    public static bool IsGuest => CurrentUser == null;
    public static bool IsCustomer => RoleName == "Покупатель";
    public static bool IsStaff => RoleName == "Сотрудник" || RoleName == "Администратор";
    public static bool IsAdmin => RoleName == "Администратор";
    public static string Caption =>
        CurrentUser == null ? "Гость" : $"{CurrentUser.FullName} · {RoleName}";

    public static async Task<List<User>> GetUsersAsync()
    {
        using var db = Db.Create();
        return await db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.LastName)
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task LoginAsync(int userId)
    {
        using var db = Db.Create();
        CurrentUser = await db.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .SingleAsync(u => u.UserId == userId);
    }

    public static void Logout()
    {
        CurrentUser = null;
    }

    // Проверки прав. Вызываются в сервисах ПЕРЕД изменением данных.
    // Если прав нет — выбрасываем ошибку, и операция не выполняется.

    public static void RequireStaff()
    {
        if (!IsStaff)
            throw new AppException("Недостаточно прав: нужна роль сотрудника или администратора.");
    }

    public static void RequireAdmin()
    {
        if (!IsAdmin)
            throw new AppException("Недостаточно прав: нужна роль администратора.");
    }
}