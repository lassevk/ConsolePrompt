using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsolePrompt
{
    class Program
    {
        private static readonly Dictionary<char, string> _Colors =
            new Dictionary<char, string>
            {
                ['R'] = "\x1b[1;31m",
                ['r'] = "\x1b[0;31m",
                ['G'] = "\x1b[1;32m",
                ['g'] = "\x1b[0;32m",
                ['C'] = "\x1b[1;36m",
                ['c'] = "\x1b[0;36m",
                ['B'] = "\x1b[1;34m",
                ['b'] = "\x1b[0;34m",
            };

        static void Main(string[] args)
        {
            var formatters =
                new IPromptFormatter[]
                {
                    new AdminFormatter(),
                    new UsernameFormatter(),
                    new CurrentDirectoryFormatter(),
                    new GitStatusFormatter()
                };

            var parts =
                (from formatter in formatters
                 from part in formatter.Format(args)
                 where !string.IsNullOrWhiteSpace(part)
                 select part).ToList();

            var output = string.Join(" ", parts);
            int index = 0;
            while (index < output.Length)
            {
                int nextColorIndex = output.IndexOf("|", index);
                if (nextColorIndex < 0)
                {
                    Console.Write(output.Substring(index));
                    index = output.Length;
                }
                else
                {
                    if (nextColorIndex > index)
                    {
                        Console.Write(output.Substring(index, nextColorIndex - index));
                        index = nextColorIndex;
                    }

                    index++;
                    if (_Colors.TryGetValue(output[index], out var color))
                        Console.Write(color);
                    else
                        throw new InvalidOperationException($"Unsupported color '{output[index]}'");

                    index++;
                }
            }

            Console.WriteLine("\x1b[0m");
        }
    }
}