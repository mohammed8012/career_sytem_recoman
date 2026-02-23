using System;
using System.Collections.Generic;

namespace career_sytem_recoman.Models.Entities;

public partial class Communication
{
    public int CommId { get; set; }

    public string CommType { get; set; } = null!;

    public string SenderType { get; set; } = null!;

    public int? SenderId { get; set; }

    public string ReceiverType { get; set; } = null!;

    public int ReceiverId { get; set; }

    public string? Title { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? NotificationType { get; set; }
}
