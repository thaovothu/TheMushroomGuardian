using System.Collections.Generic;

public class SharedVariableList<T>
{
    private List<SharedVariable<T>> list = new();
    public IReadOnlyList<SharedVariable<T>> List => list.AsReadOnly();
    public void Add(SharedVariable<T> v)
    {
        list.Add(v);
    }
    public void Remove(SharedVariable<T> v)
    {
        list.Remove(v); 
    }
    public SharedVariable<T> Get(string name)
    {
        foreach (var v in list)
        {
            if (v.Name == name)
                return v;
        }
        return null;    
    }
}