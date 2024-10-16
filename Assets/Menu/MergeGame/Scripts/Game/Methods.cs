 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; 

public static class Methods
{
    public enum Side
    {
        top, bottom, left, right, middle, topLeft, topRight, bottonLeft, bottomRight
    }
    private static System.Random rng = new System.Random();
  
    public static void RestartGame()
    {
        using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        const int kIntent_FLAG_ACTIVITY_CLEAR_TASK = 0x00008000;
        const int kIntent_FLAG_ACTIVITY_NEW_TASK = 0x10000000;

        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage",
            Application.identifier);

        // Start new activity
        intent.Call<AndroidJavaObject>("setFlags",
            kIntent_FLAG_ACTIVITY_NEW_TASK | kIntent_FLAG_ACTIVITY_CLEAR_TASK);
        currentActivity.Call("startActivity", intent);
        currentActivity.Call("finish");

        // Kill current process
        AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
        int pid = process.CallStatic<int>("myPid");
        process.CallStatic("killProcess", pid);
    }

    public static bool Contains(this Enum @enum, Enum value)
    {
        if (!@enum.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Any())
        {
            Debug.LogError("Non flag Enum " + @enum.GetType());
            return default;
        }
        return (@enum.GetHashCode() & value.GetHashCode()) == value.GetHashCode();
    }

    public static T Add<T>(this Enum @enum, Enum value) where T : Enum
    {
        if (!@enum.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Any())
        {
            Debug.LogError("Non flag Enum " + @enum.GetType());
            return default;
        }
        return (T)Enum.ToObject(@enum.GetType(), @enum.GetHashCode() | value.GetHashCode());
    }
    public static T Remove<T>(this Enum @enum, Enum value) where T : Enum
    {
        //напиши проверку если Enum Не не содержит атрибута   [System.Flags]
        if (!@enum.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Any())
        {
            Debug.LogError("Non flag Enum " + @enum.GetType());
            return default;
        }

        return (T)Enum.ToObject(@enum.GetType(), @enum.GetHashCode() & ~value.GetHashCode());
    }
    /// <summary>
    /// возвращает масив с просчитаными шансами по весу
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targets"></param>
    /// <param name="weights"></param>
    /// <param name="getName"></param>
    /// <returns></returns>
    public static List<string> GetWeightInfo<T>(List<T> targets, List<int> weights, Func<T, string> getName)
    {
        List<string> list = new List<string>();

        if (targets.Count != weights.Count)
        {
            Debug.LogError("Ошибка: размеры списков targets и weights не совпадают!");
            return list;
        }

        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            T target = targets[i];
            int weight = weights[i];
            string name = getName(target);
            string item = name + " " + Math.Round(weight / (float)totalWeight * 100, 1) + "%";
            list.Add(item);
        }

        return list;
    }
    public static void NullizeLinks<T>(this T original) where T : class
    {
        Type typeOfClass = typeof(T);
        var fields = typeOfClass.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var field in fields.Where(f => f.FieldType.IsClass || f.FieldType.IsInterface))
        {
            field.SetValue(original, null);
        }

    }

    public static void DestroyChilds(this Transform self)
    {
        List<Transform> childs = self.Cast<Transform>().ToList();
        foreach (Transform item in childs)
        {
            item.SetParent(null);
            GameObject.Destroy(item.gameObject);
        }
    }
    /// <summary>
    /// Возвращает новый массив с новым значением
    /// </summary>
    public static T[] Add<T>(this T[] array, T value)
    {
        var spellList = array.ToList();
        spellList.Add(value);
        return spellList.ToArray();
    }
    /// <summary>
    /// Возвращает новый массив с новым значением
    /// </summary>
    public static T[] Remove<T>(this T[] array, T value)
    {
        var spellList = array.ToList();
        spellList.Remove(value);
        return spellList.ToArray();
    }

    public static T NextAfterFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var found = false;
        foreach (var item in source)
        {
            if (found) return item;
            if (predicate(item)) found = true;
        }
        return default;
    }
    public static void ActiveOnly(this IEnumerable<GameObject> source, GameObject go, bool activate = true)
    {
        foreach (var item in source)
        {
            if (item == go) item.SetActive(activate);
            else item.SetActive(!activate);
        }
    }
    public static T NextAfterFirst<T>(this List<T> source, Func<T, bool> predicate)
    {
        var found = false;
        foreach (var item in source)
        {
            if (found) return item;
            if (predicate(item)) found = true;
        }
        return default;
    }
    public static T NextAfterLast<T>(this List<T> source, Func<T, bool> predicate)
    {
        var last = source.LastOrDefault(predicate);
        if (last == null) return default;
        int indexOfLast = source.IndexOf(last);
        if (source.Count <= indexOfLast + 1) return default;
        return source[indexOfLast + 1];
    }
    public static T NextAfterLast<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var last = source.LastOrDefault(predicate);
        if (last == null) return default;
        int indexOfLast = source.ToList().IndexOf(last);
        if (source.Count() <= indexOfLast + 1) return default;
        return source.ToList()[indexOfLast + 1];
    }
    public static void OrderChildsByComponent<T>(this Transform me, Func<T, int> getIndex, bool revese = false) where T : MonoBehaviour
    {
        List<T> list = new List<T>();
        foreach (Transform item in me)
        {
            list.Add(item.gameObject.GetComponent<T>());
        }

        if (!revese) list = list.OrderBy(el => getIndex(el)).ToList();
        else list = list.OrderByDescending(el => getIndex(el)).ToList();

        foreach (var item in list)
        {
            item.gameObject.transform.SetSiblingIndex(list.IndexOf(item));
        }
    }

    /// <summary>
    /// округление 12304 до 12k
    /// </summary>
    /// <returns></returns>
    public static string RoundToThousand(this int num, int startFrom = 10000)
    {
        if (num < 10000) return num.ToString();

        int roundedNumber = Mathf.RoundToInt(num / 1000.0f);
        return roundedNumber + "K";

    }
    public static string GetWordOfTMPro(this TMPro.TextMeshProUGUI pro)
    {
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(pro, Input.mousePosition, null);
        if (wordIndex != -1)
        {
            return pro.textInfo.wordInfo[wordIndex].GetWord();
        }
        else return "";
    }

    public static Vector2 RandomCircleSystem()
    {
        return new Vector2(GetRandomSystem(-1, 1f), GetRandomSystem(-1, 1f));
    }
    public static Vector3 RandomSphereSystem()
    {
        return new Vector3(GetRandomSystem(-1, 1f), GetRandomSystem(-1, 1f), GetRandomSystem(-1, 1f));
    }
    public static int GetRandomSystem(int min, int max)
    {
        return rng.Next(min, max);
    }
    public static float GetRandomSystem(float min, float max)
    {
        var randomDouble = min + (max - min) * rng.NextDouble();
        return Convert.ToSingle(randomDouble);
    }
    public static bool GetChance(this int percent, string from = "")
    {
        // Debug.Log($"GetChance " + UnityEngine.Random.seed);
        if (percent > 99) return true;
        if (percent < 1) return false;
        int random = UnityEngine.Random.Range(0, 100);
        return percent > random;
    }
    public static bool GetChanceSystem(this int percent, string from = "")
    {
        if (percent > 99) return true;
        if (percent < 1) return false;
        int random = GetRandomSystem(0, 100);
        return percent > random;
    }
    public static float ToFloat(this string text, string dot = ",")
    {
        //  Debug.Log($"parse string({text})");
        NumberFormatInfo formatInfo = new NumberFormatInfo();
        formatInfo.NumberDecimalSeparator = dot;
        return float.Parse(text, formatInfo);
    }
    /// <summary>
    /// возвращает указанный процент числа
    /// </summary>
    /// <param name="value"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public static int GetPercent(this int value, int percent)
    {
        return (int)((float)value / 100 * percent);
    }

    public static int RoundTo(this int value, int to = 5)
    {
        //  return Mathf.CeilToInt((float)number / to) * nearest;
        return Mathf.RoundToInt((float)value / (float)to) * to;
    }
    public static int GetPercent(this float value, int percent)
    {
        return (int)(value / 100 * percent);
    }
    /// <summary>
    /// Возвращает рандомное число между X Y
    /// </summary>
    public static float GetRandom(this Vector2 r)
    {
        // Debug.Log($"GetRandom " + UnityEngine.Random.seed);
        return UnityEngine.Random.Range(r.x, r.y);
    }
    public static void SetupWidth(this RectTransform rect, float width)
    {
        Vector2 sizeDelta = rect.sizeDelta;
        sizeDelta.x = width;
        rect.sizeDelta = sizeDelta;

    }
    public static void ShrinkToLeft(this RectTransform rectTransform, float shrinkValue)
    {
        Vector2 anchorPos = rectTransform.anchoredPosition;
        Vector2 sizeDelta = rectTransform.sizeDelta;

        anchorPos.x += shrinkValue;
        sizeDelta.x += shrinkValue * 2;

        rectTransform.anchoredPosition = anchorPos;
        rectTransform.sizeDelta = sizeDelta;
    }
    public static void SetupFullScreen(this RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
    public static void SetZeroZ(this Transform trans)
    {
        var p = trans.localPosition;
        p.z = 0;
        trans.localPosition = p;
    }
    public static void SetupHeight(this RectTransform rect, float height)
    {
        Vector2 sizeDelta = rect.sizeDelta;
        sizeDelta.y = height;
        rect.sizeDelta = sizeDelta;
    }
    public static void SetupRect(this RectTransform transform, Rect rect)
    {
        transform.anchoredPosition = new Vector2(rect.x, rect.y);
        transform.sizeDelta = new Vector2(rect.width, rect.height);
    }
    public static void SetAutoSpace(this GridLayoutGroup grid, int minSpacing = 10, int maxPadding = 10)
    {
        if (grid.transform.childCount < 2)
        {
            Debug.Log($"Недостаточно элементов({grid.transform.childCount}) для просчета ГРИД ");
            return;
        }
        grid.spacing = minSpacing * Vector2.one;
        grid.padding = new RectOffset(0, 0, 0, 0);

        float contentWidth = grid.GetComponent<RectTransform>().rect.width;
        float cellWidth = grid.cellSize.x + grid.spacing.x;
        int cellCount = Mathf.FloorToInt(contentWidth / cellWidth);
        int freeSpace = (int)contentWidth - (int)grid.cellSize.x * cellCount;
        int freeSpaceWithPadding = freeSpace - maxPadding * 2;
        if (freeSpaceWithPadding < minSpacing) freeSpaceWithPadding = freeSpace;
        else grid.padding = new RectOffset(maxPadding, maxPadding, maxPadding, maxPadding);
        int space = freeSpaceWithPadding / (cellCount - 1);
        grid.spacing = new Vector2(space, space);

    }
    public static int HorizontalCount(this GridLayoutGroup grid)
    {
        float width = grid.GetComponent<RectTransform>().rect.width;
        return (int)(width / ((grid.cellSize.x + grid.spacing.x + grid.padding.left + grid.padding.right) - grid.spacing.x));
    }
    public static void ScrollToChild(this ScrollRect scrollRect, RectTransform target)
    {

        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = scrollRect.viewport.localPosition;
        Vector2 childLocalPosition = target.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        scrollRect.content.localPosition = result;
    }
    public static void ScrollTo(this ScrollRect scrollRect, RectTransform target, RectTransform contentPanel)
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect.vertical && scrollRect.horizontal) Debug.LogError("HORIZONTAL AND VERTICAL");
        if (scrollRect.vertical == true)
        {
            contentPanel.anchoredPosition =
                    (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(0, contentPanel.position.y))
                    - (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(0, target.position.y));
        }
        if (scrollRect.horizontal == true)
        {
            contentPanel.anchoredPosition =
               (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(contentPanel.position.x, 0))
               - (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(target.position.x, 0));
        }
    }

    public static void ScrollTo(this ScrollRect scrollRect, RectTransform target, float offset, RectTransform contentPanel)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 targetPosition = Vector2.zero;
        Vector2 contentPosition = Vector2.zero;
        Vector2 scrollPosition = Vector3.zero;
        if (scrollRect.vertical && scrollRect.horizontal) Debug.LogError("HORIZONTAL AND VERTICAL");
        if (scrollRect.vertical == true)
        {
            targetPosition = scrollRect.transform.InverseTransformPoint(new Vector3(0, target.position.y));
            contentPosition = scrollRect.transform.InverseTransformPoint(new Vector3(0, contentPanel.position.y));
            scrollPosition = contentPosition - targetPosition + new Vector2(0, offset);
        }
        if (scrollRect.horizontal == true)
        {
            targetPosition = scrollRect.transform.InverseTransformPoint(new Vector3(target.position.x, 0));
            contentPosition = scrollRect.transform.InverseTransformPoint(new Vector3(contentPanel.position.x, 0));
            scrollPosition = contentPosition - targetPosition + new Vector2(offset, 0);
        }

        contentPanel.anchoredPosition = scrollPosition;
    }
    public static void ScrollTo(this ScrollRect scrollRect, Vector3 target, RectTransform contentPanel)
    {
        Canvas.ForceUpdateCanvases();
        if (scrollRect.vertical && scrollRect.horizontal) Debug.LogError("HORIZONTAL AND VERTICAL");
        if (scrollRect.vertical == true)
        {
            contentPanel.anchoredPosition =
                    (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(0, contentPanel.position.y))
                    - (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(0, target.y));
        }
        if (scrollRect.horizontal == true)
        {
            contentPanel.anchoredPosition =
               (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(contentPanel.position.x, 0))
               - (Vector2)scrollRect.transform.InverseTransformPoint(new Vector3(target.x, 0));
        }
    }
    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
    public static Vector3 GetPositionWithZOffset(this Transform myTransform, Transform targetTransform, float distanceBefor)
    {
        Vector3 direction = (myTransform.position - targetTransform.position).normalized;
        Vector3 targetPosition = targetTransform.position + direction * distanceBefor;
        return targetPosition;
    }
    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, -100);
    }
    public static void ScrollToRight(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
    /// <summary>
    /// возвращает рандомный элеметн списка
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="myList"></param>
    /// <returns></returns>
    /// 
    public static bool IsContainsAny<T>(this List<T> one, List<T> target)
    {
        for (int i = 0; i < target.Count; i++)
        {
            if (one.Contains(target[i]))
                return true;
        }
        return false;
    }
    public static T GetRandomElement<T>(this List<T> myList)
    {
        //   Debug.Log("GetRandomElement");
        int r = UnityEngine.Random.Range(0, myList.Count);
        if (myList.Count == 0)
        {
            Debug.LogError("Список пуст " + typeof(T));
            return default;
        }
        return myList[r];
    }
    /// <summary>
    /// возвращает раномное ко-во уникальных элементов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="myList"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<T> GetRandomElements<T>(this List<T> myList, int count)
    {
        //   Debug.Log("GetRandomElements" + UnityEngine.Random.seed);
        int r = UnityEngine.Random.Range(0, myList.Count);
        if (myList.Count == 0)
        {
            Debug.LogError("Список пуст " + typeof(T));
            return myList;
        }
        if (count >= myList.Count) return myList;


        List<T> randomElements = new List<T>(myList);

        for (int i = myList.Count - 1; i >= count; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            randomElements.RemoveAt(randomIndex);
        }
        return randomElements;
    }
    public static T GetRandomElementSystem<T>(this List<T> myList)
    {
        int r = GetRandomSystem(0, myList.Count);
        return myList[r];
    }
    public static void RemoveDuplicates(this List<string> list)
    {
        List<string> uniqueList = new List<string>();

        foreach (string item in list)
        {
            if (!uniqueList.Contains(item))
            {
                uniqueList.Add(item);
            }
        }

        list.Clear();
        list.AddRange(uniqueList);
    }
    public static void RemoveDuplicates<T, V>(this List<T> list, Func<T, V> dublicateTarget)
    {
        List<T> uniqueList = new List<T>();

        foreach (T item in list)
        {
            V itemValue = dublicateTarget(item);
            bool isDuplicate = uniqueList.Any(x => dublicateTarget(x).Equals(itemValue));
            if (!isDuplicate)
            {
                uniqueList.Add(item);
            }
        }

        list.Clear();
        list.AddRange(uniqueList);
    }
    /// <summary>
    /// возвращает послдение Х обьектов
    /// </summary>
    public static List<T> GetLastCount<T>(this List<T> myList, int count)
    {
        //напиши логику взятия последнийх count обьектов из массива или всех что есть
        if (count > myList.Count) return myList;
        return myList.GetRange(myList.Count - count, count);
    }
    /// <summary>
    /// Перемешивание Списка
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

    }
    /// <summary>
    /// Перемешивание Списка
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void ShuffleSystem<T>(this IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void Move<T>(this List<T> list, int i, int j)
    {
        var elem = list[i];
        list.RemoveAt(i);
        list.Insert(j, elem);
    }
    public static void Swap<T>(this List<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    public static void MakeForOtherList<T1, T2>(this List<T1> owner, List<T2> child, System.Action<T1, T2> action)
    {

        for (int i = 0; i < owner.Count; i++)
        {
            if (child.Count == i)
            {
                Debug.LogError("Child list out of index");
                return;
            }
            action(owner[i], child[i]);
        }
    }
    public static List<string> ToList(this string self, char separator)
    {
        if (string.IsNullOrEmpty(self)) return new List<string>();
        return self.Split(separator).ToList();
    }


    /// <summary>
    /// идентичны ли массивы по элементам
    /// </summary>
    public static bool IsEqualArray<T>(this List<T> array, List<T> other)
    {
        if (array.Count == 0 && other.Count == 0) return true;
        if (array.Count != other.Count) return false;

        for (int i = 0; i < array.Count; i++)
        {
            if (!array[i].Equals(other[i])) return false;
        }
        return true;
    }
    /// <summary>
    /// начинается ли this с таких же элементов  как full
    /// </summary>

    public static bool IsEqualStart<T>(this List<T> array, List<T> full)
    {
        if (array.Count > full.Count) return false;
        if (array.Count == 0)
        {
            Debug.Log("empty");
            return true;
        }


        for (int i = 0; i < full.Count; i++)
        {
            if (i >= array.Count)
            {
                Debug.Log("2");
                return true;
            }
            if (!full[i].Equals(array[i])) return false;
        }
        Debug.Log("1");
        Debug.Log(array.ToStringArray() + "==" + full.ToStringArray());
        return true;
    }
    public static string ToStringArray<T>(this IEnumerable<T> array, string separator = " ")
    {
        string str = "";
        foreach (var item in array)
        {
            str += item + separator;
        }
        if (str.Length > 0) str = str.Remove(str.Length - 1);
        return str;
    }
    /// <summary>
    /// Возвращает значение из скобок
    /// </summary>
    public static string GetWordFromBrackets(this string text, char bracket = '[', char bracket2 = ']')
    {
        int start = text.IndexOf(bracket) + 1;
        int end = text.IndexOf(bracket2, start);
        return text.Substring(start, end - start);
    }
    /// <summary>
    /// возвращает текст без скобок и значений в них
    /// </summary>
    public static string TrimBracketsInclude(this string text, char bracket = '[', char bracket2 = ']')
    {
        int start = text.IndexOf(bracket);
        int end = text.IndexOf(bracket2, start);
        string trimTarget = text.Substring(start, end + 1 - start);
        return text.Replace(trimTarget, "");
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> array)
    {
        return array == null || array.Count() == 0;
    }
    public static Dictionary<string, string> ToDictionary(this IEnumerable<string> array, char separator = ':')
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach (var item in array)
        {
            var splited = item.Split(separator);
            dict.Add(splited[0], splited[1]);
        }
        return dict;
    }

    public static List<string> GetWordsFromBrackets(this string text, char separator = '@')
    {
        string pattern = "\\" + separator + "(.*?)" + "\\" + separator;
        MatchCollection matches = Regex.Matches(text, pattern);

        List<string> wordsInText = new List<string>();
        foreach (Match match in matches)
        {
            wordsInText.Add(match.Groups[1].Value);
        }

        if (wordsInText.Count == 0)
        {
            wordsInText.Add(text);
        }

        return wordsInText;
    }

    public static string GetValueFromJson(string json, string key)
    {
        //   input = input.Replace("\\", "");
        string pattern = $"{key}\":\"?(.*?)\"?(,|\\}})";
        Match match = Regex.Match(json, pattern);

        if (match.Success) return match.Groups[1].Value;
        else return "";
    }

    public static string AddValueToString(this string text, string addValue, char separator)
    {
        if (string.IsNullOrEmpty(text)) text += addValue;
        else text += separator + addValue;
        return text;

    }
    public static string RemoveLast(this string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        if (text.Length < 1) return "";
        return text.Remove(text.Length - 1);


    }
    public static int ToInt(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("Pasing field Empty");
            return 0;
        }
        if (int.TryParse(text, out int v))
        {
            return v;
        }
        else
        {
            Debug.LogError($"Pasing field error ({text})");
            return 0;
        }

    }
    public static bool ToBool(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("Pasing field Empty");
            return false;
        }
        if (text == "True" || text == "true")
        {
            return true;
        }
        if (text == "False" || text == "false")
        {
            return false;
        }
        Debug.LogError("Parsing error to bool " + text);
        return false;

    }

    public static bool IsNullOrEmpty(this string text)
    {
        return string.IsNullOrEmpty(text);
    }
    public static void FadeAll(this Transform t, float value)
    {
        foreach (var item in GetAllChilds(t))
        {
            var text = item.GetComponent<TextMeshProUGUI>();
            var image = item.GetComponent<Image>();
            if (text) text.Fade(value);
            if (image) image.Fade(value);
        }
    }
    public static void DoFadeAll(this Transform t, float value, float duration, params string[] igroneNames)
    {
        foreach (var item in GetAllChilds(t))
        {
            if (igroneNames.ToList().Contains(item.name)) continue;
            var text = item.GetComponent<TextMeshProUGUI>();
            var image = item.GetComponent<Image>();
            //if (text) text.DOFade(value, duration).SetUpdate(true).SetLink(item.gameObject);
            //if (image) image.DOFade(value, duration).SetUpdate(true).SetLink(item.gameObject);
        }
    }
    public static void SetMateraialToImages(this Transform t, Material mat)
    {
        foreach (var item in GetAllChilds(t))
        {
            var image = item.GetComponent<Image>();
            if (image && !image.GetComponent<Mask>()) image.material = mat;
        }
    }
    public static void RemoveMateraialFromImages(this Transform t)
    {
        foreach (var item in GetAllChilds(t))
        {
            var image = item.GetComponent<Image>();
            if (image) image.material = null;
        }
    }
    public static void Fade(this Image self, float a = 0)
    {
        self.color = new Color(self.color.r, self.color.g, self.color.b, a);
    }
    public static void Fade(this TextMeshProUGUI self, float a = 0)
    {
        self.color = new Color(self.color.r, self.color.g, self.color.b, a);
    }
    public static void Fade(this MeshRenderer self, float a = 0)
    {
        self.material.color = new Color(self.material.color.r, self.material.color.g, self.material.color.b, a);
    }


    //public static Tween DoStandartShakeScale(this Transform t)
    //{
    //    return t.DOShakeScale(1f, 0.1f, 3, 90, false).SetUpdate(true).SetLoops(-1, LoopType.Yoyo).SetLink(t.gameObject);
    //}


    /// <summary>
    /// выводил все поля класса и вложенных класов кроме моно
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="split"></param>
    public static void LogFields(this object obj, string split = "\n")
    {

        if (obj == null)
        {
            Debug.LogWarning("Object is null");
            return;
        }

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string logString = obj.GetType().Name + ":" + split;
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType.Namespace == null)
            {
                object value = field.GetValue(obj);
                string valueString = GetValueString(value);
                if (field.FieldType.IsArray)
                {
                    valueString = "[" + valueString + "]";
                }
                logString += $"{field.Name}: {valueString}" + split;
            }
            else

            if (field.DeclaringType != typeof(MonoBehaviour)
                && field.DeclaringType != typeof(ScriptableObject)
                    && !field.FieldType.Namespace.StartsWith("UnityEngine")
                    && !field.FieldType.Namespace.StartsWith("UnityEditor"))
            {
                object value = field.GetValue(obj);
                string valueString = GetValueString(value);
                if (field.FieldType.IsArray)
                {
                    valueString = "[" + valueString + "]";
                }
                logString += $"{field.Name}: {valueString}" + split;
            }
        }

        Debug.Log(logString);
    }
    public static void LogField(this object obj)
    {
        string split = "\n";
        if (obj == null)
        {
            Debug.LogWarning("Object is null");
            return;
        }

        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string logString = obj.GetType().Name + ":" + split;
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType.Namespace == null)
            {
                object value = field.GetValue(obj);
                string valueString = GetValueString(value);
                if (field.FieldType.IsArray)
                {
                    valueString = "[" + valueString + "]";
                }
                logString += $"{field.Name}: {valueString}" + split;
            }
            else

            if (field.DeclaringType != typeof(MonoBehaviour)
                && field.DeclaringType != typeof(ScriptableObject)
                    && !field.FieldType.Namespace.StartsWith("UnityEngine")
                    && !field.FieldType.Namespace.StartsWith("UnityEditor"))
            {
                object value = field.GetValue(obj);
                string valueString = GetValueString(value);
                if (field.FieldType.IsArray)
                {
                    valueString = "[" + valueString + "]";
                }
                logString += $"{field.Name}: {valueString}" + split;
            }
        }

        Debug.Log(logString);
    }
    private static string GetValueString(object value)
    {
        if (value == null)
        {
            return "null";
        }

        Type type = value.GetType();
        if (type.IsArray)
        {
            Array array = (Array)value;
            string arrayString = "";
            for (int i = 0; i < array.Length; i++)
            {
                object element = array.GetValue(i);
                string elementString = GetValueString(element);
                arrayString += elementString;
                if (i < array.Length - 1)
                {
                    arrayString += ", ";
                }
            }
            return arrayString;
        }
        else if (type.IsClass && type != typeof(string))
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            string objectString = "{";
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.Namespace == null)
                {
                    object fieldValue = field.GetValue(value);
                    string fieldValueString = GetValueString(fieldValue);
                    objectString += $"{field.Name}: {fieldValueString}, ";
                }
                else if (field.DeclaringType != typeof(MonoBehaviour) &&
                     field.DeclaringType != typeof(ScriptableObject) &&
                     !field.FieldType.Namespace.StartsWith("UnityEngine") &&
                     !field.FieldType.Namespace.StartsWith("UnityEditor"))
                {
                    object fieldValue = field.GetValue(value);
                    string fieldValueString = GetValueString(fieldValue);
                    objectString += $"{field.Name}: {fieldValueString}, ";
                }
            }
            if (objectString.EndsWith(", "))
            {
                objectString = objectString.Substring(0, objectString.Length - 2);
            }
            objectString += "}";
            return objectString;
        }
        else
        {
            return value.ToString();
        }
    }
    //public static Tween LoopRotate(this Transform transform, float speed = 1)
    //{
    //    return transform
    //         .DORotate(transform.localEulerAngles + Vector3.forward * 360, speed, RotateMode.FastBeyond360)
    //         .SetUpdate(true).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear); ;
    //}

    public static List<Transform> GetParents(this Transform transform)
    {
        List<Transform> parents = new List<Transform>();
        parents.Add(transform);
        Transform t = transform;
        while (t.parent != null)
        {
            parents.Add(t.parent);
            t = t.parent;
        }
        return parents;
    }
    public static void SelectLog<T>(this List<T> list, Func<T, object> selector, string separator = " ", string prefix = " ")
    {
        Debug.Log(prefix + list.Select(selector).ToList().ToStringArray(separator));
    }
    public static bool NullOrEmpty<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return true;
        }
        return false;
    }
    public static T GetRandomByWeight<T>(int[] weight, T[] items)
    {
        List<PriorityClass<T>> proorityClasses = new();
        for (int i = 0; i < weight.Length; i++)
        {
            proorityClasses.Add(new PriorityClass<T>(items[i], weight[i]));
        }
        return GetRandomByWeight(proorityClasses, (r) => r.priority).main;
    }
    public static T GetRandomByWeight<T>(IReadOnlyList<T> list, Func<T, float> weightGetter)
    {
        //Debug.Log("GetRandomByWeight");
        List<T> newList = new List<T>();
        foreach (var item in list)
        {
            if (weightGetter(item) == 0) continue;
            newList.Add(item);
        }
        float weightSum = 0;
        foreach (var element in newList)
        {
            var weight = weightGetter(element);
            if (weight < 0)
            {
                Debug.LogError($"Element {element} have weight {weight} but weight must me >= 0");
            }
            weightSum += weight;
        }
        if (weightSum <= 0)
        {
            Debug.LogError("List must contain at least one element with weight > 0");
        }

        var targetWeight = UnityEngine.Random.Range(0, weightSum);
        // Debug.Log($"seed " + UnityEngine.Random.seed);
        foreach (var element in newList)
        {
            targetWeight -= weightGetter(element);
            if (targetWeight <= 0)
            {
                return element;
            }
        }
        throw new InvalidOperationException();
    }
    public static T GetRandomByWeight<T>(IReadOnlyList<T> list, Func<T, int> weightGetter, System.Random systemRandom)
    {
        List<T> newList = new List<T>();
        foreach (var item in list)
        {
            if (weightGetter(item) == 0) continue;
            newList.Add(item);
        }
        int weightSum = 0;
        foreach (var element in newList)
        {
            var weight = weightGetter(element);
            if (weight < 0)
            {
                Debug.LogError($"Element {element} have weight {weight} but weight must me >= 0");
            }
            weightSum += weight;
        }
        if (weightSum <= 0)
        {
            Debug.LogError("List must contain at least one element with weight > 0");
        }

        var targetWeight = systemRandom.Next(0, weightSum);
        foreach (var element in newList)
        {
            targetWeight -= weightGetter(element);
            if (targetWeight <= 0)
            {
                return element;
            }
        }
        throw new InvalidOperationException();
    }

    public static void SetAnchor(this RectTransform rect, Side side)
    {
        if (side == Side.bottom)
        {
            rect.anchorMin = new Vector2(0.5f, 0.0f);
            rect.anchorMax = new Vector2(0.5f, 0.0f);
        }
        if (side == Side.top)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
        }
        if (side == Side.left)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
        }
        if (side == Side.right)
        {
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
        }
        if (side == Side.middle)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        if (side == Side.topLeft)
        {
            rect.anchorMin = new Vector2(0.0f, 1f);
            rect.anchorMax = new Vector2(0.0f, 1f);
        }
        if (side == Side.topRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
        }
        if (side == Side.bottonLeft)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
        }
        if (side == Side.bottomRight)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
        }
    }
    public static void ForeachEnum<T>(Action<T> action) where T : Enum
    {
        Array values = Enum.GetValues(typeof(T));

        foreach (T value in values)
        {
            action(value);
        }
    }
    public static T ToEnum<T>(this string self)
    {
        try
        {
            return (T)Enum.Parse(typeof(T), self);
        }
        catch (Exception)
        {

            throw new Exception($"Failed parsing ({self})");
        }

    }


    public static T With<T>(this T self, Action<T> set)
    {
        set.Invoke(self);
        return self;
    }
    public static T With<T>(this T self, Action<T> set, bool when)
    {
        if (when) set.Invoke(self);
        return self;
    }
    public static T With<T>(this T self, Action<T> set, Func<bool> when)
    {
        if (when()) set.Invoke(self);
        return self;
    }



    public static T MoveNext<T>(this T self) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        var currentIndex = Array.IndexOf(values, self);
        if (currentIndex >= values.Length - 1)
        {
            return default(T);
        }
        return values[currentIndex + 1];
    }
    public static bool Contains<T>(this T self, string key) where T : Enum
    {
        var current = self;
        while (true)
        {
            self = self.MoveNext();
            if (self.ToString() == key) return true;
            if (current.ToString() == self.ToString()) break;
        }
        return false;
    }
    public static List<Transform> GetAllChilds(this Transform transform)
    {
        var ch = transform.GetComponentsInChildren<Transform>(true);
        return ch.ToList();
    }

    public static bool IsValidEmail(string email)
    {
        string expression = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";

        if (Regex.IsMatch(email, expression))
        {
            if (Regex.Replace(email, expression, string.Empty).Length == 0)
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsEnglishOnly(string text)
    {
        return !Regex.IsMatch(text, "[^A-Za-z]+");
    }

    public static string TrimmAllTags(this string text)
    {
        string pattern = @"<color=#(?:[0-9a-fA-F]{3}){1,2}>|<color=[a-zA-Z]+>";
        return Regex.Replace(text, pattern, "");
    }




    public static async void TestSpeed(System.Action act, int iterations = 1, string name = "")
    {
        int t = Environment.TickCount;

#if UNITY_EDITOR
        UnityEngine.Profiling.Profiler.BeginSample("test");
#endif

        for (int i = 0; i < iterations; i++)
        {
            act.Invoke();
        }
        var noFrame = Environment.TickCount - t;
#if UNITY_EDITOR
        UnityEngine.Profiling.Profiler.EndSample();
#endif
        await Task.Yield();


        Debug.Log($"<color=red>Speed of</color>({act.Method.Name})({name}) is  Noframe({noFrame})" +
            $"withFrame({Environment.TickCount - t})" +
            $"<color=green>ms</color>  <color=yellow>iterations</color>({iterations})");

    }
    public static string ConvertToString(this Dictionary<string, string> dict, char separator = ':')
    {
        string s = "";
        foreach (var item in dict)
        {
            s += item.Key + separator + item.Value + " ";
        }
        return s.RemoveLast();
    }

    /*   [Obsolete("устаревшая но точно рабочая")]
       public static string GetStringWithKeyWords(string text, char separator, params object[] reflectionTargets)
       {
           //if (!text.Contains(separator)) return text;
           if (separator == '@' && !text.Contains(separator)) return text;
           var wordList = text.GetWordsFromBrackets(separator);
           text = text.Replace(separator.ToString(), "");
           foreach (var word in wordList)
           {
               foreach (var target in reflectionTargets)
               {
                   var obj = GetObjectOfWord(word, target);
                   if (obj != null)
                   {
                       text = text.Replace(word, obj.ToString());
                       break;
                   }
               }
           }
           return text;
       }
       private static object GetObjectOfWord(string paramName, object reflectionObject)
       {
           object lastObject = reflectionObject;

           foreach (var itemName in paramName.Split('.'))//ЕЩЕ ДЛЯ ПРОПЕРТИ НАПИСАТЬ
           {
               int index = 0;
               string partName = itemName;
               if (partName.Contains('['))
               {
                   partName = itemName.TrimBracketsInclude();
                   index = itemName.GetWordFromBrackets().ToInt();
               }

               var prop = lastObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                          .FirstOrDefault(field => field.Name == partName);
               if (prop != null)
               {
                   var propValue = prop.GetValue(lastObject);
                   var list = propValue as IList;
                   if (list != null)
                   {
                       if (list.Count <= index)
                       {
                           Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                           return null;
                       }
                       lastObject = list[index];
                   }
                   else lastObject = propValue;

               }
               else
               {
                   var field = lastObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                         .FirstOrDefault(field => field.Name == partName);
                   if (field == null)
                   {
                       // Debug.LogError($"Ошибка не нашлось параметра с именем({paramName}) в обьекте({lastObject}) ");
                       return null;
                   }
                   var fielValue = field.GetValue(lastObject);
                   var list = fielValue as IList;
                   if (list != null)
                   {
                       if (list.Count <= index)
                       {
                           Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                           return null;
                       }
                       lastObject = list[index];
                   }
                   else lastObject = fielValue;
               }

           }
           return lastObject;
       }*/





    public static bool IsContainsError(this List<UnityWebRequestAsyncOperation> requests)
    {
        foreach (var item in requests)
        {
            if (item.webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(item.webRequest.error + " " + item.webRequest.url);
                return true;
            }
        }
        return false;
    }
    public static bool IsAllRequestsDone(this List<UnityWebRequestAsyncOperation> requests)
    {
        return requests.All(r => r.isDone);
    }




    public static T CloneClass<T>(this T original) where T : class
    {
        Type typeOfClass = typeof(T);
        var newClass = (T)Activator.CreateInstance(typeOfClass);

        var fields = typeOfClass.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        var baseFields = typeOfClass.BaseType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        fields = fields.Concat(baseFields).ToArray();
        foreach (var field in fields)
        {
            var origValue = field.GetValue(original);
            field.SetValue(newClass, origValue);
        }
        var inter = typeOfClass.GetProperties();
        foreach (var field in inter)
        {
            if (field.GetSetMethod() == null) continue;
            var origValue = field.GetValue(original);
            field.SetValue(newClass, origValue);
        }

        return newClass;
    }

    public static T CloneSO<T>(this T original) where T : ScriptableObject
    {
        Type typeOfClass = typeof(T);
        var newClass = ScriptableObject.Instantiate(original);

        var fields = typeOfClass.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        var baseFields = typeOfClass.BaseType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        fields = fields.Concat(baseFields).ToArray();
        foreach (var field in fields)
        {
            var origValue = field.GetValue(original);
            field.SetValue(newClass, origValue);
        }
        var inter = typeOfClass.GetProperties();
        foreach (var field in inter)
        {
            if (field.GetSetMethod() == null) continue;
            var origValue = field.GetValue(original);
            field.SetValue(newClass, origValue);
        }

        return newClass;
    }



    public static object GetPropertyReflection<T>(this T original, string propertyName) where T : class
    {
        Type typeOfClass = typeof(T);
        if (string.IsNullOrEmpty(propertyName))
        {
            Debug.LogError($"Property name ({propertyName}) of ({typeOfClass}) empty");
            return null;
        }
        var prop = typeOfClass.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (prop != null) return prop.GetValue(original);
        var baseprop = typeOfClass.BaseType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (baseprop != null) return prop.GetValue(original);


        var field = typeOfClass.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        //  Debug.Log($"ref field of {propertyName} of {typeOfClass} = {field}");
        if (field != null) return field.GetValue(original);
        var basefield = typeOfClass.BaseType.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        if (basefield != null) return field.GetValue(original);

        throw new Exception($"Property({propertyName}) not exist on {typeOfClass.Name} of ({typeOfClass})");
        //   Debug.Log(TextMod.Colorize($"Property({propertyName}) not exist on {typeOfClass.Name} of ({typeOfClass})", Color.red));
        //  return null;
    }


    //public static string GetStringWithKeyWords(string text, char separator, bool lightSearch, params object[] reflectionTargets)
    //{
    //    if (separator == '@' && !text.Contains(separator)) return text;
    //    if (text.Contains("if("))
    //    {
    //        text = ReplaceIfConditions(text, reflectionTargets);
    //    }

    //    var wordList = text.GetWordsFromBrackets(separator);
    //    text = text.Replace(separator.ToString(), "");
    //    foreach (var word in wordList)
    //    {

    //        foreach (var target in reflectionTargets)
    //        {
    //            var obj = GetObjectByName(word, target, lightSearch);
    //            if (obj != null)
    //            {
    //                text = text.Replace(word, obj.ToString());
    //                break;
    //            }
    //        }
    //    }
    //    return text;
    //}
    //private static string ReplaceIfConditions(string text, object[] reflectionTargets)
    //{
    //    List<string> ifWords = GetIfCondition(text);
    //    foreach (var ifWord in ifWords)
    //    {
    //        foreach (var target in reflectionTargets)
    //        {
    //            var obj = GetObjectByName(ifWord, target, false);
    //            if (obj != null)
    //            {
    //                text = text.Replace($"@{ifWord}@", obj.ToString());
    //                break;
    //            }
    //        }
    //    }

    //    var conditions = ExtractIfConditionParts(text);
    //    List<string> replaceArray = new List<string>();

    //    foreach (var condition in conditions)
    //    {
    //        if (condition[0].ToLower().Contains("false") || condition[0].ToLower().Contains("true"))
    //        {
    //            if (LogicParser.Parse(condition[0])) replaceArray.Add(condition[1]);
    //            else replaceArray.Add(condition[2]);
    //        }
    //        else
    //        {
    //            if (MathParser.ParseBool(condition[0])) replaceArray.Add(condition[1]);
    //            else replaceArray.Add(condition[2]);
    //        }

    //    }
    //    text = ReplaceIfConditions(text, replaceArray.ToArray());
    //    return text;
    //}
    private static string ReplaceIfConditions(string input, params string[] replacements)
    {
        string pattern = @"if\s*\([^)]*\)\s*{[^}]*}\s*else\s*{[^}]*}";
        MatchCollection matches = Regex.Matches(input, pattern);
        if (matches.Count > replacements.Length)
        {
            Debug.LogError("Warning: The number of replacements is less than the number of conditions!");
        }
        for (int i = 0; i < matches.Count; i++)
        {
            string replacement = replacements[i % replacements.Length];//модуль в случае выхода за массив
            input = input.Replace(matches[i].Value, replacement);
        }
        return input;
    }
    private static List<string[]> ExtractIfConditionParts(string input)
    {
        List<string[]> resultList = new List<string[]>();

        Regex regex = new Regex(@"if\((.*?)\)\{(.*?)\}\s*else\{(.*?)\}");
        MatchCollection matches = regex.Matches(input);

        foreach (Match match in matches)
        {
            string condition = match.Groups[1].Value; // Условие
            string ifValue = match.Groups[2].Value; // Значение if
            string elseValue = match.Groups[3].Value; // Значение else

            resultList.Add(new string[] { condition, ifValue, elseValue });
        }
        return resultList;
    }
    private static List<string> GetIfCondition(string text)
    {
        List<string> ifWords = new List<string>();
        string pattern = @"if\(((?=[^)]*\s*@([^@]+)@)[^)]*)\)";
        Regex regex = new Regex(pattern);
        var matches = regex.Matches(text);

        foreach (Match match in matches)
        {
            string insideIf = match.Groups[1].Value;

            string ifWord = Regex.Match(insideIf, @"@([^@]+)@").Groups[1].Value;
            ifWords.Add(ifWord);
        }

        return ifWords;
        // List<string> ifWords = new List<string>();
        // string pattern =  @"@([^@]+)@";
        // Regex regex = new Regex(pattern);
        // var matches = regex.Matches(text);
        //
        // foreach (Match match in matches)
        // {
        //     ifWords.Add(match.Groups[1].Value);
        // }
        //
        // return ifWords;
    }


    public static object GetObjectByName(string paramName, object reflectionObject, bool lightSearch, bool excludeParentClasses = false)
    {
        object lastObject = reflectionObject;
        //  try
        {
            foreach (var itemName in paramName.Split('.'))//ЕЩЕ ДЛЯ ПРОПЕРТИ НАПИСАТЬ
            {
                // Debug.Log($"item name " + itemName);
                int index = 999999;
                string args = "";
                string partName = itemName;
                string setValue = "";

                SetupParametrs(ref index, ref args, ref partName, ref setValue);
                int agrumentCount = args.IsNullOrEmpty() ? 0 : args.Split(',').Length;
                if (!setValue.IsNullOrEmpty()) setValue = GetFullValue(paramName).ToString();
                // Debug.Log($"partName " + partName);
                //  Debug.Log($"lastObject = " + lastObject);

                #region static
                if (!lightSearch && lastObject is Type staticClass)//проверка является ли последний обьект ТИПОМ (статик класс)
                {
                    //   Debug.Log($"Статика была прошлой - " + lastObject);
                    BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    if (excludeParentClasses) flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

                    var property = staticClass.GetProperties(flags)
                          .FirstOrDefault(field => field.Name == partName);

                    var field = staticClass.GetFields(flags)
                     .FirstOrDefault(field => field.Name == partName);

                    var method = staticClass.GetMethods(flags)
                    .FirstOrDefault(field => field.Name == partName && agrumentCount == field.GetParameters().Length);

                    if (property != null)
                    {
                        //      Debug.Log($"propertyS " + partName);
                        if (!setValue.IsNullOrEmpty())
                        {
                            try
                            {
                                property.SetValue(null, Convert.ChangeType(setValue, property.PropertyType));
                            }
                            catch (Exception)
                            {
                                var valueObject = GetObjectByName(setValue, reflectionObject, lightSearch);
                                property.SetValue(null, valueObject);
                            }
                            //  property.SetValue(null, Convert.ChangeType(setValue, property.PropertyType));
                            return lastObject = property.GetValue(null);
                        }
                        var propertyValue = property.GetValue(null);
                        //   Debug.Log($"propertyS propertyValue " + propertyValue);
                        var list = propertyValue as IList;
                        if (list != null && index != 999999)//новое&& index != 999999
                        {
                            if (list.Count <= index)
                            {
                                Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                                return propertyValue;
                            }
                            lastObject = list[index];
                        }
                        else lastObject = propertyValue;
                        continue;
                    }
                    if (field != null)
                    {
                        //   Debug.Log($"fieldS " + partName);
                        if (!setValue.IsNullOrEmpty())
                        {
                            try
                            {
                                field.SetValue(null, Convert.ChangeType(setValue, field.FieldType));
                            }
                            catch (Exception)
                            {
                                var valueObject = GetObjectByName(setValue, reflectionObject, lightSearch);
                                field.SetValue(null, valueObject);
                            }
                            field.SetValue(null, Convert.ChangeType(setValue, field.FieldType));
                            return lastObject = field.GetValue(null);
                        }
                        var fieldValue = field.GetValue(null);
                        var list = fieldValue as IList;
                        if (list != null && index != 999999)//новое&& index != 999999
                        {
                            if (list.Count <= index)
                            {
                                Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                                return fieldValue;
                            }
                            lastObject = list[index];
                        }
                        else lastObject = fieldValue;
                        continue;
                    }
                    if (method != null)
                    {
                        //   Debug.Log($"methodS " + partName);
                        var arg = args.Split(',');
                        if (args.IsNullOrEmpty()) arg = new string[] { };
                        var methodValue = method.Invoke(null, ConvertArguments(method, arg));
                        //   Debug.Log($"methodValueS " + methodValue);
                        var list = methodValue as IList;
                        if (list != null && index != 999999)//новое&& index != 999999
                        {
                            if (list.Count <= index)
                            {
                                Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                                return null;
                            }
                            lastObject = list[index];
                        }
                        else lastObject = methodValue;
                        continue;
                    }
                    continue;
                }
                #endregion

                var prop = lastObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                           .FirstOrDefault(field => field.Name == partName);
                if (prop != null)
                {
                    //  Debug.Log($"property " + partName);
                    if (!setValue.IsNullOrEmpty())
                    {
                        try
                        {
                            prop.SetValue(lastObject, Convert.ChangeType(setValue, prop.PropertyType));
                        }
                        catch (Exception)
                        {
                            var valueObject = GetObjectByName(setValue, reflectionObject, lightSearch);
                            prop.SetValue(lastObject, valueObject);
                        }
                        // prop.SetValue(lastObject, Convert.ChangeType(setValue, prop.PropertyType));
                        return prop.GetValue(lastObject);
                    }
                    var propValue = prop.GetValue(lastObject);
                    var list = propValue as IList;
                    if (list != null && index != 999999)//новое&& index != 999999
                    {
                        if (list.Count <= index)
                        {
                            Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                            return null;
                        }
                        lastObject = list[index];
                    }
                    else lastObject = propValue;

                }
                else
                {
                    var field = lastObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                          .FirstOrDefault(field => field.Name == partName);
                    if (field != null)
                    {
                        if (!setValue.IsNullOrEmpty())
                        {
                            TrySetValue(reflectionObject, lightSearch, lastObject, setValue, field);
                            return field.GetValue(lastObject);
                        }
                        var fielValue = field.GetValue(lastObject);
                        var list = fielValue as IList;
                        if (list != null && index != 999999)//новое&& index != 999999
                        {
                            if (list.Count <= index)
                            {
                                Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                                return fielValue;
                            }
                            lastObject = list[index];
                        }
                        else lastObject = fielValue;

                    }
                    else
                    {
                        //Debug.Log($"check method ({partName}) в классе "+lastObject );
                        var method = lastObject.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                         .FirstOrDefault(m => m.Name == partName);
                        bool extension = false;
                        if (method == null && !lightSearch)
                        {
                            extension = true;
                            method = GetExtensionMethod(lastObject, partName);
                        }

                        if (method != null)
                        {
                            //  Debug.Log($"method " + partName);
                            var arg = args.Split(',');
                            if (args.IsNullOrEmpty()) arg = new string[] { };



                            object methodValue = null;
                            if (extension)
                            {
                                //      Debug.Log($"юзаем расширение " + paramName);
                                var arguments = ConvertArguments(method, arg, true).ToList();
                                arguments.Insert(0, lastObject);
                                methodValue = method.Invoke(null, arguments.ToArray());
                            }
                            else
                            {
                                //      Debug.Log($"юзаем функцию " + paramName);
                                var arguments = ConvertArguments(method, arg, false).ToList();
                                methodValue = method.Invoke(lastObject, arguments.ToArray());
                            }
                            //  Debug.Log($"methodValue " + methodValue);
                            var list = methodValue as IList;
                            if (list != null && index != 999999)//новое&& index != 999999
                            {
                                if (list.Count <= index)
                                {
                                    Debug.LogError($"Ошибка выхода за индекс массива в класса({lastObject}) слово: " + paramName);
                                    return null;
                                }
                                lastObject = list[index];
                            }
                            else lastObject = methodValue;
                        }
                        else if (!lightSearch)
                        {
                            var fullName = partName.Replace(">", ".");
                            var classType = Type.GetType(fullName);
                            fullName += ",UnityEngine";
                            if (classType == null) classType = Type.GetType(fullName);

                            if (classType == null) return null;
                            lastObject = classType;
                        }
                        else return null;

                    }


                }

            }

        }
        //  catch (Exception a)
        //  {
        //      Debug.LogError($"ReflectError({paramName})({lastObject}):" + a);
        //      return 0;
        //  }
        return lastObject;
    }
    private static void SetupParametrs(ref int index, ref string args, ref string partName, ref string setValue)
    {
        if (partName.Contains('('))
        {
            args = partName.GetWordFromBrackets('(', ')');
            partName = partName.TrimBracketsInclude('(', ')');
        }
        if (partName.Contains('['))
        {
            index = partName.GetWordFromBrackets().ToInt();
            partName = partName.TrimBracketsInclude();

        }
        if (partName.Contains('='))
        {
            setValue = partName.Split('=')[1];
            partName = partName.Split('=')[0];
        }
    }


    private static void TrySetValue(object reflectionObject, bool lightSearch, object lastObject, string setValue, FieldInfo field)
    {
        try
        {
            field.SetValue(lastObject, Convert.ChangeType(setValue, field.FieldType));
        }
        catch (Exception e)
        {
            try
            {
                field.SetValue(lastObject, Enum.Parse(field.FieldType, setValue));
            }
            catch (Exception a)
            {
                var valueObject = GetObjectByName(setValue, reflectionObject, lightSearch);
                field.SetValue(lastObject, valueObject);
            }
        }
    }
    private static object GetFullValue(string paramName)
    {
        return paramName.Split('=')[1];
    }
    private static MethodInfo GetExtensionMethod(object lastObject, string methodName)
    {
        Assembly extensionAssembly = Assembly.GetAssembly(lastObject.GetType());

        Type extensionClassType = extensionAssembly.GetTypes()
         .FirstOrDefault(t => t.IsSealed && t.GetMethods(BindingFlags.Public | BindingFlags.Static)
         .Any(m => m.Name == methodName));


        if (extensionClassType != null)
        {
            MethodInfo extensionMethod = extensionClassType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == methodName);

            if (extensionMethod != null)
            {
                // Debug.Log($"Найден метод-расширение: {extensionClassType.Name}.{extensionMethod.Name}");
                return extensionMethod;
            }
        }
        return null;
    }
    public static object[] ConvertArguments(MethodInfo method, string[] args, bool ignoreFirst = false)
    {
        if (args.Length == 0) return new object[] { };
        var parameters = method.GetParameters();
        object[] convertedArgs = new object[args.Length];

        for (int i = 0; i < args.Length; i++)
        {
            int s = 0;
            if (ignoreFirst) s = 1;
            Type parameterType = parameters[i + s].ParameterType;
            var converter = TypeDescriptor.GetConverter(parameterType);
            convertedArgs[i] = converter.ConvertFrom(args[i]);
        }

        return convertedArgs;
    }







    public static string GetLibraryDirectory()
    {
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        if (unityPlayer == null)
            throw new InvalidOperationException("unityPlayer == null");

        var _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (_currentActivity == null)
            throw new InvalidOperationException("_currentActivity == null");

        AndroidJavaObject packageManager = _currentActivity.Call<AndroidJavaObject>("getPackageManager");

        if (packageManager == null)
            throw new InvalidOperationException("packageManager == null");

        string packageName = _currentActivity.Call<string>("getPackageName");

        if (string.IsNullOrEmpty(packageName))
            throw new InvalidOperationException("string.IsNullOrEmpty(packageName)");

        const int GetMetaData = 128;
        AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, GetMetaData);

        if (packageInfo == null)
            throw new InvalidOperationException("packageInfo == null");

        AndroidJavaObject applicationInfo = packageInfo.Get<AndroidJavaObject>("applicationInfo");

        if (applicationInfo == null)
            throw new InvalidOperationException("applicationInfo == null");

        string nativeLibraryDir = applicationInfo.Get<string>("nativeLibraryDir");

        if (string.IsNullOrEmpty(nativeLibraryDir))
            throw new InvalidOperationException("string.IsNullOrEmpty(nativeLibraryDir)");
        return nativeLibraryDir;
    }
    public static string CalculateSHA256(string filePath)
    {
        try
        {
            foreach (var item in Directory.GetFiles(filePath))
            {
                Debug.Log("file " + item);
            }
        }
        catch (Exception)
        {

        }
        filePath += "/libil2ccp.so";
        Debug.Log("path " + filePath);

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream))
                {
                    byte[] fileBytes = binaryReader.ReadBytes((int)fileStream.Length);

                    // Вывод содержимого файла в виде массива байт
                    foreach (byte b in fileBytes)
                    {
                        Console.Write(Convert.ToString(b, 16).PadLeft(2, '0') + " ");
                    }
                    var sha256 = System.Security.Cryptography.SHA256.Create();
                    byte[] hash = sha256.ComputeHash(fileBytes);
                    return BitConverter.ToString(hash).Replace("-", String.Empty);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("reader=" + e);

        }
        using (var stream = File.OpenRead(filePath))
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", String.Empty);
        }
    }


    public static Coroutine DOMyShakeScale(this Transform target, float duration, float strength, int vibrato)
    {
        Vector3 originalScale = target.localScale;
        var mono = target.GetComponent<MonoBehaviour>();
        mono.StopAllCoroutines();
        return mono.StartCoroutine
            (ShakeScaleCoroutine(target, originalScale, duration, strength, vibrato));
    }

    private static IEnumerator ShakeScaleCoroutine(Transform target, Vector3 originalScale, float duration, float strength, int vibrato)
    {
        float timer = 0f;
        Vector3 newScale = originalScale;
        while (timer < duration)
        {
            float t = timer / duration;
            float noise = Mathf.PerlinNoise(Time.unscaledTime * vibrato, Time.unscaledTime * vibrato) * 2 - 1;
            float shakeAmountX = noise * strength * Mathf.Lerp(1f, 0f, t);
            float shakeAmountY = noise * strength * Mathf.Lerp(1f, 0f, t);

            newScale = originalScale + new Vector3(shakeAmountX, shakeAmountY, 0f);
            target.localScale = newScale;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        float fadeDuration = 0.5f;
        float elapsed = 0f;

        Vector3 finalScale = originalScale;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            target.localScale = Vector3.Lerp(newScale, finalScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        target.localScale = originalScale;
    }

    public static Coroutine DoMyShakePosition(this Transform target, float duration, float strength, int vibrato)
    {
        Vector3 originalPosition = target.position;
        var mono = target.GetComponent<MonoBehaviour>();
        //   mono.StopAllCoroutines();
        return mono
            .StartCoroutine(ShakePositionCoroutine(target, originalPosition, duration, strength, vibrato));
    }
    private static IEnumerator ShakePositionCoroutine(Transform target, Vector3 originalPosition, float duration, float strength, int vibrato)
    {
        Vector3 newPosition = originalPosition;
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;

            float xOffset = Mathf.Sin(t * Mathf.PI * vibrato) * strength;
            float yOffset = Mathf.Cos(t * Mathf.PI * vibrato) * strength;

            newPosition = originalPosition + new Vector3(xOffset, yOffset, 0f);

            target.position = newPosition;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        Vector3 finalPosition = originalPosition;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            target.position = Vector3.Lerp(newPosition, finalPosition, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        target.position = originalPosition;
    }
}

public class AntiHackProperty
{
    public bool setupped = false;
    // private static bool enabled = true;

    private readonly int _hackValue;
    private string _stringCount = "0";
    private int _count = 0;

    public override string ToString()
    {
        return $"_stringCount({_stringCount}) _count({_count})";
    }
    public int value
    {
        get
        {
            //   Debug.Log("get "+_stringCount);
            //   if (!enabled) return _count;
            int realValue = (_count >> 1) + _hackValue;
            if (realValue.ToString() != _stringCount)
            {
                Debug.LogError($"Hacked? ({realValue}!={_stringCount})");
                _stringCount = "0";
                _count = -_hackValue << 1;
                return 0;
            }
            return realValue;
        }
        set
        {
            //  Debug.Log("Set " + _stringCount);
            //  if (!enabled)
            //  {
            //      _count = value;
            //      return;
            //  }
            _stringCount = value.ToString();
            _count = (value - _hackValue) << 1;
        }
    }
    public AntiHackProperty(int hackValue)
    {
        this._hackValue = hackValue;
        _stringCount = "0";
        _count = -hackValue << 1;
        // if (enabled)
        // {
        //     this._hackValue = hackValue;
        //     _stringCount = "0";
        //     _count = -hackValue << 1;
        // }
        // else
        // {
        //     _count = 0;
        // }
    }
}
public class PriorityClass<T>
{
    public T main;
    public int priority;
    public PriorityClass(T main, int priority)
    {
        this.main = main;
        this.priority = priority;
    }
    public override string ToString()
    {
        return "" + main + " " + priority;
    }
}
