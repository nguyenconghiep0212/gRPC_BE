using IotGrpcLearning.Infrastructure;
using Xunit;

namespace IotGrpcLearning.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var (hash, salt) = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(result);
    }
}