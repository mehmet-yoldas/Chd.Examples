using Chd.Coordination.Core;
using Xunit;

namespace Chd.UnitTest;

/// <summary>
/// CoordinationContext için kapsamlı testler
/// </summary>
public class CoordinationContextTests
{
    [Fact]
    public void Context_Should_Create_With_CorrelationId()
    {
        // Act
        var context = CoordinationContext.Create();

        // Assert
        Assert.NotNull(context);
        Assert.False(string.IsNullOrEmpty(context.CorrelationId));
        Assert.Null(context.LockKey);
        Assert.Null(context.SagaId);
    }

    [Fact]
    public void Context_Should_Create_With_Custom_CorrelationId()
    {
        // Arrange
        var customCorrelationId = Guid.NewGuid().ToString();

        // Act
        var context = new CoordinationContext(customCorrelationId);

        // Assert
        Assert.Equal(customCorrelationId, context.CorrelationId);
    }

    [Fact]
    public void Context_Should_Create_With_LockKey()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var lockKey = "resource:123";

        // Act
        var context = new CoordinationContext(correlationId, lockKey: lockKey);

        // Assert
        Assert.Equal(correlationId, context.CorrelationId);
        Assert.Equal(lockKey, context.LockKey);
        Assert.Null(context.SagaId);
    }

    [Fact]
    public void Context_Should_Create_With_SagaId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var sagaId = "order:456";

        // Act
        var context = new CoordinationContext(correlationId, sagaId: sagaId);

        // Assert
        Assert.Equal(correlationId, context.CorrelationId);
        Assert.Null(context.LockKey);
        Assert.Equal(sagaId, context.SagaId);
    }

    [Fact]
    public void Context_Should_Create_With_All_Properties()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var lockKey = "resource:789";
        var sagaId = "order:101";

        // Act
        var context = new CoordinationContext(correlationId, lockKey, sagaId);

        // Assert
        Assert.Equal(correlationId, context.CorrelationId);
        Assert.Equal(lockKey, context.LockKey);
        Assert.Equal(sagaId, context.SagaId);
    }

    [Fact]
    public void Context_Should_Generate_Unique_CorrelationIds()
    {
        // Act
        var context1 = CoordinationContext.Create();
        var context2 = CoordinationContext.Create();
        var context3 = CoordinationContext.Create();

        // Assert
        Assert.NotEqual(context1.CorrelationId, context2.CorrelationId);
        Assert.NotEqual(context2.CorrelationId, context3.CorrelationId);
        Assert.NotEqual(context1.CorrelationId, context3.CorrelationId);
    }

    [Fact]
    public void Context_Should_Be_Immutable()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var lockKey = "resource:123";
        var sagaId = "order:456";
        var context = new CoordinationContext(correlationId, lockKey, sagaId);

        // Act & Assert
        Assert.Equal(correlationId, context.CorrelationId);
        Assert.Equal(lockKey, context.LockKey);
        Assert.Equal(sagaId, context.SagaId);
        
        // Properties should be read-only (cannot be modified after creation)
        // This is validated at compile time by the record type
    }

    [Fact]
    public void Context_Should_Support_ToString()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var context = new CoordinationContext(correlationId);

        // Act
        var result = context.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(correlationId, result);
    }

    [Fact]
    public void Context_Should_Support_Equality()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var lockKey = "resource:123";
        var sagaId = "order:456";

        var context1 = new CoordinationContext(correlationId, lockKey, sagaId);
        var context2 = new CoordinationContext(correlationId, lockKey, sagaId);
        var context3 = new CoordinationContext(Guid.NewGuid().ToString(), lockKey, sagaId);

        // Assert
        Assert.Equal(context1, context2);
        Assert.NotEqual(context1, context3);
    }

    [Fact]
    public void Context_Should_Support_GetHashCode()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var lockKey = "resource:123";
        var sagaId = "order:456";
        var context = new CoordinationContext(correlationId, lockKey, sagaId);

        // Act
        var hashCode = context.GetHashCode();

        // Assert
        Assert.NotEqual(0, hashCode);
    }

    [Fact]
    public void Context_Should_Track_Request_Flow()
    {
        // Arrange - Simulating a request flow
        var requestContext = CoordinationContext.Create();
        
        // Act - Request enters lock section
        var lockContext = new CoordinationContext(
            requestContext.CorrelationId, 
            lockKey: "payment:process");

        // Then enters saga
        var sagaContext = new CoordinationContext(
            requestContext.CorrelationId,
            lockKey: lockContext.LockKey,
            sagaId: "order:123");

        // Assert - Should maintain same correlation ID throughout
        Assert.Equal(requestContext.CorrelationId, lockContext.CorrelationId);
        Assert.Equal(requestContext.CorrelationId, sagaContext.CorrelationId);
        Assert.NotNull(lockContext.LockKey);
        Assert.NotNull(sagaContext.SagaId);
    }
}
