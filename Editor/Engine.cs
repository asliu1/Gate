using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace Editor
{
  class Engine
  {
    #region PublicInterface
    // Public Interface - Methods to be called by GUI

    public void MapClicked(Point loc, bool rightClick = false)
    {
      AddCommand(Command.CreateMapClickCmd(loc, rightClick));
    }

    public void LoadTileSheet(string filename, int tileSize)
    {
      AddCommand(Command.CreateLoadTileSheetCmd(filename, tileSize));
    }

    public void TileSelected(int sheetID, int tileIndex)
    {
      AddCommand(Command.CreateTileSelectedCmd(sheetID, tileIndex));
    }

    #endregion

    #region CommandProccessing
    // Methods to handle all Command types

    private void ProcessCommands()
    {
      while (m_cmdQueue.Count() > 0)
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
          case Command.Type.TILESHEET_LOAD:
            CreateTileSheet(cmd.m_tags[0], cmd.m_props[0]);
            break;
          case Command.Type.TILE_SELECTED:
            UpdateSelectedTile(cmd.m_props[0], cmd.m_props[1]);
            break;
          case Command.Type.MSG_TEXT:
            DisplayMessage(cmd.m_tags[0]);
            break;
          case Command.Type.MSG_POPUP:
            PopUpMessage(cmd.m_tags[0]);
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
      DisplayMessage(msg);
    }

    private void MapRightClicked(int xLoc, int yLoc)
    {
      int xCord = xLoc / 64;
      int yCord = yLoc / 64;
      string msg = String.Format("Map Right Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", xLoc, yLoc, xCord, yCord);
      DisplayMessage(msg);
    }

    private void CreateTileSheet(string filename, int tileSize)
    {
      int sheetID = m_tileManager.CreateNewSheet(filename, tileSize);

      if(sheetID != -1)
      {
        m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.AddTileSheet(m_tileManager.GetSheet(sheetID)); });
      }
    }

    private void UpdateSelectedTile(int sheetID, int tileIndex)
    {
      DisplayMessage("Tile Selected - SheetID: " + sheetID.ToString() + " Tile Index: " + tileIndex.ToString());
    }

    private void DisplayMessage(string msg)
    {
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(msg); });
    }

    private void PopUpMessage(string msg)
    {
      // TODO
    }

    #endregion

    #region EngineImplementation

    public Engine(MainWindow mainWindow)
    {
      m_mainWindow = mainWindow;
      m_running = false;
      m_queueLock = new object();
      m_queueThread = new Thread(Run);
      m_queueThread.Name = "Engine Queue Thread";
      m_cmdQueue = new Queue<Command>(0);
      m_tileManager = new TileManager();
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

    private void AddCommand(Command cmd)
    {
      lock (m_queueLock)
      {
        m_cmdQueue.Enqueue(cmd);
        Monitor.Pulse(m_queueLock);
      }
    }

    private Engine(ref Engine original) { }
    private MainWindow m_mainWindow;
    private bool m_running;
    private object m_queueLock;
    private Thread m_queueThread;
    private Queue<Command> m_cmdQueue;
    private TileManager m_tileManager;

    #endregion

    #region CommandClass

    private class Command
    {
      public enum Type { 
        NONE, 
        MAP_LEFT_CLICK, 
        MAP_RIGHT_CLICK,
        MSG_TEXT,
        MSG_POPUP,
        TILESHEET_LOAD,
        TILE_SELECTED
      };

      static public Command CreateMapClickCmd(Point loc, bool rightClick)
      {
        Command cmd = new Command();
        cmd.m_type = rightClick ? Type.MAP_RIGHT_CLICK : Type.MAP_LEFT_CLICK;
        cmd.m_props.Add((int)loc.X);
        cmd.m_props.Add((int)loc.Y);
        return cmd;
      }

      static public Command CreateMsgCmd(string msg, bool popUp = false)
      {
        Command cmd = new Command();
        cmd.m_type = popUp ? Type.MSG_POPUP : Type.MSG_TEXT;
        cmd.m_tags.Add(msg);
        return cmd;
      }

      static public Command CreateLoadTileSheetCmd(string filename, int tileSize)
      {
        Command cmd = new Command();
        cmd.m_type = Type.TILESHEET_LOAD;
        cmd.m_tags.Add(filename);
        cmd.m_props.Add(tileSize);
        return cmd;
      }

      static public Command CreateTileSelectedCmd(int sheetID, int tileIndex)
      {
        Command cmd = new Command();
        cmd.m_type = Type.TILE_SELECTED;
        cmd.m_props.Add(sheetID);
        cmd.m_props.Add(tileIndex);
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

    #endregion
  }
}
