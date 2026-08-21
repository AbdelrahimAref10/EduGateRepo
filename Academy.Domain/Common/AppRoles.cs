namespace Academy.Domain.Common;

public static class AppRoles
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string Teacher = nameof(Teacher);
    public const string Student = nameof(Student);
    public const string Parent = nameof(Parent);

    public static class Ids
    {
        public const int SuperAdmin = 1;
        public const int Teacher = 2;
        public const int Student = 3;
        public const int Parent = 4;
    }

    public static string ToRoleName(Enums.AppRole role) => role switch
    {
        Enums.AppRole.SuperAdmin => SuperAdmin,
        Enums.AppRole.Teacher => Teacher,
        Enums.AppRole.Student => Student,
        Enums.AppRole.Parent => Parent,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    public static int ToRoleId(Enums.AppRole role) => role switch
    {
        Enums.AppRole.SuperAdmin => Ids.SuperAdmin,
        Enums.AppRole.Teacher => Ids.Teacher,
        Enums.AppRole.Student => Ids.Student,
        Enums.AppRole.Parent => Ids.Parent,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}
