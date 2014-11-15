using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using Microsoft.Win32;

namespace Editor
{
  /// <summary>
  /// Interaction logic for ImportSheetWindow.xaml
  /// </summary>
  public partial class ImportSheetWindow : Window
  {
    private String[] m_sizeStrings = new string[] { "32x32", "64x64", "128x128", "256x256" };
    private int[] m_sizes = new int[] { 32, 64, 128, 256 };

    private string m_filename;
    private int m_tileSize;
    private Engine m_engine;

    public ImportSheetWindow(Engine mainEngine)
    {
      InitializeComponent();
      m_engine = mainEngine;
      m_filename = "";
      m_tileSize = 0;

      // Populate TileSize Dropdown
      foreach(string size in m_sizeStrings)
      {
        sizeCombo.Items.Add(size);
      }
      sizeCombo.SelectedIndex = 0;
    }

    private void BrowseClicked(object sender, RoutedEventArgs e)
    {
      // Configure open file dialog box
      OpenFileDialog fileDlg = new OpenFileDialog();

      fileDlg.Filter = "Image Files (*.bmp, *.gif, *.jpg, *.png, *.tif)|*.bmp; *.gif; *.jpg; *.png; *.tif; *.tiff"; // Supported Image Files
      fileDlg.Filter += "|All files (*.*)|*.*"; // Option to view all file types
      fileDlg.InitialDirectory = Directory.GetCurrentDirectory() +  "\\TileSheets";

      Nullable<bool> result = fileDlg.ShowDialog();

      if(result == true) // User clicked OK
      {
        fileNameBox.Text = fileDlg.FileName;
      }
    }

    private void ImportClicked(object sender, RoutedEventArgs e)
    {
      m_filename = fileNameBox.Text;

      // Get TileSize
      int index = sizeCombo.SelectedIndex;
      m_tileSize = m_sizes[index];

      if (m_filename.Length > 0)
      {
        m_engine.LoadTileSheet(m_filename, m_tileSize, ImportCallback);
      }
      else
      {
        userMsgBox.Text = "Please Select an Image file.";
      }
    }

    private void ImportCallback(SheetError err)
    {
      // Check Error Code and pass message to user if needed
      switch (err)
      {
        case (SheetError.SUCCESS):
          this.Close();
          break;
        case (SheetError.SIZE_MISMATCH):
          MessageBox.Show(this, "The tile size does not match image evenly and tiles may contain filler pixels.");
          this.Close();
          break;
        case (SheetError.TOO_LARGE):
          int max = Int16.MaxValue;
          userMsgBox.Text = "Selected Image is too large. Max width/height is " + max.ToString() + " pixels.";
          break;
        case (SheetError.INVALD_ARG):
          userMsgBox.Text = "File path is invalid.";
          break;
        case (SheetError.NOT_FOUND):
          userMsgBox.Text = "The requested file was not found.";
          break;
        case (SheetError.UNSUPPORTED):
          userMsgBox.Text = "The requested file type is not supported.";
          break;
        default:
          userMsgBox.Text = "An internal error was encountered, please try again.";
          break;
      }
    }

    public string GetFileName() { return m_filename; }
    public int GetTileSize() { return m_tileSize; }
  }
}
