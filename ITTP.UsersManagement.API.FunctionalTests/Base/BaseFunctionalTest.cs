﻿using ITTP.UsersManagement.API.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ITTP.UsersManagement.API.FunctionalTests;

public abstract class BaseFunctionalTest : IClassFixture<TestWebAppFactory>
{
    protected IServiceScope _scope;
    protected UsersManagementDbContext dbContext;
    protected HttpClient _client;

    public BaseFunctionalTest(TestWebAppFactory factory)
    {
        _scope = factory.Services.CreateScope();

        dbContext = _scope.ServiceProvider.GetRequiredService<UsersManagementDbContext>();

        dbContext.Database.Migrate();

        _client = factory.CreateClient();
    }
}