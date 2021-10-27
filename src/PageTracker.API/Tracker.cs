using System.Collections.Concurrent;

namespace PageTracker.API;

public class Tracker
{
    //using concurrent disctionary to handle concurrency as it wraps thread safety and locking
    //value is a second concurrent dictionary with dummy value. Hashset could be used but
    //does not handle concurrency.
    ConcurrentDictionary<Uri, ConcurrentDictionary<string, bool>> _tracker = new();

    public void RegisterVisit(Uri url, string visitorId)
    {
        _tracker.AddOrUpdate(
            url, 
            new ConcurrentDictionary<string, bool>(new[]{ new KeyValuePair<string, bool>(visitorId, true) }),
            (uri, visitors) =>
            {
                visitors.AddOrUpdate(
                    visitorId,
                    true,
                    (visitorId, _) => { return _; });
                return visitors;
            });
    }

    public object GetUniqueVisitorCount(Uri url)
    {
        return _tracker.ContainsKey(url) && _tracker.TryGetValue(url, out var visitors)
                ? visitors.Count
                : 0;
    }
}
