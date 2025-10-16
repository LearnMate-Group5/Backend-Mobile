using System;
using System.Collections.Generic;

namespace Application.Users.Commands
{
    internal static class UserRoleCatalog
    {
        private static readonly Dictionary<string, string> _normalizedRoleNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["User"] = "User",
            ["Staff"] = "Staff",
            ["Admin"] = "Admin"
        };

        public static bool TryGetCanonicalName(string? value, out string canonicalName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                canonicalName = string.Empty;
                return false;
            }

            return _normalizedRoleNames.TryGetValue(value.Trim(), out canonicalName!);
        }

        public static IReadOnlyCollection<string> SupportedRoles => _normalizedRoleNames.Values;
    }
}
