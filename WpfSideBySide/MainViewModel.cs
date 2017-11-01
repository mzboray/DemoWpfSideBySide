using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PlugInInterfaces;

namespace WpfSideBySide
{
    class MainViewModel : ViewModelBase
    {
        private object _selectedPlugIn, _content;
        private bool _canSearch = true;
        private string _searchPath;

        public MainViewModel()
        {
            LoadedAssemblies = new ObservableCollection<AssemblyViewModel>();

            LoadCommand = new DelegateCommand(async o =>
            {
                if (o is string path)
                {
                    CanSearch = false;
                    var assemblies = await LoadFromPath(path);
                    LoadedAssemblies.Clear();
                    foreach (var asm in assemblies)
                    {
                        if (asm.HasPlugIn)
                        {
                            LoadedAssemblies.Add(asm);
                        }
                    }
                    CanSearch = true;
                }
            }, o => CanSearch);

            SearchPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        }

        public string SearchPath
        {
            get => _searchPath;
            set
            {
                SetAndRaise(ref _searchPath, value);
            }
        }

        public bool CanSearch
        {
            get => _canSearch;
            set => SetAndRaise(ref _canSearch, value);
        }

        public object SelectedPlugIn
        {
            get => _selectedPlugIn;
            set => SetAndRaise(ref _selectedPlugIn, value);
        }

        public object Content
        {
            get => _content;
            set => SetAndRaise(ref _content, value);
        }

        public DelegateCommand LoadCommand { get; set; }

        public ObservableCollection<AssemblyViewModel> LoadedAssemblies { get; set; }

        public void Show(AssemblyViewModel asmVm)
        {
            try
            {
                Content = asmVm.PlugIn.GetControl();
            }
            catch (Exception e)
            {
                Content = e;
            }
        }

        private Task<List<AssemblyViewModel>> LoadFromPath(string path)
        {
            return Task.Run(() =>
            {
                var assemblies = new List<AssemblyViewModel>();

                if (!Directory.Exists(path))
                {
                    return assemblies;
                }

                foreach (string dllFile in Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories))
                {
                    var vm = new AssemblyViewModel()
                    {
                        Path = dllFile
                    };

                    assemblies.Add(vm);

                    try
                    {
                        var name = AssemblyName.GetAssemblyName(dllFile);
                        vm.AssemblyName = name.Name;
                        vm.AssemblyVersion = name.Version.ToString();
                        vm.PublicKeyToken = HashHelper.ToString(name.GetPublicKeyToken());
                    }
                    catch (Exception e)
                    {
                        vm.Error = e.ToString();
                        continue;
                    }

                    try
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(dllFile);
                        vm.FileVersion = versionInfo.FileVersion;
                    }
                    catch (Exception e)
                    {
                        vm.Error = e.ToString();
                        continue;
                    }

                    try
                    {
                        var asm = Assembly.LoadFrom(dllFile);
                        vm.Assemby = asm;
                        var pluginType = asm.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IPlugIn)));
                        if (pluginType == null)
                        {
                            continue;
                        }

                        var plugin = (IPlugIn)Activator.CreateInstance(pluginType);
                        vm.PlugIn = plugin;
                    }
                    catch (Exception e)
                    {
                        vm.Error = e.ToString();
                        continue;
                    }
                }

                return assemblies;
            });
        }

    }

    class AssemblyViewModel
    {
        public string AssemblyName { get; set; }

        public string FileVersion { get; set; }

        public string PublicKeyToken { get; set; }

        public string AssemblyVersion { get; set; }

        public string Path { get; set; }

        public string Error { get; set; }

        public Assembly Assemby { get; set; }

        public IPlugIn PlugIn { get; set; }

        public string PlugInName => PlugIn?.Name ?? string.Empty;

        public bool HasPlugIn => PlugIn != null;

        public bool HasError => !string.IsNullOrEmpty(Error);
    }


    class DelegateCommand : ICommand
    {
        private Func<object, bool> _canExecute;
        private Action<object> _execute;

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }


    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetAndRaise<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            var comparer = EqualityComparer<T>.Default;
            if (!comparer.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    internal static class HashHelper
    {
        public static string ToString(byte[] hash)
        {
            var builder = new StringBuilder();
            foreach (byte b in hash)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
