using System;
using System.Collections.Generic;

namespace ConsolePrompt
{
    internal class CurrentDirectoryFormatter : IPromptFormatter
    {
        public IEnumerable<string> Format(string[] args)
        {
            yield return $"|G{Environment.CurrentDirectory}";
        }
    }
}