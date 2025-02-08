Here is the C# code equivalent to the provided C code:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class VipsUtil
{
    // vips_slist_equal
    public static bool SListEqual<T>(IList<T> l1, IList<T> l2)
    {
        while (l1.Count > 0 && l2.Count > 0)
        {
            if (!EqualityComparer<T>.Default.Equals(l1[0], l2[0]))
                return false;

            l1.RemoveAt(0);
            l2.RemoveAt(0);
        }

        return l1.Count == 0 && l2.Count == 0;
    }

    // vips_slist_map2
    public static T SListMap2<T, U>(IList<U> list, Func<U, T> fn, T a, T b)
    {
        var copy = new List<U>(list);
        T result = default(T);

        foreach (var item in copy)
            if ((result = fn(item, a, b)) != null)
                break;

        return result;
    }

    // vips_slist_map2_rev
    public static T SListMap2Rev<T, U>(IList<U> list, Func<U, T> fn, T a, T b)
    {
        var copy = new List<U>(list);
        copy.Reverse();
        T result = default(T);

        foreach (var item in copy)
            if ((result = fn(item, a, b)) != null)
                break;

        return result;
    }

    // vips_slist_map4
    public static T SListMap4<T, U, V, W>(IList<U> list,
        Func<U, V, W, T> fn, T a, T b, T c, T d)
    {
        var copy = new List<U>(list);
        T result = default(T);

        foreach (var item in copy)
            if ((result = fn(item, a, b, c, d)) != null)
                break;

        return result;
    }

    // vips_slist_fold2
    public static T SListFold2<T, U>(IList<U> list, T start,
        Func<T, U, T, T> fn, T a, T b)
    {
        T c = start;

        foreach (var item in list)
            if ((c = fn(item, c, a, b)) == null)
                return default(T);

        return c;
    }

    // vips_slist_filter
    public static IList<U> SListFilter<T, U>(IList<U> list,
        Func<U, bool> fn, T a, T b)
    {
        var filtered = new List<U>();

        foreach (var item in list)
            if (!fn(item))
                filtered.Add(item);

        return filtered;
    }

    // vips_slist_free_all
    public static void SListFreeAll<T>(IList<T> list) where T : IDisposable
    {
        foreach (var item in list)
            ((IDisposable)item).Dispose();

        list.Clear();
    }

    // vips_map_equal
    public static bool MapEqual<U, V>(U a, V b)
    {
        return EqualityComparer<V>.Default.Equals(a as V, b);
    }

    // vips_hash_table_map
    public static T HashTableMap<T, U>(IDictionary<string, U> hash,
        Func<U, T> fn, T a, T b)
    {
        var pair = new { A = a, B = b, Fn = fn, Result = default(T) };

        if (hash.TryGetValue("result", out pair.Result))
            return pair.Result;

        foreach (var kvp in hash)
            if ((pair.Result = fn(kvp.Value, a, b)) != null)
                break;

        return pair.Result;
    }

    // vips_iscasepostfix
    public static bool IsCasePostfix(string a, string b)
    {
        int m = a.Length;
        int n = b.Length;

        if (n > m)
            return false;

        return string.Compare(a.Substring(m - n), b, StringComparison.OrdinalIgnoreCase) == 0;
    }

    // vips_isprefix
    public static bool IsPrefix(string a, string b)
    {
        for (int i = 0; a[i] != '\0' && b[i] != '\0'; i++)
            if (a[i] != b[i])
                return false;

        if (a[i] != '\0')
            return false;

        return true;
    }

    // strcspne
    public static int StrCspne(string s, string reject)
    {
        size_t skip = 0;

        if (reject.Contains("\\"))
            return s.IndexOfAny(reject.ToCharArray());

        while (true)
        {
            skip += s.IndexOfAny(reject.ToCharArray(), skip);

            if (skip == 0 || s[skip] == '\0' || s[skip - 1] != '\\')
                break;

            skip++;
        }

        return skip;
    }

    // vips_break_token
    public static string BreakToken(string str, string brk)
    {
        int ch;

        if (str == null || str.Length == 0)
            return null;

        str = str.TrimStart(brk.ToCharArray());

        if (str.Length == 0)
            return null;

        var token = new { Token = default(VipsToken), String = new string(str, 0, 1) };

        while ((ch = str[0]) != '\0')
        {
            switch (ch)
            {
                case '[':
                    token.Token = VipsToken.Left;
                    break;

                case ']':
                    token.Token = VipsToken.Right;
                    break;

                case '=':
                    token.Token = VipsToken.Equals;
                    break;

                case ',':
                    token.Token = VipsToken.Comma;
                    break;

                default:
                    if (char.IsLetterOrDigit(ch))
                        return str.Substring(0, 1);

                    var q = str.IndexOfAny(new[] { '[', ']', '=', ',', '\"', '\'' }, 1);
                    if (q == -1)
                        return str.Substring(0, 1);

                    token.Token = VipsToken.String;
                    token.String = str.Substring(0, q + 1).TrimEnd(brk.ToCharArray());
                    break;
            }

            str = str.Substring(q + 1);
        }

        return null;
    }

    // vips_filename_suffix_match
    public static bool FilenameSuffixMatch(string path, string[] suffixes)
    {
        var basename = Path.GetFileName(path);

        if (basename.EndsWith("]", StringComparison.Ordinal))
            basename = basename.Substring(0, basename.Length - 1);

        foreach (var suffix in suffixes)
            if (IsCasePostfix(basename, suffix))
                return true;

        return false;
    }

    // vips_file_length
    public static long FileLength(int fd)
    {
#if G_OS_WIN32
        var st = new _stati64();
#else
        var st = new stat();
#endif

        if (fstat(fd, ref st) != 0)
        {
            VipsErrorSystem(errno, "vips_file_length", "%s", _("unable to get file stats"));
            return -1;
        }

        return st.st_size;
    }

    // vips__write
    public static int Write(int fd, byte[] buf, size_t count)
    {
        while (count > 0)
        {
            var chunkSize = Math.Min(1024 * 1024 * 1024, count);
            var nWritten = write(fd, buf, chunkSize);

            if (nWritten <= 0)
            {
                VipsErrorSystem(errno, "vips__write", "%s", _("write failed"));
                return -1;
            }

            buf = new byte[count - nWritten];
            count -= nWritten;
        }

        return 0;
    }

    // vips__fopen
    public static FileStream FOpen(string filename, string mode)
    {
#if G_OS_WIN32
        if (mode[0] == 'w')
            VipsSetCreateTime(_fileno(FOpen(filename, "r")));
#endif

        return File.Open(filename, FileMode.OpenOrCreate);
    }

    // vips__file_open_read
    public static FileStream FileOpenRead(string filename, string fallbackDir)
    {
#if G_PLATFORM_WIN32 || G_WITH_CYGWIN
        if (text_mode)
            mode = "rN";
        else
            mode = "rbN";
#else
        mode = "re";
#endif

        var fp = FOpen(filename, mode);

        if (fp != null)
            return fp;

        if (fallbackDir != null && !filename.Contains("\\"))
        {
            var path = Path.Combine(fallbackDir, filename);
            fp = FOpen(path, mode);
        }

        VipsErrorSystem(errno, "vips__file_open_read", _("unable to open file \"%s\" for reading"), filename);

        return null;
    }

    // vips__fgetc
    public static int FGetChar(FileStream stream)
    {
        var ch = stream.Read(new byte[1], 0, 1)[0];

        if (ch == '\r')
            ch = stream.Read(new byte[1], 0, 1)[0];

        return ch;
    }

    // vips__gvalue_new
    public static GValue GValueNew(Type type)
    {
        var value = new GValue();
        value.Init(type);

        return value;
    }

    // vips__gslist_gvalue_free
    public static void GSListGValueFree(IList<GValue> list)
    {
        foreach (var item in list)
            ((IDisposable)item).Dispose();

        list.Clear();
    }

    // vips__gslist_gvalue_copy
    public static IList<GValue> GSListGValueCopy(IList<GValue> list)
    {
        var copy = new List<GValue>();

        foreach (var item in list)
            copy.Add(GValue.Copy(item));

        return copy;
    }

    // vips__gslist_gvalue_merge
    public static IList<GValue> GSListGValueMerge(IList<GValue> a, IList<GValue> b)
    {
        var merged = new List<GValue>(a);

        foreach (var item in b)
            if (!merged.Any(i => GValue.Equals(i, item)))
                merged.Add(item);

        return merged;
    }

    // vips__gslist_gvalue_get
    public static string GSListGValueGet(IList<GValue> list)
    {
        var builder = new StringBuilder();

        foreach (var item in list)
            builder.AppendLine(GValue.ToString(item));

        return builder.ToString();
    }

    // vips__seek_no_error
    public static long SeekNoError(int fd, long pos, int whence)
    {
#if G_OS_WIN32
        return _lseeki64(fd, pos, whence);
#else
        var new_pos = lseek(fd, pos, whence);

        if (new_pos == -1)
            VipsErrorSystem(errno, "vips__seek", "%s", _("unable to seek"));

        return new_pos;
#endif
    }

    // vips__seek
    public static long Seek(int fd, long pos, int whence)
    {
        var new_pos = SeekNoError(fd, pos, whence);

        if (new_pos == -1)
            VipsErrorSystem(errno, "vips__seek", "%s", _("unable to seek"));

        return new_pos;
    }

    // vips__ftruncate
    public static int FTruncate(int fd, long pos)
    {
#if G_OS_WIN32
        Seek(fd, pos, SEEK_SET);
        if (!SetEndOfFile((IntPtr)fd))
        {
            VipsErrorSystem(GetLastError(), "vips__ftruncate", "%s", _("unable to truncate"));
            return -1;
        }
#else
        if (ftruncate(fd, pos))
        {
            VipsErrorSystem(errno, "vips__ftruncate", "%s", _("unable to truncate"));
            return -1;
        }
#endif

        return 0;
    }

    // vips_existsf
    public static bool Exists(string name)
    {
        var path = string.Format(name);

        return File.Exists(path);
    }

    // vips_isdirf
    public static bool IsDir(string name)
    {
        var path = string.Format(name);

        return Directory.Exists(path);
    }

    // vips_mkdirf
    public static int Mkdir(string name)
    {
        var path = string.Format(name);

        if (Directory.CreateDirectory(path).FullName != path)
            VipsError("mkdirf", _("unable to create directory \"%s\", %s"), path, g_strerror(errno));

        return 0;
    }

    // vips_rmdirf
    public static int Rmdir(string name)
    {
        var path = string.Format(name);

        if (Directory.Delete(path))
            VipsError("rmdir", _("unable to remove directory \"%s\", %s"), path, g_strerror(errno));

        return 0;
    }

    // vips_rename
    public static int Rename(string old_name, string new_name)
    {
        var old_path = string.Format(old_name);
        var new_path = string.Format(new_name);

        if (File.Move(old_path, new_path))
            VipsError("rename", _("unable to rename file \"%s\" as \"%s\", %s"), old_name, new_name, g_strerror(errno));

        return 0;
    }

    // vips__token_get
    public static string TokenGet(string p, out VipsToken token, char[] string, int size)
    {
        if (string.Length > 0)
            string[0] = '\0';

        var q = BreakToken(p, " \t\n\r");

        switch ((char)q[0])
        {
            case '[':
                token = VipsToken.Left;
                break;

            case ']':
                token = VipsToken.Right;
                break;

            case '=':
                token = VipsToken.Equals;
                break;

            case ',':
                token = VipsToken.Comma;
                break;

            default:
                if (char.IsLetterOrDigit(q[0]))
                    return q.Substring(0, 1);

                var str = new string(q, 0, 1).TrimEnd(" \t\n\r".ToCharArray());
                token = VipsToken.String;
                break;
        }

        return q;
    }

    // vips__token_must
    public static string TokenMust(string p, out VipsToken token, char[] string, int size)
    {
        if ((p = TokenGet(p, out token, string, size)) == null)
            VipsError("get_token", "%s", _("unexpected end of string"));

        return p;
    }

    // vips__token_need
    public static string TokenNeed(string p, VipsToken need_token, char[] string, int size)
    {
        var token = default(VipsToken);

        if ((p = TokenMust(p, out token, string, size)) == null)
            return null;

        if (token != need_token)
            VipsError("get_token", _("expected %s, saw %s"), VipsEnumNick(VIPS_TYPE_TOKEN, need_token), VipsEnumNick(VIPS_TYPE_TOKEN, token));

        return p;
    }

    // vips__token_segment
    public static string TokenSegment(string p, out VipsToken token, char[] string, int size)
    {
        if ((p = TokenGet(p, out token, string, size)) == null)
            return null;

        if (token == VipsToken.String && p[0] == '[')
        {
            var sub_token = default(VipsToken);
            var sub_string = new char[VIPS_PATH_MAX];
            int depth;
            int i;

            depth = 0;
            do
            {
                if ((p = TokenMust(p, out sub_token, sub_string, VIPS_PATH_MAX)) == null)
                    return null;

                switch (sub_token)
                {
                    case VipsToken.Left:
                        depth++;
                        break;

                    case VipsToken.Right:
                        depth--;
                        break;
                }
            } while (!(sub_token == VipsToken.Right && depth == 0));

            var i2 = Math.Min(p - p, size);
            string.Substring(0, i2);

            return p;
        }

        return p;
    }

    // vips__token_segment_need
    public static string TokenSegmentNeed(string p, VipsToken need_token, char[] string, int size)
    {
        var token = default(VipsToken);

        if ((p = TokenSegment(p, out token, string, size)) == null)
            return null;

        if (token != need_token)
            VipsError("get_token", _("expected %s, saw %s"), VipsEnumNick(VIPS_TYPE_TOKEN, need_token), VipsEnumNick(VIPS_TYPE_TOKEN, token));

        return p;
    }

    // vips__find_rightmost_brackets
    public static string FindRightMostBrackets(string p)
    {
        var start = new[] { p };
        var tokens = new[] { default(VipsToken) };
        var str = new char[VIPS_PATH_MAX];
        int n, i;
        int nest;

        for (n = 0; n < VIPS_NUMBER(tokens); n++)
            if ((p = TokenGet(start[n], out tokens[n], str, VIPS_PATH_MAX)) == null)
                return null;

        if (n >= VIPS_NUMBER(tokens))
            return null;

        if (tokens[n - 1] != VipsToken.Right)
            return null;

        nest = 0;
        for (i = n - 1; i >= 0; i--)
        {
            if (tokens[i] == VipsToken.Right)
                nest++;
            else if (tokens[i] == VipsToken.Left)
                nest--;

            if (nest == 0)
                break;
        }

        if (nest != 0)
            return null;

        return start[i];
    }

    // vips__filename_split8
    public static void FilenameSplit8(string name, char[] filename, char[] option_string)
    {
        string.Substring(0, VIPS_PATH_MAX);
        var p = FindRightMostBrackets(filename);

        if (p != null)
            string.Substring(p, VIPS_PATH_MAX);
        else
            string.Substring("", VIPS_PATH_MAX);
    }

    // vips_ispoweroftwo
    public static int IsPowerOfTwo(int p)
    {
        int i, n;

        for (i = 0, n = 0; p > 0; i++, p >>= 1)
            if ((p & 1) != 0)
                n++;

        return n == 1 ? i : 0;
    }

    // vips_amiMSBfirst
    public static int AmiMSBFirst()
    {
#if G_BYTE_ORDER == G_BIG_ENDIAN
        return 1;
#elif G_BYTE_ORDER == G_LITTLE_ENDIAN
        return 0;
#else
#error "Byte order not recognised"
#endif
    }

    // vips__temp_dir
    public static string TempDir()
    {
        var tmpd = Environment.GetEnvironmentVariable("TMPDIR");

#if G_OS_WIN32
        if (tmpd == null)
        {
            var buf = new char[256];
            GetWindowsDirectoryW(buf, 256);

            if (GetTempPath(256, buf))
                tmpd = buf;
            else
                tmpd = "C:\\temp";
        }
#else
        tmpd = "/tmp";
#endif

        return tmpd;
    }

    // vips__temp_name
    public static string TempName(string format)
    {
        var global_serial = new int();
        var serial = Interlocked.Increment(ref global_serial);
        var file = $"vips-{serial}-{Guid.NewGuid()}";

        var path = Path.Combine(TempDir(), file);

        return path;
    }

    // vips__change_suffix
    public static void ChangeSuffix(string name, char[] out, int mx, string new_suffix, string[] olds, int nolds)
    {
        string.Substring(0, mx);
        while ((var p = strrchr(out, '.')) != null)
        {
            for (int i = 0; i < nolds; i++)
                if (string.Equals(p, olds[i], StringComparison.OrdinalIgnoreCase))
                    *p = '\0';

            if (*p != '\0')
                break;
        }

        string.Substring(0, mx);
        string.Concat(new_suffix);
    }

    // vips__parse_size
    public static ulong ParseSize(string size_string)
    {
        var units = new[]
        {
            { 'k', 1024 },
            { 'm', 1024 * 1024 },
            { 'g', 1024 * 1024 * 1024 },
            { 'b', 1024 * 1024 * 1024 }
        };

        ulong size;
        int n;
        char unit;

        var unit_str = size_string;

        n = sscanf(size_string, "%" + G_GUINT64_FORMAT + " %s", out size, out unit);

        if (n > 1)
            foreach (var u in units)
                if (char.ToLower(unit) == u.unit)
                    size *= u.multiplier;

        return size;
    }

    // vips_enum_string
    public static string EnumString(GType enm, int v)
    {
        var value = GEnumValue.Get(enm, v);

        return value.ValueName;
    }

    // vips_enum_nick
    public static string EnumNick(GType enm, int v)
    {
        var value = GEnumValue.Get(enm, v);

        return value.ValueNick;
    }

    // vips_enum_from_nick
    public static int EnumFromNick(string domain, GType type, string nick)
    {
        var class_ = GTypeClass.GetType(type);
        var enum_class = GEnumClass.GetType(class_);
        var enum_value = GEnumValue.Get(enum_class, nick);

        return enum_value.Value;
    }

    // vips_flags_from_nick
    public static int FlagsFromNick(string domain, GType type, string nick)
    {
        var class_ = GTypeClass.GetType(type);
        var flags_class = GFlagsClass.GetType(class_);
        var flags_value = GFlagsValue.Get(flags_class, nick);

        return flags_value.Value;
    }

    // vips__substitute
    public static int Substitute(char[] buf, size_t len, char[] sub)
    {
        var buflen = buf.Length;
        var sublen = sub.Length;

        int lowest_n;
        char* sub_start;
        char* p;
        char* sub_end;
        size_t before_len, marker_len, after_len, final_len;

        g_assert(buflen < len);

        lowest_n = -1;
        sub_start = null;
        sub_end = null;
        for (p = buf; (p = strchr(p, '%')) != null; p++)
            if (g_ascii_isdigit(p[1]))
            {
                char* q;

                for (q = p + 1; g_ascii_isdigit(*q); q++)
                    ;
                if (q[0] == 's')
                {
                    int n;

                    n = atoi(p + 1);
                    if (lowest_n == -1 || n < lowest_n)
                    {
                        lowest_n = n;
                        sub_start = p;
                        sub_end = q + 1;
                    }
                }
            }

        if (!sub_start)
            for (p = buf; (p = strchr(p, '%')) != null; p++)
                if (p[1] == 's')
                {
                    sub_start = p;
                    sub_end = p + 2;
                    break;
                }

        if (!sub_start)
            return -1;

        before_len = sub_start - buf;
        marker_len = sub_end - sub_start;
        after_len = buflen - (before_len + marker_len);
        final_len = before_len + sublen + after_len + 1;
        if (final_len > len)
            return -1;

        memmove(buf + before_len + sublen, buf + before_len + marker_len, after_len + 1);
        memmove(buf + before_len, sub, sublen);

        return 0;
    }

    // vips_realpath
    public static string Realpath(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            var cwd = Directory.GetCurrentDirectory();
            var real_path = Path.Combine(cwd, path);
            return real_path;
        }
        else
            return path;
    }

    // vips__random_add
    public static uint RandomAdd(uint hash, int value)
    {
        return (hash ^ (uint)value) * 16777619u;
    }

    // vips__random
    public static uint Random(uint seed)
    {
        var hash = 2166136261u;

        return RandomAdd(hash, seed);
    }

    // vips_icc_dir
    public static string IccDir()
    {
#if G_OS_WIN32
        var wwindowsdir = new char[MAX_PATH];
        if (GetWindowsDirectoryW(wwindowsdir, MAX_PATH))
        {
            var windowsdir = g_utf16_to_utf8(wwindowsdir, -1, null, null, null);
            var full_path = Path.Combine(windowsdir, "system32", "spool", "drivers", "color");
            return full_path;
        }
#endif

        return VIPS_ICC_DIR;
    }

    // vips__icc_dir
    public static string IccDir()
    {
        static GOnce once = G_ONCE_INIT;

        return (string)g_once(&once, () => IccDir(), null);
    }

    // vips__windows_prefix
    public static string WindowsPrefix()
    {
#if G_OS_WIN32
        var prefix = g_win32_get_package_installation_directory_of_module(vips_dll);

#else
        var prefix = Environment.GetEnvironmentVariable("VIPSHOME");
#endif

        return prefix;
    }

    // vips__windows_prefix
    public static string WindowsPrefix()
    {
        static GOnce once = G_ONCE_INIT;

        return (string)g_once(&once, () => WindowsPrefix(), null);
    }

    // vips__get_iso8601
    public static string GetIso8601()
    {
#if GLIB_CHECK_VERSION(2, 62, 0)
        var now = g_date_time_new_now_local();
        return g_date_time_format_iso8601(now);
#else
        var now = new GTimeVal();
        g_get_current_time(ref now);
        return g_time_val_to_iso8601(ref now);
#endif

    }

    // vips_strtod
    public static int Strtod(string str, out double out)
    {
        out = 0;

        for (var p = str; *p != '\0'; p++)
            if (g_ascii_isdigit(*p))
                break;
        if (*p == '\0')
            return -1;

        var n = sscanf(str, "%" + G_GUINT64_FORMAT + " %s", out);

        if (n > 1)
            return 0;

        return -1;
    }
}
```