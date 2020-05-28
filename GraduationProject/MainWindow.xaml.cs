using GraduationProject.Modules.ExternalWork;
using GraduationProject.Modules.Encryption;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using Microsoft.Win32;

namespace GraduationProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Текущий выбранный вопроса
        /// </summary>
        private int QuestionIndex = 0;

        /// <summary>
        /// Список текущих вариантов ответа
        /// </summary>
        private List<RadioButton> RadioButtonAnswerList = null;

        /// <summary>
        /// Список вопросов
        /// </summary>
        private List<Modules.Question.Question> QuestionList = null;

        /// <summary>
        /// Путь до базы данных
        /// </summary>
        private string dbPath = null;

        /// <summary>
        /// Доступ к БД
        /// </summary>
        private UsingAccess UsAc = null;

        private string UserLogin = null;
        private string UserPassword = null;

        public MainWindow()
        {
            try
            {
                LoadingData();
            }
            catch
            {
                MessageBox.Show("Файл параметров оказался битым, удалить его?", "Ошибка чтения файла с параметрами", MessageBoxButton.OK, MessageBoxImage.Error);
                MessageBoxResult resultDeleteParametrsFile = MessageBox.Show("Файл с параметрами оказался битым, вы хотите его удалить?", "Ошибка чтения", MessageBoxButton.YesNo);
                if (resultDeleteParametrsFile == MessageBoxResult.Yes)
                {
                    try
                    {
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName);
                    }
                    catch (Exception ex2)
                    {
                        MessageBox.Show(ex2.ToString(), "Ошибка удаления файла", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }


            if (dbPath == null)
            {
                //Если не удалось найти файл БД, то предложить сделать это
                MessageBoxResult resultCreateDb = MessageBox.Show("Файл базы данных не найден, вы хотите указать путь до неё?", "Подключение к базе данных", MessageBoxButton.YesNo);
                if (resultCreateDb == MessageBoxResult.Yes)
                {
                    //Создание нового подключения
                    //Получение пути до новой БД
                    OpenFileDialog dialog = new OpenFileDialog
                    {
                        Filter = "Файлы баз данных|*.mdb; *.accdb|Все файлы|*.*"
                    };
                    dialog.ShowDialog();

                    try
                    {
                        UsAc = new UsingAccess(dialog.FileName);

                        //Сохранение пути в файле
                        File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName, Encrypt.Shifrovka(dialog.FileName, Key.encryptionKeyInFile));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Ошибка подключения к базе данных");
                    }

                    if (UsAc != null)
                    {
                        if (!CheckDB(dialog.FileName))
                        {
                            MessageBoxResult resultEditDb = MessageBox.Show("Данная база данных не предназначена для работы с текущим приложением. Но её можно переделать и использовать в данном приложении. Вы хотите этого? (Внимание! Если данный файл уже содержит таблицы, то могут возникнуть проблемы, рекомендуется сделать копию данной базы данных, для сохранения целостности информации на ней)", "Некорректный файл базы данных", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                            if (resultEditDb == MessageBoxResult.Yes)
                            {
                                //Добавление нужных таблиц

                                //Сохранение пути в файле
                                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName, Encrypt.Shifrovka(dialog.FileName, Key.encryptionKeyInFile));
                            }
                            else
                            {
                                //Просто выйти из приложения
                                Environment.Exit(1);
                            }
                        }
                    }
                }
                else
                {
                    //Просто выйти из приложения
                    Environment.Exit(1);
                }

            }
            else if (dbPath != null)
            {
                //Если удалось найти файл БД, то проверка на корректность БД
                if (!CheckDB(dbPath))
                {
                    MessageBox.Show("Используемый файл базы данных не имеет корректных данных для работы с приложением");

                };
            }

            var QList = new List<Modules.Question.Question>
            {
                new Modules.Question.Question("Как долго Вы ожидали ответ от нашего центра обслуживания клиентов?",
                                              "Очень долго",
                                              "Долго",
                                              "В пределах нормы",
                                              "Быстро",
                                              "Реакция была почти мгновенная."),

                new Modules.Question.Question("Насколько внимательно слушали Вас представители центра обслуживания клиентов?",
                                              "Очень внимательно",
                                              "Внимательно",
                                              "Не слишком внимательно",
                                              "Совсем невнимательно"),

                new Modules.Question.Question("Насколько активны были представители обслуживания клиентов при попытке помочь Вам?",
                                              "Очень активны",
                                              "Активны",
                                              "Скорее активны",
                                              "Не слишком активны",
                                              "Совсем неактивны"),

                new Modules.Question.Question("Как быстро сумели представители нашего обслуживания клиентов Вам помочь?",
                                              "Очень быстро",
                                              "Быстро",
                                              "В пределах нормы",
                                              "Медленно",
                                              "Очень медленно"),

                new Modules.Question.Question("По Вашему мнению, представитель нашего центра обслуживания клиентов достаточно хорошо информирован?",
                                              "Очень хорошо",
                                              "Хорошо",
                                              "Скорее хорошо",
                                              "Скорее плохо",
                                              "Плохо"),

                new Modules.Question.Question("Насколько понятна была информация, которую Вы получили у нашего обслуживания клиентов?",
                                              "Полностью понятна",
                                              "Понятна",
                                              "Почти понятна",
                                              "Не слишком понятна",
                                              "Совсем непонятна"),

                new Modules.Question.Question("На сколько Ваших вопросов ответили представители нашего обслуживания клиентов?",
                                              "На все вопросы",
                                              "На большинство вопросов",
                                              "На половину вопросов",
                                              "На меньшинство вопросов",
                                              "На мои вопросы не было отвечено."),

                new Modules.Question.Question("Насколько был для Вас наш центр обслуживание клиентов полезным?",
                                              "Очень полезно",
                                              "Частично полезно",
                                              "Не слишком полезно",
                                              "Не было полезно"),

                new Modules.Question.Question("Каково было для Вас сотрудничество с нашим обслуживанием клиентов?",
                                              "Немного лучше, чем были мои ожидания.",
                                              "На немного лучше, чем были мои ожидания.",
                                              "Такое, как я ожидал/а.",
                                              "Немного хуже, чем были мои ожидания.",
                                              "Другое")
            };

            InitializeComponent();
            StartPoling(QList, Core, false);
        }

        private void StartPoling(List<Modules.Question.Question> questList, Grid grid, bool RandomAnswerPosition)
        {
            QuestionIndex = 0;
            QuestionList = questList;
            QuestionBuilder(questList[0], grid, RandomAnswerPosition, null);
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            bool GoNext = false;
            string Select = null;

            foreach (RadioButton radioButton in RadioButtonAnswerList)
            {
                if (radioButton.IsChecked == true)
                {
                    MessageBox.Show(radioButton.Name + "\n" + radioButton.Content);
                    GoNext = true;
                    Select = radioButton.Name.Replace("a", "");
                }
            }

            if (GoNext)
            {
                QuestionList[QuestionIndex].selectAnswer = Convert.ToInt32(Select);
                QuestionIndex++;

                string ButtonContent = null;
                if (QuestionIndex == QuestionList.Count - 1)
                    ButtonContent = "Завершить опрос";
                else
                    ButtonContent = "Следующий вопрос";

                if (QuestionIndex >= QuestionList.Count)
                {
                    //MessageBox.Show("Опрос был завершен");
                    Core.Children.Clear();
                    return;
                }

                QuestionBuilder(QuestionList[QuestionIndex], Core, false, ButtonContent);
            }
        }

        private void QuestionBuilder(Modules.Question.Question question, Grid grid, bool randomPosition, string ButtonContent)
        {
            grid.Children.Clear();

            RadioButtonAnswerList = new List<RadioButton>();
            Thickness marg = new Thickness(0, 0, 0, 4);

            //Создание блока с содержимым            
            StackPanel WrPanel = new StackPanel
            {
                Name = "WrPanel",
                Margin = new Thickness(8),
                MaxWidth = 600,
                MinWidth = 300,
            };

            //Создание вопроса
            TextBlock TxBlock = new TextBlock
            {
                Name = "Question",
                Text = question.QuestionText,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = new TextWrapping(),
            };
            WrPanel.Children.Add(TxBlock);


            //Выбор расположения ответов
            if (randomPosition)
            {
                Random rand = new Random();
                List<int> BusyIndexList = new List<int>();
                while (BusyIndexList.Count < question.AnswerList.Count)
                {
                    int newIndex = rand.Next(0, question.AnswerList.Count);
                    if (BusyIndexList.IndexOf(newIndex) != -1)
                        continue;

                    var RadButton = new RadioButton
                    {
                        Name = "a" + newIndex.ToString(),
                        IsChecked = false,
                        Content = question.AnswerList[newIndex].Text,
                        Margin = marg,
                    };
                    BusyIndexList.Add(newIndex);
                    WrPanel.Children.Add(RadButton);
                    RadioButtonAnswerList.Add(RadButton);
                }
            }
            else
            {
                //Создание вариантов ответа
                int AnswerIndex = 0;
                foreach (Modules.Question.Answer answer in question.AnswerList)
                {
                    var RadButton = new RadioButton
                    {
                        Name = "a" + AnswerIndex.ToString(),
                        IsChecked = false,
                        Content = answer.Text,
                        Margin = marg,
                    };
                    AnswerIndex++;

                    WrPanel.Children.Add(RadButton);
                    RadioButtonAnswerList.Add(RadButton);
                }
            }

            //Создание кнопки "далее"
            if (ButtonContent == null)
                ButtonContent = "Следующий вопрос";

            Button Butto = new Button
            {
                Content = ButtonContent,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 0, 0),
            };
            Butto.Click += NextQuestion_Click;
            WrPanel.Children.Add(Butto);

            grid.Children.Add(WrPanel);
        }

        private void LoadingData()
        {
            //Имена и пути файлов, расположенных в каталоге с .EXE файлом приложения
            string[] allfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

            //Определение ближайщего файла БД с корректным содержимым
            foreach (string filename in allfiles)
            {
                if (filename.EndsWith(".mdb"))
                {
                    if (CheckDB(filename))
                        dbPath = filename;
                }
                else if (filename.EndsWith(".accdb"))
                {
                    if (CheckDB(filename))
                        dbPath = filename;
                }
            }

            //Если корректная БД найдена, переключиться на неё, игнорируя все остальное
            if (dbPath != null)
            {
                UsAc = new UsingAccess(dbPath);
                return;
            }

            //Опредение файла с сохраненными параметрами доступа
            foreach (string filename in allfiles)
            {
                if (filename.EndsWith(Key.settingsFileName))
                {
                    //Сохранение содержимого файла
                    string[] LineOfFile = File.ReadAllLines(filename, Encoding.GetEncoding(1251));
                    if (LineOfFile.Length >= 1)
                    {
                        string tempDbPath = null;
                        Encrypt Encrypt = new Encrypt();
                        try
                        {
                            //Расшифровра пути до базы данных
                            tempDbPath = Encrypt.DeShifrovka(LineOfFile[0], Key.encryptionKeyInFile);
                        }
                        catch
                        {
                            throw new Exception("Ошибка при попытке расшифровать путь до базы данных");
                        }

                        //Создание подключения                        
                        UsAc = new UsingAccess(tempDbPath);
                        dbPath = tempDbPath;
                        try
                        {
                            //Попытка расшифровать и сохранить логин пользователя
                            UserLogin = Encrypt.DeShifrovka(LineOfFile[1], Key.encryptionKeyInFile);
                        }
                        catch { }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет БД на корректность к работе
        /// </summary>
        /// <param name="path">Путь до БД</param>
        /// <returns>Корректность БД</returns>
        private bool CheckDB(string path)
        {
            UsingAccess ChUsAc = null;
            try
            {
                ChUsAc = new UsingAccess(path);
                ChUsAc.ConnectOpen();

                //Проверка на содержание соответсвующих таблиц
                DataView result = ChUsAc.Execute("SELECT AuthLog.*, Core.*, Polls.*, User.* FROM Core, AuthLog INNER JOIN([User] INNER JOIN Polls ON User.Login = Polls.AuthorLogin) ON AuthLog.Login = User.Login");

                //Проверка на содержание нужного ключа
                result = ChUsAc.Execute($@"SELECT dbKey FROM Core where dbKey = ""{Key.dbKey}""");
                if (result.Count == 1)
                {
                    ChUsAc.ConnectClose();
                    return true;
                }
            }
            catch { }

            ChUsAc.ConnectClose();
            return false;
        }
    }

    /// <summary>
    /// Хранилище ключей
    /// </summary>
    static class Key
    {
        /// <summary>
        /// Ключ, который должен содержаться в таблце Core, на рабочей БД
        /// </summary>
        public const string dbKey = "jKsaUeqk";

        /// <summary>
        /// Ключ шифрования пути, логина и пароля, хранимых вне БД.
        /// </summary>
        public const string encryptionKeyInFile = "qnvI&ofLLL";

        /// <summary>
        /// Ключ шифрования логина и пароля, хранимых в БД.
        /// </summary>
        public const string encryptionKeyInDb = "jq&8ajd0Ajd*on1-fz@%";

        /// <summary>
        /// Название файла с содержимым для работы
        /// </summary>
        public const string settingsFileName = "parameters.dat";
    }
}
