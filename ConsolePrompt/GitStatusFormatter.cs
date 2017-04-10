using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using JetBrains.Annotations;

namespace ConsolePrompt
{
    public class GitStatusFormatter : IPromptFormatter
    {
        public IEnumerable<string> Format(string[] args)
        {
            var repoRoot = FindRepositoryRoot(Environment.CurrentDirectory);
            if (repoRoot == null)
                yield break;

            ConfigureGit();
            var repo = new Repository(repoRoot);
            var output = new StringBuilder();
            output.Append("|c(");

            var currentBranch = repo.GetCurrentBranch();
            if (!string.IsNullOrWhiteSpace(currentBranch))
            {
                output.Append(currentBranch);

                var upstreamBranch = repo.GetUpstreamBranch();
                if (!string.IsNullOrWhiteSpace(upstreamBranch))
                {
                    output.Append(" -> ").Append(upstreamBranch);

                    var (ahead, behind) = repo.GetAheadBehindInformation(currentBranch, upstreamBranch);
                    if (ahead != 0 && behind != 0)
                        output.Append($" |C+{ahead}/-{behind}");
                    else if (ahead != 0)
                        output.Append($" |C+{ahead}");
                    else if (behind != 0)
                        output.Append($" |C-{behind}|R (FF)");
                }
            }
            else
                output.Append("empty");

            var states = GetRepositoryStates(repoRoot, repo);
            if (states.Count > 0)
            {
                output.Append("|C");
                foreach (var state in states)
                    output.Append(' ').Append(state);
            }

            output.Append("|c)");
            yield return output.ToString();
        }

        private static void ConfigureGit()
        {
            Environment.SetEnvironmentVariable("GIT_CURL_VERBOSE", string.Empty);
            Environment.SetEnvironmentVariable("GIT_TRACE", string.Empty);
        }

        private static string FindRepositoryRoot(string startingPath)
        {
            var path = Path.GetFullPath(startingPath);
            while (true)
            {
                var gitObjectPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitObjectPath) || File.Exists(gitObjectPath))
                    return path;

                if (File.Exists(Path.Combine(path, "HEAD")) && Directory.Exists(Path.Combine(path, "objects")))
                    return path;

                var parentPath = Path.GetFullPath(Path.Combine(path, ".."));
                if (parentPath == path)
                    return null;

                path = parentPath;
            }
        }

        [NotNull]
        private static List<string> GetRepositoryStates([NotNull] string repositoryPath, [NotNull] Repository repository)
        {
            var gitObjectPath = Path.Combine(repositoryPath, ".git");
            if (File.Exists(gitObjectPath))
                return GetStatesOfWorkTree(gitObjectPath, repository);
            return GetStatesOfRepository(gitObjectPath, repository);
        }

        [NotNull]
        private static List<string> GetStatesOfWorkTree([NotNull] string gitObjectPath, [NotNull] Repository repository)
        {
            var gitdir = File.ReadAllText(gitObjectPath);
            var ma = Regex.Match(gitdir, @"^gitdir:\s+(?<path>.*)$");
            if (!ma.Success)
                return new List<string>();

            var workTreeDir = ma.Groups["path"]?.Value;
            if (string.IsNullOrWhiteSpace(workTreeDir))
                return new List<string>();

            return GetStatesOfRepository(workTreeDir, repository);
        }

        [NotNull]
        private static List<string> GetStatesOfRepository([NotNull] string repositoryPath, [NotNull] Repository repository)
        {
            var result = new List<string>();
            if (File.Exists(Path.Combine(repositoryPath, "MERGE_HEAD")))
            {
                result.Add("MERGE");
                if (repository.HasUnmergedChanges())
                    result.Add("CONFLICT");
            }
            if (Directory.Exists(Path.Combine(repositoryPath, "rebase-apply")) || Directory.Exists(Path.Combine(repositoryPath, "rebase-merge")))
            {
                result.Add("REBASE");
                if (repository.HasUnmergedChanges())
                    result.Add("CONFLICT");
            }
            if (File.Exists(Path.Combine(repositoryPath, "BISECT_START")))
            {
                result.Add("BISECT");
            }
            if (File.Exists(Path.Combine(repositoryPath, "CHERRY_PICK_HEAD")))
            {
                result.Add("CHERRY");
            }

            return result;
        }

    }
}
