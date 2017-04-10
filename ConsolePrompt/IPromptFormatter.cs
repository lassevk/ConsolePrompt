using System.Collections.Generic;
using System.Text;

namespace ConsolePrompt
{
    internal interface IPromptFormatter
    {
        IEnumerable<string> Format(string[] args);
    }
}