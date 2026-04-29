using System.Text;
using System.Text.RegularExpressions;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TextReplacementRule = TtsCommunicationTool.Core.Models.TextReplacement;

namespace TtsCommunicationTool.Infrastructure.TextReplacement;

/// <summary>
/// Single-pass text replacement: collects match positions across all rules
/// (in SortOrder), marks spans as consumed so earlier rules take priority
/// over overlapping later rules, then builds the output string.
/// </summary>
public sealed class TextReplacementService : ITextReplacementService
{
    private readonly IConfigService _config;

    public TextReplacementService(IConfigService config)
    {
        _config = config;
    }

    public string Apply(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var settings = _config.CurrentConfig.TextReplacements;
        if (!settings.IsEnabled) return input;

        var rules = settings.Rules
            .Where(r => r.IsEnabled && !string.IsNullOrEmpty(r.TriggerText))
            .OrderBy(r => r.SortOrder)
            .ToList();

        if (rules.Count == 0) return input;

        // Collect non-overlapping substitutions in (start, length, replacement) order
        // Earlier rules claim their spans first; later rules skip any overlap.
        var consumed = new List<(int Start, int End)>();
        var substitutions = new List<(int Start, int End, string Replacement)>();

        foreach (var rule in rules)
        {
            var matches = FindMatches(input, rule);
            foreach (var (start, end) in matches)
            {
                if (!Overlaps(consumed, start, end))
                {
                    consumed.Add((start, end));
                    substitutions.Add((start, end, rule.ReplacementText));
                }
            }
        }

        if (substitutions.Count == 0) return input;

        // Sort substitutions by start position and build the output
        substitutions.Sort((a, b) => a.Start.CompareTo(b.Start));

        var sb = new StringBuilder(input.Length);
        int cursor = 0;
        foreach (var (start, end, replacement) in substitutions)
        {
            if (start > cursor)
                sb.Append(input, cursor, start - cursor);
            sb.Append(replacement);
            cursor = end;
        }
        if (cursor < input.Length)
            sb.Append(input, cursor, input.Length - cursor);

        return sb.ToString();
    }

    private static IEnumerable<(int Start, int End)> FindMatches(string input, TextReplacementRule rule)
    {
        var regexOptions = RegexOptions.None;
        if (!rule.IsCaseSensitive)
            regexOptions |= RegexOptions.IgnoreCase;

        string pattern = rule.WholeWordOnly
            ? $@"\b{Regex.Escape(rule.TriggerText)}\b"
            : Regex.Escape(rule.TriggerText);

        var matches = Regex.Matches(input, pattern, regexOptions);
        foreach (Match m in matches)
            yield return (m.Index, m.Index + m.Length);
    }

    private static bool Overlaps(List<(int Start, int End)> consumed, int start, int end)
    {
        foreach (var (cs, ce) in consumed)
        {
            // Overlap if ranges are not strictly before or after each other
            if (start < ce && end > cs)
                return true;
        }
        return false;
    }
}
