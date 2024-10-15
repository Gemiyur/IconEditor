using System.Windows.Input;

namespace IconEditor
{
    /// <summary>
    /// Статический класс команд приложения.
    /// </summary>
    public static class AppCommands
    {
        #region Команды группы "Значок".

        /// <summary>
        /// Команда создания нового значка.
        /// </summary>
        public static RoutedUICommand NewIcon { get; private set; }

        /// <summary>
        /// Команда открытия файла значка.
        /// </summary>
        public static RoutedUICommand OpenIcon { get; private set; }

        /// <summary>
        /// Команда сохранения файла значка.
        /// </summary>
        public static RoutedUICommand SaveIcon { get; private set; }

        /// <summary>
        /// Команда сохранения файла значка с другим именем.
        /// </summary>
        public static RoutedUICommand SaveIconAs { get; private set; }

        /// <summary>
        /// Команда выхода из приложения.
        /// </summary>
        public static RoutedUICommand Exit { get; private set; }

        #endregion

        #region Команды группы "Изображение".

        /// <summary>
        /// Команда добавления изображения в значок.
        /// </summary>
        public static RoutedUICommand AddImage { get; private set; }

        /// <summary>
        /// Команда удаления изображения из значка.
        /// </summary>
        public static RoutedUICommand DeleteImage { get; private set; }

        /// <summary>
        /// Команда копирования изображения в буфер обмена.
        /// </summary>
        public static RoutedUICommand CopyImage { get; private set; }

        /// <summary>
        /// Команда вырезания изображения в буфер обмена.
        /// </summary>
        public static RoutedUICommand CutImage { get; private set; }

        /// <summary>
        /// Команда вставки изображения из буфера обмена.
        /// </summary>
        public static RoutedUICommand PasteImage { get; private set; }

        /// <summary>
        /// Команда экспорта изображения в файл.
        /// </summary>
        public static RoutedUICommand ExportImage { get; private set; }

        #endregion

        #region Команды группы "Справка".

        /// <summary>
        /// Команда отображения окна "О программе".
        /// </summary>
        public static RoutedUICommand About { get; private set; }

        #endregion

        /// <summary>
        /// Статический конструктор класса. Инициализирует команды приложения.
        /// </summary>
        static AppCommands()
        {
            // Команды группы "Значок".
            NewIcon = new RoutedUICommand("Новый", "NewIcon", typeof(AppCommands));
            OpenIcon = new RoutedUICommand("Открыть...", "OpenIcon", typeof(AppCommands));
            SaveIcon = new RoutedUICommand("Сохранить", "SaveIcon", typeof(AppCommands));
            SaveIconAs = new RoutedUICommand("Сохранить как...", "SaveIconAs", typeof(AppCommands));
            Exit = new RoutedUICommand("Выход", "Exit", typeof(AppCommands));

            // Команды группы "Изображение".
            CopyImage = new RoutedUICommand("Копировать", "CopyImage", typeof(AppCommands));
            CutImage = new RoutedUICommand("Вырезать", "CutImage", typeof(AppCommands));
            PasteImage = new RoutedUICommand("Вставить", "PasteImage", typeof(AppCommands));
            AddImage = new RoutedUICommand("Добавить...", "AddImage", typeof(AppCommands));
            DeleteImage = new RoutedUICommand("Удалить...", "DeleteImage", typeof(AppCommands));
            ExportImage = new RoutedUICommand("Экспорт...", "ExportImage", typeof(AppCommands));

            // Команды группы "Справка".
            About = new RoutedUICommand("О программе", "About", typeof(AppCommands));
        }
    }
}
