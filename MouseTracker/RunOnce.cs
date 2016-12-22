// source: https://rosettacode.org/wiki/Determine_if_only_one_instance_is_running
// Use this class in your process to guard against multiple instances
using System;
using System.Threading;

/// <summary>
/// RunOnce should be instantiated in the calling processes main clause
/// (preferably using a "using" clause) and then calling process
/// should then check AlreadyRunning and do whatever is appropriate
/// </summary>
public class RunOnce : IDisposable
{
    public RunOnce(string name)
    {
        m_name = name;
        AlreadyRunning = false;

        bool created_new = false;

        m_mutex = new Mutex(false, m_name, out created_new);

        AlreadyRunning = !created_new;
    }

    ~RunOnce()
    {
        DisposeImpl(false);
    }

    public bool AlreadyRunning
    {
        get { return m_already_running; }
        private set { m_already_running = value; }
    }

    private void DisposeImpl(bool is_disposing)
    {
        GC.SuppressFinalize(this);

        if (is_disposing)
        {
            m_mutex.Close();
        }
    }

    #region IDisposable Members

    public void Dispose()
    {
        DisposeImpl(true);
    }

    #endregion

    private string m_name;
    private bool m_already_running;
    private Mutex m_mutex;
}