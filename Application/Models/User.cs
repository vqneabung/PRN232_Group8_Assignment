using System;
using System.Collections.Generic;

namespace Application.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Class> ClassExaminerNavigations { get; set; } = new List<Class>();

    public virtual ICollection<Class> ClassLecturerNavigations { get; set; } = new List<Class>();

    public virtual Role Role { get; set; } = null!;
}
