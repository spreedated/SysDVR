﻿using SDL2;
using SysDVR.Client.Core;
using SysDVR.Client.GUI;
using SysDVR.Client.Targets.Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SysDVR.Client.Platform
{
    internal static class Resources
    {
#if ANDROID_LIB
        // Android is such a shit platform that you can't fopen(), all resources must be read like this
        // thank god SDL already wraps its stupid interface
        public static byte[] ReadResouce(string path)
        {
            Console.WriteLine($"Loading resource {path}");

            var file = SDL.SDL_RWFromFile(path, "r").AssertNotNull(SDL.SDL_GetError);

            var len = (int)SDL.SDL_RWsize(file);
            var buf = new byte[len];

            var read = SDL.SDL_RWread(file, buf, 1, len);
            if (read != len)
                throw new Exception($"Loading resource {path} failed: {SDL.SDL_GetError()}");

            SDL.SDL_RWclose(file);
            return buf;
        }


        static string BasePath = "";        
        static string ResourcePath(string x) => x;

        public static bool HasDiskAccessPermission() 
        {
            if (!Program.Native.PlatformSupportsDiskAccess)
                return false;

            if (Program.Native.SysGetFileAccessInfo(out var canWrite, out _))
                return canWrite;
            else 
                Console.WriteLine("SysGetFileAccessInfo failed");

            return false;
        }

        public static bool CanRequestDiskAccessPermission() 
        {
            if (!Program.Native.PlatformSupportsDiskAccess)
                return false;

            if (Program.Native.SysGetFileAccessInfo(out _, out var canRequest))
                return canRequest;
            else
                Console.WriteLine("SysGetFileAccessInfo failed");

            return false;
        }

        public static void RequestDiskAccessPermission()
        {
            if (!Program.Native.PlatformSupportsDiskAccess)
                throw new NotImplementedException();

            if (!Program.Native.SysGetFileAccessInfo(out var hasPermission, out var canRequest))
                throw new NotImplementedException();

            if (hasPermission)
                return;

            if (!canRequest)
                return;

            Program.Native.SysRequestFileAccess();
        }
#else
        static string BasePath = Path.Combine(AppContext.BaseDirectory, "runtimes");
        
        static string ResourcePath(string x) => Path.Combine(BasePath, "resources", x);
        
        public static byte[] ReadResouce(string path) => File.ReadAllBytes(path);

        public static bool HasDiskAccessPermission() => true;
        public static bool CanRequestDiskAccessPermission() => true;
        public static void RequestDiskAccessPermission() => throw new NotImplementedException();
#endif

        public static string RuntimesFolder => BasePath;
        public static string MainFont => ResourcePath("OpenSans.ttf");
        public static string LoadingImage => ResourcePath("loading.yuv");

        public readonly static LazyImage Logo = new LazyImage(ResourcePath("logo.png"));
        public readonly static LazyImage UsbIcon = new LazyImage(ResourcePath("ico_usb.png"));
        public readonly static LazyImage WifiIcon = new LazyImage(ResourcePath("ico_wifi.png"));

        public static string? GetBuildId() 
        {
            try
            {
                var s = ReadResouce(ResourcePath("buildid.txt"));
                return Encoding.UTF8.GetString(s);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't load build id file" + ex.ToString());
            }

            return null;
        }
    }
}
