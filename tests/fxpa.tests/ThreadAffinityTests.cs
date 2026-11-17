using System;
using System.Threading;
using System.Windows.Forms;
using Xunit;

namespace fxpa.tests;

/// <summary>
/// Thread-affinity gate.
///
/// The application sets Control.CheckForIllegalCrossThreadCalls = false in
/// Program.Main for .NET Framework parity, which means cross-thread control
/// access that would have crashed on the old runtime now hangs or corrupts
/// state instead (documented in upgrade addenda SS9/SS10). The test host
/// defaults the guard back on; these tests pin that the guard is actually
/// enforced here, so any new cross-thread access introduced by a refactor
/// fails the suite instead of shipping as an intermittent freeze.
///
/// Baseline expectation: the CURRENT code passes because no test touches
/// controls from a foreign thread — the gate exists to catch future ones.
/// </summary>
public class ThreadAffinityTests
{
    private static void RunSta(Action action)
    {
        var thread = new Thread(() => action())
        {
            IsBackground = true,
            Name = "fxpa.tests-sta",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [Fact]
    public void Cross_thread_handle_access_throws_when_the_guard_is_on()
    {
        Control? created = null;
        RunSta(() =>
        {
            var control = new Control();
            _ = control.Handle; // force handle creation on the owning STA thread
            created = control;
        });
        var control2 = created!;

        bool previous = Control.CheckForIllegalCrossThreadCalls;
        Control.CheckForIllegalCrossThreadCalls = true;
        try
        {
            Assert.Throws<InvalidOperationException>(() => _ = control2.Handle);
        }
        finally
        {
            Control.CheckForIllegalCrossThreadCalls = previous;
            RunSta(control2.Dispose);
        }
    }

    [Fact]
    public void Same_thread_handle_access_is_permitted_under_the_guard()
    {
        RunSta(() =>
        {
            using var control = new Control();
            bool previous = Control.CheckForIllegalCrossThreadCalls;
            Control.CheckForIllegalCrossThreadCalls = true;
            try
            {
                _ = control.Handle;
            }
            finally
            {
                Control.CheckForIllegalCrossThreadCalls = previous;
            }
        });
    }

    [Fact]
    public void Chart_control_enforces_thread_affinity()
    {
        ChartControl? created = null;
        RunSta(() =>
        {
            var control = new ChartControl();
            _ = control.Handle; // force handle creation on the owning STA thread
            created = control;
        });
        var control2 = created!;

        bool previous = Control.CheckForIllegalCrossThreadCalls;
        Control.CheckForIllegalCrossThreadCalls = true;
        try
        {
            Assert.Throws<InvalidOperationException>(() => _ = control2.Handle);
        }
        finally
        {
            Control.CheckForIllegalCrossThreadCalls = previous;
            RunSta(control2.Dispose);
        }
    }
}
