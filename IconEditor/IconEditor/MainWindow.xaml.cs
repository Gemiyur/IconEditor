using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using IconEditor.Comparers;
using IconEditor.Icons;

namespace IconEditor;

/// <summary>
/// Класс главного окна.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Тип ресурса для значка в файле значка.
    /// </summary>
    const short IconType = 1;

    /// <summary>
    /// Базовый заголовок окна без имени значка.
    /// </summary>
    private readonly string Caption;

    /// <summary>
    /// Имя файла значка с полным путём.
    /// </summary>
    private string FileName = string.Empty;

    /// <summary>
    /// Был ли изменён значок?
    /// </summary>
    private bool IconChanged;

    /// <summary>
    /// Имя значка.
    /// </summary>
    private string IconName => string.IsNullOrEmpty(FileName) ? "(новый)" : Path.GetFileNameWithoutExtension(FileName);

    /// <summary>
    /// Список представлений кадров значка.
    /// </summary>
    private readonly List<IconFrameView> FrameViews = [];

    #region Диалоги открытия и сохранения файлов.

    private readonly OpenFileDialog OpenIconDialog = new()
    {
        CheckFileExists = true,
        CheckPathExists = true,
        ValidateNames = true,
        Title = "Открыть значок",
        Filter = "Файлы значков|*.ico"
    };

    private readonly OpenFileDialog OpenImageDialog = new()
    {
        CheckFileExists = true,
        CheckPathExists = true,
        Multiselect = true,
        ValidateNames = true,
        Title = "Добавить изображение",
        Filter = "Файлы изображений|*.bmp;*.dib;*.gif;*.jpe;*.jpeg;*.jpg;*.png"
    };

    private readonly SaveFileDialog SaveIconDialog = new()
    {
        Title = "Сохранить значок как",
        ValidateNames = true,
        AddExtension = true,
        DefaultExt = ".ico",
        Filter = "Значок|*.ico"
    };

    private readonly SaveFileDialog SaveImageDialog = new()
    {
        Title = "Сохранить изображение как",
        ValidateNames = true,
        AddExtension = true,
        DefaultExt = ".png",
        Filter = "Portable Network Graphics|*.png"
    };

    private readonly ImageFormat[] ImageFormats =
    [
        ImageFormat.Png
    ];

    #endregion

    /// <summary>
    /// Инициализирует новый экземпляр класса.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Caption = Title;
        UpdateTitle();
        foreach (var size in App.ImageSizes)
        {
            SizesComboBox.Items.Add($"{size} x {size}");
        }
        ImagesListBox.ItemsSource = FrameViews;
    }

    /// <summary>
    /// Сортирует список представлений кадров значка по размеру.
    /// </summary>
    private void SortFrameViews()
    {
        FrameViews.Sort(new MultiKeyComparer(
            [new IntKeyComparer(x => ((IconFrameView)x).Size), new IntKeyComparer(x => ((IconFrameView)x).BitmapSize)]));
    }

    /// <summary>
    /// Обновляет список кадров значка.
    /// </summary>
    private void UpdateImagesListBox()
    {
        ImagesListBox.ItemsSource = null;
        ImagesListBox.ItemsSource = FrameViews;
        StatusBarImagesCount.Text = ImagesListBox.Items.Count.ToString();
        StatusBarSelectedCount.Text = ImagesListBox.SelectedItems.Count.ToString();
    }

    /// <summary>
    /// Обновляет заголовок окна в соответствии с именем значка.
    /// </summary>
    private void UpdateTitle() => Title = $"{Caption} - {IconName}";

    /// <summary>
    /// Загружает значок из файла с указанным именем.
    /// </summary>
    /// <param name="fileName">Имя фойла с полным путём.</param>
    private void LoadIcon(string fileName)
    {
        // Считываем значок из файла.
        var frames = new List<IconFrame>();
        using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            stream.Position = 0;
            using (var reader = new BinaryReader(stream))
            {
                // Заголовок значка.
                if (reader.ReadInt16() != 0 || reader.ReadInt16() != IconType)
                {
                    MessageBox.Show("Файл не является файлом значка.", Title);
                    return;
                }
                int count = reader.ReadInt16();
                if (count < 1)
                {
                    // Значок не содержит кадров/изображений.
                    return;
                }
                // Кадры значка без изображений.
                for (var i = 0; i < count; i++)
                    frames.Add(new IconFrame(reader));
                // Изображения кадров значка. Они хранятся в конце.
                foreach (var frame in frames)
                    frame.ReadImage(reader);
            }
        }
        // Добавляем представления кадров значка.
        foreach (var frame in frames)
        {
            // TODO: Вываливается по исключению для сторонних файлов значков.
            using (var stream = new MemoryStream())
            {
                stream.Write(frame.Image);
                var bitmap = new Bitmap(stream);
                FrameViews.Add(new IconFrameView(bitmap));
            }
        }
    }

    /// <summary>
    /// Сохраняет значок в файл с указанным именем. Если файл существует, то он перезаписывается.
    /// </summary>
    /// <param name="fileName">Имя фойла с полным путём.</param>
    private void SaveIcon(string fileName)
    {
        // Создаём список кадров значка.
        var frames = new List<IconFrame>();
        foreach (var view in FrameViews)
        {
            if (view.Size != view.BitmapSize)
                view.Bitmap = view.GetResizedBitmap();
            using (var stream = new MemoryStream())
            {
                view.Bitmap.Save(stream, ImageFormat.Png);
                frames.Add(new IconFrame(view.Bitmap.Width, view.Bitmap.Height, (int)stream.Length, stream.ToArray()));
            }
        }
        // Записываем значок в файл.
        using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        {
            using (var writer = new BinaryWriter(stream))
            {
                // Заголовок значка.
                writer.Write((short)0);
                writer.Write(IconType);
                writer.Write((short)frames.Count);
                var offset = (frames.Count * 16) + 6;
                // Кадры значка без изображений.
                foreach (var frame in frames)
                {
                    frame.Write(writer);
                    writer.Write(offset);
                    offset += frame.Bytes;
                }
                // Изображения кадров значка. Они хранятся в конце.
                foreach (var frame in frames)
                    writer.Write(frame.Image);
            }
        }
        var index = ImagesListBox.SelectedIndex;
        UpdateImagesListBox();
        ImagesListBox.SelectedIndex = index;
    }

    /// <summary>
    /// Сохраняет значок в файл с запросом на сохранение, если значок был изменён.
    /// </summary>
    private void SaveIconIfChanged()
    {
        if (!IconChanged ||
            MessageBox.Show("Значок был изменён. Сохранить его?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }
        if (!string.IsNullOrEmpty(FileName))
        {
            SaveIcon(FileName);
            IconChanged = false;
            return;
        }
        if (App.SimpleBool(SaveIconDialog.ShowDialog()))
        {
            SaveIcon(SaveIconDialog.FileName);
            IconChanged = false;
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveIconIfChanged();
    }

    #region Обработчики событий элементов управления.

    private void ImagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var EditorPanelWasEnabled = EditorPanel.IsEnabled;
        StatusBarSelectedCount.Text = ImagesListBox.SelectedItems.Count.ToString();
        if (ImagesListBox.SelectedItems.Count != 1)
        {
            PreviewImage.Source = null;
            EditorPanel.IsEnabled = false;
            if (IsVisible && EditorPanelWasEnabled)
            {
                ((System.Windows.Controls.Image)RotateLeftButton.Content).Source =
                    App.GetBitmap(@"Images\Buttons\Disabled\RotateLeft.png");
                ((System.Windows.Controls.Image)RotateRightButton.Content).Source =
                    App.GetBitmap(@"Images\Buttons\Disabled\RotateRight.png");
                ((System.Windows.Controls.Image)FlipHorizontalButton.Content).Source =
                    App.GetBitmap(@"Images\Buttons\Disabled\FlipHorizontal.png");
                ((System.Windows.Controls.Image)FlipVerticalButton.Content).Source =
                    App.GetBitmap(@"Images\Buttons\Disabled\FlipVertical.png");
            }
            return;
        }
        var view = (IconFrameView)ImagesListBox.SelectedItem;
        PreviewImage.Source = view.BitmapFrame;
        if (SizesComboBox.SelectedIndex == App.ImageSizes.IndexOf(view.Size))
        {
            PreviewImage.Width = view.Size;
            PreviewImage.Height = view.Size;
        }
        else
        {
            SizesComboBox.SelectedIndex = App.ImageSizes.IndexOf(view.Size);
        }
        EditorPanel.IsEnabled = true;
        if (IsVisible && !EditorPanelWasEnabled)
        {
            ((System.Windows.Controls.Image)RotateLeftButton.Content).Source =
                App.GetBitmap(@"Images\Buttons\Enabled\RotateLeft.png");
            ((System.Windows.Controls.Image)RotateRightButton.Content).Source =
                App.GetBitmap(@"Images\Buttons\Enabled\RotateRight.png");
            ((System.Windows.Controls.Image)FlipHorizontalButton.Content).Source =
                App.GetBitmap(@"Images\Buttons\Enabled\FlipHorizontal.png");
            ((System.Windows.Controls.Image)FlipVerticalButton.Content).Source =
                App.GetBitmap(@"Images\Buttons\Enabled\FlipVertical.png");
        }
    }

    private int SizesComboBoxSelectedIndex = -1;

    private void SizesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var size = App.ImageSizes[SizesComboBox.SelectedIndex];
        var view = (IconFrameView)ImagesListBox.SelectedItem;
        view.Size = size;
        SortFrameViews();
        UpdateImagesListBox();
        ImagesListBox.SelectedItem = view;
        PreviewImage.Width = size;
        PreviewImage.Height = size;
    }

    private void SizesComboBox_DropDownOpened(object sender, EventArgs e)
    {
        SizesComboBoxSelectedIndex = SizesComboBox.SelectedIndex;
    }

    private void SizesComboBox_DropDownClosed(object sender, EventArgs e)
    {
        if (SizesComboBoxSelectedIndex != SizesComboBox.SelectedIndex)
            IconChanged = true;
    }

    private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
    {
        var view = FrameViews[ImagesListBox.SelectedIndex];
        view.RotateLeft();
        PreviewImage.Source = view.BitmapFrame;
        IconChanged = true;
    }

    private void RotateRightButton_Click(object sender, RoutedEventArgs e)
    {
        var view = FrameViews[ImagesListBox.SelectedIndex];
        view.RotateRight();
        PreviewImage.Source = view.BitmapFrame;
        IconChanged = true;
    }

    private void FlipHorizontalButton_Click(object sender, RoutedEventArgs e)
    {
        var view = FrameViews[ImagesListBox.SelectedIndex];
        view.FlipHorizontal();
        PreviewImage.Source = view.BitmapFrame;
        IconChanged = true;
    }

    private void FlipVerticalButton_Click(object sender, RoutedEventArgs e)
    {
        var view = FrameViews[ImagesListBox.SelectedIndex];
        view.FlipVertical();
        PreviewImage.Source = view.BitmapFrame;
        IconChanged = true;
    }

    #endregion

    #region Обработчики команд группы "Значок".

    private void NewIcon_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SaveIconIfChanged();
        FrameViews.Clear();
        UpdateImagesListBox();
        FileName = string.Empty;
        UpdateTitle();
        IconChanged = false;
    }

    private void OpenIcon_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SaveIconIfChanged();
        if (!App.SimpleBool(OpenIconDialog.ShowDialog()))
            return;
        FrameViews.Clear();
        LoadIcon(OpenIconDialog.FileName);
        UpdateImagesListBox();
        FileName = OpenIconDialog.FileName;
        UpdateTitle();
        IconChanged = false;
    }

    private void SaveIcon_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = FrameViews.Any() && IconChanged;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmap(
            e.CanExecute ? @"Images\Buttons\Enabled\SaveIcon.png" : @"Images\Buttons\Disabled\SaveIcon.png");
        ((System.Windows.Controls.Image)SaveIconButton.Content).Source = bitmap;
        ((System.Windows.Controls.Image)SaveIconMenuItem.Icon).Source = bitmap;
    }

    private void SaveIcon_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(FileName))
        {
            SaveIcon(FileName);
            IconChanged = false;
            return;
        }
        if (!App.SimpleBool(SaveIconDialog.ShowDialog()))
            return;
        SaveIcon(SaveIconDialog.FileName);
        FileName = SaveIconDialog.FileName;
        UpdateTitle();
        IconChanged = false;
    }

    private void SaveIconAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = FrameViews.Any();
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmap(
            e.CanExecute ? @"Images\Buttons\Enabled\SaveIconAs.png" : @"Images\Buttons\Disabled\SaveIconAs.png");
        ((System.Windows.Controls.Image)SaveIconAsButton.Content).Source = bitmap;
        ((System.Windows.Controls.Image)SaveIconAsMenuItem.Icon).Source = bitmap;
    }

    private void SaveIconAs_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SaveIconIfChanged();
        if (!App.SimpleBool(SaveIconDialog.ShowDialog()))
            return;
        SaveIcon(SaveIconDialog.FileName);
        FileName = SaveIconDialog.FileName;
        UpdateTitle();
        IconChanged = false;
    }

    private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    #endregion

    #region Обработчики команд группы "Изображение".

    private void AddImage_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (!App.SimpleBool(OpenImageDialog.ShowDialog()) || OpenImageDialog.FileNames.Length < 1)
        {
            return;
        }
        if (OpenImageDialog.FileNames.Length == 1)
        {
            var view = new IconFrameView(new Bitmap(OpenImageDialog.FileName));
            FrameViews.Add(view);
            SortFrameViews();
            UpdateImagesListBox();
            ImagesListBox.SelectedIndex = FrameViews.IndexOf(view);
        }
        else
        {
            foreach (var fileName in OpenImageDialog.FileNames)
            {
                FrameViews.Add(new IconFrameView(new Bitmap(fileName)));
            }
            SortFrameViews();
            UpdateImagesListBox();
        }
        IconChanged = true;
    }

    private void DeleteImage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ImagesListBox != null && ImagesListBox.SelectedItems.Count > 0;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmap(
            e.CanExecute ? @"Images\Buttons\Enabled\DeleteImage.png" : @"Images\Buttons\Disabled\DeleteImage.png");
        ((System.Windows.Controls.Image)DeleteImageButton.Content).Source = bitmap;
        ((System.Windows.Controls.Image)DeleteImageMenuItem.Icon).Source = bitmap;
    }

    private void DeleteImage_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (MessageBox.Show("Удалить выбранные изображения?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;
        var views = ImagesListBox.SelectedItems.Cast<IconFrameView>().ToList();
        FrameViews.RemoveAll(views.Contains);
        UpdateImagesListBox();
        IconChanged = true;
    }

    private void ExportImage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = ImagesListBox != null && ImagesListBox.SelectedItems.Count == 1;
        if (!IsVisible)
            return;
        var bitmap = App.GetBitmap(
            e.CanExecute ? @"Images\Buttons\Enabled\ExportImage.png" : @"Images\Buttons\Disabled\ExportImage.png");
        ((System.Windows.Controls.Image)ExportImageButton.Content).Source = bitmap;
        ((System.Windows.Controls.Image)ExportImageMenuItem.Icon).Source = bitmap;
    }

    private void ExportImage_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (App.SimpleBool(SaveImageDialog.ShowDialog()))
        {
            ((IconFrameView)ImagesListBox.SelectedItem).Bitmap.Save(
                SaveImageDialog.FileName, ImageFormats[SaveImageDialog.FilterIndex - 1]);
        }
    }

    #endregion

    #region Обработчики команд группы "Справка".

    private void About_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        new AboutDialog() { Owner = this }.ShowDialog();
    }

    #endregion
}