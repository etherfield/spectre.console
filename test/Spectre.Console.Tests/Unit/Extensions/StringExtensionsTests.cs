namespace Spectre.Console.Tests.Unit.Extensions;

public sealed class StringExtensionsTests
{
    public sealed class TheTruncateMethod
    {
        [Theory]
        [InlineData("𧼢A中")]
        [InlineData("A中𧼢")]
        [InlineData("中𧼢A")]
        [InlineData("中𧼢")]
        [InlineData("𧼢中")]
        [InlineData("中A")]
        [InlineData("A中")]
        [InlineData("𧼢")]
        [InlineData("中")]
        [InlineData("A")]
        public void Should_Truncate_String_With_Wide_Unicode_Characters(string inputText)
        {
            var maxExpectedWidth = inputText.Length;
            while (maxExpectedWidth >= 0)
            {
                var result = inputText.Truncate(maxExpectedWidth);
                result.Length.ShouldBeLessThanOrEqualTo(maxExpectedWidth);

                maxExpectedWidth--;
            }
        }

        [Theory]
        [InlineData("", 1)]
        [InlineData(null, 1)]
        [InlineData("", -1)]
        [InlineData(null, -1)]
        [InlineData("abc", 4)]
        public void Should_Return_The_Same_String_If_Parameters_Are_Incorrect(string inputText, int maxExpectedWidth)
        {
            var result = inputText.Truncate(maxExpectedWidth);
            result.ShouldBe(inputText);
        }
    }
}
