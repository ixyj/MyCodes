namespace IFELanguageHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComImport]
    [Guid("019F7152-E6DB-11D0-83C3-00C04FDDB82E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFELanguage
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int Open();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int Close();
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetJMorphResult(
            [In] uint dwRequest,
            [In] uint dwCMode,
            [In] int cwchInput,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pwchInput,
            [In] IntPtr pfCInfo,
            [Out] out IntPtr ppResult
        );
    }

    public class PinyinHelper
    {
        public const int FELANG_REQ_REV = 0x00030000;
        public const int FELANG_CMODE_PINYIN = 0x00000100;
        public const int FELANG_CMODE_NOINVISIBLECHAR = 0x40000000;
        public const int CLSCTX_INPROC_SERVER = 1;
        public const int CLSCTX_INPROC_HANDLER = 2;
        public const int CLSCTX_LOCAL_SERVER = 4;
        public const int CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;
        public const int S_OK = 0x00000000;

        private static object lockObj = new object();
        public static bool coInitialized = Initialize();
        private static Dictionary<char, string> Tone;
        private static IntPtr vMorrslt;
        private static IFELanguage vLanguage;

        [DllImport("ole32.dll")]
        public static extern int CLSIDFromString(
              [MarshalAs(UnmanagedType.LPWStr)] string lpsz, out Guid clsid);
        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid,
                IntPtr pUnkOuter, uint dwClsContext,
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid,
                out IntPtr pv);
        [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CoInitialize(IntPtr pvReserved);
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);

        [DllImport("kernel32.dll")]
        public static extern int FormatMessage(int dwFlags, IntPtr lpSource,
                int dwMessageId, int dwLanguageId,
                StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        public static string GetMessage(int errorCode)
        {
            var lpBuffer = new StringBuilder(0x200);
            return FormatMessage(FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY,
                IntPtr.Zero, errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0 ?
            lpBuffer.ToString() : "Unknown";
        }

        public static bool Initialize()
        {
            lock (lockObj)
            {
                if (coInitialized)
                {
                    Console.WriteLine("Already initialized");
                    return true;
                }

                Tone = new Dictionary<char, string>
                {
                    {'ā',"a1"},
                    {'á',"a2"},
                    {'ǎ',"a3"},
                    {'à',"a4"}, 
                    {'ō',"o1"},
                    {'ó',"o2"},
                    {'ǒ',"o3"},
                    {'ò',"o4"},
                    {'ē',"e1"},
                    {'é',"e2"},
                    {'ě',"e3"},
                    {'è',"e4"}, 
                    {'ī',"i1"},
                    {'í',"i2"},
                    {'ǐ',"i3"},
                    {'ì',"i4"}, 
                    {'ū',"u1"},
                    {'ú',"u2"},
                    {'ǔ',"u3"},
                    {'ù',"u4"}, 
                    {'ǖ',"v1"},
                    {'ǘ',"v2"},
                    {'ǚ',"v3"},
                    {'ǜ',"v4"},
                    {'ü',"v5"} };

                CoInitialize(IntPtr.Zero);
                // Console.WriteLine(unchecked((int)0x80040154));
                Guid vGuidIme;
                var vError = CLSIDFromString("{E4288337-873B-11D1-BAA0-00AA00BBB8C0}", out vGuidIme);
                if (vError != S_OK)
                {
                    Console.WriteLine($"Err: Failed to get CLSID: {vError}");
                    return false;
                }
                var vGuidLanguage = new Guid("019F7152-E6DB-11D0-83C3-00C04FDDB82E");
                IntPtr vPPV;
                vError = CoCreateInstance(vGuidIme, IntPtr.Zero, CLSCTX_SERVER, vGuidLanguage, out vPPV);
                if (vError != S_OK)
                {
                    Console.WriteLine($"Err: Failed to create COM object: {vError}");
                    return false;
                }
                vLanguage = Marshal.GetTypedObjectForIUnknown(vPPV, typeof(IFELanguage)) as IFELanguage;
                vError = vLanguage.Open();
                if (vError != S_OK)
                {
                    Console.WriteLine($"Err: Failed to open IFELanguage: {vError}");
                    return false;
                }
                coInitialized = true;
                Console.WriteLine("InitializeLanguage Succeeded");
                return true;
            }
        }

        public static int CloseLanguage()
        {
            return vLanguage != null ? vLanguage.Close() : S_OK;
        }

        // Label "[STAThread]" at Main() before using it
        public static string GetPinyin(string vInput)
        {
            if (string.IsNullOrWhiteSpace(vInput))
            {
                return string.Empty;
            }

            var vError = vLanguage.GetJMorphResult(FELANG_REQ_REV, FELANG_CMODE_PINYIN | FELANG_CMODE_NOINVISIBLECHAR,
                vInput.Length, vInput, IntPtr.Zero, out vMorrslt);
            if (vError != S_OK)
            {
                Console.WriteLine("Err: Failed to GetJMorphResult");
                return null;
            }
            var vPinYin = Marshal.PtrToStringUni(Marshal.ReadIntPtr(vMorrslt, 4), Marshal.ReadInt16(vMorrslt, 8));
            var vMonoRubyPos = Marshal.ReadIntPtr(vMorrslt, 28);
            var iMonoRubyPos = Marshal.ReadInt16(vMonoRubyPos);
            vMonoRubyPos = (IntPtr)((int)vMonoRubyPos + 2);
            var pinyin = new StringBuilder();
            foreach (var input in vInput)
            {
                var iNextMonoRubyPos = Marshal.ReadInt16(vMonoRubyPos);
                if (input >= '\uD800' && input <= '\uD8FF')
                {
                    // For Supplementary plane in in UTF-16, jump it
                    pinyin.Append("'").Append(input);
                    continue;
                }

                if (iNextMonoRubyPos == iMonoRubyPos)
                {
                    // No Pinyin tranformation
                    pinyin.Append("'").Append(input);
                }
                else
                {
                    // Has Pinyin tranformation
                    var py = vPinYin.Substring(iMonoRubyPos, iNextMonoRubyPos - iMonoRubyPos);
                    var tone = 5;
                    pinyin.Append("'");
                    foreach (char c in py)
                    {
                        string p;
                        if (Tone.TryGetValue(c, out p))
                        {
                            pinyin.Append(p.First());
                            tone = char.IsDigit(p.Last()) ? int.Parse(p.Last().ToString()) : tone;
                        }
                        else
                        {
                            pinyin.Append(c);
                        } 
                    }

                    pinyin.Append(tone);
                }

                vMonoRubyPos = (IntPtr)((int)vMonoRubyPos + 2);
                iMonoRubyPos = iNextMonoRubyPos;
            }

            CoTaskMemFree(vMorrslt);
            return pinyin.ToString().Trim(new[] { '\'' });
        }
    }
}
