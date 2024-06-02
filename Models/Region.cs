using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tourism;

public partial class Region
{
    public int RegionId { get; set; }

    [Display(Name = "Загальна інформація")]
    public string? Info { get; set; }

    [Display(Name = "Назва")]
    public string? Name { get; set; }
    
    [Display(Name = "Фото")]
    public string? MainPhoto { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
