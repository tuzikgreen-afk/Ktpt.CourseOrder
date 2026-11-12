using Ktpt.CourseOrder.Data;
using Ktpt.CourseOrder.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ktpt.CourseOrder.Services;

public static class OrderService
{
    public static async Task<int> CreateOrderAsync(int gameId, int quantity, decimal currentPrice)
    {
        // 1. Проверки без БД
        if (Session.CurrentUser == null)
            throw new AppException("Гость не может оформить заказ. Войдите в систему.");

        if (quantity <= 0)
            throw new AppException("Количество должно быть больше нуля.");

        // 2. Открываем подключение
        using var db = Db.Create();

        // 3. Начинаем транзакцию
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            // 4. Находим игру
            var game = await db.Games
                .SingleOrDefaultAsync(g => g.GameId == gameId);

            if (game == null)
                throw new AppException("Игра не найдена.");

            // 5. Проверяем остаток
            if (game.AvailableStock < quantity)
                throw new AppException($"Недостаточно товара. Доступно: {game.AvailableStock} шт.");

            // 6. Уменьшаем остаток и сразу сохраняем состояние игры
            game.AvailableStock -= quantity;
            await db.SaveChangesAsync(); // Фиксируем новый остаток на складе

            // 7. Создаём заказ отдельно
            var order = new Order
            {
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                ClientId = Session.CurrentUser!.UserId
            };

            var orderItem = new OrderItem
            {
                GameId = gameId, // Работаем строго по ID, чтобы не путать трекер связей
                Quantity = quantity,
                UnitPrice = currentPrice
            };

            order.OrderItems.Add(orderItem);

            db.Orders.Add(order);
            await db.SaveChangesAsync(); // Сохраняем сам заказ



            // 8. Подтверждаем транзакцию
            await transaction.CommitAsync();

            return order.OrderId;
        }
        catch
        {
            // 9. Если что-то пошло не так — откат
            await transaction.RollbackAsync();
            throw;
        }
    }
    // ---------- 2. Список всех заказов ----------
    public static async Task<List<Order>> GetOrdersAsync()
    {
        Session.RequireStaff();
        using var db = Db.Create();
        return await db.Orders
            .Include(o => o.Client)
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.OrderId)
            .AsNoTracking()
            .ToListAsync();
    }

    // ---------- 3. Один заказ со всем составом ----------
    public static async Task<Order?> GetOrderAsync(int orderId)
    {
        Session.RequireStaff();
        using var db = Db.Create();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Game)
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.OrderId == orderId);
    }

    // ---------- 4. Изменить дату заказа ----------
    public static async Task ChangeDateAsync(int orderId, DateOnly newDate)
    {
        Session.RequireAdmin();
        using var db = Db.Create();
        Order? order = await db.Orders.FindAsync(orderId);
        if (order == null)
            throw new AppException("Заказ не найден.");
        order.OrderDate = newDate;
        await db.SaveChangesAsync();
    }

    // ---------- 5. Удаление заказа ----------
    public static async Task DeleteAsync(int orderId, int? itemId = null)
    {
        if (itemId == null)
            Session.RequireStaff();
        else
            Session.RequireAdmin();

        using var db = Db.Create();
        using var transaction = await db.Database.BeginTransactionAsync();

        List<OrderItem> items = await db.OrderItems
            .Where(i => i.OrderId == orderId && (itemId == null || i.OrderItemId == itemId))
            .ToListAsync();

        if (items.Count == 0)
            throw new AppException("Эти данные уже удалены. Обновите список.");

        // Возвращаем места
        foreach (OrderItem item in items)
        {
            await db.Games
                .Where(g => g.GameId == item.GameId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    g => g.AvailableStock,
                    g => g.AvailableStock + item.Quantity));
        }

        db.OrderItems.RemoveRange(items);
        await db.SaveChangesAsync();

        // Если в заказе не осталось позиций — удаляем и сам заказ
        bool hasItems = await db.OrderItems.AnyAsync(i => i.OrderId == orderId);
        if (!hasItems)
            await db.Orders.Where(o => o.OrderId == orderId).ExecuteDeleteAsync();

        await transaction.CommitAsync();
    }
}