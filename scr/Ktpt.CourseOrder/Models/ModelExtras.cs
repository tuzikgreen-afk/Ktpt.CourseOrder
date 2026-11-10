namespace Ktpt.CourseOrder.Models;

public partial class User
{
    // ФИО одной строкой
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    // Как пользователь отображается в ComboBox
    public override string ToString() => $"{Login} — {FullName}";
}

public partial class OrderItem
{
    // Стоимость строки заказа: количество × цена
    public decimal Total => Quantity * UnitPrice;

    // Как позиция выглядит в списке
    public override string ToString() =>
        $"{Game?.GameName} × {Quantity} = {Total:N2} руб.";
}

public partial class Order
{
    // Как заказ выглядит в списке
    public override string ToString() =>
        $"Заказ №{OrderId} от {OrderDate:dd.MM.yyyy} — {Client?.FullName}";
}