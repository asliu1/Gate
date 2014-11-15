using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Editor
{
  using BitmapCache = Dictionary<Tuple<int, int>, Renderer.CacheEntry>;
  using CacheKey = Tuple<int, int>;

  class Renderer
  {
    #region Structures

    public struct CacheEntry
    {
      public Bitmap m_bitmap;
      public int m_count;

      public CacheEntry(Bitmap bmp, int count)
      {
        m_bitmap = bmp;
        m_count = count;
      }
    }

    class MapTile
    {
      public int m_sheetID;
      public int m_tileIndex;

      public MapTile()
      {
        m_sheetID = -1;
        m_tileIndex = -1;
      }

      public MapTile(int sheetID, int tileIndex)
      {
        m_sheetID = sheetID;
        m_tileIndex = tileIndex;
      }
    }

    #endregion

    #region Core

    private BitmapCache m_cache;
    private List<MapTile> m_tiles;
    private Point m_mapSize;
    private bool m_running = false;
    private Thread m_renderThread;
    private object m_renderLock;
    private MainWindow m_mainWindow;

    public Renderer(MainWindow mainWindow, int numCols, int numRows)
    {
      m_mainWindow = mainWindow;
      m_mapSize.X = numCols;
      m_mapSize.Y = numRows;
      m_cache = new BitmapCache();
      m_tiles = new List<MapTile>();
      for (int index = 0; index < numCols * numRows; ++index)
      {
        m_tiles.Add(new MapTile());
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
          Monitor.Wait(m_renderLock, 250);
          // Draw
          int width = m_mapSize.X * TileSheet.TILE_DISPLAY_SIZE;
          int height = m_mapSize.Y * TileSheet.TILE_DISPLAY_SIZE;
          Bitmap bmp = new Bitmap(width, height);
          DrawBackground(ref bmp);
          UpdateMap(bmp);
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

    public void DrawTile(int sheet, int tileIndex, IntPtr src, int xCord, int yCord)
    {
      lock (m_renderLock)
      {
        MapTile tile = m_tiles[xCord + (m_mapSize.X * yCord)];

        if (tile.m_sheetID == sheet && tile.m_tileIndex == tileIndex)
        {
          // No Change
          return;
        }

        // Check if Bitmap is in Cache
        CacheKey key = new CacheKey(sheet, tileIndex);
        if (!m_cache.ContainsKey(key))
        {
          CacheImage(key, src);
        }
        else
        {
          m_cache[key] = new CacheEntry(m_cache[key].m_bitmap, m_cache[key].m_count + 1);
        }

        // Assign Values to Tile
        tile.m_sheetID = sheet;
        tile.m_tileIndex = tileIndex;

        Monitor.Pulse(m_renderLock);
      }
    }

    public void ClearTile(int xCord, int yCord)
    {
      lock (m_renderLock)
      {
        MapTile tile = m_tiles[xCord + (m_mapSize.X * yCord)];

        if(tile.m_sheetID == -1)
        {
          // No Change
          return;
        }

        // Decrement Cache Count
        CacheKey key = new CacheKey(tile.m_sheetID, tile.m_tileIndex);
        if (m_cache.ContainsKey(key))
        {
          m_cache[key] = new CacheEntry(m_cache[key].m_bitmap, m_cache[key].m_count - 1);

          if(m_cache[key].m_count <= 0)
          {
            // Last Tile using this cached Image, remove from cache
            m_cache.Remove(key);
          }
        }

        tile.m_sheetID = -1;

        Monitor.Pulse(m_renderLock);
      }
    }

    #endregion

    #region Cache

    private void CacheImage(CacheKey key, IntPtr src)
    {
      Bitmap newBitmap = new Bitmap(Image.FromHbitmap(src));
      CacheEntry entry = new CacheEntry(newBitmap, 1);
      m_cache.Add(key, entry);
    }

    #endregion

    #region Rendering

    private void DrawBackground(ref Bitmap bitmap)
    {
      int col = 0;
      int row = 0;
      int tileSize = TileSheet.TILE_DISPLAY_SIZE;

      GraphicsUnit units = GraphicsUnit.Pixel;
      Graphics gfx = Graphics.FromImage(bitmap);
      Rectangle srcRect = new Rectangle(0, 0, tileSize, tileSize);
      Pen pen = new Pen(Color.Black, 1);
      // Loop through Tile Array and Draw Tiles
      foreach(MapTile tile in m_tiles)
      {
        Rectangle destRect = new Rectangle(col * tileSize, row * tileSize, tileSize, tileSize);

        if(tile.m_sheetID == -1)
        {
          // Draw Grid Square
          gfx.DrawRectangle(pen, destRect);
        }
        else
        {
          // Draw Bitmap
          CacheKey key = new CacheKey(tile.m_sheetID, tile.m_tileIndex);
          if (m_cache.ContainsKey(key))
          {
            Bitmap tempBmp = m_cache[key].m_bitmap;
            gfx.DrawImage(tempBmp, destRect, srcRect, units);
          }
          else
          {
            gfx.DrawRectangle(pen, destRect);
          }
        }

        ++col;
        if(col >= m_mapSize.X)
        {
          col = 0;
          ++row;
        }
      }

      gfx.Dispose();
    }

    private void DrawPlaceables(ref Bitmap bitmap)
    {

    }

    private void UpdateMap(Bitmap bmp)
    {
      IntPtr srcPtr = bmp.GetHbitmap();
      m_mainWindow.Dispatcher.InvokeAsync((Action)delegate() 
      {
        m_mainWindow.DrawMap(srcPtr, bmp.Width, bmp.Height);
        API.DeleteObject(srcPtr);
      });
    }

    #endregion
  }
}
