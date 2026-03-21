
private static string? ExtractJsonBySessionId(string bodyText, string? sessionId)
{
    if (string.IsNullOrWhiteSpace(bodyText)) return null;

    var candidates = new List<string>();

    int searchFrom = 0;
    while (true)
    {
        int sidIdx = bodyText.IndexOf("\"session_id\"", searchFrom, StringComparison.Ordinal);
        if (sidIdx < 0) break;

        // Walk BACKWARD dari sidIdx → cari root '{' pembuka
        int rootOpen = -1;
        int depth = 0;
        for (int i = sidIdx; i >= 0; i--)
        {
            if (bodyText[i] == '}') depth++;
            else if (bodyText[i] == '{')
            {
                if (depth == 0) { rootOpen = i; break; }
                depth--;
            }
        }

        if (rootOpen >= 0)
        {
            var extracted = ExtractBalancedJson(bodyText, rootOpen);
            if (extracted != null && !candidates.Contains(extracted))
                candidates.Add(extracted);
        }

        searchFrom = sidIdx + 1;
    }

    if (candidates.Count == 0)
    {
        Console.WriteLine("[ExtractJson] ⚠️ No JSON with session_id found.");
        return null;
    }

    Console.WriteLine($"[ExtractJson] Found {candidates.Count} JSON candidate(s).");

    var realCandidates = candidates.Where(c => !IsTemplateResponse(c)).ToList();
    var pool = realCandidates.Count > 0 ? realCandidates : candidates;

    if (!string.IsNullOrEmpty(sessionId))
    {
        var match = pool.LastOrDefault(c => c.Contains(sessionId));
        if (match != null)
        {
            Console.WriteLine($"[ExtractJson] ✅ Matched session_id ({match.Length} chars).");
            return match;
        }
    }

    var last = pool.Last();
    Console.WriteLine($"[ExtractJson] ℹ️ Using last candidate ({last.Length} chars).");
    return last;
}

   /// <summary>
    /// Returns true if JSON still contains "..." placeholder (template/prompt echo).
    /// </summary>
    private static bool IsTemplateResponse(string json)
        => json.Contains("\"...\"") || json.Contains(": \"...\"");
// ── TAMBAH method baru ini (helper untuk ExtractJsonBySessionId) ──

private static string? ExtractBalancedJson(string text, int openPos)
{
    int depth = 0;
    bool inString = false;
    bool escape = false;

    for (int i = openPos; i < text.Length; i++)
    {
        char c = text[i];

        if (escape)              { escape = false; continue; }
        if (c == '\\' && inString) { escape = true; continue; }
        if (c == '"')            { inString = !inString; continue; }
        if (inString)            continue;

        if      (c == '{') depth++;
        else if (c == '}')
        {
            depth--;
            if (depth == 0)
                return text[openPos..(i + 1)];
        }
    }

    return null; // unbalanced
}
