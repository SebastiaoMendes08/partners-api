using System.Reflection;
using Partners.Api.Controllers;
using Partners.Application.Partners;
using Partners.Core.Partners;
using Partners.Infrastructure.IoC;
using Partners.Infrastructure.Persistence;

namespace Partners.ArchitectureTests;

public class LayerDependencyTests
{
    private const string CoreName = "Partners.Core";
    private const string ApplicationName = "Partners.Application";
    private const string InfrastructureName = "Partners.Infrastructure";
    private const string IoCName = "Partners.Infrastructure.IoC";

    private static readonly Assembly Core = typeof(Partner).Assembly;
    private static readonly Assembly Application = typeof(PartnerService).Assembly;
    private static readonly Assembly Infrastructure = typeof(PartnersDbContext).Assembly;
    private static readonly Assembly IoC = typeof(DependencyInjection).Assembly;
    private static readonly Assembly Api = typeof(PartnersController).Assembly;

    [Fact]
    public void Core_does_not_depend_on_other_layers_or_frameworks()
    {
        Assert.Empty(ProjectReferences(Core));
        Assert.DoesNotContain(References(Core), name => name.StartsWith("Microsoft.EntityFrameworkCore"));
        Assert.DoesNotContain(References(Core), name => name.StartsWith("Microsoft.AspNetCore"));
    }

    [Fact]
    public void Application_only_depends_on_core()
    {
        Assert.Equal([CoreName], ProjectReferences(Application));
        Assert.DoesNotContain(References(Application), name => name.StartsWith("Microsoft.EntityFrameworkCore"));
        Assert.DoesNotContain(References(Application), name => name.StartsWith("Microsoft.AspNetCore"));
    }

    [Fact]
    public void Infrastructure_only_depends_on_core()
    {
        Assert.Equal([CoreName], ProjectReferences(Infrastructure));
    }

    [Fact]
    public void IoC_composes_application_and_infrastructure()
    {
        Assert.Equivalent(new[] { CoreName, ApplicationName, InfrastructureName }, ProjectReferences(IoC));
    }

    [Fact]
    public void Api_does_not_depend_on_infrastructure()
    {
        var references = ProjectReferences(Api);

        Assert.DoesNotContain(InfrastructureName, references);
        Assert.Contains(IoCName, references);
    }

    private static List<string> References(Assembly assembly) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

    private static List<string> ProjectReferences(Assembly assembly) =>
        References(assembly).Where(name => name.StartsWith("Partners.")).Order().ToList();
}
