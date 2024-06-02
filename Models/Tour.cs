using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tourism;

public partial class Tour
{
    public int TourId { get; set; }

    [Display(Name = "Місто")]
    public int? CityId { get; set; }

    [Display(Name = "Загальна інформація")]
    public string? Info { get; set; }
    
    [Display(Name = "Обкладинка")]
    public string? MainPhoto { get; set; }

    [Display(Name = "Ціна")]
    public int? Price { get; set; }

    [Display(Name = "Час початку")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Час кінця")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Кількість місць")]
    public int? Capacity { get; set; }

    [Display(Name = "Кількість доступних місць")]
    public int? AvaibleTickets { get; set; }


    [Display(Name = "Категорія")]
    public int? CategoryId { get; set; }

    [Display(Name = "Стартова локація")]
    public string? StartPointName { get; set; }

    [Display(Name = "Координати стартової локації")]
    public string? StartPointGeo { get; set; }

    [Display(Name = "Категорія")]
    public virtual Category? Category { get; set; }

    [Display(Name = "Місто")]
    public virtual City? City { get; set; }

    [Display(Name = "Назва")]
    [Required(ErrorMessage = "Введіть назву туру")]
    public string? Name { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    [NotMapped]
    public List<int> Guides {get; set;} = new List<int>();
}
