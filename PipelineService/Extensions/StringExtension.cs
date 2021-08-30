namespace PipelineService.Extensions
{
    public static class StringExtension
    {
        public static string LastChars(this string input, int charCount)
        {
            if (input == null || input.Length < charCount)
            {
                return input;
            }

            return input[^charCount..];
        }
    }
}