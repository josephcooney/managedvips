Here is the converted C# code:

```csharp
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vips {
    public class FileMapping {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, uint flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, IntPtr dwNumberOfBytesToMap, out IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

        public static bool MmapSupported(int fd) {
            IntPtr baseaddr;
            int length = 4096;
            long offset = 0;

#if G_OS_WIN32
            {
                IntPtr hFile = _get_osfhandle(fd);
                uint flProtect = 0x02; // PAGE_READONLY
                IntPtr hMMFile = CreateFileMapping(hFile, IntPtr.Zero, flProtect, 0, 0, null);

                if (hMMFile == IntPtr.Zero) return false;

                baseaddr = MapViewOfFileEx(hMMFile, 0x04, 0, 0, 0, IntPtr.Zero);
                if (baseaddr == IntPtr.Zero) {
                    CloseHandle(hMMFile);
                    return false;
                }

                UnmapViewOfFile(baseaddr);
            }
#else
            {
                int prot = 1; // PROT_READ
                int flags = 2; // MAP_SHARED

                baseaddr = mmap(0, length, prot, flags, fd, offset);
                if (baseaddr == IntPtr.Zero) return false;

                munmap(baseaddr, length);
            }
#endif

            return true;
        }

        public static IntPtr Mmap(int fd, bool writeable, int length, long offset) {
#if DEBUG
            Console.WriteLine("vips__mmap: length = 0x{0:x}, offset = 0x{1:x}", length, offset);
#endif

#if G_OS_WIN32
            {
                IntPtr hFile = _get_osfhandle(fd);

                uint flProtect;
                uint dwDesiredAccess;

                if (writeable) {
                    flProtect = 0x04; // PAGE_READWRITE
                    dwDesiredAccess = 0x0002; // FILE_MAP_WRITE
                }
                else {
                    flProtect = 0x02; // PAGE_READONLY
                    dwDesiredAccess = 0x0001; // FILE_MAP_READ
                }

                IntPtr hMMFile = CreateFileMapping(hFile, IntPtr.Zero, flProtect, 0, 0, null);

                if (hMMFile == IntPtr.Zero) {
                    vips_error_system(GetLastError(), "vips_mapfile", "%s", "unable to CreateFileMapping");
                    return IntPtr.Zero;
                }

                baseaddr = MapViewOfFileEx(hMMFile, dwDesiredAccess, 0, 0, 0, IntPtr.Zero);

                if (baseaddr == IntPtr.Zero) {
                    vips_error_system(GetLastError(), "vips_mapfile", "%s", "unable to MapViewOfFile");
                    CloseHandle(hMMFile);
                    return IntPtr.Zero;
                }

                CloseHandle(hMMFile);
            }
#else
            {
                int prot;
                int flags;

                if (writeable) prot = 3; // PROT_WRITE
                else prot = 1; // PROT_READ

                flags = 2; // MAP_SHARED

#if MAP_NOCACHE
                flags |= 8; // MAP_NOCACHE
#endif

                baseaddr = mmap(0, length, prot, flags, fd, offset);
                if (baseaddr == IntPtr.Zero) {
                    vips_error_system(errno, "vips_mapfile", "%s", "unable to mmap");
                    return IntPtr.Zero;
                }
            }
#endif

            return baseaddr;
        }

        public static int Munmap(IntPtr start, int length) {
#if G_OS_WIN32
            if (!UnmapViewOfFile(start)) {
                vips_error_system(GetLastError(), "vips_mapfile", "%s", "unable to UnmapViewOfFile");
                return -1;
            }
#else
            if (munmap(start, length) < 0) {
                vips_error_system(errno, "vips_mapfile", "%s", "unable to munmap file");
                return -1;
            }
#endif

            return 0;
        }

        public static int MapFile(VipsImage image) {
            struct stat st = new struct stat();
            mode_t m;

            if (image.baseaddr != IntPtr.Zero) return 0;

            // Check the size of the file; if it is less than 64 bytes, then flag an error
            if (image.file_length < 64) {
                vips_error("vips_mapfile", "%s", "file is less than 64 bytes");
                return -1;
            }

            if (fstat(image.fd, ref st) == -1) {
                vips_error("vips_mapfile", "%s", "unable to get file status");
                return -1;
            }
            m = (mode_t)st.st_mode;

            // Check that the file is a regular file
            if (!S_ISREG(m)) {
                vips_error("vips_mapfile", "%s", "not a regular file");
                return -1;
            }

            image.baseaddr = Mmap(image.fd, false, image.file_length, 0);

            if (image.baseaddr == IntPtr.Zero) return -1;

            image.length = image.file_length;

            return 0;
        }

        public static int MapFileRW(VipsImage image) {
            struct stat st = new struct stat();
            mode_t m;

            if (image.baseaddr != IntPtr.Zero) return 0;

            // Check the size of the file; if it is less than 64 bytes, then flag an error
            if (fstat(image.fd, ref st) == -1) {
                vips_error("vips_mapfilerw", "%s", "unable to get file status");
                return -1;
            }
            m = (mode_t)st.st_mode;

            // Check that the file is a regular file
            if (image.file_length < 64 || !S_ISREG(m)) {
                vips_error("vips_mapfile", "%s", "unable to read data");
                return -1;
            }

            image.baseaddr = Mmap(image.fd, true, image.file_length, 0);

            if (image.baseaddr == IntPtr.Zero) return -1;

            image.length = image.file_length;

            return 0;
        }

        public static int RemapFileRW(VipsImage image) {
#if G_OS_WIN32
            {
                IntPtr hFile = _get_osfhandle(image.fd);
                IntPtr hMMFile;

                if (hMMFile = CreateFileMapping(hFile, IntPtr.Zero, 0x04, 0, 0, null)) {
                    baseaddr = MapViewOfFileEx(hMMFile, 0x0002, 0, 0, 0, image.baseaddr);

                    if (baseaddr == IntPtr.Zero) {
                        vips_error_system(GetLastError(), "vips_mapfile", "%s", "unable to CreateFileMapping");
                        CloseHandle(hMMFile);
                        return -1;
                    }

                    UnmapViewOfFile(image.baseaddr);
                }
            }
#else
            {
                image.dtype = VIPS_IMAGE_MMAPINRW;

                baseaddr = mmap(image.baseaddr, image.length, 3, 2 | 8, image.fd, 0);

                if (baseaddr == IntPtr.Zero) {
                    vips_error("vips_mapfile", "%s", "unable to mmap");
                    return -1;
                }
            }
#endif

            image.baseaddr = baseaddr;

            return 0;
        }
    }

    public class VipsImage {
        public int fd;
        public long file_length;
        public IntPtr baseaddr;
        public int length;
        public int dtype;
        public string filename;
    }

    public static class VipsError {
        public static void Error(string message, string format, params object[] args) {
            Console.WriteLine(message + ": " + string.Format(format, args));
        }
    }

    public static class VipsSystemError {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetLastError();

        public static void SystemError(int errorCode, string message, string format, params object[] args) {
            Console.WriteLine(message + ": " + string.Format(format, args));
            Console.WriteLine("Error code: " + errorCode);
        }
    }

    public class VipsConfig {
        [DllImport("kernel32.dll")]
        private static extern IntPtr _get_osfhandle(int fd);

        public static IntPtr GetOsHandle(int fd) {
            return _get_osfhandle(fd);
        }
    }
}
```

Note that I've assumed the `VipsImage` struct and the `vips_error` function are defined elsewhere in your codebase. If not, you'll need to define them as well.

Also note that this is a direct translation of the C code, without any optimizations or improvements specific to C#.