using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tourism;

public partial class Category
{
    public int CategoryId { get; set; }

    [Display(Name = "Загальна інформація")]
    public string? Info { get; set; }

    [Display(Name = "Фото")]
    public string? MainPhoto { get; set; }

    [Display(Name = "Назва")]
    public string? Name { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
