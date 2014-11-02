using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Editor
{
  class API { [DllImport("WINGDI32.dll")] public static extern int DeleteObject(IntPtr hObject); }

  public enum SheetError { SUCCESS, UNSUPPORTED, TOO_LARGE, NOT_FOUND, INVALD_ARG, SIZE_MISMATCH };

  class TileSheet
  {
    public const int TILE_DISPLAY_SIZE = 64;

    private string m_fileName;
    private int m_tileSize;
    private int m_id;
    private List<IntPtr> m_tileSources;
    private SheetError m_error;
    
    /// <summary>
    ///  Attemps to create the TileSheet. After creating, check the GetError method
    ///  for SUCCESS to ensure Sheet was created correctly.
    /// </summary>
    public TileSheet(string filePath, int tileSize, int id)
    {
      // Strip path from filename
      try
      {
        m_fileName = Path.GetFileName(filePath);
      }
      catch(ArgumentException)
      {
        m_error = SheetError.INVALD_ARG;
        return;
      }

      // Attempt to Load Image
      Bitmap bitmap = null;
      try
      {
        bitmap = new Bitmap(filePath);

        if (bitmap != null)
        {
          m_error = SheetError.SUCCESS;
        }
        else
        {
          m_error = SheetError.UNSUPPORTED;
        }
      }
      catch(FileNotFoundException)
      {
        m_error = SheetError.NOT_FOUND;
      }

      // Image Loaded, prepare sheet for use
      if(m_error == SheetError.SUCCESS)
      {
        m_tileSize = tileSize;
        m_id = id;
        m_tileSources = new List<IntPtr>(0);
        CreateTiles(bitmap);
        bitmap.Dispose();
      }
    }

    ~TileSheet()
    {
      // The HBITMAP handles need to be deleted
      foreach(IntPtr temp in m_tileSources)
      {
        if (temp != IntPtr.Zero)
        {
          API.DeleteObject(temp);
        }
      }

      m_tileSources.Clear();
    }

    private void CreateTiles(Bitmap srcBitmap)
    {
      Bitmap bitmap;
      GraphicsUnit units = GraphicsUnit.Pixel;

      int extraWidth = srcBitmap.Width % m_tileSize;
      int extraHeight = srcBitmap.Height % m_tileSize;

      // Check if the supplied image is not evenly divided by the supplied tile size.
      // This is not an error, but a warning should be given to the user and the
      // rightmost and/or bottommost tiles will have the extra space filled
      if(extraWidth > 0 || extraHeight > 0)
      {
        int wFillBuffer = m_tileSize - extraWidth;
        int hFillBuffer = m_tileSize - extraHeight;

        bitmap = new Bitmap(srcBitmap.Width + wFillBuffer, srcBitmap.Height + hFillBuffer);

        Graphics gfx = Graphics.FromImage(bitmap);

        gfx.FillRectangle(new SolidBrush(Color.HotPink), 0, 0, bitmap.Width, bitmap.Height);
        Rectangle original = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
        gfx.DrawImage(srcBitmap, original, original, units);

        gfx.Dispose();

        m_error = SheetError.SIZE_MISMATCH;
      }
      else
      {
        // Size divided evenly, just use passed bitmap
        bitmap = srcBitmap;
      }

      if(bitmap.Width > Int16.MaxValue || bitmap.Height > Int16.MaxValue)
      {
        // The TileSheet is too big for the editor to use
        m_error = SheetError.TOO_LARGE;
        return;
      }

      int cols = bitmap.Width / m_tileSize;
      int rows = bitmap.Height / m_tileSize;

      Bitmap tempMap = null;
      Rectangle destRect = new Rectangle(0, 0, TILE_DISPLAY_SIZE, TILE_DISPLAY_SIZE);
      Rectangle srcRect;

      for(int rIndex = 0; rIndex < rows; ++rIndex)
      {
        for (int cIndex = 0; cIndex < cols; ++cIndex)
        {
          tempMap = new Bitmap(TILE_DISPLAY_SIZE, TILE_DISPLAY_SIZE);
          srcRect = new Rectangle(cIndex * m_tileSize, rIndex * m_tileSize, m_tileSize, m_tileSize);

          // Draw Tile to Temp Bitmap
          Graphics gfx = Graphics.FromImage(tempMap);
          gfx.DrawImage(bitmap, destRect, srcRect, units);
          gfx.Dispose();

          m_tileSources.Add(tempMap.GetHbitmap());  // Thi INTPTR will be deleted in the Destructor
        }
      }
    }

    public IntPtr GetTileSourceAt(int index)
    {
      if(index >= 0 && index < m_tileSources.Count)
      {
        return m_tileSources.ElementAt(index);
      }

      return IntPtr.Zero;
    }

    public int GetNumTiles() { return m_tileSources.Count; }
    public string GetFileName() { return m_fileName; }
    public int GetTileSize() { return m_tileSize; }
    public int GetID() { return m_id; }
    public SheetError GetError() { return m_error; }
  }

  class TileManager
  {
    private List<TileSheet> m_sheets;
    private SortedDictionary<int, int> m_idMap;
    private int m_nextId;
    
    public TileManager()
    {
      m_nextId = 0;
    }

    public int CreateNewSheet(string filePath, int tileSize)
    {
      int sheetId = m_nextId;
      TileSheet newSheet = new TileSheet(filePath, tileSize, sheetId);

      // Check Error Code and pass message to user if needed
      switch(newSheet.GetError())
      {
        case(SheetError.SUCCESS):
          m_sheets.Add(newSheet);
          m_idMap.Add(sheetId, m_sheets.Count - 1);  // Map the ID to the index
          ++m_nextId; 
          break;
        case (SheetError.SIZE_MISMATCH):
          m_sheets.Add(newSheet);
          m_idMap.Add(sheetId, m_sheets.Count - 1);  // Map the ID to the index
          ++m_nextId; 
          break;
        case (SheetError.TOO_LARGE):
          sheetId = -1;
          break;
        case (SheetError.INVALD_ARG):
          sheetId = -1;
          break;
        case (SheetError.NOT_FOUND):
          sheetId = -1;
          break;
        case (SheetError.UNSUPPORTED):
          sheetId = -1;
          break;
        default:
          sheetId = -1;
          break;
      }

      return sheetId;
    }

    /// <summary>
    /// Returns the Sheet associated with the given ID or 'null' if ID is invalid.
    /// </summary>
    public TileSheet GetSheet(int id)
    {
      int index = -1;

      if(m_idMap.TryGetValue(id, out index))
      {
        return m_sheets.ElementAt(index);
      }

      return null;  // Failed to locate ID in map
    }
  }
}
