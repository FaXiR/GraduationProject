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
using GraduationProject.Modules.Question;

namespace GraduationProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private enum Grids
        {
            MainWindow,
            Authorization,
            AccountEdit,
            AccountCreate,
            PollList,
            PollPass,
            PollEditMenu,
            PollEditing,
            PollResultList,
        }

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

        /// <summary>
        /// ФИО пользователя
        /// </summary>
        private string UserFio = null;

        /// <summary>
        /// Логин пользователя
        /// </summary>
        private string UserLogin = null;

        /// <summary>
        /// Авторизован ли пользователь
        /// </summary>
        private bool UserAuthrorization = false;

        /// <summary>
        /// Уровень допуска пользователя. 0 - Админ. 1 - Сотрудник. 2 - Гость.
        /// </summary>
        private int UserToleranceLvl = 2;

        /// <summary>
        /// Выбранная редактируемая таблицы опроса
        /// /// </summary>
        private string PollSelectedToEdit;

        /// <summary>
        /// Выбранная редактируемая таблицы опроса
        /// /// </summary>
        private string PollSelectedToPoling;

        private QuestionTextManipulator QTM = new QuestionTextManipulator();

        public MainWindow()
        {
            InitializeComponent();

            //Подключение к базе данных
            ConnectDB();
            UsAc.ConnectOpen();

            //Вывод панели авторизации
            if (UserLogin != null)
            {
                F_Login.Text = UserLogin;
                SelectGrid(Grids.Authorization);
                F_Password.Focus();
            }
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

            if (RadioButtonAnswerList.Count == 0)
            {
                GoNext = true;
            }
            else
            {
                foreach (RadioButton radioButton in RadioButtonAnswerList)
                {
                    if (radioButton.IsChecked == true)
                    {
                        GoNext = true;
                        Select = radioButton.Name.Replace("a", "");
                    }
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
                    Core.Children.Clear();
                    MessageBox.Show("Вы ответили на все вопросы в анкете");

                    UsAc.ExecuteNonQuery($@"INSERT INTO Result (PollTableName, Result, DateOfPassage) VALUES (""{PollSelectedToPoling}"", ""{QTM.QuestionListResultToString(QuestionList)}"", NOW())");

                    SelectGrid(Grids.MainWindow);
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
                Background = null,
                Width = 120,
            };
            Butto.Click += NextQuestion_Click;
            WrPanel.Children.Add(Butto);

            grid.Children.Add(WrPanel);
        }

        /// <summary>
        /// Пробует подключиться к БД. Если возникают проблемы, то показывает сообщения с действиями Да/Нет для решения проблемы
        /// </summary>
        private void ConnectDB()
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
                        File.Delete(Environment.CurrentDirectory + Key.settingsFileName);
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
                        dbPath = dialog.FileName;

                        //Сохранение пути в файле
                        File.WriteAllText(Environment.CurrentDirectory + Key.settingsFileName, Encrypt.Shifrovka(dialog.FileName, Key.encryptionKeyInFile));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Ошибка подключения к базе данных");
                    }

                    if (UsAc != null)
                    {
                        if (!CheckDB(dialog.FileName))
                        {
                            MessageBoxResult resultEditDb = MessageBox.Show("Данная база данных не предназначена для работы с текущим приложением. Но её можно адаптировать, сделать это? (Внимание! Если данный база уже содержит таблицы, то это может вызвать критические ошибки в приложении!)", "Некорректный файл базы данных", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                            if (resultEditDb == MessageBoxResult.Yes)
                            {
                                //Добавление нужных таблиц
                                UsAc.ConnectOpen();
                                try
                                {
                                    UsAc.ExecuteNonQuery("Create Table Core(dbKey string Not Null primary key)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };

                                try
                                {
                                    UsAc.ExecuteNonQuery($"INSERT INTO Core (dbKey) VALUES ('{Key.dbKey}');");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };
                                try
                                {
                                    UsAc.ExecuteNonQuery("Create Table Polls(PollTableName string PRIMARY KEY, Title string NULL, Description string NULL, AuthorLogin string NULL, Ovners string NULL, DateOfCreation string NULL, MinUserLvl integer NULL, CODE MEMO NULL)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };
                                try
                                {
                                    UsAc.ExecuteNonQuery($"Create Table Result (PollTableName string NULL, Result MEMO NULL, DateOfPassage DateTime)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };
                                try
                                {
                                    UsAc.ExecuteNonQuery("Create Table [User] (FIO string NULL, Login string NOT Null primary key, [Password] string NULL, role integer NULL)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };

                                UsAc.ConnectClose();

                                //Сохранение пути в файле
                                File.WriteAllText(Environment.CurrentDirectory + Key.settingsFileName, Encrypt.Shifrovka(dialog.FileName, Key.encryptionKeyInFile));

                                //Создание учетной записи
                                SelectGrid(Grids.AccountCreate);
                                F_Account_LVL.Text = "0";
                            }
                            else
                            {
                                //Просто выйти из приложения
                                Environment.Exit(0);
                            }
                        }
                    }
                }
                else
                {
                    //Просто выйти из приложения
                    Environment.Exit(0);
                }

            }
            else if (dbPath != null)
            {
                //Если удалось найти файл БД, то проверка на корректность БД
                if (!CheckDB(dbPath))
                {
                    MessageBox.Show("Используемый файл базы данных не имеет корректных данных для работы с приложением. Перезапустите приложение, чтобы указать новый путь.");
                    File.Delete(Environment.CurrentDirectory + Key.settingsFileName);
                    Environment.Exit(0);
                }
                else if (UsAc.Execute("SELECT * FROM [User]").Count == 0)
                {
                    //Создание учетной записи
                    SelectGrid(Grids.AccountCreate);
                    F_Account_LVL.Text = "0";
                }
            }
        }

        /// <summary>
        /// Пробует подключиться к ближайщей БД, либо определить файл с параметрами и взять данные из него
        /// </summary>
        private void LoadingData()
        {
            //Имена и пути файлов, расположенных в каталоге с .EXE файлом приложения
            string[] allfiles = Directory.GetFiles(Environment.CurrentDirectory.ToString());

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
                            UserLogin = LineOfFile[1];
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
                DataView result = ChUsAc.Execute("SELECT * FROM Core, [User], Polls");

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

        private void SelectPoll(object sender, RoutedEventArgs e)
        {
            string s = ((Button)e.OriginalSource).Name.ToString();

            string tableName = s.Substring(1);
            string QuestionSTRING = UsAc.Execute($@"SELECT CODE From Polls where PollTableName = ""{tableName}""").Table.Rows[0]["CODE"].ToString();
            var z = QTM.StringToQuestionList(QuestionSTRING);

            StartPoling(z, Core, false);

            SelectGrid(Grids.PollPass);

            PollSelectedToPoling = tableName;
        }

        private void SelectPollToEdit(object sender, RoutedEventArgs e)
        {
            string s = ((Button)e.OriginalSource).Name.ToString();
            string tableName = s.Substring(1);
            //MessageBox.Show("527", tableName);
            var table = UsAc.Execute($@"SELECT * FROM Polls Where PollTableName = ""{tableName}""");

            F_ButtonDeletePoll.IsEnabled = true;

            EditPoll(table);
            SelectGrid(Grids.PollEditing);
        }

        private void Button_Click_CreatePoll(object sender, RoutedEventArgs e)
        {
            if (UserToleranceLvl >= 2)
            {
                MessageBox.Show("Данная возможность доступна только для сотрдуников");
                return;
            }

            SelectGrid(Grids.PollEditMenu);
        }

        private void Button_Click_PassResult(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.PollResultList);
        }

        private void Button_Click_PassPoll(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.PollList);
        }

        private void F_RectangleOnUser_MouseEnter(object sender, MouseEventArgs e)
        {
            F_RectangleOnUser.Fill = new SolidColorBrush(Color.FromArgb(50, 229, 229, 255));
        }

        private void F_RectangleOnUser_MouseLeave(object sender, MouseEventArgs e)
        {
            F_RectangleOnUser.Fill = new SolidColorBrush(Color.FromArgb(0, 229, 229, 255));
        }

        private void F_RectangleOnUser_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UserAuthrorization == false)
                SelectGrid(Grids.Authorization);
            else
                SelectGrid(Grids.AccountEdit);
        }

        /// <summary>
        /// Событие нажатии кнопки в TextBox Логина
        /// </summary>
        private void F_KeyUp_Login(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                F_Password.Focus();
                F_Password.SelectAll();
            }
        }

        /// <summary>
        /// Событие нажатии кнопки в PasswordBox пароля
        /// </summary>
        private void F_KeyUp_Password(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                AttemptEnter();
            }
        }

        private void AttemptEnter()
        {
            if (F_Login.Text.Length < 4)
            {
                F_Login.Focus();
                F_Login.SelectAll();
                return;
            }
            else if (F_Password.Password.Length < 4)
            {
                F_Password.Focus();
                F_Password.SelectAll();
                return;
            }

            if (CheckLogPas())
            {
                StreamWriter str = new StreamWriter(Environment.CurrentDirectory + Key.settingsFileName);
                str.WriteLine(Encrypt.Shifrovka(dbPath, Key.encryptionKeyInFile));
                str.WriteLine(F_Login.Text);
                str.Close();

                AutorizationUser(F_Login.Text);
                SelectGrid(Grids.MainWindow);

                F_Login.Clear();
                F_Password.Clear();
            }
            else
            {
                F_Password.Clear();
                F_Login.Focus();
                F_Login.SelectAll();
            }
        }

        private bool CheckLogPas()
        {
            //Поиск записи в БД
            var UserPassword = UsAc.Execute($@"SELECT Password FROM [User] WHERE Login = ""{F_Login.Text}""");
            if (UserPassword.Count == 0)
            {
                return false;
            }

            string Password;
            try
            {
                Password = Encrypt.DeShifrovka(UserPassword.Table.Rows[0]["Password"].ToString(), Key.encryptionKeyInDb);
            }
            catch
            {
                MessageBox.Show("Невозможно получить пароль для сравнения. Обратитесь к администратору.", "Ошибка дешифровки пароля");
                return false;
            }

            if (Password == F_Password.Password)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SelectGrid(Grids grid)
        {
            F_StackPanelPollList.Children.Clear();
            F_StackPanelPollListOnUser.Children.Clear();

            F_MainWindow.Visibility = Visibility.Hidden;
            F_Authorization.Visibility = Visibility.Hidden;
            F_AccountEdit.Visibility = Visibility.Hidden;
            F_PollList.Visibility = Visibility.Hidden;
            F_PollEditMainMenu.Visibility = Visibility.Hidden;
            F_PollEdit.Visibility = Visibility.Hidden;
            Core.Visibility = Visibility.Hidden;
            F_PollListResult.Visibility = Visibility.Hidden;

            PollSelectedToPoling = null;

            switch (grid)
            {
                case Grids.MainWindow:
                    F_MainWindow.Visibility = Visibility.Visible;
                    F_LargeText.Text = "Главное меню";
                    break;

                case Grids.Authorization:
                    F_Authorization.Visibility = Visibility.Visible;
                    F_LargeText.Text = "Авторизация";
                    break;

                case Grids.AccountEdit:
                    F_AccountEdit.Visibility = Visibility.Visible;
                    F_LargeText.Text = "Управление аккаунтом";

                    F_Account_NewPass.Clear();
                    F_Account_NewPass2.Clear();
                    F_Account_OldPass.Clear();

                    F_Account_Login.IsEnabled = false;
                    F_Account_OldPass.IsEnabled = true;
                    F_Account_SaveButton.IsEnabled = true;
                    F_Account_LogOut.IsEnabled = true;

                    F_Account_Login.Text = UserLogin;
                    F_Account_FIO.Text = UserFio;
                    F_Account_LVL.Text = UserToleranceLvl.ToString();

                    F_Account_ButtonChangePassword.Content = "Изменить";
                    break;

                case Grids.AccountCreate:
                    F_AccountEdit.Visibility = Visibility.Visible;
                    F_LargeText.Text = "Создание аккаунта";

                    F_Account_NewPass.Clear();
                    F_Account_NewPass2.Clear();
                    F_Account_OldPass.Clear();

                    F_Account_Login.IsEnabled = true;
                    F_Account_OldPass.IsEnabled = false;
                    F_Account_SaveButton.IsEnabled = false;
                    F_Account_LogOut.IsEnabled = false;

                    F_Account_Login.Clear();
                    F_Account_FIO.Clear();
                    F_Account_LVL.Text = "2";

                    F_Account_ButtonChangePassword.Content = "Создать";
                    break;

                case Grids.PollList:
                    CreatePollList();
                    F_LargeText.Text = "Список доступных опросов";
                    F_PollList.Visibility = Visibility.Visible;
                    break;

                case Grids.PollPass:
                    F_LargeText.Text = "Прохождение опроса";
                    Core.Visibility = Visibility.Visible;
                    break;

                case Grids.PollEditMenu:
                    F_LargeText.Text = "Управление опросами";
                    F_PollEditMainMenu.Visibility = Visibility.Visible;
                    CreatepollListToEdit();
                    break;

                case Grids.PollEditing:
                    F_LargeText.Text = "Редактирование опроса";
                    F_PollEdit.Visibility = Visibility.Visible;
                    break;

                case Grids.PollResultList:
                    CreatepollListToResult();
                    F_LargeText.Text = "Просмотр результатов";
                    F_PollListResult.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void EditPoll(DataView table)
        {
            F_PollEditing.Children.Clear();

            //Создание трех панелей
            var InfoOfPollStackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 0, 0, 8)
            };
            var RequiredInfoStackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 0, 0, 8),
                IsEnabled = false,
                Visibility = Visibility.Hidden,
                Height = 0,
            };
            var QuestionStackPanel = new StackPanel()
            {
                Margin = new Thickness(0, 0, 0, 8)
            };

            //Создание заголовков
            {
                var MainText1 = new TextBlock
                {
                    Text = "Информация об опросе",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(10, 106, 0)),
                    Margin = new Thickness(8, 0, 0, 0),
                };

                var MainText2 = new TextBlock
                {
                    Text = "Первичные данные, заполняемые пользователями",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(10, 106, 0)),
                    Margin = new Thickness(8, 0, 0, 0),
                };
                var MainText3 = new TextBlock
                {
                    Text = "Вопросы, на которые отвечает пользователь",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(10, 106, 0)),
                    Margin = new Thickness(8, 0, 0, 0),
                };

                var MainRectangle1 = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Fill = new SolidColorBrush(Color.FromArgb(45, 10, 106, 0)),
                    Margin = new Thickness(8, 0, 16, 4),
                    Height = 1,
                };

                var MainRectangle2 = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Fill = new SolidColorBrush(Color.FromArgb(45, 10, 106, 0)),
                    Margin = new Thickness(8, 0, 16, 4),
                    Height = 1,
                };

                var MainRectangle3 = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Fill = new SolidColorBrush(Color.FromArgb(45, 10, 106, 0)),
                    Margin = new Thickness(8, 0, 16, 4),
                    Height = 1,
                };


                InfoOfPollStackPanel.Children.Add(MainText1);
                InfoOfPollStackPanel.Children.Add(MainRectangle1);
                //RequiredInfoStackPanel.Children.Add(MainText2);
                //RequiredInfoStackPanel.Children.Add(MainRectangle2);
                QuestionStackPanel.Children.Add(MainText3);
                QuestionStackPanel.Children.Add(MainRectangle3);
            }

            //Создание полей ввода для информации об Опросе
            {
                var InfoOfPollStackPanelTitle = new Grid()
                {
                    Margin = new Thickness(0, 0, 0, 4)
                };
                var TextBlockTitle = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = "Название опроса",
                    Width = 140,
                    Height = 22,
                };
                var TextBoxTitle = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(114, 0, 0, 0),
                    MinWidth = 100,
                    // Height = 22,
                    //MaxWidth = 800,
                    TextWrapping = TextWrapping.Wrap,
                };

                InfoOfPollStackPanelTitle.Children.Add(TextBlockTitle);
                InfoOfPollStackPanelTitle.Children.Add(TextBoxTitle);
                InfoOfPollStackPanel.Children.Add(InfoOfPollStackPanelTitle);

                var InfoOfPollStackPanelDescription = new Grid()
                {
                    Margin = new Thickness(0, 0, 0, 4)
                };
                var TextBlockDescroption = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = "Описание опроса",
                    Width = 140,
                    Height = 22,
                };
                var TextBoxDescription = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(114, 0, 0, 0),
                    MinWidth = 100,
                    //Height = 22,
                    //MaxWidth = 800,
                    TextWrapping = new TextWrapping(),
                };

                InfoOfPollStackPanelDescription.Children.Add(TextBlockDescroption);
                InfoOfPollStackPanelDescription.Children.Add(TextBoxDescription);
                InfoOfPollStackPanel.Children.Add(InfoOfPollStackPanelDescription);

                var RadioBittonpollForEmployee = new CheckBox()
                {
                    Content = "Опрос доступен только для зарегистрированных сотрдуников",
                };

                InfoOfPollStackPanel.Children.Add(RadioBittonpollForEmployee);
            }

            //Создание полей ввода для информации об Пользоваетеле
            {
                var CheckBoxFIO = new CheckBox()
                {
                    Content = "ФИО пользователя",
                    Margin = new Thickness(0, 0, 0, 4),
                };

                var CheckBoxBirthday = new CheckBox()
                {
                    Content = "Дата рождения",
                    Margin = new Thickness(0, 0, 0, 4),
                };

                //RequiredInfoStackPanel.Children.Add(CheckBoxFIO);
                //RequiredInfoStackPanel.Children.Add(CheckBoxBirthday);
            }

            //Кнопка, добавляющая блок вопрос/ответ
            var ButtonAddFillResponce = new Button()
            {
                Background = null,
                Content = "Добавить блок вопрос/ответ",
                Width = 240,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 0, 4, 16),
            };
            ButtonAddFillResponce.Click += FF_Click_AddFillResponce;

            //Добавление всего этого добра в форму
            F_PollEditing.Children.Add(InfoOfPollStackPanel);
            F_PollEditing.Children.Add(RequiredInfoStackPanel);
            F_PollEditing.Children.Add(QuestionStackPanel);
            F_PollEditing.Children.Add(ButtonAddFillResponce);

            //Содержимое опросника
            if (table == null)
            {
                AddFullResponce((StackPanel)F_PollEditing.Children[2]);
                AddFullResponce((StackPanel)F_PollEditing.Children[2]);

                PollSelectedToEdit = null;
            }
            else
            {
                PollSelectedToEdit = table.Table.Rows[0]["PollTableName"].ToString();

                //Первичные данные Название, Описание, ЮзерЛВЛ
                string Name = table.Table.Rows[0]["Title"].ToString();
                string Descript = table.Table.Rows[0]["Description"].ToString();

                ((TextBox)((Grid)((StackPanel)F_PollEditing.Children[0]).Children[2]).Children[1]).Text = Name;
                ((TextBox)((Grid)((StackPanel)F_PollEditing.Children[0]).Children[3]).Children[1]).Text = Descript;

                bool check = true;
                if (table.Table.Rows[0]["MinUserLvl"].ToString() == "2")
                    check = false;
                ((CheckBox)((StackPanel)F_PollEditing.Children[0]).Children[4]).IsChecked = check;


                string QuestionTEXT = table.Table.Rows[0]["CODE"].ToString();

                List<Question> QuestionList = QTM.StringToQuestionList(QuestionTEXT);
                for (int i = 0; i < QuestionList.Count; i++)
                {
                    //Создание вопроса
                    AddFullResponce((StackPanel)F_PollEditing.Children[2]);

                    //Текст вопроса
                    ((TextBox)((Grid)((StackPanel)F_PollEditing.Children[2]).Children[i + 2]).Children[1]).Text = QuestionList[i].QuestionText;

                    //Добавление вариантов ответа
                    foreach (Answer answer in QuestionList[i].AnswerList)
                    {
                        var TextBoxx = new TextBox()
                        {
                            Text = answer.Text,
                            Margin = new Thickness(0, 0, 0, 4),
                            MinWidth = 60,
                            HorizontalAlignment = HorizontalAlignment.Left,
                        };
                        ((StackPanel)((Grid)((StackPanel)F_PollEditing.Children[2]).Children[i + 2]).Children[3]).Children.Add(TextBoxx);
                    }
                }
            }
        }

        private void Button_Click_SavePoll(object sender, RoutedEventArgs e)
        {
            var rand = new Random();

            bool NewPoll = false;
            string PollTableName = null;
            if (PollSelectedToEdit == null)
            {
                NewPoll = true;
                int ChisloPoputok = 0;
                while (ChisloPoputok < 100)
                {
                    PollTableName = rand.Next(1, 60000).ToString();

                    ChisloPoputok++;
                    var result = UsAc.Execute($@"SELECT PollTableName From Polls where PollTableName = ""{PollTableName}""");

                    if (result.Count == 0)
                        break;
                }
            }
            else
            {
                PollTableName = PollSelectedToEdit;
            }

            string Title = ((TextBox)((Grid)((StackPanel)F_PollEditing.Children[0]).Children[2]).Children[1]).Text;

            if (Title.Length < 4)
            {
                MessageBox.Show("Текущее название опроса слишком короткое");
                return;
            }

            List<Question> QuestionList = new List<Question>();
            //Создание массива с вопросами
            {
                int QuestCount = ((StackPanel)F_PollEditing.Children[2]).Children.Count - 2;

                if (QuestCount == 0)
                {
                    MessageBox.Show("Нельзя сохранить опрос, в котором нет вопросов");
                    return;
                }

                for (int i = 0; i < QuestCount; i++)
                {
                    bool SuccessAdd = true;
                    var Quest = (Grid)((StackPanel)F_PollEditing.Children[2]).Children[i + 2];

                    string QuestText = ((TextBox)Quest.Children[1]).Text;

                    if (QuestText.Length < 4)
                        if (MessageBox.Show("В одном из блоков опроса отсуствует вопрос, пропустить его и продолжить?", "Отсуствие текста вопроса", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                            continue;
                        else
                            return;


                    int AnswerCount = (((StackPanel)Quest.Children[3]).Children.Count);

                    if (AnswerCount < 2)
                        if (MessageBox.Show($@"Для вопроса ""{QuestText}"" указано мало ответов (меньше 2), пропустить его и продолжить?", "Отсуствие вариантов ответа", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                            continue;
                        else
                            return;

                    var AnswerList = new List<string>();
                    for (int z = 0; z < AnswerCount; z++)
                    {
                        string answer = ((TextBox)((StackPanel)Quest.Children[3]).Children[z]).Text;
                        if (answer.Length == 0)
                            if (MessageBox.Show($@"Для вопроса ""{QuestText}"" один из ответов пустой, пропустить его и продолжить?", "Пустой вариант ответа", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                                SuccessAdd = false;
                            else
                                return;

                        AnswerList.Add(answer);
                    }

                    if (SuccessAdd)
                        QuestionList.Add(new Question(QuestText, AnswerList));
                }
            }

            //Перегонка массива в текст
            string QuestionListOnString = QTM.QuestionListToString(QuestionList);

            string Description = ((TextBox)((Grid)((StackPanel)F_PollEditing.Children[0]).Children[3]).Children[1]).Text;
            int MinUserLvl = 2;
            if ((bool)((CheckBox)((StackPanel)F_PollEditing.Children[0]).Children[4]).IsChecked)
            {
                MinUserLvl = 1;
            }

            if (NewPoll)
            {
                UsAc.ExecuteNonQuery($@"INSERT INTO Polls (PollTableName, Title, Description, AuthorLogin, DateOfCreation, MinUserLvl, CODE) VALUES (""{PollTableName}"", ""{Title}"",""{Description}"", ""{UserLogin}"", ""{DateTime.Now.ToShortDateString()}"",{MinUserLvl} , ""{QuestionListOnString}"")");
            }
            else
            {
                UsAc.ExecuteNonQuery($@"UPDATE Polls SET Title=""{Title}"", Description=""{Description}"", MinUserLvl={MinUserLvl}, CODE=""{QuestionListOnString}"" WHERE PollTableName = ""{PollTableName}""");
            }

            SelectGrid(Grids.PollEditMenu);
        }

        private void FF_Click_AddResponce(object sender, RoutedEventArgs e)
        {
            StackPanel SelectGrid = (StackPanel)((Grid)((Button)e.OriginalSource).Parent).Children[3];

            var TextBoxx = new TextBox()
            {
                Margin = new Thickness(0, 0, 0, 4),
                MinWidth = 60,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            SelectGrid.Children.Add(TextBoxx);
        }

        private void FF_Click_DeleteResponce(object sender, RoutedEventArgs e)
        {
            StackPanel SelectGrid = (StackPanel)((Grid)((Button)e.OriginalSource).Parent).Children[3];

            if (SelectGrid.Children.Count == 0)
            {
                return;
            }

            SelectGrid.Children.Remove(SelectGrid.Children[SelectGrid.Children.Count - 1]);
        }

        //Вызов кнопкой добавления куска опроса
        private void FF_Click_AddFillResponce(object sender, RoutedEventArgs e)
        {
            AddFullResponce((StackPanel)F_PollEditing.Children[2]);
        }

        //Добавление куска опроса (Вопрос, ответы)
        private void AddFullResponce(StackPanel stackPanel)
        {
            //Каркас
            var QuestionGrid = new Grid()
            {
                //Background = new SolidColorBrush(Color.FromArgb(50, 82, 145, 82)),
                Margin = new Thickness(8),
                Name = "Grid_" + stackPanel.Children.Count.ToString(),
            };

            //Задний фон
            var BackRectangle = new Rectangle()
            {
                Stroke = Brushes.Black,
                RadiusY = 4,
                RadiusX = 4,
            };

            //Подсказка "Вопрос:"
            var QuestionTextBlock = new TextBlock()
            {
                Text = "Вопрос:",
                FontSize = 14,
                Margin = new Thickness(8, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            //Ввод вопроса
            var QuestionTextBox = new TextBox()
            {
                MinWidth = 60,
                FontSize = 14,
                Margin = new Thickness(75, 2, 128, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            //Подсказка "Ответы:"
            var AnswerTextBlock = new TextBlock()
            {
                Text = "Ответы:",
                FontSize = 14,
                Margin = new Thickness(8, 40, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            //Кнопка удаление данного вопросника
            var ButtonDeleteThis = new Button()
            {
                Content = "Удалить вопрос",
                Background = null,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 2, 2, 0),
                Name = "ButtonInGrid_" + stackPanel.Children.Count.ToString(),
            };
            ButtonDeleteThis.Click += FF_Click_DeleteFillResponce;

            //Содержит варианты ответов
            var StackQuestionInGrid = new StackPanel()
            {
                Margin = new Thickness(75, 40, 4, 40),
                Name = "StackInGrid_" + stackPanel.Children.Count.ToString(),
                MinHeight = 26,
            };

            //Кнопка добавления варианта ответа
            var ButtonAddQuestion = new Button()
            {
                Content = "Добавить",
                Background = null,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 0, 4, 4),
                Name = "AddButtonInGrid_" + stackPanel.Children.Count.ToString(),
            };
            ButtonAddQuestion.Click += FF_Click_AddResponce;

            //Кнопка удаления варианта ответа
            var ButtonDeleteQuestion = new Button()
            {
                Content = "Удалить",
                Background = null,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(128, 0, 4, 4),
                Name = "DeleteButtonInGrid_" + stackPanel.Children.Count.ToString(),
            };
            ButtonDeleteQuestion.Click += FF_Click_DeleteResponce;

            //Скрещивание всего ужаса
            QuestionGrid.Children.Add(QuestionTextBlock);
            QuestionGrid.Children.Add(QuestionTextBox);
            QuestionGrid.Children.Add(ButtonDeleteThis);
            QuestionGrid.Children.Add(StackQuestionInGrid);
            QuestionGrid.Children.Add(ButtonAddQuestion);
            QuestionGrid.Children.Add(ButtonDeleteQuestion);
            QuestionGrid.Children.Add(AnswerTextBlock);
            QuestionGrid.Children.Add(BackRectangle);

            stackPanel.Children.Add(QuestionGrid);
        }

        //Вызов кнопкой удаления куска опроса (Вопрос с ответами)
        private void FF_Click_DeleteFillResponce(object sender, RoutedEventArgs e)
        {
            ((StackPanel)F_PollEditing.Children[2]).Children.Remove((UIElement)((Button)e.OriginalSource).Parent);
        }

        private void F_ButtonClick_CreateNewUser(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.AccountCreate);
        }

        private void CreatepollListToResult()
        {
            F_StackPanelPollListOnResult.Children.Clear();

            DataView Find_Polls = UsAc.Execute($"SELECT Polls.PollTableName, Polls.Title, Polls.Description, User.FIO, Polls.DateOfCreation FROM Polls LEFT JOIN[User] ON Polls.AuthorLogin = User.Login");

            if (Find_Polls.Count == 0)
            {
                return;
            }

            DataTable Polls = Find_Polls.Table;
            for (int i = 0; i < Find_Polls.Count; i++)
            {
                var newStac = new StackPanel
                {
                    Margin = new Thickness(4, 4, 4, 16),
                };

                var Title = new TextBlock
                {
                    Text = Polls.Rows[i]["Title"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                };
                newStac.Children.Add(Title);

                if (Polls.Rows[i]["Description"].ToString() != "")
                {
                    var Description = new TextBlock
                    {
                        Text = Polls.Rows[i]["Description"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Description);
                }

                if (Polls.Rows[i]["FIO"].ToString() != "")
                {
                    var Author = new TextBlock
                    {
                        Text = "Автор: " + Polls.Rows[i]["FIO"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Author);
                }

                var Date = new TextBlock
                {
                    Text = "Дата создания: " + Polls.Rows[i]["DateOfCreation"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                };
                newStac.Children.Add(Date);

                var Gridd = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                };

                var newButton = new Button
                {
                    Content = "Просмотреть результаты",
                    Margin = new Thickness(0, 4, 0, 4),
                    Name = "_" + Polls.Rows[i]["PollTableName"].ToString(),
                    Background = null,
                    Width = 160,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                //newButton.Click += SelectPollToEdit;

                var newButton2 = new Button
                {
                    Content = "Вывести результаты в Excell",
                    Margin = new Thickness(164, 4, 0, 4),
                    Name = "_" + Polls.Rows[i]["PollTableName"].ToString(),
                    Background = null,
                    Width = 160,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                newButton2.Click += ResultOutToExcell;

                Gridd.Children.Add(newButton);
                Gridd.Children.Add(newButton2);

                newStac.Children.Add(Gridd);

                F_StackPanelPollListOnResult.Children.Add(newStac);
            }
        }

        private void ResultOutToExcell(object sender, RoutedEventArgs e)
        {
            //Получение имени опроса для Excell
            string Title = ((TextBlock)((StackPanel)((Grid)((Button)e.OriginalSource).Parent).Parent).Children[0]).Text;

            //Получение кодового имени таблицы
            string tableName = ((Button)e.OriginalSource).Name.ToString().Substring(1);

            //Получение содержимого опроса из таблицы
            string STRINGQUESTION = UsAc.Execute($@"Select CODE from Polls where PollTableName = ""{tableName}""").Table.Rows[0]["CODE"].ToString();
            List<Question> QuestionList = QTM.StringToQuestionList(STRINGQUESTION);

            //Получение списка результатов
            var tableOfResult = UsAc.Execute($@"SELECT DateOfPassage, Result FROM Result Where PollTableName = ""{tableName}""");
            var tableResult = tableOfResult.Table;

            //Получение заголовков
            List<string> QuestionText = new List<string>();
            QuestionText.Add("Отметка времени");
            foreach (Question ans in QuestionList)
            {
                bool ShowError = true;
                try
                {
                    QuestionText.Add(ans.QuestionText);
                }
                catch (Exception ex)
                {
                    if (ShowError)
                        MessageBox.Show("Ошибка, некоторые результаты не сходятся с опросом \n\r\n\r" + ex.ToString(), "Ошибка");

                    ShowError = false;
                    continue;
                }

            }
            var Excell = new CourseProject.Modules.ExcellOut(Title, QuestionText);

            for (int i = 0; i < tableOfResult.Count; i++)
            {
                List<string> AnswerText = new List<string>();
                AnswerText.Add(tableResult.Rows[i]["DateOfPassage"].ToString());
                List<Question> AnswerTextList;
                try
                {
                    AnswerTextList = QTM.StringResultToResultToQuestionList(QuestionList, tableOfResult.Table.Rows[i]["Result"].ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Критическая ошибка");
                    if (UserToleranceLvl < 2)
                    {
                        if (MessageBox.Show("Ошибка просмотра результатов. Для исправления данной ошибки рекомендуется удалить все прошлые результаты по данному опросу, сделать это?", "Ошибка вывода результатов", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            UsAc.ExecuteNonQuery($@"DELETE FROM Result Where PollTableName = ""{tableName}""");
                        }
                        return;
                    }
                    return;
                }
                foreach (Question aquest in AnswerTextList)
                {
                    bool ShowError = true;
                    try
                    {
                        AnswerText.Add(aquest.selectAnswerText);
                    }
                    catch (Exception ex)
                    {
                        if (ShowError)
                            MessageBox.Show("Ошибка, некоторые результаты не сходятся с опросом \n\r\n\r" + ex.ToString(), "Ошибка");

                        ShowError = false;
                        continue;
                    }
                }
                Excell.AddLine(AnswerText);
            }
            try
            {
                Excell.OutToExcell();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка вывода результатов в Excell");
            }

        }

        private void CreatepollListToEdit()
        {
            F_StackPanelPollListOnUser.Children.Clear();

            DataView Find_Polls;
            if (UserToleranceLvl == 0)
            {
                Find_Polls = UsAc.Execute($"SELECT Polls.PollTableName, Polls.Title, Polls.Description, User.FIO, Polls.DateOfCreation FROM Polls LEFT JOIN[User] ON Polls.AuthorLogin = User.Login");
            }
            else
            {
                Find_Polls = UsAc.Execute($@"SELECT Polls.PollTableName, Polls.Title, Polls.Description, User.FIO, Polls.DateOfCreation FROM Polls LEFT JOIN[User] ON Polls.AuthorLogin = User.Login Where Polls.AuthorLogin = ""{UserLogin}""");
            }

            if (Find_Polls.Count == 0)
            {
                return;
            }

            DataTable Polls = Find_Polls.Table;
            for (int i = 0; i < Find_Polls.Count; i++)
            {
                var newStac = new StackPanel
                {
                    Margin = new Thickness(4, 4, 4, 16),
                };

                var Title = new TextBlock
                {
                    Text = Polls.Rows[i]["Title"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                };
                newStac.Children.Add(Title);

                if (Polls.Rows[i]["Description"].ToString() != "")
                {
                    var Description = new TextBlock
                    {
                        Text = Polls.Rows[i]["Description"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Description);
                }

                if (Polls.Rows[i]["FIO"].ToString() != "")
                {
                    var Author = new TextBlock
                    {
                        Text = "Автор: " + Polls.Rows[i]["FIO"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Author);
                }

                var Date = new TextBlock
                {
                    Text = "Дата создания: " + Polls.Rows[i]["DateOfCreation"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                };
                newStac.Children.Add(Date);

                var newButton = new Button
                {
                    Content = "Редактировать опрос",
                    Margin = new Thickness(0, 4, 0, 4),
                    Name = "_" + Polls.Rows[i]["PollTableName"].ToString(),
                    Background = null,
                    Width = 160,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                newButton.Click += SelectPollToEdit;
                newStac.Children.Add(newButton);

                F_StackPanelPollListOnUser.Children.Add(newStac);
            }
        }

        private void CreatePollList()
        {
            DataView Find_Polls = UsAc.Execute($"SELECT Polls.PollTableName, Polls.Title, Polls.Description, User.FIO, Polls.DateOfCreation FROM Polls LEFT JOIN[User] ON Polls.AuthorLogin = User.Login Where Polls.MinUserLvl >= {UserToleranceLvl}");
            if (Find_Polls.Count == 0)
            {
                MessageBox.Show("Опросов для прохождения нет");
                SelectGrid(Grids.MainWindow);
            }
            DataTable Polls = Find_Polls.Table;

            F_StackPanelPollList.Children.Clear();
            for (int i = 0; i < Find_Polls.Count; i++)
            {
                var newStac = new StackPanel
                {
                    Margin = new Thickness(4, 4, 4, 16),
                };

                var Title = new TextBlock
                {
                    Text = Polls.Rows[i]["Title"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                };
                newStac.Children.Add(Title);

                if (Polls.Rows[i]["Description"].ToString() != "")
                {
                    var Description = new TextBlock
                    {
                        Text = Polls.Rows[i]["Description"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Description);
                }

                if (Polls.Rows[i]["FIO"].ToString() != "")
                {
                    var Author = new TextBlock
                    {
                        Text = "Автор: " + Polls.Rows[i]["FIO"].ToString(),
                        Margin = new Thickness(0, 0, 0, 4),
                    };
                    newStac.Children.Add(Author);
                }

                var Date = new TextBlock
                {
                    Text = "Дата создания: " + Polls.Rows[i]["DateOfCreation"].ToString(),
                    Margin = new Thickness(0, 0, 0, 4),
                };
                newStac.Children.Add(Date);

                var newButton = new Button
                {
                    Content = "Пройти опрос",
                    Margin = new Thickness(0, 4, 0, 4),
                    Name = "_" + Polls.Rows[i]["PollTableName"].ToString(),
                    Background = null,
                    Width = 160,
                    HorizontalAlignment = HorizontalAlignment.Left,
                };
                newButton.Click += SelectPoll;
                newStac.Children.Add(newButton);

                F_StackPanelPollList.Children.Add(newStac);
            }
        }

        private void F_Button_ClickToEnter(object sender, RoutedEventArgs e)
        {
            AttemptEnter();
        }

        private void F_Button_ClickToEnterAtUser(object sender, RoutedEventArgs e)
        {
            AutorizationUser(null);
            F_Login.Clear();
            F_Password.Clear();
            SelectGrid(Grids.MainWindow);
        }

        private void AutorizationUser(string Login)
        {
            if (Login == null)
            {
                UserLogin = null;
                UserFio = null;
                UserAuthrorization = false;
                F_UserName.Text = "Гость";
                UserToleranceLvl = 2;
            }
            else
            {
                var ResultOfUser = UsAc.Execute($@"SELECT FIO, Role From [User] Where Login = ""{Login}""");
                UserLogin = Login;
                UserFio = ResultOfUser.Table.Rows[0]["FIO"].ToString();
                UserAuthrorization = true;
                F_UserName.Text = UserFio;
                UserToleranceLvl = Convert.ToInt32(ResultOfUser.Table.Rows[0]["Role"].ToString());
            }

            switch (UserToleranceLvl)
            {
                case 0:
                    F_UserLogo.Source = new BitmapImage(new Uri("Resources/Image/Admin.png", UriKind.Relative));
                    break;
                case 1:
                    F_UserLogo.Source = new BitmapImage(new Uri("Resources/Image/employee.png", UriKind.Relative));
                    break;
                case 2:
                    F_UserLogo.Source = new BitmapImage(new Uri("Resources/Image/User.png", UriKind.Relative));
                    break;
            }
        }

        private void F_ButtonLogOut_Click(object sender, RoutedEventArgs e)
        {
            AutorizationUser(null);
            SelectGrid(Grids.Authorization);
        }

        private void F_ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            if (UserAuthrorization)
            {
                //Поиск записи в БД
                var UserPassword = UsAc.Execute($@"SELECT Password FROM [User] WHERE Login = ""{UserLogin}""");
                if (UserPassword.Count == 0)
                {
                    UserPassword = UsAc.Execute($@"SELECT Password FROM [User] WHERE Login = ""{F_Account_Login.Text}""");
                    if (UserPassword.Count == 0)
                    {
                        MessageBox.Show("Ошибка, пользователь с таким логином не найден");
                        return;
                    }
                }

                //Дешифровка пароля
                string Password;
                try
                {
                    Password = Encrypt.DeShifrovka(UserPassword.Table.Rows[0]["Password"].ToString(), Key.encryptionKeyInDb);
                }
                catch
                {
                    MessageBox.Show("Невозможно получить пароль для сравнения. Обратитесь к администратору.", "Ошибка дешифровки пароля");
                    return;
                }

                //Проверка паролей
                if (Password == F_Account_OldPass.Password)
                {
                    if (F_Account_NewPass.Password.Length < 4 || F_Account_NewPass2.Password.Length < 4 || F_Account_OldPass.Password.Length < 4)
                    {
                        MessageBox.Show("Пароль должен состоять как минимум из 4 символов");
                        return;
                    }
                    else if (F_Account_NewPass.Password != F_Account_NewPass2.Password)
                    {
                        MessageBox.Show("Пароли не совпадают");
                        return;
                    }
                    else if (F_Account_OldPass.Password == F_Account_NewPass.Password)
                    {
                        MessageBox.Show("Старый и новый пароль совпадают");
                        return;
                    }

                    try
                    {
                        string OldPassword = Encrypt.Shifrovka(F_Account_OldPass.Password, Key.encryptionKeyInDb);
                        string NewPassword = Encrypt.Shifrovka(F_Account_NewPass.Password, Key.encryptionKeyInDb);
                        UsAc.ExecuteNonQuery($@"UPDATE [User] SET [User].Password = ""{NewPassword}"" WHERE [User].Login = ""{F_Account_Login.Text}"" and [User].Password = ""{OldPassword}""");
                        MessageBox.Show("Пароль был успешно изменен");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Ошибка изменения пароля");
                        return;
                    }

                    F_Account_OldPass.Password = null;
                    F_Account_NewPass.Password = null;
                    F_Account_NewPass2.Password = null;
                }
                else
                {
                    MessageBox.Show("Старый пароль введен неверно");
                    return;
                }
            }
            else
            {
                if (F_Account_Login.Text.Length < 4)
                {
                    MessageBox.Show("Логин должен состоять из 4 и более символов");
                    return;
                }
                else if (F_Account_FIO.Text.Length < 3)
                {
                    MessageBox.Show("ФИО слишком короткое");
                    return;
                }
                if (F_Account_NewPass.Password.Length < 4 || F_Account_NewPass2.Password.Length < 4)
                {
                    MessageBox.Show("Пароль должен состоять как минимум из 4 символов");
                    return;
                }
                else if (F_Account_NewPass.Password != F_Account_NewPass2.Password)
                {
                    MessageBox.Show("Пароли не совпадают");
                    return;
                }

                if (UsAc.Execute($@"SELECT Login FROM [User] Where Login = ""{F_Account_Login.Text}""").Count != 0)
                {
                    MessageBox.Show("Данный логин уже используется, введите другой");
                    return;
                }

                try
                {
                    string password = Encrypt.Shifrovka(F_Account_NewPass.Password, Key.encryptionKeyInDb);
                    UsAc.ExecuteNonQuery($@"INSERT INTO [User] ([FIO], [Login], [Password], [Role]) VALUES (""{F_Account_FIO.Text}"", ""{F_Account_Login.Text}"", ""{password}"" , {F_Account_LVL.Text})");
                    MessageBox.Show("Запись успешно создана!");
                    AutorizationUser(F_Account_Login.Text);
                    SelectGrid(Grids.MainWindow);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка создания новой учетной записи");
                }
            }
        }

        private void Button_Click_DeletePoll(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите удалить данный опрос? (Вместе с ним удаляться и результаты)", "Удаление опроса", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                UsAc.ExecuteNonQuery($@"DELETE FROM Polls Where PollTableName = ""{PollSelectedToEdit}""");
                UsAc.ExecuteNonQuery($@"DELETE FROM Result Where PollTableName = ""{PollSelectedToEdit}""");

                SelectGrid(Grids.PollEditMenu);
            }
        }

        private void F_CreateNewPoll(object sender, RoutedEventArgs e)
        {
            EditPoll(null);
            SelectGrid(Grids.PollEditing);

            F_ButtonDeletePoll.IsEnabled = false;
        }

        private void F_Account_GoToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.MainWindow);
        }

        private void F_Account_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserFio == F_Account_FIO.Text)
            {
                return;
            }

            try
            {
                UsAc.ExecuteNonQuery($@"UPDATE [User] SET [User].FIO = ""{F_Account_FIO.Text}"" WHERE [User].Login = ""{F_Account_Login.Text}""");
                MessageBox.Show("ФИО было обновлено");
                AutorizationUser(F_Account_Login.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка изменения ФИО");
            }
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
