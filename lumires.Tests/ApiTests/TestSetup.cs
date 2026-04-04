using FastEndpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.ApiTests;

public static class TestSetup
{
    [Before(Assembly)]
    public static void Init()
    {
        Factory.RegisterTestServices(s =>
        {
            s.AddRouting();
            s.AddSingleton(Mock.Of<LinkGenerator>());
        });
    }
}