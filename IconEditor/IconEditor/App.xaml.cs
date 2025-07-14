using System.Windows;
using System.Windows.Media.Imaging;

namespace IconEditor;

/// <summary>
/// Класс приложения.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Список размеров изображений.
    /// </summary>
    public static readonly List<int> ImageSizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 192, 256, 384, 512];

    /// <summary>
    /// Возвращает BitmapImage из указанного файла изображения.
    /// </summary>
    /// <param name="path">Uri (путь) к файлу.</param>
    /// <returns></returns>
    public static BitmapImage GetBitmap(string path) => new BitmapImage(new Uri(path, UriKind.Relative));

    /// <summary>
    /// Возвращает ближайший меньший размер изображения для указанного размера изображения.
    /// </summary>
    /// <param name="size">Размер изображения.</param>
    /// <returns>Ближайший меньший размер изображения.</returns>
    public static int NearestImageSize(int size)
    {
        if (ImageSizes.Contains(size))
            return size;
        if (size < ImageSizes.First())
            return ImageSizes.First();
        if (size > ImageSizes.Last())
            return ImageSizes.Last();
        return ImageSizes.Last(x => x < size);
    }

    // TODO: Заменить в коде SimpleBool сравнением с true.
    /// <summary>
    /// Возвращает логическое значение из логического значения, допускающего неопределённое значение.
    /// </summary>
    /// <param name="value">Логическое значение, допускающее неопределённое значение.</param>
    /// <returns>Логическое значение.</returns>
    public static bool SimpleBool(bool? value) => value ?? false;
}
