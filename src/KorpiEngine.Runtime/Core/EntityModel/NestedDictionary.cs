namespace KorpiEngine.Core.EntityModel;

internal class NestedDictionary<TKey, TInternalKey, TValue> : Dictionary<TKey, Dictionary<TInternalKey, TValue>> where TKey : notnull where TInternalKey : notnull
{
    public void Add(TKey key, TInternalKey internalKey, TValue value)
    {
        if (!ContainsKey(key))
            Add(key, new Dictionary<TInternalKey, TValue>());
        
        this[key].Add(internalKey, value);
    }
    
    
    public bool Remove(TKey key, TInternalKey internalKey)
    {
        if (!ContainsKey(key))
            return false;
        
        return this[key].ContainsKey(internalKey) && this[key].Remove(internalKey);
    }


    public IEnumerable<TValue> IterateValues(TKey key)
    {
        if (!ContainsKey(key))
            return new List<TValue>();
        
        return this[key].Values;
    }
}