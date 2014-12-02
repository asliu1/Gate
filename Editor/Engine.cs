using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace Editor
{
  public class Engine
  {
    #region PublicInterface
    // Public Interface - Methods to be called by GUI

    public void MapClicked(Point loc, bool rightClick = false)
    {
      AddCommand(Command.CreateMapClickCmd(loc, rightClick));
    }

    public void LoadTileSheet(string filename, int tileSize, Command.ImportCallBack cBack)
    {
      AddCommand(Command.CreateLoadTileSheetCmd(filename, tileSize, cBack));
    }

    public void TileSelected(int sheetID, int tileIndex)
    {
      AddCommand(Command.CreateTileSelectedCmd(sheetID, tileIndex));
    }

    public void SaveLevelClicked(string filename)
    {
      AddCommand(Command.CreateSaveLevelCmd(filename));
    }

    public void LoadLevelClicked(string filename)
    {
      AddCommand(Command.CreateLoadLevelCmd(filename));
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
            MapLeftClicked(cmd);
            break;
          case Command.Type.MAP_RIGHT_CLICK:
            MapRightClicked(cmd);
            break;
          case Command.Type.TILESHEET_LOAD:
            CreateTileSheet(cmd);
            break;
          case Command.Type.TILE_SELECTED:
            UpdateSelectedTile(cmd);
            break;
          case Command.Type.MSG_TEXT:
            DisplayMessage(cmd);
            break;
          case Command.Type.MSG_POPUP:
            PopUpMessage(cmd);
            break;
          case Command.Type.SAVE_LEVEL:
            SaveLevel(cmd);
            break;
          case Command.Type.LOAD_LEVEL:
            LoadLevel(cmd);
            break;
          default:
            break;
        }
      }
    }

    private void MapLeftClicked(Command command)
    {
      try
      {
        MapClickCommand cmd = (MapClickCommand)command;

        int xCord = (int)cmd.m_loc.X / 64;
        int yCord = (int)cmd.m_loc.Y / 64;
        string msg = String.Format("Map Left Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", cmd.m_loc.X, cmd.m_loc.Y, xCord, yCord);
        DisplayMessage(msg);

        // TODO Check Mode
        if (m_selectedTile.Item1 != -1)
        {
          m_renderer.DrawTile(m_selectedTile.Item1, m_selectedTile.Item2, xCord, yCord);
        }
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void MapRightClicked(Command command)
    {
      try
      {
        MapClickCommand cmd = (MapClickCommand)command;
        int xCord = (int)cmd.m_loc.X / 64;
        int yCord = (int)cmd.m_loc.Y / 64;
        string msg = String.Format("Map Right Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", cmd.m_loc.X, cmd.m_loc.Y, xCord, yCord);
        DisplayMessage(msg);

        // TODO Check Mode
        m_renderer.ClearTile(xCord, yCord);
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void CreateTileSheet(Command command)
    {
      try
      {
        ImportSheetCommand cmd = (ImportSheetCommand)command;
        int sheetID = 0;
        SheetError err = m_tileManager.CreateNewSheet(cmd.m_filename, cmd.m_tileSize, ref sheetID);

        // Check Error - SIZE_MISMATCH is considered okay for now
        if ((err == SheetError.SUCCESS || err == SheetError.SIZE_MISMATCH) && sheetID != -1)
        {
          m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.AddTileSheet(m_tileManager.GetSheet(sheetID)); });
          DisplayMessage("Importing " + cmd.m_filename + " at " + cmd.m_tileSize.ToString() + " pixels per tile.");
        }
        m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { cmd.m_callBack(err); });
      }
      catch (InvalidCastException){/*Add Logging in Future for Internal Error*/}
    }

    private void UpdateSelectedTile(Command command)
    {
      try
      {
        TileSelectCommand cmd = (TileSelectCommand)command;
        DisplayMessage("Tile Selected - SheetID: " + cmd.m_sheetID.ToString() + " Tile Index: " + cmd.m_tileIndex.ToString());
        m_selectedTile = new Tuple<int, int>(cmd.m_sheetID, cmd.m_tileIndex);
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void DisplayMessage(Command command)
    {
      try
      {
        MessageCommand cmd = (MessageCommand)command;
        m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(cmd.m_msg); });
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void DisplayMessage(string msg)
    {
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(msg); });
    }

    private void PopUpMessage(Command command)
    {
      try
      {
        // TODO
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void PopUpMessage(string msg)
    {
      // TODO
    }

    private void SaveLevel(Command cmd)
    {
       try
       {
         SaveLevelCommand command = (SaveLevelCommand)cmd;
         DisplayMessage("Save Level clicked filename : " + command.m_filename); 
         //TODO
       }
       catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    private void LoadLevel(Command cmd)
    {
      try
      {
         LoadLevelCommand command = (LoadLevelCommand)cmd;
         DisplayMessage("Load Level clicked filename : " + command.m_filename);
         //TODO
      }
      catch (InvalidCastException) {/*Add Logging in Future for Internal Error*/}
    }

    #endregion

    #region EngineImplementation

    public Engine(MainWindow mainWindow)
    {
      m_mainWindow = mainWindow;
      m_tileManager = new TileManager();
      m_renderer = new Renderer(m_mainWindow, m_tileManager, 20, 20);
      m_running = false;
      m_queueLock = new object();
      m_queueThread = new Thread(Run);
      m_queueThread.Name = "Engine Queue Thread";
      m_cmdQueue = new Queue<Command>(0);
      m_selectedTile = new Tuple<int, int>(-1, -1);
    }

    ~Engine()
    {
      Stop();
    }

    private void Run()
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
      m_renderer.Start();
    }

    public void Stop()
    {
      m_renderer.Stop();
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
    private Renderer m_renderer;
    private MainWindow m_mainWindow;
    private bool m_running;
    private object m_queueLock;
    private Thread m_queueThread;
    private Queue<Command> m_cmdQueue;
    private TileManager m_tileManager;
    private Tuple<int, int> m_selectedTile;

    #endregion
  }
}
