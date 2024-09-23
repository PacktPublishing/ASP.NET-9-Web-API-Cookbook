using books.Repositories;
using books.Services;
using NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Diagnostics;

namespace Tests.Services;

public abstract class BooksServiceTestsBase
{
    protected Fixture Fixture { get; }
    protected IUrlHelper UrlHelper { get; }
    protected IBooksRepository Repository { get; }
    protected BooksService Service { get; }

    protected BooksServiceTestsBase()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoNSubstituteCustomization());
        Repository = Substitute.For<IBooksRepository>();
        UrlHelper = new TestUrlHelper();
        Service = new BooksService(Repository);
    }

    private class TestUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotImplementedException();

        public string? Action(UrlActionContext actionContext)
        {
            Debug.WriteLine($"Action called with: Action={actionContext.Action}, Controller={actionContext.Controller}");
            Debug.WriteLine($"Values: {string.Join(", ", actionContext.Values?.GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(actionContext.Values)}") ?? Array.Empty<string>())}");

            var pageSize = actionContext.Values?.GetType().GetProperty("pageSize")?.GetValue(actionContext.Values);
            var lastId = actionContext.Values?.GetType().GetProperty("lastId")?.GetValue(actionContext.Values);

            return $"/api/books?action={actionContext.Action}&controller={actionContext.Controller}&pageSize={pageSize}&lastId={lastId}";
        }

        public string? Content(string? contentPath) => throw new NotImplementedException();
        public bool IsLocalUrl(string? url) => throw new NotImplementedException();
        public string? Link(string? routeName, object? values) => throw new NotImplementedException();
        public string? RouteUrl(UrlRouteContext routeContext) => throw new NotImplementedException();
    }
}
