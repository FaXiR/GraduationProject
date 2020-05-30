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
    public partial class MainWindow : System.Windows.Window
    {
        private enum Grids
        {
            MainWindow,
            Authorization,
            AccountEdit,
            AccountCreate,
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
            //StartPoling(QList, Core, false);
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
                                    UsAc.ExecuteNonQuery("Create Table Polls(PollTableName string NULL, Title string NULL, Description string NULL, AuthorLogin string NULL, Ovners string NULL, DateOfCreation string NULL)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };
                                try
                                {
                                    UsAc.ExecuteNonQuery("Create Table [User] (FIO string NULL, Login string NOT Null primary key, [Password] string NULL, role integer NULL)");
                                }
                                catch (Exception ex) { MessageBox.Show(ex.ToString()); };

                                UsAc.ConnectClose();

                                //Сохранение пути в файле
                                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName, Encrypt.Shifrovka(dialog.FileName, Key.encryptionKeyInFile));

                                //Создание учетной записи
                                SelectGrid(Grids.AccountCreate);
                                F_Account_LVL.Text = "0";
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
                    MessageBox.Show("Используемый файл базы данных не имеет корректных данных для работы с приложением. Перезапустите приложение, чтобы указать новый путь.");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName);
                    Environment.Exit(1);
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

        private void Button_Click_CreatePoll(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_PassPoll(object sender, RoutedEventArgs e)
        {

        }

        private void F_RectangleOnUser_MouseEnter(object sender, MouseEventArgs e)
        {
            F_RectangleOnUser.Fill = new SolidColorBrush(Color.FromArgb(125, 229, 229, 255));
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
                StreamWriter str = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + Key.settingsFileName);
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
            F_MainWindow.Visibility = Visibility.Hidden;
            F_Authorization.Visibility = Visibility.Hidden;
            F_AccountEdit.Visibility = Visibility.Hidden;

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
                    F_Account_GoToMainWindow.IsEnabled = true;
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
                    F_Account_GoToMainWindow.IsEnabled = false;
                    F_Account_SaveButton.IsEnabled = false;
                    F_Account_LogOut.IsEnabled = false;

                    F_Account_Login.Clear();
                    F_Account_FIO.Clear();
                    F_Account_LVL.Text = "2";

                    F_Account_ButtonChangePassword.Content = "Создать";
                    break;
            }
        }

        private void F_ButtonClick_CreateNewUser(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.AccountCreate);
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
            SelectGrid(Grids.MainWindow);
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

        private void F_Account_GoToMainWindow_Click(object sender, RoutedEventArgs e)
        {
            SelectGrid(Grids.MainWindow);
        }

        private void F_Account_SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
