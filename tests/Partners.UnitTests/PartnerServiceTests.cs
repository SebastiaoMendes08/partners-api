using Partners.Application.Partners;
using Partners.Application.Partners.Dtos;
using Partners.Core.Common;
using Partners.Core.Partners;
using Partners.UnitTests.Fakes;

namespace Partners.UnitTests;

public class PartnerServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 9, 30, 0, TimeSpan.Zero);
    private static readonly CreatePartnerDto ValidCreate = new(" Ana Silva ", "001.234.567", "CUSTOMER", null, 100m);
    private const string ValidVersion = "AAAAAAAAB9E=";

    private readonly FakePartnerRepository _repository = new();
    private readonly PartnerService _service;

    public PartnerServiceTests()
    {
        _service = new PartnerService(_repository, new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task Create_returns_normalized_partner_with_server_dates()
    {
        var result = await _service.CreateAsync(ValidCreate);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ana Silva", result.Value.Name);
        Assert.Equal("001234567", result.Value.TaxNumber);
        Assert.True(result.Value.IsActive);
        Assert.Equal(Now.UtcDateTime, result.Value.CreatedAt);
        Assert.Equal(Now.UtcDateTime, result.Value.UpdatedAt);
        Assert.Single(_repository.Partners);
    }

    [Fact]
    public async Task Create_with_invalid_data_returns_validation_error_without_saving()
    {
        var result = await _service.CreateAsync(ValidCreate with { TaxNumber = "123", CreditLimit = 10.999m });

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(["TaxNumber", "CreditLimit"], result.Error.ValidationErrors!.Keys);
        Assert.Equal(0, _repository.SaveCalls);
    }

    [Fact]
    public async Task Create_returns_conflict_reported_by_the_repository()
    {
        _repository.SaveResult = PartnerErrors.DuplicateTaxNumber("001234567");

        var result = await _service.CreateAsync(ValidCreate);

        Assert.True(result.IsFailure);
        Assert.Equal(PartnerErrors.DuplicateTaxNumber("001234567"), result.Error);
    }

    [Fact]
    public async Task Get_unknown_id_returns_not_found()
    {
        var id = Guid.NewGuid();

        var result = await _service.GetByIdAsync(id);

        Assert.True(result.IsFailure);
        Assert.Equal(PartnerErrors.NotFound(id), result.Error);
    }

    [Fact]
    public async Task List_with_invalid_query_returns_validation_error_without_querying()
    {
        var result = await _service.ListAsync(new ListPartnersQuery { Page = 0, Type = "OTHER" });

        Assert.True(result.IsFailure);
        Assert.Equal(["Type", "Page"], result.Error.ValidationErrors!.Keys);
        Assert.Equal(0, _repository.ListCalls);
    }

    [Fact]
    public async Task Update_validates_the_request_before_loading_the_partner()
    {
        var result = await _service.UpdateAsync(Guid.NewGuid(), ToUpdate(ValidCreate) with
        {
            IsActive = null,
            Version = "not-base64!"
        });

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(["IsActive", "Version"], result.Error.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task Update_unknown_id_returns_not_found()
    {
        var id = Guid.NewGuid();

        var result = await _service.UpdateAsync(id, ToUpdate(ValidCreate));

        Assert.True(result.IsFailure);
        Assert.Equal(PartnerErrors.NotFound(id), result.Error);
        Assert.Equal(0, _repository.SaveCalls);
    }

    [Fact]
    public async Task Update_returns_conflict_reported_by_the_repository()
    {
        var created = await _service.CreateAsync(ValidCreate);
        _repository.SaveResult = PartnerErrors.VersionConflict(created.Value.Id);

        var result = await _service.UpdateAsync(created.Value.Id, ToUpdate(ValidCreate));

        Assert.True(result.IsFailure);
        Assert.Equal(PartnerErrors.VersionConflict(created.Value.Id), result.Error);
    }

    private static UpdatePartnerDto ToUpdate(CreatePartnerDto dto) =>
        new(dto.Name, dto.TaxNumber, dto.Type, dto.Email, dto.CreditLimit, true, ValidVersion);
}
