using Partners.Application.Common;
using Partners.Application.Partners;
using Partners.Application.Partners.Dtos;
using Partners.Core.Errors;
using Partners.Core.Partners;

namespace Partners.UnitTests;

public class PartnerValidatorTests
{
    private static readonly CreatePartnerDto ValidInput = new("Ana Silva", "001234567", "CUSTOMER", null, 100m);

    [Fact]
    public void Valid_input_is_normalized()
    {
        var input = new CreatePartnerDto(" Ana Silva ", "001.234.567", "BOTH", "  ana@example.com ", 1500.5m);

        var data = Validate(input, out var errors);

        Assert.Equal(0, errors.Count);
        Assert.NotNull(data);
        Assert.Equal("Ana Silva", data.Name);
        Assert.Equal("001234567", data.TaxNumber);
        Assert.Equal(PartnerType.Both, data.Type);
        Assert.Equal("ana@example.com", data.Email);
        Assert.Equal(1500.5m, data.CreditLimit);
    }

    [Theory]
    [InlineData("001.234.567", "001234567")]
    [InlineData("001 234 567", "001234567")]
    [InlineData("001-234-567", "001234567")]
    [InlineData(" 0 0 1.2-3 4567 ", "001234567")]
    public void Tax_number_separators_are_removed(string taxNumber, string expected)
    {
        var data = Validate(ValidInput with { TaxNumber = taxNumber }, out _);

        Assert.Equal(expected, data?.TaxNumber);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    [InlineData("12345678A")]
    [InlineData("123/456/789")]
    [InlineData("123\t456789")]
    [InlineData("١٢٣٤٥٦٧٨٩")]
    public void Invalid_tax_number_is_rejected(string taxNumber)
    {
        AssertSingleError(ValidInput with { TaxNumber = taxNumber }, "TaxNumber");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("  A  ")]
    public void Name_must_have_at_least_two_characters_after_trim(string? name)
    {
        AssertSingleError(ValidInput with { Name = name }, "Name");
    }

    [Fact]
    public void Name_length_limit_is_applied_after_trim()
    {
        var hundred = new string('a', 100);

        Assert.NotNull(Validate(ValidInput with { Name = $"  {hundred}  " }, out _));
        AssertSingleError(ValidInput with { Name = hundred + "a" }, "Name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_email_is_stored_as_null(string? email)
    {
        var data = Validate(ValidInput with { Email = email }, out _);

        Assert.NotNull(data);
        Assert.Null(data.Email);
    }

    [Theory]
    [InlineData("ana")]
    [InlineData("ana@")]
    [InlineData("ana@example")]
    [InlineData("ana silva@example.com")]
    [InlineData("ana@@example.com")]
    public void Invalid_email_is_rejected(string email)
    {
        AssertSingleError(ValidInput with { Email = email }, "Email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("customer")]
    [InlineData("Customer")]
    [InlineData("0")]
    [InlineData("CUSTOMER,SUPPLIER")]
    public void Type_must_be_one_of_the_supported_codes(string? type)
    {
        AssertSingleError(ValidInput with { Type = type }, "Type");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.01")]
    [InlineData("10.99")]
    [InlineData("10.990")]
    [InlineData("1000000")]
    public void Credit_limit_within_range_is_accepted(string value)
    {
        Assert.NotNull(Validate(ValidInput with { CreditLimit = decimal.Parse(value) }, out _));
    }

    [Theory]
    [InlineData("10.999")]
    [InlineData("0.001")]
    [InlineData("-0.01")]
    [InlineData("1000000.01")]
    public void Credit_limit_out_of_range_or_with_excess_precision_is_rejected(string value)
    {
        AssertSingleError(ValidInput with { CreditLimit = decimal.Parse(value) }, "CreditLimit");
    }

    [Fact]
    public void Missing_credit_limit_is_rejected()
    {
        AssertSingleError(ValidInput with { CreditLimit = null }, "CreditLimit");
    }

    [Fact]
    public void All_errors_are_reported_together()
    {
        var input = new CreatePartnerDto("", "abc", "OTHER", "invalid", 10.999m);

        Validate(input, out var errors);

        var exception = Assert.Throws<ValidationException>(errors.ThrowIfAny);
        Assert.Equal(["Name", "TaxNumber", "Type", "Email", "CreditLimit"], exception.Errors.Keys);
    }

    private static PartnerData? Validate(CreatePartnerDto input, out ValidationErrors errors)
    {
        errors = new ValidationErrors();
        return PartnerValidator.Validate(input, errors);
    }

    private static void AssertSingleError(CreatePartnerDto input, string field)
    {
        var data = Validate(input, out var errors);

        Assert.Null(data);
        var exception = Assert.Throws<ValidationException>(errors.ThrowIfAny);
        Assert.Equal([field], exception.Errors.Keys);
    }
}
