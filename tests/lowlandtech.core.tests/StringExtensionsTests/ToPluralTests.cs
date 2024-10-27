namespace LowlandTech.Core.Tests.StringExtensionsTests;

public partial class StringExtensionsTests
{
    [Theory]
    [InlineData("man", "men")]
    [InlineData("woman", "women")]
    [InlineData("child", "children")]
    [InlineData("tooth", "teeth")]
    [InlineData("foot", "feet")]
    [InlineData("mouse", "mice")]
    [InlineData("belief", "beliefs")]
    public void ToPlural_ShouldReturnExceptionCase_WhenInputIsIrregular(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("city", "cities")]
    [InlineData("puppy", "puppies")]
    [InlineData("boy", "boys")]
    [InlineData("key", "keys")]
    public void ToPlural_ShouldReplaceYWithIes_WhenEndingWithYButNotPrecededByVowel(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("cactus", "cactuses")]
    [InlineData("octopus", "octopuses")]
    public void ToPlural_ShouldAddEs_WhenEndingWithUs(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("boss", "bosses")]
    [InlineData("glass", "glasses")]
    public void ToPlural_ShouldAddEs_WhenEndingWithSs(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("box", "boxes")]
    [InlineData("match", "matches")]
    [InlineData("brush", "brushes")]
    public void ToPlural_ShouldAddEs_WhenEndingWithXOrChOrSh(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("leaf", "leaves")]
    [InlineData("knife", "knives")]
    public void ToPlural_ShouldReplaceFOrFeWithVes_WhenEndingWithFOrFe(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("cat", "cats")]
    [InlineData("dog", "dogs")]
    [InlineData("book", "books")]
    public void ToPlural_ShouldAddS_WhenInputDoesNotMatchOtherRules(string input, string expected)
    {
        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("car", "car")]
    [InlineData("bus", "bus")]
    [InlineData("class", "class")]
    public void ToPlural_ShouldReturnSingularForm_WhenNumberIsOne(string input, string expected)
    {
        // Act
        var result = input.ToPlural(1);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToPlural_ShouldReturnEmptyString_WhenInputIsNull()
    {
        // Arrange
        string input = null;

        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToPlural_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = input.ToPlural();

        // Assert
        result.Should().BeEmpty();
    }
}
