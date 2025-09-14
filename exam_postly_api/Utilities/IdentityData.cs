using System.Security.Claims;

namespace exam_postly_api.Utilities;

public class IdentityData
{
    public const string AdminUserClaimName = "role";
    public const string AdminPolicyName = "Admin";
}