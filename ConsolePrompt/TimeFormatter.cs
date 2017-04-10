using System;
using System.Collections.Generic;

namespace ConsolePrompt
{
    internal class TimeFormatter : IPromptFormatter
    {
        public IEnumerable<string> Format(string[] args)
        {
            yield return $"|Y{DateTime.Now:T}";
        }
    }
}