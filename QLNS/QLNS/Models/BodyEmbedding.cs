using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class BodyEmbedding
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public byte[] Embedding { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
