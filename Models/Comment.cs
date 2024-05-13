using System;
using System.Collections.Generic;

namespace Tourism;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? UserId { get; set; }

    public int? TourId { get; set; }

    public string? Text { get; set; }

    public int? ParentCommentId { get; set; }

    public virtual ICollection<Comment> InverseParentComment { get; set; } = new List<Comment>();

    public virtual Comment? ParentComment { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual User? User { get; set; }
}
