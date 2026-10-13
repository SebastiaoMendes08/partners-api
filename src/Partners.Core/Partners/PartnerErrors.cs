using Partners.Core.Common;

namespace Partners.Core.Partners;

public static class PartnerErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Partner not found.", $"Partner '{id}' was not found.");

    public static Error DuplicateTaxNumber(string taxNumber) =>
        Error.Conflict("Duplicate tax number.", $"A partner with tax number '{taxNumber}' already exists.");

    public static Error VersionConflict(Guid id) =>
        Error.Conflict("Version conflict.",
            $"Partner '{id}' was changed by another request. Get the latest version and try again.");
}
