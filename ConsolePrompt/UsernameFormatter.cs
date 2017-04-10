using System;
using System.Collections.Generic;

namespace ConsolePrompt
{
    internal class UsernameFormatter : IPromptFormatter
    {
        public IEnumerable<string> Format(string[] args)
        {
            yield return $"|g{Environment.UserDomainName}\\{Environment.UserName}";
        }
    }
}