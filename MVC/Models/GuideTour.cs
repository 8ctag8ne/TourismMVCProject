using System;
using System.Collections.Generic;

namespace Tourism;

public partial class GuideTour
{
    public int TourId { get; set; }

    public int GuideId { get; set; }

    public virtual User Guide { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
