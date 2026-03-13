using IotGrpcLearning.Infrastructure;
using IotGrpcLearning.Interfaces;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace IotGrpcLearning.Tests.Infrastructure;

public class PasswordServiceTests
{
    private readonly IPasswordService _passwordService;

    public PasswordServiceTests()
    {
        var options = Options.Create(new PasswordOptions());
        _passwordService = new PasswordService(options);
    }

    [Fact]
    public void HashPassword_ShouldReturnHashAndSalt()
    {
        // Arrange
        var password = "TestPassword123!";  

        // Act
        var (hash, salt) = _passwordService.HashPassword(password);

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
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash, salt);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var (hash, salt) = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPasswordWithMetadata_ShouldReturnFormattedString()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var result = _passwordService.HashPasswordWithMetadata(password);

        // Assert
        Assert.StartsWith("v1$", result);
        Assert.Contains("$100000$", result); // default iterations
        var parts = result.Split('$');
        Assert.Equal(4, parts.Length);
    }

    [Fact]
    public void VerifyPasswordWithMetadata_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedWithMetadata = _passwordService.HashPasswordWithMetadata(password);

        // Act
        var result = _passwordService.VerifyPasswordWithMetadata(password, hashedWithMetadata);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPasswordWithMetadata_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hashedWithMetadata = _passwordService.HashPasswordWithMetadata(password);

        // Act
        var result = _passwordService.VerifyPasswordWithMetadata(wrongPassword, hashedWithMetadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _passwordService.HashPassword(null!));
    }
}