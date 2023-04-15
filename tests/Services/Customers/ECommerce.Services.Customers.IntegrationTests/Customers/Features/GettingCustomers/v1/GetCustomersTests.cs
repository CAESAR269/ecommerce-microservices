using ECommerce.Services.Customers.Customers.Features.GettingCustomers.v1;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using FluentAssertions;
using Humanizer;
using MongoDB.Driver;
using Tests.Shared.Fixtures;
using Tests.Shared.XunitCategories;
using Xunit.Abstractions;

namespace ECommerce.Services.Customers.IntegrationTests.Customers.Features.GettingCustomers.v1;

public class GetCustomersTests : CustomerServiceIntegrationTestBase
{
    public GetCustomersTests(
        SharedFixtureWithEfCoreAndMongo<Api.Program, CustomersDbContext, CustomersReadDbContext> sharedFixture,
        ITestOutputHelper outputHelper
    )
        : base(sharedFixture, outputHelper) { }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task can_get_existing_customers_list_from_db()
    {
        // Arrange
        var fakeCustomers = new FakeCustomerReadModel().Generate(3);
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomers.ToArray());

        // Act
        var query = new GetCustomers();
        var listResult = (await SharedFixture.SendAsync(query)).Customers;

        // Assert
        listResult.Should().NotBeNull();
        listResult.Items.Should().NotBeEmpty();
        listResult.Items.Should().HaveCount(3);
        listResult.PageNumber.Should().Be(1);
        listResult.PageSize.Should().Be(10);
        listResult.TotalCount.Should().Be(3);
    }

    [Fact]
    [CategoryTrait(TestCategory.Integration)]
    internal async Task can_get_existing_customers_list_with_correct_page_size_and_page()
    {
        // Arrange
        var fakeCustomers = new FakeCustomerReadModel().Generate(3);
        await SharedFixture.InsertMongoDbContextAsync(fakeCustomers.ToArray());

        // Act
        var query = new GetCustomers() { PageNumber = 1, PageSize = 2 };
        var listResult = (await SharedFixture.SendAsync(query)).Customers;

        // Assert
        listResult.Should().NotBeNull();
        listResult.Items.Should().NotBeEmpty();
        listResult.Items.Should().HaveCount(2);
        listResult.PageNumber.Should().Be(1);
        listResult.PageSize.Should().Be(2);
        listResult.TotalCount.Should().Be(3);
    }
}
