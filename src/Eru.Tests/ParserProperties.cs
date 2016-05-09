using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eru.Tests
{
    public class Generators
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {

        private static readonly Gen<NonEmptyString> WordGenerator =
            Arb
                .Default.NonEmptyString()
                .Generator
                .Where(s => Regex.IsMatch(s.Get, "^[a-zA-Z]+$"));

        public static Arbitrary<NonEmptyString> ArbitraryWord => Arb.From(WordGenerator);

    }

    public class ParserProperties
    {
        public ParserProperties()
        {
            Arb.Register<Generators>();
        }


        [Fact]
        public void Parsing_the_empty_string_return_an_empty_result()
        {
            var itemParser = Parser.Item();

            var parsedEmptyString = itemParser.Parse("");

            parsedEmptyString.Should().BeEmpty();
        }

        [Property(Verbose = true)]
        public void Parsing_a_non_empty_string_returns_the_first_character_and_the_remainder_of_the_string(
            NonEmptyString input)
        {
            var itemParser = Parser.Item();

            var parsedEmptyString = itemParser.Parse(input.Get);

            var firstCharacter = input.Get[0];
            parsedEmptyString.First().Item1.Should().Be(firstCharacter);
            parsedEmptyString.First().Item2.Should().ContainInOrder(input.Get.ToCharArray().Skip(1));
        }

        [Property(Verbose = true)]
        public void The_parser_that_always_fails_always_returns_an_empty_result(
            string input)
        {
            var fail = Parser.Fail<char>();

            var parsedString = fail.Parse(input);

            parsedString.Should().BeEmpty();
        }

        [Property(Verbose = true)]
        public void The_parser_that_always_succeeds_always_returns_the_provided_value_and_the_unaltered_input(
            NonEmptyString value, NonEmptyString input)
        {
            var succeed = Parser.Return(value);
            
            var parsedString = succeed.Parse(input.Get);

            parsedString.First().Item1.Should().Be(value);
        }

        [Property(Verbose = true)]
        public void Parsing_a_non_empty_string_returns_the_whole_word(
            NonEmptyString input)
        {
            //arrange
            var wordParser = Parser.Word();

            //act
            var parsedWord = wordParser.Parse(input.Get);

            //assert
            parsedWord.First().Item1.Should().Be(input.Get);
        }
    }
}
