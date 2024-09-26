using Domain.Interfaces;
using Domain.Quota;

namespace Domain.Tests;

public class QuotaTableShould
{
    [Fact]
    public void ThrowInvalidOperationException()
    {
        // arrange
        var sut = new QuotaTable("quota", "project", "user");
        var testEvent = new TestEvent("id", "created");

        // act
        var result = Assert.Throws<InvalidOperationException>(() => sut.Apply(testEvent));

        // assert
        Assert.IsType<InvalidOperationException>(result);
    }

    private record TestEvent(string Id, string CreatedBy) : IDomainEvent;
}