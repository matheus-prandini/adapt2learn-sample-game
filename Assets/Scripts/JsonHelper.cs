using System;
using UnityEngine;

public static class JsonHelper
{
    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    public static T[] FromJson<T>(string json)
    {
        var wrapped = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        var wrapper = new Wrapper<T> { Items = array };
        return JsonUtility.ToJson(wrapper);
    }
}
