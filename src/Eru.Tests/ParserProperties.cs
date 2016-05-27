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
                Arb.Default.NonEmptyString().Generator
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
            NonEmptyString input)
        {
            var fail = Parser.Fail<char>();

            var parsedString = fail.Parse(input.Get);

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

        [Fact]
        public void Upper()
        {
            var upper = Parser.Upper();

            var parsedCharacter = upper.Parse("Hello");

            parsedCharacter.First().Item1.Should().Be('H');
            new string(parsedCharacter[0].Item2).Should().Be("ello");
        }

        [Fact]
        public void Word()
        {
            var upper = Parser.Word();

            var parsedString = upper.Parse("Hello");

            parsedString.First().Item1.Should().Be("Hello");
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