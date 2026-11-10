using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
