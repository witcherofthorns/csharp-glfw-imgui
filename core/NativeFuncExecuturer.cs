using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Core
{
    public static class NativeFuncExecuturer
    {
        private const int RtldLazy = 0x0001;

        private static class Windows {
            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
            [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryW(string lpszLib);
        }

        private static class Linux {
            [DllImport("libdl.so.2")]
            public static extern IntPtr dlopen(string path, int flags);
            [DllImport("libdl.so.2")]
            public static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        public static IntPtr LoadLibraryExt(string libName)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var ret = LoadLibrary(Path.Combine(baseDirectory, "runtimes", PlatformRid, "native", libName));

            if (ret == IntPtr.Zero) ret = LoadLibrary(libName);
            if (ret == IntPtr.Zero) throw new Exception("Failed to load library: " + libName);
            return ret;
        }

        private static IntPtr LoadLibrary(string libName)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Windows.LoadLibraryW(libName)
                : Linux.dlopen(libName, RtldLazy);
        }

        public static T LoadFunction<T>(IntPtr library, string function)
        {
            var ret = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Windows.GetProcAddress(library, function)
                : Linux.dlsym(library, function);

            return Marshal.GetDelegateForFunctionPointer<T>(ret);
        }

        private static string PlatformRid
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Environment.Is64BitProcess)
                    return "win-x64";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Environment.Is64BitProcess)
                    return "win-x86";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return "linux-x64";

                return "unknown";
            }
        }
    }
}