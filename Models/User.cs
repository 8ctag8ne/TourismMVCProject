using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Tourism;

public partial class User : IdentityUser<int>
{
    [Display(Name = "Про себе")]
    public string? Info { get; set; }

    [Display(Name = "Фото профілю")]
    public string? ProfilePhoto { get; set; }

    [NotMapped]
    public IFormFile? ProfilePic { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Order> OrderGuides { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderUsers { get; set; } = new List<Order>();
}
