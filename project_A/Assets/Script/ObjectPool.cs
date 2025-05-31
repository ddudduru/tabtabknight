using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for MonoBehaviours.
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _root;
    private readonly Stack<T> _stack = new Stack<T>();

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _root = parent ?? new GameObject($"{typeof(T).Name}Pool").transform;
        for (int i = 0; i < initialSize; i++)
        {
            T inst = GameObject.Instantiate(_prefab, _root);
            inst.gameObject.SetActive(false);
            _stack.Push(inst);
        }
    }

    public T Pop(Vector3 position, Quaternion rotation)
    {
        T item;
        if (_stack.Count > 0)
        {
            item = _stack.Pop();
            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
        }
        else
        {
            item = GameObject.Instantiate(_prefab, position, rotation, _root);
        }
        return item;
    }

    public void Push(T item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(_root);
        _stack.Push(item);
    }
}