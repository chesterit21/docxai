using System.Collections.Concurrent;
using Shuba.Worker.AI.Models;
using Shuba.Worker.AI.WebAI;
using Shuba.Worker.AI.WebAI.Selectors;

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
                Provider = new WebAiProvider { WebAiName = settings.DeepSeek.Name, WebAiUrl = settings.DeepSeek.Url },
                Selectors = DeepSeekSelectors.Get()
            },
            new()
            {
                Provider = new WebAiProvider { WebAiName = settings.Qwen.Name, WebAiUrl = settings.Qwen.Url },
                Selectors = QwenSelectors.Get()
            },
            new()
            {
                Provider = new WebAiProvider { WebAiName = settings.ZAi.Name, WebAiUrl = settings.ZAi.Url },
                Selectors = ZAiSelectors.Get()
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
                    _providerBusy[slot.Provider.WebAiName] = true; // mark busy
                    Console.WriteLine($"[ProviderManager] 🔒 Acquired: {slot.Provider.WebAiName}");
                    return slot;
                }
            }
        }
        Console.WriteLine("[ProviderManager] ⚠️ All providers busy!");
        return null;
    }

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
