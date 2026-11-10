using System;
using System.Collections.Generic;

namespace Ktpt.CourseOrder.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
