using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Editor
{
  /// <summary>
  /// Interaction logic for NewLevelWindow.xaml
  /// </summary>
  public partial class NewLevelWindow : Window
  {
    private Engine m_engine;
    private string m_levelname;
    private int m_cols;
    private int m_rows;

    public NewLevelWindow(Engine engine)
    {
      InitializeComponent();

      m_engine = engine;
      m_levelname = "";
      m_cols = 1;
      m_rows = 1;
      colBox.Text = m_cols.ToString();
      rowBox.Text = m_rows.ToString();
    }

    private void CreateClicked(object sender, RoutedEventArgs e)
    {
      m_levelname = levelNameBox.Text;

      if(!int.TryParse(colBox.Text, out m_cols))
      {
        userMsgBox.Text = "Error parsing Columns";
        return;
      }

      if (!int.TryParse(rowBox.Text, out m_rows))
      {
        userMsgBox.Text = "Error parsing Rows";
        return;
      }


      if (m_levelname.Length > 0)
      {
        userMsgBox.Text = "Create Clicked";
        /*
        m_engine.NewLevel(m_levelname, m_cols, m_rows, LevelCallback);
        */
      }
      else
      {
        userMsgBox.Text = "Please enter a level name.";
      }
    }

    private void LevelCallback(LevelError err)
    {
      switch (err)
      {
        case (LevelError.SUCCESS):
          this.Close();
          break;
        case (LevelError.ALREADY_EXISTS):
          MessageBox.Show(this, "A level with that name already exists.");
          break;
        default:
          userMsgBox.Text = "An internal error was encountered, please try again.";
          break;
      }
    }

    private void TestInput(object sender, TextCompositionEventArgs e)
    {
      string inputText = e.Text;
      int value = 0;
      if(!int.TryParse(inputText, out value))
      {
        // TODO
      }
      else
      {
        // TODO
      }
    }
  }
}
