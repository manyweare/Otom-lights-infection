using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Helper
{
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    // Shuffle list using Fisher-Yates shuffle.
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static IList<T> GetRandomItemsFromList<T>(IList<T> list, int numItemsToTake)
    {
        if (numItemsToTake > list.Count)
            numItemsToTake = list.Count;

        // Copy input list into new temporary list.
        IList<T> tempList = list.Select(item => (T)item).ToList();

        // Shuffle list to randomize order of elements.
        tempList.Shuffle();
        tempList.Take(numItemsToTake);

        return tempList;
    }

    public static string GetHex(int decimalValue)
    {
        string alpha = "0123456789ABCDEF";
        string result = "" + alpha[decimalValue];
        return result;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        Color32 c = color;
        string hex = "#" + c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static IEnumerator PanTextureLinear(GameObject obj, string textureName, Vector2 offset, float duration)
    {
        if (!obj.renderer.enabled)
            yield break;

        if (duration <= 0f)
            duration = 1000000000f;

        Vector2 uvOffset = Vector2.zero;
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            uvOffset += (offset * Time.deltaTime);
            obj.renderer.material.SetTextureOffset(textureName, uvOffset);
            elapsedTime = Time.time - startTime;
            yield return null;
        }
    }

    public static IEnumerator PanTextureEased(GameObject obj, string textureName, Vector2 offset, float duration)
    {
        if (!obj.renderer.enabled)
            yield break;

        if (duration <= 0f)
            Debug.LogWarning("WARNING: Duration for PanTexture needs to be > 0.", obj);

        Vector2 uvOffset = Vector2.zero;
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            uvOffset += (offset * Time.deltaTime) * 1.1f;
            obj.renderer.material.SetTextureOffset(textureName, uvOffset);
            elapsedTime = Time.time - startTime;
            yield return null;
        }
    }
}