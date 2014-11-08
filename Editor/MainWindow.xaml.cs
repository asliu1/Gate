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

      // Map Draw Test Code -----------------------------------------------------------------------
      int cols = 20;
      int rows = 10;
      int gridSize = 64;

      MapCanvas.Height = rows * gridSize;
      MapCanvas.Width = cols * gridSize;
      GeometryGroup gridSquares = new GeometryGroup();

      for(int rIndex = 0; rIndex < rows; ++rIndex)
      {
        for(int cIndex = 0; cIndex < cols; ++cIndex)
        {
          Rect tempRect = new Rect((cIndex * gridSize), (rIndex * gridSize), gridSize, gridSize);
          gridSquares.Children.Add(new RectangleGeometry(tempRect));
        }
      }

      GeometryDrawing grid = new GeometryDrawing();
      grid.Geometry = gridSquares;
      grid.Pen = new Pen(Brushes.Black, 1);

      DrawingImage geoImage = new DrawingImage(grid);
      geoImage.Freeze();

      Image image = new Image();
      image.Source = geoImage;

      MapCanvas.Children.Add(image);
      // ------------------------------------------------------------------------------------------
    }

    void WindowClosing(object sender, CancelEventArgs e)
    {
      m_mainEngine.Stop();
    }

    public void UpdateSysMsg(string msg)
    {
      SysMsgBox.Text = msg;
    }

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
    }

    // Level
    private void NewLevelClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("New Level Clicked");
    }

    private void LoadLevelClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Load Level Clicked");
    }

    private void SaveLevelClicked(object sender, RoutedEventArgs e)
    {
      UpdateSysMsg("Save Level Clicked");
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
      UpdateSysMsg("Import Tile Sheet Clicked");

      ImportSheetWindow importWin = new ImportSheetWindow();
      importWin.Owner = this;
      importWin.ShowDialog();

      string filename = importWin.GetFileName();
      int tileSize = importWin.GetTileSize();

      if(filename.Length != 0)
      {
        UpdateSysMsg("Importing " + filename + " at " + tileSize.ToString() + " pixels per tile.");
        // m_mainEngine.LoadTileSheet(filename, tileSize);
      }
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
}
