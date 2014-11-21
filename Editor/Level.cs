using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Editor
{

  public enum LevelError { SUCCESS, ALREADY_EXISTS };

  class Level
  {
    private string m_name;
    private int m_cols;
    private int m_rows;
    private List<Tile> m_tiles;
    private object m_deltaLock;
    private List<Delta> m_deltas;

    #region PublicInterface

    public void PaintTile(Point loc, int sheetID, int tileIndex)
    {
      int index = ((int)loc.Y * m_cols) + (int)loc.X;
      m_tiles[index].m_sheetID = sheetID;
      m_tiles[index].m_tileIndex = tileIndex;

      AddDelta(index, m_tiles[index]);
    }

    public void PaintTiles(Point cornerOne, Point cornerTwo, int sheetID, int tileIndex)
    {
      Rect tileRect = new Rect(cornerOne, cornerTwo);

      for(int rIndex = 0; rIndex < tileRect.Height; ++rIndex)
      {
        for (int cIndex = 0; cIndex < tileRect.Width; ++cIndex)
        {
          Point loc = new Point(cIndex + tileRect.X, rIndex + tileRect.Y);
          PaintTile(loc, sheetID, tileIndex);
        }
      }
    }

    public void ClearTile(Point loc)
    {
      PaintTile(loc, -1, -1);
    }

    public void ClearTiles(Point cornerOne, Point cornerTwo)
    {
      PaintTiles(cornerOne, cornerTwo, -1, -1);
    }

    /// <summary>
    /// This method should only be called by the rendered when it is updating
    /// its view of the level, otherwise deltas may be lost and will fail to
    /// be rendered.
    /// </summary>
    public List<Delta> GetDeltas()
    {
      List<Delta> copy;
      lock (m_deltaLock)
      {
        copy = new List<Delta>(m_deltas);
        m_deltas.Clear();
      }
      return copy;
    }

    #endregion

    #region Core

    public Level(string name, int cols, int rows)
    {
      m_name = name;
      m_cols = cols;
      m_rows = rows;
      m_tiles = new List<Tile>(m_cols * m_rows);
      m_deltaLock = new Object();
      m_deltas = new List<Delta>(0);
    }

    private void AddDelta(int index, Tile tile)
    {
      lock (m_deltaLock)
      {
        m_deltas.Add(Delta.CreateTileDelta(index, tile));
      }
    }

    #endregion
  }

  public class Tile
  {
    public int m_sheetID;
    public int m_tileIndex;

    public Tile()
    {
      m_sheetID = -1;
      m_tileIndex = -1;
    }

    public Tile(int sheetID, int tileIndex)
    {
      m_sheetID = sheetID;
      m_tileIndex = tileIndex;
    }
  }
}
