﻿using System;
using System.Linq;

namespace Eru
{
    public delegate (T parsedCharacter, char[] remainder)[] Parser<T>(char[] input);

    public static class Parser
    {
        public static Parser<T> Fail<T>() =>
            _ =>
                new (T, char[])[0];

        public static Parser<TResult> Return<TResult>(TResult result) =>
            input =>
                new[] {(result, input)};

        public static Parser<char> Item() =>
            input => input.Any()
                ? Return(input.First())(input.Skip(1).ToArray())
                : Fail<char>()(input);

        public static (T parsedToken, char[] remainder)[] Parse<T>(this Parser<T> parser, char[] input) =>
            parser(input);

        public static (T parsedToken, char[] remainder)[] Parse<T>(this Parser<T> parser, string input) => string
            .IsNullOrWhiteSpace(input)
            ? new (T, char[])[0]
            : parser.Parse(input.ToCharArray());

        /// <summary>
        ///     Try this parser. Where it fails, try the appended one
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="nextParser">The next parser.</param>
        /// <returns></returns>
        public static Parser<T> Otherwise<T>(this Parser<T> parser, Parser<T> nextParser) => input =>
        {
            var parsedInput = parser(input);

            return parsedInput.Any()
                ? parsedInput
                : nextParser(input);
        };

        /// <summary>
        ///     First of all, the parser is applied to the input string,
        ///     yielding a list of (value, char[]) pairs / tuples. Now since func is a
        ///     function that takes a value and returns a parser,
        ///     it can be applied to each value (and unconsumed input char[]) in turn.
        ///     This results in a list of lists of (value, char[]) pairs,
        ///     that can then be flattened to a single list using concat
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TU"></typeparam>
        /// <param name="parser"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Parser<TU> Bind<T, TU>(this Parser<T> parser, Func<T, Parser<TU>> func) => input =>
        {
            var parsedInput = parser(input);

            var parsers = parsedInput
                .Select(tuple => func(tuple.parsedCharacter)(tuple.remainder)).ToArray();

            return parsers.Any()
                ? parsers.SelectMany(tuples => tuples).ToArray()
                : new (TU, char[])[0];
        };

        public static Parser<char> Where(Func<char, bool> predicate) =>
            Item().Bind(
                c => predicate(c)
                    ? Return(c)
                    : Fail<char>());

        public static Parser<char> Digit() =>
            input =>
                Where(char.IsDigit)(input);

        public static Parser<char> Lower() =>
            input =>
                Where(char.IsLower)(input);

        public static Parser<char> Upper() =>
            input =>
                Where(char.IsUpper)(input);

        public static Parser<char> Letter() =>
            Lower().Otherwise(Upper());

        public static Parser<char> Alphanumeric() =>
            Letter().Otherwise(Digit());

        public static Parser<char> Equals(char c) =>
            input =>
                Where(c.Equals)(input);

        public static Parser<string> Word() =>
            NonEmptyWord().Otherwise(Return(""));

        private static Parser<string> NonEmptyWord() =>
            Letter().Bind(c => Word().Bind(s => Return(c + s)));
    }
}
