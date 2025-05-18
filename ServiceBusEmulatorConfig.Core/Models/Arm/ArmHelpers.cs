using System.Text.RegularExpressions;

namespace ServiceBusEmulatorConfig.Core.Models.Arm
{
    public static class ArmHelpers
    {
        private static readonly Regex ConcatRegex = new Regex(@"\[concat\((.*?)\)\]", RegexOptions.Compiled);
        private static readonly Regex ParameterRegex = new Regex(@"parameters\('(.*?)'\)", RegexOptions.Compiled);
        
        /// <summary>
        /// Extracts the actual name from an ARM template resource name that may contain concat expression
        /// </summary>
        public static string ExtractNameFromArmExpression(string armExpression)
        {
            if (string.IsNullOrEmpty(armExpression))
                return string.Empty;
                
            // If it's a concat expression, extract the content
            if (armExpression.StartsWith("[concat("))
            {
                var match = ConcatRegex.Match(armExpression);
                if (match.Success && match.Groups.Count > 1)
                {
                    // Split by commas and take the parts that are literal strings (enclosed in single quotes)
                    var parts = match.Groups[1].Value.Split(',');
                    var result = string.Empty;
                    
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (trimmed.StartsWith("'") && trimmed.EndsWith("'"))
                        {
                            // Extract the content inside the quotes
                            result += trimmed.Substring(1, trimmed.Length - 2);
                        }
                        else if (trimmed.Contains("parameters("))
                        {
                            // Handle parameter references - we'll skip these for now
                            // This prevents the literal "')]" text from being included in the output
                            continue;
                        }
                    }
                    
                    return result;
                }
            }
            
            // Clean up any trailing "')]" or similar sequences
            if (armExpression.EndsWith("\u0027)]"))
            {
                armExpression = armExpression.Substring(0, armExpression.Length - 4);
            }
            
            return armExpression;
        }
    }
}
