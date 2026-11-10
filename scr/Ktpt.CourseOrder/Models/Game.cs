using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class Game
{
    public int GameId { get; set; }

    public string GameCode { get; set; } = null!;

    public string GameName { get; set; } = null!;

    public int CategoryId { get; set; }

    public int PublisherId { get; set; }

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public decimal BasePrice { get; set; }

    public int AvailableStock { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Publisher Publisher { get; set; } = null!;
}
