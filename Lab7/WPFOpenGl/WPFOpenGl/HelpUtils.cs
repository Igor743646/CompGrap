using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class HelpUtils
{
    public static void Swap<T>(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }

    public static void Bound<T>(T min, ref T val, T max) where T : IComparable
    {
        if (val.CompareTo(min) < 0)
            val = min;
        if (val.CompareTo(max) > 0)
            val = max;
    }

    public static string GetTextFileFromRes(string directory, string filename, string extension, Assembly assembly = null)
    {
        return GetTextFileFromRes(directory, filename, extension, Encoding.UTF8, assembly);
    }

    public static string GetTextFileFromRes(string directory, string filename, string extension, Encoding encoding, Assembly assembly = null)
    {
        var stream = GetStreamFromRes(directory, filename, extension, assembly);
        using (TextReader reader = new StreamReader(stream, encoding))
        {
            return reader.ReadToEnd();
        }
    }

    public static string GetTextFileFromRes(string name, Assembly assembly = null)
    {
        return GetTextFileFromRes(name, Encoding.UTF8, assembly);
    }

    public static string GetTextFileFromRes(string name, Encoding encoding, Assembly assembly = null)
    {
        var stream = GetStreamFromRes(name, assembly);
        using (TextReader reader = new StreamReader(stream, encoding))
        {
            return reader.ReadToEnd();
        }
    }

    public static Stream GetStreamFromRes(string directory, string filename, string extension, Assembly assembly = null)
    {
        var name = new List<string>();
        if (!String.IsNullOrWhiteSpace(directory))
            name.AddRange(directory.Trim('.', '\\', '/').Split('\\', '/'));
        name.Add(filename);
        name.Add(extension.TrimStart('.'));
        var strname = String.Join(".", name);
        return GetStreamFromRes(strname, assembly);
    }

    public static Stream GetStreamFromRes(string name, Assembly assembly = null)
    {
        var resname = GetInternalResourceName(name, ref assembly);
        if (String.IsNullOrEmpty(resname))
            throw new Exception(String.Format("Запрашиваемый файл ресурсов \"{0}\" в сборке \"{1}\" не найден", name, assembly.GetName().Name));
        return assembly.GetManifestResourceStream(resname);
    }

    private static string GetInternalResourceName(string resourceName, ref Assembly resourceAssembly)
    {
        resourceAssembly = resourceAssembly ?? Assembly.GetEntryAssembly();
        var resourcenames = resourceAssembly.GetManifestResourceNames();
        var rootnamespace = String.Join(".", resourcenames
            .Select(n => n.Split('.')).Aggregate((r, n) => {
                for (int i = 0; i < Math.Min(r.Length, n.Length); ++i)
                    if (r[i] != n[i]) return r.Take(i).ToArray();
                return r;
            }));
        var searchresname = new List<string>(rootnamespace.Split('.'));
        for (int i = searchresname.Count - 1; i >= 0; --i)
            searchresname[i] = String.Join(".", searchresname.Take(i + 1));
        searchresname.Add(resourceAssembly.GetName().Name);
        searchresname.Add(resourceName);
        searchresname.Reverse();
        var foundresname = searchresname.Select(n => String.Format("{0}.{1}", n, resourceName)).Intersect(resourcenames).ToList();
        return foundresname.FirstOrDefault();
    }

    public static List<string> GetResourceNames(Assembly assembly = null)
    {
        return (assembly ?? Assembly.GetEntryAssembly()).GetManifestResourceNames().ToList();
    }

    public static long Factorial<T>(T number)
    {
        long result = 1;
        long stop = (long)Convert.ChangeType(number, typeof(long));

        for (long i = 2; i <= stop; i++)
        {
            result *= i;
        }

        return result;
    }

    public static long Comb<T>(T n, T k) where T : struct
    {
        long a = (long)Convert.ChangeType(n, typeof(long));
        long b = (long)Convert.ChangeType(k, typeof(long));

        return Factorial(a) / (Factorial(b) * Factorial(a-b));
    }
}
