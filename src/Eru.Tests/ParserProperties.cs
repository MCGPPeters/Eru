using System.Linq;
using System.Text.RegularExpressions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eru.Tests
{
    public class Generators
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        private static readonly Gen<NonEmptyString> WordGenerator =
            Arb.Default.NonEmptyString()
                .Generator
                .Where(s =>
                    !s.Get.Contains("\r") &&
                    !s.Get.Contains("\n") &&
                    !s.Get.Contains("\t"))
                .Where(s =>
                {
                    var regEx = new Regex(@"^[A-Za-z]*$");
                    return regEx.Match(s.Get).Success;
                });

        public static Arbitrary<NonEmptyString> ArbitraryWord => Arb.From(WordGenerator);
    }

    public class ParserProperties
    {
        public ParserProperties() => Arb.Register<Generators>();

        [Property(Verbose = true,
            DisplayName =
                "Parsing an item in a non empty string should return the first character and the remainder of the string")]
        public void Test2(NonEmptyString input)
        {
            var itemParser = Parser.Item();

            var parsedEmptyString = itemParser.Parse(input.Get);

            var firstCharacter = input.Get[0];
            Assert.Equal(firstCharacter, parsedEmptyString.First().parsedToken);
            Assert.Equal(input.Get.ToCharArray().Skip(1), parsedEmptyString.First().remainder);
        }

        [Property(Verbose = true, DisplayName = "The parser that always fails doesn't return a parsed character")]
        public void Test3(NonEmptyString input)
        {
            var fail = Parser.Fail<char>();

            var parsedString = fail.Parse(input.Get);

            Assert.Empty(parsedString);
        }

        [Property(Verbose = true,
            DisplayName = "The parser that always succeeds returnd the provided value and the unaltered input")]
        public void Test4(NonEmptyString value, NonEmptyString input)
        {
            var succeed = Parser.Return(value);

            var parsedString = succeed.Parse(input.Get);

            Assert.Equal(value, parsedString.First().parsedToken);
            Assert.Equal(input.Get.ToCharArray(), parsedString.First().remainder);
        }

        [Property(Verbose = true)]
        public void Parsing_a_non_empty_string_returns_the_whole_word(
            NonEmptyString input)
        {
            var wordParser = Parser.Word();

            var parsedWord = wordParser.Parse(input.Get);

            Assert.Equal(input.Get, parsedWord.First().parsedToken);
        }

        [Fact(DisplayName = "The parser of upper case letters")]
        public void Parsing_a_string_where_the_next_character_is_an_uppercase_letter_parses_that_character()
        {
            var upper = Parser.Upper();

            var parsedCharacter = upper.Parse("Hello");

            Assert.Equal('H', parsedCharacter.First().parsedToken);
            Assert.Equal("ello", new string(parsedCharacter[0].remainder));
        }

        [Fact(DisplayName = "Parsing the empty string should return an empty result")]
        public void Test1()
        {
            var itemParser = Parser.Item();

            var parsedEmptyString = itemParser.Parse("");

            Assert.Empty(parsedEmptyString);
        }
    }
}