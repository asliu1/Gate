using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace Editor
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private Engine m_mainEngine;

    public MainWindow()
    {
      InitializeComponent();

      m_mainEngine = new Engine(this);
      m_mainEngine.Start();
    }

    private void WindowClosing(object sender, CancelEventArgs e)
    {
      m_mainEngine.Stop();
    }

    #region Public API

    public void UpdateSysMsg(string msg)
    {
      SysMsgBox.Text = msg;
    }

    public void TileButtonClicked(object sender, MouseButtonEventArgs e)
    {
      TileImage button;
      try
      {
        button = (TileImage)sender;
        tilePreview.Source = button.Source;
        m_mainEngine.TileSelected(button.GetSheetID(), button.GetIndex());
      }
      catch(InvalidCastException)
      {
        return;
      }
    }

    public void AddTileSheet(TileSheet sheet)
    {
      const int MAX_COLS = 4;

      // Create Grid
      int numTiles = sheet.GetNumTiles();
      int numRows = numTiles / MAX_COLS;

      if (numTiles % MAX_COLS > 0)
      {
        ++numRows;
      }

      Grid sheetGrid = new Grid();
      // Add Rows
      for(int index = 0; index < numRows; ++index)
      {
        RowDefinition row = new RowDefinition();
        row.Height = GridLength.Auto;
        sheetGrid.RowDefinitions.Add(row);
      }
      // Add Cols
      for (int index = 0; index < MAX_COLS; ++index)
      {
        ColumnDefinition col = new ColumnDefinition();
        col.Width = GridLength.Auto;
        sheetGrid.ColumnDefinitions.Add(col);
      }

      // Add Images
      int currCol = 0;
      int currRow = 0;

      for(int index = 0; index < numTiles; ++index)
      {
        TileImage nextImg = new TileImage(sheet.GetID(), index);
        nextImg.Margin = new Thickness(3);
        nextImg.Source = sheet.GetBitmapSourceAt(index);
        nextImg.MouseLeftButtonUp += TileButtonClicked;

        sheetGrid.Children.Add(nextImg);
        Grid.SetRow(nextImg, currRow);
        Grid.SetColumn(nextImg, currCol);

        ++currCol;

        if(currCol >= MAX_COLS)
        {
          currCol = 0;
          ++currRow;
        }
      }

      // Create Expander
      Expander sheetExpander = new Expander();
      sheetExpander.Header = sheet.GetFileName();
      sheetExpander.Content = sheetGrid;

      tileStackPanel.Children.Add(sheetExpander);
    }

    public void DrawMap(DrawingGroup group)
    {
      DrawingBrush brush = new DrawingBrush(group);
      int width = (int)group.Bounds.Width;
      int height = (int)group.Bounds.Height;

      if(width != MapCanvas.Width)
      {
        MapCanvas.Width = width;
      }
      if (height != MapCanvas.Height)
      {
        MapCanvas.Height = height;
      }

      MapCanvas.Background = brush;
    }

    #endregion

    #region Menu Methods

    // File
    private void NewGameClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("New Game Clicked");
    }

    private void LoadGameClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Load Game Clicked");
    }

    private void SaveGameClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Save Game Clicked");
    }

    private void SaveGameAsClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Save Game As Clicked");
    }

    private void ExitClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Exit Clicked");
      this.Close();
    }

    // Level
    private void NewLevelClicked(object sender, RoutedEventArgs e)
    {
      NewLevelWindow importWin = new NewLevelWindow(m_mainEngine);
      importWin.Owner = this;
      importWin.ShowDialog();
    }

    private void LoadLevelClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Load Level Clicked");
      m_mainEngine.LoadLevelClicked("level.json"); //TODO popup window
    }

    private void SaveLevelClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Save Level Clicked");
      m_mainEngine.SaveLevelClicked("level.json"); //TODO popup window
    }

    private void SaveLevelAsClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Save Level As Clicked");
    }

    // Tools
    private void ImportComponentsClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Import Components Clicked");
    }

    private void ImportTileSheetClicked(object sender, RoutedEventArgs e)
    {
      ImportSheetWindow importWin = new ImportSheetWindow(m_mainEngine);
      importWin.Owner = this;
      importWin.ShowDialog();
    }

    private void ImportTesterClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Import Tester Clicked");
    }

    private void CreatePrototypeClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Create Prototype Clicked");
    }

    // View
    private void ShowConfigClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Show Config Clicked");
    }

    private void ShowToolsClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Show Tools Clicked");
    }

    private void ShowHintsClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Show Hints Clicked");
    }

    private void ShowMessagesClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Show Messages Clicked");
    }

    private void ShowActionQueuesClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Show Action Queues Clicked");
    }

    // Test
    private void TestClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Test Clicked");
    }

    // Help
    private void HelpClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Help Clicked");
    }

    #endregion

    #region Map Clicks

    private void MapLeftClick(object sender, MouseButtonEventArgs e)
    {
      Point loc = e.GetPosition(MapCanvas);
      m_mainEngine.MapClicked(loc);
    }

    private void MapRightClick(object sender, MouseButtonEventArgs e)
    {
      Point loc = e.GetPosition(MapCanvas);
      m_mainEngine.MapClicked(loc, true);
    }

    #endregion
  }

  public class TileImage : System.Windows.Controls.Image
  {
    private int m_sheetID;
    private int m_index;

    public TileImage(int sheetID, int index)
      : base()
    {
      m_sheetID = sheetID;
      m_index = index;
    }

    public int GetSheetID() { return m_sheetID; }
    public int GetIndex() { return m_index; }
  }
}
