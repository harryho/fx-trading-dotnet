using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;

namespace fxpa.ui;

/// <summary>
/// Owns a dedicated STA thread that runs a real WinForms message loop.
///
/// Why this exists
/// ---------------
/// xunit v2 executes test bodies on MTA thread-pool threads. WinForms controls
/// created on an MTA thread misbehave: OLE drag/drop, clipboard, context menus
/// and several GDI+ paths require a single-threaded apartment, and the app
/// itself (see <c>tests/fxpa.tests/ThreadAffinityTests.cs</c>) pins the
/// cross-thread guard because the production code touches controls from
/// provider threads.
///
/// The host therefore does what a real application does:
///   * one dedicated STA thread,
///   * one hidden top-level <see cref="Form"/> whose handle is created on that
///     thread (a control must own its HWND on the thread that will paint it),
///   * a live <see cref="Application.Run(Form)"/> message pump, so that the
///     <c>Control.BeginInvoke</c> calls the chart code issues are actually
///     processed instead of silently queueing up forever.
///
/// The hidden form is parked off-screen (Location = -32000,-32000) so the test
/// does not steal focus from the developer's desktop, but it is a genuine,
/// visible, realised window - the chart pane really paints.
///
/// Every entry point is timeout-guarded: <see cref="Run{T}"/> never blocks
/// longer than the supplied budget, so a regression that deadlocks the UI
/// thread fails the test instead of hanging CI.
/// </summary>
internal sealed class StaTestHost : IDisposable
{
    private readonly Thread _uiThread;
    private readonly ManualResetEventSlim _ready = new ManualResetEventSlim(false);
    private readonly string _name;
    private Form? _dispatcher;
    private Exception? _startupFailure;
    private bool _disposed;

    public StaTestHost(string name)
    {
        _name = name;

        _uiThread = new Thread(RunMessageLoop)
        {
            IsBackground = true,
            Name = name,
        };
        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        if (!_ready.Wait(TimeSpan.FromSeconds(30)))
        {
            throw new TimeoutException(
                $"The STA host '{name}' did not start its message loop within 30 seconds.");
        }

        if (_startupFailure is not null)
        {
            throw new InvalidOperationException(
                $"The STA host '{name}' could not create a window. " +
                "This machine most likely has no interactive Windows desktop session " +
                "(headless CI agent, service account, or session 0). " +
                $"Underlying error: {_startupFailure.Message}",
                _startupFailure);
        }
    }

    /// <summary>True when the process runs in an interactive Windows session.</summary>
    public bool HasInteractiveDesktop => Environment.UserInteractive;

    private void RunMessageLoop()
    {
        try
        {
            var form = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000),
                Size = new Size(200, 200),
                Text = _name,
            };

            _dispatcher = form;

            // Force the HWND to be created on THIS (STA) thread. A handle created
            // on a provider thread breaks ControlCollection.Add later on, and
            // Control.Invoke needs the handle to exist before it can marshal.
            IntPtr handle = form.Handle;

            _ready.Set();

            // Real message pump. Returns when the form is closed.
            Application.Run(form);
        }
        catch (Exception ex)
        {
            _startupFailure = ex;
            _ready.Set();
        }
    }

    /// <summary>
    /// Runs <paramref name="body"/> on the STA thread, inside the live message
    /// loop, and returns its result.
    /// </summary>
    /// <exception cref="TimeoutException">
    /// The body did not complete within <paramref name="timeout"/>.
    /// </exception>
    public T Run<T>(Func<T> body, TimeSpan timeout)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(StaTestHost));
        }

        Form dispatcher = _dispatcher
            ?? throw new InvalidOperationException($"The STA host '{_name}' has no dispatcher form.");

        object? result = null;
        Exception? failure = null;

        var worker = new Thread(() =>
        {
            try
            {
                result = dispatcher.Invoke(body);
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                failure = tie.InnerException;
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        })
        {
            IsBackground = true,
            Name = _name + "-worker",
        };

        worker.Start();

        if (!worker.Join(timeout))
        {
            // The UI thread is wedged. The worker thread is a background thread,
            // so the test host can still tear the process down.
            throw new TimeoutException(
                $"The UI scenario on STA thread '{_name}' did not finish within {timeout.TotalSeconds:0} seconds. " +
                "The chart code most likely deadlocked (for example a blocking cross-thread wait).");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        return (T)result!;
    }

    /// <summary>
    /// Lets the message pump drain whatever the chart code posted with
    /// <c>Control.BeginInvoke</c> (the chart does this whenever a series
    /// updates), so the pane is in its settled state before it is rendered.
    /// </summary>
    public void Pump(int rounds = 3)
    {
        Run(() =>
        {
            for (int i = 0; i < rounds; i++)
            {
                Application.DoEvents();
                Thread.Sleep(15);
            }
            return true;
        }, TimeSpan.FromSeconds(30));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            Form? dispatcher = _dispatcher;
            if (dispatcher is not null && dispatcher.IsHandleCreated)
            {
                // Close must happen on the owning thread.
                dispatcher.Invoke(new Action(() => dispatcher.Close()));
            }
        }
        catch (Exception)
        {
            // Best effort: the process is going away anyway.
        }

        _uiThread.Join(TimeSpan.FromSeconds(10));
        _ready.Dispose();
    }
}
