namespace LowlandTech.Core.Tests.StringExtensionsTests;

public partial class StringExtensionsTests
{
    [Fact]
    public void ComputeSha256Hash_ShouldReturnEmptyString_WhenInputIsNull()
    {
        // Arrange
        string input = null;

        // Act
        var result = input.ComputeSha256Hash();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ComputeSha256Hash_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = input.ComputeSha256Hash();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ComputeSha256Hash_ShouldReturnCorrectHash_WhenInputIsNonEmpty()
    {
        // Arrange
        var input = "Hello, World!";
        var expectedHash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

        // Act
        var result = input.ComputeSha256Hash();

        // Assert
        result.Should().Be(expectedHash);
    }


    [Fact]
    public void ComputeSha256Hash_ShouldReturnSameHash_ForSameInput()
    {
        // Arrange
        var input = "Test string for hashing";

        // Act
        var result1 = input.ComputeSha256Hash();
        var result2 = input.ComputeSha256Hash();

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void ComputeSha256Hash_ShouldReturnDifferentHash_ForDifferentInput()
    {
        // Arrange
        var input1 = "First input";
        var input2 = "Second input";

        // Act
        var result1 = input1.ComputeSha256Hash();
        var result2 = input2.ComputeSha256Hash();

        // Assert
        result1.Should().NotBe(result2);
    }
}