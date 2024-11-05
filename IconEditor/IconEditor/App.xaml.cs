using System.Windows;
using System.Windows.Threading;

namespace IconEditor
{
    #region Задачи (TODO).

    // TODO: Может стоит добавить 384 и 512 в список размеров изображений?

    #endregion

    /// <summary>
    /// Класс приложения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Список размеров изображений.
        /// </summary>
        public static readonly List<int> ImageSizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 192, 256];

        /// <summary>
        /// Список масштабов экрана Windows 10.
        /// </summary>
        public static readonly List<double> Scales = [1.0, 1.25, 1.5, 1.75];

        /// <summary>
        /// Возвращает логическое значение из логического значения, допускающего неопределённое значение.
        /// </summary>
        /// <param name="value">Логическое значение, допускающее неопределённое значение.</param>
        /// <returns>Логическое значение.</returns>
        public static bool SimpleBool(bool? value) => value ?? false;

        /// <summary>
        /// Аналог System.Windows.Forms.Application.DoEvents.
        /// </summary>
        public static void DoEvents()
        {
            Current?.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

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
    }
}
