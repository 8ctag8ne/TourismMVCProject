using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism;

public partial class Order
{
    
    public int OrderId { get; set; }

    [Display(Name = "Користувач")]
    public int? UserId { get; set; }

    [Display(Name = "Тур")]
    public int? TourId { get; set; }

    [Display(Name = "Гід")]
    public int? GuideId { get; set; }

    [Display(Name = "Статус")]
    public string? Status { get; set; }

    public virtual User? Guide { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual User? User { get; set; }
}

