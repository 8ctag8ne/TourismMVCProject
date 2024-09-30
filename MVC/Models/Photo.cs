using System;
using System.Collections.Generic;

namespace Tourism;

public partial class Photo
{
    public int PhotoId { get; set; }

    public int? TourId { get; set; }

    public string? Path { get; set; }

    public virtual Tour? Tour { get; set; }
}
