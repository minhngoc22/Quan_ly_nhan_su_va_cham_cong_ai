using System;
using System.Collections.Generic;

namespace QLNS.Models;

public partial class FaceEmbedding
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public byte[]? Embedding { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
