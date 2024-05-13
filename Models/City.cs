using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tourism;

public partial class City
{
    public int CityId { get; set; }

    public int? RegionId { get; set; }

    [Display(Name = "Загальна інформація")]
    public string? Info { get; set; }

    public virtual Region? Region { get; set; }

    [Display(Name = "Назва")]
    public string? Name { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
