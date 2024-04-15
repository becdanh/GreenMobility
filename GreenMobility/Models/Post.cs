using System;
using System.Collections.Generic;

namespace GreenMobility.Models;

public partial class Post
{
    public int PostId { get; set; }

    public string Title { get; set; } = null!;

    public string Contents { get; set; } = null!;

    public string Thumb { get; set; } = null!;

    public bool Published { get; set; }

    public string? Alias { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Author { get; set; }

    public int? AccountId { get; set; }
}
