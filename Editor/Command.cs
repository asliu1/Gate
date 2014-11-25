using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Editor
{
  public class Command
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
      SAVE_LEVEL
    };

    #region PublicAPI

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

    static public Command CreateLoadTileSheetCmd(string filename, int tileSize, ImportCallBack cBack)
    {
      ImportSheetCommand cmd = new ImportSheetCommand(filename, tileSize, cBack);
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

    static public Command SaveLevelCmd(string filename) //Angela add
    {
      Command cmd = new Command();
      cmd.m_type = Type.SAVE_LEVEL;
      cmd.m_tags.Add(filename);
      return cmd;
    }

    #endregion

    protected Command(Type type)
    {
      m_type = type;
      m_props = new List<int>(0);
      m_tags = new List<string>(0);
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

  public class ImportSheetCommand : Command
  {
    public ImportCallBack m_callBack;
    public string m_filename;
    public int m_tileSize;

    public ImportSheetCommand(string filename, int tileSize, ImportCallBack cBack) : base(Type.TILESHEET_LOAD)
    {
      m_filename = filename;
      m_tileSize = tileSize;
      m_callBack = cBack;
    }
  }
}
