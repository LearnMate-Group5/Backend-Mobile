namespace Domain.Constants;

public static class UserStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static string FromBool(bool isActive) => isActive ? Active : Inactive;
}
