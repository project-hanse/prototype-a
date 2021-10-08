using System;

namespace PipelineService.Extensions
{
    public static class StringExtensions
    {
        public static string LastChars(this string input, int charCount)
        {
            if (input == null || input.Length < charCount)
            {
                return input;
            }

            return input[^charCount..];
        }

        public static string ReplaceLastOccurenceOf(this string input, string search, string replace)
        {
            var place = input.LastIndexOf(search, StringComparison.Ordinal);

            if (place == -1)
                return input;

            var result = input.Remove(place, search.Length).Insert(place, replace);
            return result;
        }
    }
}