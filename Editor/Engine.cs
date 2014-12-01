using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.IO;

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

    public void SaveLevelClicked(string filename) //Angela add
    {
      AddCommand(Command.SaveLevelCmd(filename));
    }

    public void LoadLevelClicked(string filename) //Angela add
    {
      AddCommand(Command.LoadLevelCmd(filename));
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
            CreateTileSheet(cmd);
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
          case Command.Type.SAVE_LEVEL: //Angela add
            SaveLevel(cmd.m_tags[0]);
            break;
          case Command.Type.LOAD_LEVEL: //Angela add
            LoadLevel(cmd.m_tags[0]);
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

      // TODO Check Mode
      if(m_selectedTile.Item1 != -1)
      {
        IntPtr src = m_tileManager.GetSheet(m_selectedTile.Item1).GetTileSourceAt(m_selectedTile.Item2);
        m_renderer.DrawTile(m_selectedTile.Item1, m_selectedTile.Item2, src, xCord, yCord);
      }
    }

    private void MapRightClicked(int xLoc, int yLoc)
    {
      int xCord = xLoc / 64;
      int yCord = yLoc / 64;
      string msg = String.Format("Map Right Clicked at ({0}, {1}) grid evaluates to ({2}, {3}).", xLoc, yLoc, xCord, yCord);
      DisplayMessage(msg);

      // TODO Check Mode
      m_renderer.ClearTile(xCord, yCord);
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

    private void UpdateSelectedTile(int sheetID, int tileIndex)
    {
      DisplayMessage("Tile Selected - SheetID: " + sheetID.ToString() + " Tile Index: " + tileIndex.ToString());
      m_selectedTile = new Tuple<int,int>(sheetID, tileIndex);
    }

    private void DisplayMessage(string msg)
    {
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() { m_mainWindow.UpdateSysMsg(msg); });
    }

    private void PopUpMessage(string msg)
    {
      // TODO
    }

    private void SaveLevel(string filename) //writes tilesheet info json into file
    { //Angela add
      StringBuilder sb = new StringBuilder();
      sb.Append(m_tileManager.GetJson());
      File.WriteAllText(filename, sb.ToString());
    }

    private void LoadLevel(string filename) //TODO Angela add
    {
      StreamReader file = File.OpenText(filename);
      string st = file.ReadToEnd();
      file.Close();
      st = @"{" + st + "}"; 
      m_tileManager.LoadJson(st); 
    }
    #endregion

    #region EngineImplementation

    public Engine(MainWindow mainWindow)
    {
      m_mainWindow = mainWindow;
      m_renderer = new Renderer(m_mainWindow, 20, 20);
      m_running = false;
      m_queueLock = new object();
      m_queueThread = new Thread(Run);
      m_queueThread.Name = "Engine Queue Thread";
      m_cmdQueue = new Queue<Command>(0);
      m_tileManager = new TileManager();
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

    public TileManager getTileManager()
    {
        return m_tileManager;
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