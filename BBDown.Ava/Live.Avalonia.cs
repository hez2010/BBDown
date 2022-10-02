using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace Live.Avalonia;

public interface ILiveView
{
    object CreateView(Window window);
}

internal sealed class LiveControlLoader
{
    private readonly Action<string> _logger;

    public LiveControlLoader(Action<string> logger) => _logger = logger;

    public void LoadControl(string assemblyPath, Window window)
    {
        try
        {
            _logger($"Loading assembly from {assemblyPath}");
            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            var assembly = Assembly.Load(assemblyBytes);

            _logger("Obtaining ILiveView interface implementation...");
            var interfaceType = assembly.GetType(typeof(ILiveView).FullName);
            var allImplementations = assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface)
                .ToList();

            if (allImplementations.Count == 0)
                throw new TypeLoadException($"No ILiveView interface implementations found in {assemblyPath}");
            if (allImplementations.Count > 1)
                throw new TypeLoadException(
                    $"Multiple ILiveView interface implementations found in {assemblyPath}");

            _logger("Successfully managed to obtain ILiveView interface implementation, activating...");
            var liveViewType = allImplementations.First();
            var instance = Activator.CreateInstance(liveViewType);
            var name = nameof(ILiveView.CreateView);
            var method = liveViewType.GetMethod(name) ?? interfaceType.GetMethod(name);

            if (method == null)
                throw new TypeLoadException($"Unable to obtain {nameof(ILiveView.CreateView)} method!");

            _logger($"Successfully managed to obtain the method {nameof(ILiveView.CreateView)}, creating control.");
            window.Content = method.Invoke(instance, new object[] { window });
        }
        catch (Exception exception)
        {
            window.Content = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = exception.ToString()
            };
        }
    }
}


internal sealed class LiveFileWatcher : IDisposable
{
    readonly Subject<string> _fileChanged = new Subject<string>();
    readonly Action<string> _logger;

    readonly FileSystemWatcher watcher = new FileSystemWatcher()
    {
        EnableRaisingEvents = false
    };


    public LiveFileWatcher(Action<string> logger) => _logger = logger;

    public IObservable<string> FileChanged => _fileChanged.Throttle(TimeSpan.FromSeconds(0.5));

    public void StartWatchingFileCreation(string dir, string filePath)
    {
        try
        {
            _logger($"Registering observable file system watcher for file at: {filePath}");
            watcher.Path = dir;
            watcher.Filter = Path.GetFileName(filePath);
            watcher.Changed += OnChanged;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void OnChanged(object sender, FileSystemEventArgs args)
    {
        _fileChanged.OnNext(args.FullPath);
    }

    public void Dispose()
    {
        _logger("Stopping the file creation watcher timer...");
        _fileChanged.Dispose();
        watcher.Changed -= OnChanged;
        watcher.Dispose();
    }
}

internal class LiveSourceWatcher : IDisposable
{
    private readonly Action<string> _logger;
    private Process _dotnetWatchBuildProcess;

    public LiveSourceWatcher(Action<string> logger) => _logger = logger;

    public (string dir, string file) StartRebuildingAssemblySources(string assemblyPath)
    {
        _logger("Attempting to run 'dotnet watch' command for assembly sources...");
        var binDirectoryPath = FindAscendantDirectory(assemblyPath, "bin");
        var projectDirectory = Path.GetDirectoryName(binDirectoryPath);
        if (projectDirectory == null)
            throw new IOException($"Unable to parent directory of {binDirectoryPath}");

        _logger($"Preparing .live-bin directory...");
        var dotnetWatchBuildPath = Path.Combine(binDirectoryPath, ".live-bin") + Path.DirectorySeparatorChar;
        if (Directory.Exists(dotnetWatchBuildPath))
        {
            Directory.Delete(dotnetWatchBuildPath, true);
            Directory.CreateDirectory(dotnetWatchBuildPath);
        }

        _logger($"Executing 'dotnet watch' command from {projectDirectory}, building into {dotnetWatchBuildPath}");
        _dotnetWatchBuildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"watch msbuild /p:BaseOutputPath={dotnetWatchBuildPath}",
                UseShellExecute = true,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                WorkingDirectory = projectDirectory
            }
        };

        _dotnetWatchBuildProcess.Start();
        _logger($"Successfully managed to start 'dotnet watch' process with id {_dotnetWatchBuildProcess.Id}");
        var separator = Path.DirectorySeparatorChar;
        var liveAssemblyPath = assemblyPath.Replace($"{separator}bin{separator}", $"{separator}bin{separator}.live-bin{separator}");
        return (dir: dotnetWatchBuildPath, file: liveAssemblyPath);
    }

    public void Dispose()
    {
        if (_dotnetWatchBuildProcess == null) return;
        _logger($"Killing 'dotnet watch' process {_dotnetWatchBuildProcess.Id} and dependent processes...");
        _dotnetWatchBuildProcess.Kill(true);
    }

    private static string FindAscendantDirectory(string filePath, string directoryName)
    {
        var currentPath = filePath;
        while (true)
        {
            currentPath = Path.GetDirectoryName(currentPath);
            if (currentPath == null)
                throw new IOException($"Unable to get parent directory of {filePath} named {directoryName}");

            var directoryInfo = new DirectoryInfo(currentPath);
            if (directoryName == directoryInfo.Name)
                return currentPath;
        }
    }
}


public sealed class LiveViewHost : Window, IDisposable
{
    private readonly LiveFileWatcher _assemblyWatcher;
    private readonly LiveSourceWatcher _sourceWatcher;
    private readonly IDisposable _subscription;
    private readonly Action<string> _logger;
    private readonly string _assemblyPath;

    public LiveViewHost(ILiveView view, Action<string> logger)
    {
        _logger = logger;
        _sourceWatcher = new LiveSourceWatcher(logger);
        _assemblyWatcher = new LiveFileWatcher(logger);
        _assemblyPath = view.GetType().Assembly.Location;

        var loader = new LiveControlLoader(logger);
        _subscription = _assemblyWatcher
            .FileChanged
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(path => loader.LoadControl(path, this));

        Console.CancelKeyPress += (sender, args) => Clean("Console Ctrl+C key press.", false);
        AppDomain.CurrentDomain.ProcessExit += (sender, args) => Clean("Process termination.", false);
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => Clean(args.ExceptionObject.ToString(), true);
    }

    public void StartWatchingSourceFilesForHotReloading()
    {
        _logger("Starting source and assembly file watchers...");
        var (liveAssemblyDir, liveAssemblyFile) = _sourceWatcher.StartRebuildingAssemblySources(_assemblyPath);
        _assemblyWatcher.StartWatchingFileCreation(liveAssemblyDir, liveAssemblyFile);
    }

    public void Dispose()
    {
        _logger("Disposing LiveViewHost internals...");
        _sourceWatcher.Dispose();
        _assemblyWatcher.Dispose();
        _subscription.Dispose();
        _logger("Successfully disposed LiveViewHost internals.");
    }

    private void Clean(string reason, bool exception)
    {
        _logger($"Closing live reloading window due to: {reason}");
        if (exception)
            _logger("\nNote: To prevent your app from crashing, properly handle all exceptions causing a crash.\n" +
                    "If you are using ReactiveUI and ReactiveCommand<,>s, make sure you subscribe to " +
                    "RxApp.DefaultExceptionHandler: https://reactiveui.net/docs/handbook/default-exception-handler\n" +
                    "If you are using another framework, refer to its docs considering global exception handling.\n");
        Dispose();
        Process.GetCurrentProcess().Kill();
    }
}