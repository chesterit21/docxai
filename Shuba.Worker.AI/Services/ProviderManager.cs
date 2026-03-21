using System.Collections.Concurrent;
using Shuba.Worker.AI.Models;
using Shuba.Worker.AI.WebAI;


namespace Shuba.Worker.AI.Services;

/// <summary>
/// Thread-safe manager for 3 Web AI provider slots.
/// Ensures each concurrent task gets a different idle provider.
/// </summary>
public class ProviderManager
{
    private readonly ConcurrentDictionary<string, bool> _providerBusy = new();
    private readonly object _lock = new();

    private readonly List<ProviderSlot> _slots;

    public ProviderManager(WorkerSettings settings)
    {
        _slots = new List<ProviderSlot>
        {
            new()
            {
                Provider = new WebAiProvider { WebAiName = settings.Qwen.Name, WebAiUrl = settings.Qwen.Url },
                Selectors = settings.Qwen.Selectors
            },
            new()
            {
                Provider = new WebAiProvider { WebAiName = settings.Qwen.Name, WebAiUrl = settings.Qwen.Url },
                Selectors = settings.Qwen.Selectors
            },
            new()
            {
                Provider = new WebAiProvider { WebAiName = settings.Qwen.Name, WebAiUrl = settings.Qwen.Url },
                Selectors = settings.Qwen.Selectors
            }
        };

        foreach (var slot in _slots)
        {
            _providerBusy[slot.Provider.WebAiName] = false; // idle
        }
    }

    /// <summary>
    /// Acquire an idle provider slot. Returns null if all busy.
    /// </summary>
    public ProviderSlot? AcquireIdleProvider()
    {
        lock (_lock)
        {
            foreach (var slot in _slots)
            {
                if (!_providerBusy[slot.Provider.WebAiName])
                {
                    _providerBusy[slot.Provider.WebAiName] = true;
                    Console.WriteLine($"[ProviderManager] 🔒 Acquired: {slot.Provider.WebAiName}");
                    return slot;
                }
            }
        }
        Console.WriteLine("[ProviderManager] ⚠️ All providers busy!");
        return null;
    }

    /// <summary>
    /// Acquire provider based on file size:
    /// - < 20MB  → any provider
    /// - 20-80MB → prefer DeepSeek/ZAI, tapi Qwen juga bisa (akan di-chunk 19MB)
    /// - > 80MB  → HANYA DeepSeek/ZAI
    /// </summary>
    public ProviderSlot? AcquireProviderForFileSize(long fileSizeBytes)
    {
        const long SIZE_20MB = 20L * 1024 * 1024;
        const long SIZE_80MB = 80L * 1024 * 1024;

        lock (_lock)
        {
            if (fileSizeBytes > SIZE_20MB)
            {
                // > 80MB: HANYA DeepSeek atau ZAI
                foreach (var slot in _slots)
                {
                    if (!_providerBusy[slot.Provider.WebAiName] && IsLargeFileProvider(slot.Provider.WebAiName))
                    {
                        _providerBusy[slot.Provider.WebAiName] = true;
                        Console.WriteLine($"[ProviderManager] 🔒 Acquired (>80MB): {slot.Provider.WebAiName}");
                        return slot;
                    }
                }
                Console.WriteLine("[ProviderManager] ⚠️ >80MB file but DS/ZAI busy. Will retry.");
                return null;
            }

            if (fileSizeBytes < SIZE_20MB)
            {
                // Fallback: Qwen (will be chunked to 19MB)
                foreach (var slot in _slots)
                {
                    if (!_providerBusy[slot.Provider.WebAiName])
                    {
                        _providerBusy[slot.Provider.WebAiName] = true;
                        Console.WriteLine($"[ProviderManager] 🔒 Acquired (20-80MB, fallback Qwen): {slot.Provider.WebAiName}");
                        return slot;
                    }
                }
                Console.WriteLine("[ProviderManager] ⚠️ All providers busy for 20-80MB file.");
                return null;
            }

            // < 20MB: any provider
            return AcquireIdleProviderInternal();
        }
    }

    private ProviderSlot? AcquireIdleProviderInternal()
    {
        foreach (var slot in _slots)
        {
            if (!_providerBusy[slot.Provider.WebAiName])
            {
                _providerBusy[slot.Provider.WebAiName] = true;
                Console.WriteLine($"[ProviderManager] 🔒 Acquired (<20MB): {slot.Provider.WebAiName}");
                return slot;
            }
        }
        return null;
    }

    /// <summary>
    /// DeepSeek dan ZAI bisa handle file besar.
    /// </summary>
    public static bool IsLargeFileProvider(string providerName)
        => !providerName.Contains("Qwen", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Release a provider slot back to idle.
    /// </summary>
    public void ReleaseProvider(string providerName)
    {
        lock (_lock)
        {
            if (_providerBusy.ContainsKey(providerName))
            {
                _providerBusy[providerName] = false;
                Console.WriteLine($"[ProviderManager] 🔓 Released: {providerName}");
            }
        }
    }

    /// <summary>
    /// Get count of currently idle providers.
    /// </summary>
    public int IdleCount
    {
        get
        {
            lock (_lock)
            {
                return _providerBusy.Count(kv => !kv.Value);
            }
        }
    }
}

public class ProviderSlot
{
    public WebAiProvider Provider { get; set; } = new();
    public IList<WebAiSelector> Selectors { get; set; } = new List<WebAiSelector>();
}
