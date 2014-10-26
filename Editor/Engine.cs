using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace Editor
{
  class Command
  {
    public enum Type { NONE, MAP_LEFT_CLICK, MAP_RIGHT_CLICK };

    static public Command CreateMapClickCmd(Point loc, bool leftClick = true)
    {
      Command cmd = new Command();
      cmd.m_type = leftClick ? Type.MAP_LEFT_CLICK : Type.MAP_RIGHT_CLICK;
      cmd.m_props.Add((int)loc.X);
      cmd.m_props.Add((int)loc.Y);
      return cmd;
    }

    private Command()
    {
      m_type = Type.NONE;
      m_props = new List<int>(0);
      m_tags = new List<string>(0);
    }

    public Type m_type;
    public List<int> m_props;
    public List<string> m_tags;
  }

  class Engine
  {
    public delegate void UpdateSysMsgText(string msg);

    public Engine(MainWindow mainWindow)
    {
      m_mainWindow = mainWindow;
      m_running = false;
      m_queueLock = new object();
      m_queueThread = new Thread(Run);
      m_queueThread.Name = "Engine Queue Thread";
      m_cmdQueue = new Queue<Command>(0);
    }

    ~Engine()
    {
      Stop();
    }

    public void Run()
    {
      lock (m_queueLock)
      {
        while (m_running)
        {
          Monitor.Wait(m_queueLock);
          ProcessCommands();
        }
      }
    }

    public void Start()
    {
      m_running = true;
      m_queueThread.Start();
    }

    public void Stop()
    {
      lock (m_queueLock)  
      {
        m_running = false;
        Monitor.Pulse(m_queueLock);
      }

      m_queueThread.Join();
    }

    public void AddCommand(Command cmd)
    {
      lock (m_queueLock)
      {
        m_cmdQueue.Enqueue(cmd);
        Monitor.Pulse(m_queueLock);
      }
    }

    private Engine(ref Engine original) { }

    private void ProcessCommands()
    {
      while(m_cmdQueue.Count() > 0)
      {
        Command cmd = m_cmdQueue.Dequeue();

        switch (cmd.m_type)
        {
          case Command.Type.NONE:
            break;
          case Command.Type.MAP_LEFT_CLICK:
            MapLeftClicked(cmd.m_props[0], cmd.m_props[1]);
            break;
          case Command.Type.MAP_RIGHT_CLICK:
            MapRightClicked(cmd.m_props[0], cmd.m_props[1]);
            break;
          default:
            break;
        }
      }
    }

    private void MapLeftClicked(int xLoc, int yLoc)
    {
      int xCord = xLoc / 64;
      int yCord = yLoc / 64;
      string msg = String.Format("Map Left Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", xLoc, yLoc, xCord, yCord);
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(msg); });
    }

    private void MapRightClicked(int xLoc, int yLoc)
    {
      int xCord = xLoc / 64;
      int yCord = yLoc / 64;
      string msg = String.Format("Map Right Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", xLoc, yLoc, xCord, yCord);
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(msg); });
    }

    private MainWindow m_mainWindow;
    private bool m_running;
    private object m_queueLock;
    private Thread m_queueThread;
    private Queue<Command> m_cmdQueue;
  }
}
