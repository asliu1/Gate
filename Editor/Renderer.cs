using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace Editor
{
  public class GridSquare
  {
    public int m_sheetID;
    public int m_tileIndex;
    public bool m_hasPlaceable;
    public bool m_hasTrigger;
    /*
     * Will Contain Data for Placeables/Triggers
     * public Placeable m_place;
     * public Trigger m_trigger;
     */

    public GridSquare()
    {
      m_sheetID = -1;
      m_tileIndex = -1;
      m_hasPlaceable = false;
      m_hasTrigger = false;
    }
  }

  public abstract class Delta
  {
    public int m_index;

    static public Delta CreateTileDelta(int index, Tile tile)
    {
      return new TileDelta(index, tile);
    }

    public abstract void Apply(GridSquare grid);
  }

  class TileDelta : Delta
  {
    public int m_sheetID;
    public int m_tileIndex;

    public TileDelta(int index, Tile tile)
    {
      m_index = index;
      m_sheetID = tile.m_sheetID;
      m_tileIndex = tile.m_tileIndex;
    }

    override public void Apply(GridSquare grid)
    {
      grid.m_sheetID = m_sheetID;
      grid.m_tileIndex = m_tileIndex;
    }
  }

  class PlaceableDelta : Delta
  {
    override public void Apply(GridSquare grid)
    {
    }
  }

  class TriggerDelta : Delta
  {
    override public void Apply(GridSquare grid)
    {
    }
  }

  class Renderer
  {
    public void DrawTile(int sheet, int tileIndex, int xCord, int yCord)
    {
      lock (m_renderLock)
      {
        int index = (yCord * m_cols) + xCord;
        m_grid[index].m_sheetID = sheet;
        m_grid[index].m_tileIndex = tileIndex;

        Monitor.Pulse(m_renderLock);
      }
    }

    public void ClearTile(int xCord, int yCord)
    {
      lock (m_renderLock)
      {
        int index = (yCord * m_cols) + xCord;
        m_grid[index].m_sheetID = -1;

        Monitor.Pulse(m_renderLock);
      }
    }

    #region Core

    private List<GridSquare> m_grid;
    private int m_cols;
    private int m_rows;
    private bool m_running = false;
    private Thread m_renderThread;
    private object m_renderLock;
    private MainWindow m_mainWindow;
    private TileManager m_tileManager;

    public Renderer(MainWindow mainWindow, TileManager tileManager, int numCols, int numRows)
    {
      m_mainWindow = mainWindow;
      m_tileManager = tileManager;
      m_cols = numCols;
      m_rows = numRows;
      m_grid = new List<GridSquare>();
      for (int index = 0; index < m_cols * m_rows; ++index)
      {
        m_grid.Add(new GridSquare());
      }
      m_running = false;
      m_renderThread = new Thread(Run);
      m_renderThread.Name = "Render Thread";
      m_renderLock = new Object();
    }

    ~Renderer()
    {
      Stop();
    }

    #endregion

    #region Threading

    private void Run()
    {
      lock (m_renderLock)
      {
        while (m_running)
        {
          Monitor.Wait(m_renderLock, 17);
          DrawTiles();
//          UpdateMap();
        }
      }
    }

    public void Start()
    {
      m_running = true;
      m_renderThread.Start();
    }

    public void Stop()
    {
      lock (m_renderLock)
      {
        m_running = false;
        Monitor.Pulse(m_renderLock);
      }

      m_renderThread.Join();
    }

    public void ForceDraw()
    {
      lock (m_renderLock)
      {
        Monitor.Pulse(m_renderLock);
      }
    }

    #endregion

    #region Public API
    #endregion

    #region Rendering

    private void DrawTiles()
    {
      DrawingGroup group = new DrawingGroup();
      GeometryGroup squares = new GeometryGroup();

      for(int index = 0; index < m_grid.Count; ++index)
      {
        int yCord = (index / m_cols) * TileSheet.TILE_DISPLAY_SIZE;
        int xCord = (index % m_cols) * TileSheet.TILE_DISPLAY_SIZE;
        Rect rct = new Rect(xCord, yCord, TileSheet.TILE_DISPLAY_SIZE, TileSheet.TILE_DISPLAY_SIZE);

        if (m_grid[index].m_sheetID == -1)
        {
          squares.Children.Add(new RectangleGeometry(rct));
        }
        else
        {
          BitmapSource src = m_tileManager.GetSheet(m_grid[index].m_sheetID).GetBitmapSourceAt(m_grid[index].m_tileIndex);
          ImageDrawing img = new ImageDrawing(src, rct);
          group.Children.Add(img);
        }
      }

      GeometryDrawing gridDraw = new GeometryDrawing();
      gridDraw.Brush = Brushes.White;
      gridDraw.Pen = new Pen(Brushes.Black, 1);
      gridDraw.Geometry = squares;

      group.Children.Add(gridDraw);
      group.Freeze();
      UpdateMap(group);
    }

    private void DrawPlaceables()
    {
      // TODO
    }

    private void UpdateMap(DrawingGroup group)
    {
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() 
      {
        m_mainWindow.DrawMap(group);
      });
    }

    #endregion
  }
}
