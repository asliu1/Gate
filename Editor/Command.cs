using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Editor
{
  public abstract class Command
  {
    public delegate void ImportCallBack(SheetError err);

    public enum Type
    {
      NONE,
      MAP_LEFT_CLICK,
      MAP_RIGHT_CLICK,
      MSG_TEXT,
      MSG_POPUP,
      TILESHEET_LOAD,
      TILE_SELECTED,
      SAVE_LEVEL,
      LOAD_LEVEL
    };

    #region PublicAPI

    static public Command CreateMapClickCmd(Point loc, bool rightClick)
    {
      MapClickCommand cmd = new MapClickCommand(loc, rightClick);
      return cmd;
    }

    static public Command CreateMsgCmd(string msg, bool popUp = false)
    {
      MessageCommand cmd = new MessageCommand(msg);
      return cmd;
    }

    static public Command CreateLoadTileSheetCmd(string filename, int tileSize, ImportCallBack cBack)
    {
      ImportSheetCommand cmd = new ImportSheetCommand(filename, tileSize, cBack);
      return cmd;
    }

    static public Command CreateTileSelectedCmd(int sheetID, int tileIndex)
    {
      TileSelectCommand cmd = new TileSelectCommand(sheetID, tileIndex);
      return cmd;
    }

    static public Command CreateSaveLevelCmd(string filename)
    {
      SaveLevelCommand cmd = new SaveLevelCommand(filename);
      return cmd;
    }

    static public Command CreateLoadLevelCmd(string filename)
    {
      LoadLevelCommand cmd = new LoadLevelCommand(filename);
      return cmd;
    }

    #endregion

    public Type m_type;
  }

  public class ImportSheetCommand : Command
  {
    public ImportCallBack m_callBack;
    public string m_filename;
    public int m_tileSize;

    public ImportSheetCommand(string filename, int tileSize, ImportCallBack cBack)
    {
      m_type = Type.TILESHEET_LOAD;
      m_filename = filename;
      m_tileSize = tileSize;
      m_callBack = cBack;
    }
  }

  public class MapClickCommand : Command
  {
    public Point m_loc;

    public MapClickCommand(Point loc, bool rightClick)
    {
      m_type = rightClick ? Type.MAP_RIGHT_CLICK : Type.MAP_LEFT_CLICK;
      m_loc = loc;
    }
  }

  public class MessageCommand : Command
  {
    public string m_msg;

    public MessageCommand(string msg)
    {
      m_type = Type.MSG_TEXT;
      m_msg = msg;
    }
  }

  public class TileSelectCommand : Command
  {
    public int m_sheetID;
    public int m_tileIndex;

    public TileSelectCommand(int sheetID, int tileIndex)
    {
      m_type = Type.TILE_SELECTED;
      m_sheetID = sheetID;
      m_tileIndex = tileIndex;
    }
  }

  public class SaveLevelCommand : Command
  {
    public string m_filename;

    public SaveLevelCommand(string filename)
    {
      m_type = Type.SAVE_LEVEL;
      m_filename = filename;
    }
  }

  public class LoadLevelCommand : Command
  {
    public string m_filename;

    public LoadLevelCommand(string filename)
    {
      m_type = Type.LOAD_LEVEL;
      m_filename = filename;
    }
  }
}
