using Partners.Core.Errors;

namespace Partners.Core.Partners;

public static class PartnerValidator
{
    public static PartnerData? Validate(PartnerInput input, ValidationErrors errors)
    {
        var errorCount = errors.Count;

        var name = input.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add(nameof(input.Name), "Name is required.");
        }
        else if (name.Length is < PartnerRules.NameMinLength or > PartnerRules.NameMaxLength)
        {
            errors.Add(nameof(input.Name),
                $"Name must have between {PartnerRules.NameMinLength} and {PartnerRules.NameMaxLength} characters.");
        }

        string? taxNumber = null;
        if (string.IsNullOrWhiteSpace(input.TaxNumber))
        {
            errors.Add(nameof(input.TaxNumber), "TaxNumber is required.");
        }
        else
        {
            taxNumber = PartnerRules.NormalizeTaxNumber(input.TaxNumber);
            if (!PartnerRules.IsValidTaxNumber(taxNumber))
            {
                errors.Add(nameof(input.TaxNumber),
                    "TaxNumber must have exactly 9 digits. Spaces, dots and hyphens are ignored.");
            }
        }

        var type = default(PartnerType);
        if (string.IsNullOrWhiteSpace(input.Type))
        {
            errors.Add(nameof(input.Type), "Type is required.");
        }
        else if (!PartnerTypes.TryParse(input.Type, out type))
        {
            errors.Add(nameof(input.Type), $"Type must be one of: {PartnerTypes.AllowedValues}.");
        }

        var email = PartnerRules.NormalizeEmail(input.Email);
        if (email is not null && !PartnerRules.IsValidEmail(email))
        {
            errors.Add(nameof(input.Email), "Email is not a valid email address.");
        }

        if (input.CreditLimit is not { } creditLimit)
        {
            errors.Add(nameof(input.CreditLimit), "CreditLimit is required.");
        }
        else if (creditLimit is < PartnerRules.CreditLimitMin or > PartnerRules.CreditLimitMax)
        {
            errors.Add(nameof(input.CreditLimit),
                $"CreditLimit must be between {PartnerRules.CreditLimitMin} and {PartnerRules.CreditLimitMax}.");
        }
        else if (!PartnerRules.HasValidPrecision(creditLimit))
        {
            errors.Add(nameof(input.CreditLimit),
                $"CreditLimit must have at most {PartnerRules.CreditLimitDecimals} decimal places.");
        }

        if (errors.Count > errorCount)
        {
            return null;
        }

        return new PartnerData(name!, taxNumber!, type, email, input.CreditLimit!.Value);
    }
}
