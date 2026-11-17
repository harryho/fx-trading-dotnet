using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// L7 - static gates. These tests are not about behaviour; they keep the
/// working habits of the repository from degrading:
///
/// 1. the amount of legacy "lock (this)" must not grow (the code has 213
///    of them; new code is expected to use dedicated lock objects);
/// 2. the amount of direct Console writes in the library must not grow
///    (production code belongs in the log, not stdout);
/// 3. assembly versions must stay deterministic (no "1.2.3.*" wildcards -
///    they break deterministic builds and, with them, BenchmarkDotNet's
///    child builds);
/// 4. the names of unrelated commercial projects must never appear in
///    this public repository.
///
/// Each gate allows the count to DROP (cleanups are welcome) but never
/// to rise. When a gate's baseline is lowered on purpose, update the
/// constant and say so in the commit message.
/// </summary>
public class StaticGateTests
{
    private const int LockThisBaseline = 213;
    private const int ConsoleWriteBaseline = 214;

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "fx-trading-dotnet.sln")))
        {
            dir = dir.Parent!;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static List<FileInfo> SourceFiles(string root)
    {
        var result = new List<FileInfo>();
        var stack = new Stack<DirectoryInfo>();
        stack.Push(new DirectoryInfo(Path.Combine(root, "src", "fxpa")));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var dir in current.GetDirectories())
            {
                if (dir.Name is "bin" or "obj")
                {
                    continue;
                }

                stack.Push(dir);
            }

            result.AddRange(current.GetFiles("*.cs"));
        }

        return result;
    }

    private static int CountMatches(List<FileInfo> files, string pattern, StringComparison comparison)
    {
        int count = 0;
        foreach (var file in files)
        {
            string text = File.ReadAllText(file.FullName);
            int index = 0;
            while ((index = text.IndexOf(pattern, index, comparison)) >= 0)
            {
                count++;
                index += pattern.Length;
            }
        }

        return count;
    }

    [Fact]
    public void Lock_this_usage_does_not_grow()
    {
        int count = CountMatches(SourceFiles(RepoRoot()), "lock (this)", StringComparison.Ordinal)
                    + CountMatches(SourceFiles(RepoRoot()), "lock(this)", StringComparison.Ordinal);

        Assert.True(count <= LockThisBaseline,
            $"src/fxpa now contains {count} 'lock (this)' occurrences (baseline {LockThisBaseline}). " +
            "New code must lock on dedicated objects, not on 'this'. If you removed some, " +
            "lower the baseline in StaticGateTests.");
    }

    [Fact]
    public void Direct_console_writes_do_not_grow()
    {
        int count = CountMatches(SourceFiles(RepoRoot()), "Console.Write", StringComparison.Ordinal)
                    + CountMatches(SourceFiles(RepoRoot()), "Console.Error", StringComparison.Ordinal);

        Assert.True(count <= ConsoleWriteBaseline,
            $"src/fxpa now contains {count} direct Console writes (baseline {ConsoleWriteBaseline}). " +
            "Library code must use LogUtil, not the console. If you removed some, " +
            "lower the baseline in StaticGateTests.");
    }

    [Fact]
    public void Assembly_versions_stay_deterministic()
    {
        var offenders = new List<string>();
        foreach (var file in SourceFiles(RepoRoot()))
        {
            string text = File.ReadAllText(file.FullName);
            if (text.Contains("AssemblyVersion") && text.Contains("*"))
            {
                foreach (string line in text.Split('\n'))
                {
                    if (line.Contains("AssemblyVersion") && line.Contains("*"))
                    {
                        offenders.Add($"{Path.GetFileName(file.Name)}: {line.Trim()}");
                    }
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "Wildcard assembly versions break deterministic builds (CS8357) and with them " +
            "BenchmarkDotNet's child builds: " + string.Join("; ", offenders));
    }

    [Fact]
    public void Private_project_names_listed_locally_stay_out_of_the_repository()
    {
        // The original driver for this gate: this is the public edition of
        // FXPA, and files copied here from other (closed) work must not
        // carry over any names that would link the two.
        //
        // The names themselves are private, so they are NOT stored in this
        // repository - that would defeat the purpose. List them, one per
        // line, in `%USERPROFILE%\.fxpa-static-gate-names` (or the path
        // given by FXPA_STATIC_GATE_NAMES). When that file is present the
        // gate scans every text file in the repository for them; without
        // the file the gate passes (those names mean nothing to other
        // contributors anyway).
        string listPath = Environment.GetEnvironmentVariable("FXPA_STATIC_GATE_NAMES")
                          ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                              ".fxpa-static-gate-names");
        if (!File.Exists(listPath))
        {
            return;
        }

        string[] forbiddenNames = File.ReadAllLines(listPath)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("#"))
            .ToArray();

        if (forbiddenNames.Length == 0)
        {
            return;
        }

        var root = RepoRoot();
        var offenders = new List<string>();
        var stack = new Stack<DirectoryInfo>();
        stack.Push(new DirectoryInfo(root));

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            foreach (var dir in current.GetDirectories())
            {
                if (dir.Name is "bin" or "obj" or ".git" or "BenchmarkDotNet.Artifacts")
                {
                    continue;
                }

                stack.Push(dir);
            }

            foreach (var file in current.GetFiles())
            {
                if (file.Length == 0 || file.Length > 2 * 1024 * 1024)
                {
                    continue;
                }

                string text;
                try
                {
                    text = File.ReadAllText(file.FullName, Encoding.UTF8);
                }
                catch (DecoderFallbackException)
                {
                    continue; // binary file
                }

                foreach (string name in forbiddenNames)
                {
                    if (text.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        offenders.Add($"{Path.GetRelativePath(root, file.FullName)} contains a private name from {listPath}");
                    }
                }
            }
        }

        Assert.True(offenders.Count == 0,
            "This repository is the public, open-source edition of FXPA. The following files carry " +
            "private project names that must stay invisible here - remove the references: " +
            string.Join("; ", offenders));
    }
}
