using System;
using System.Collections.Generic;
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
using static ЭКЗ_по_Form.MainWindow;

namespace ЭКЗ_по_Form
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private const int BoardSize = 8; //размер доски (8x8 клеток).
        private Button[,] buttons = new Button[BoardSize, BoardSize]; // массив кнопок, представляющих клетки доски.
        private Checker[,] board = new Checker[BoardSize, BoardSize]; //массив шашек, хранящих информацию о том, какая шашка находится на каждой клетке.
        private int wins = 0; // Победы
        private int losses = 0; // Поражения
        private bool isPlayer1Turn = true;  //bool значение чей ход сейчас

        private int? selectedRow = null; // строка
        private int? selectedCol = null; // столбец
        //int?, который является «обнуляемым» типом. переменная может хранить либо целочисленное значение, либо nul
        //переменные хранят координаты выбранной шашки. Они могут быть null, что позволяет определить, выбрана ли шашка.
        private bool isAgainstComputer = false; // с компьютером или с игроком если True- с игроком (Флаг, указывающий, играют ли против компьютера или друг против друга.)
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing; //событие закрытия окна (экстренного) 
            CreateGameBoard(); //создание доски

        }
        //Запуск с 2 игроками
        private void StartTwoPlayers_Click(object sender, RoutedEventArgs e)
        {
            isAgainstComputer = false;
            StartGame();
        }
        //Запуск игры с компьютером
        private void StartWithComputer_Click(object sender, RoutedEventArgs e)
        {
            isAgainstComputer = true;//
            isPlayer1Turn = false; // Установить, чтобы первый ход был у компьютера
            StartGame(); // Запуск игры
            ComputerMove(); // Выполнить первый ход компьютера
        }
        // для продолжения игры
        private void ContinueSavedGame_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("gamestate.txt"))
            {
                LoadGameState();
                StartGame();
            }
            else
            {
                MessageBox.Show("Нет сохранённой игры для продолжения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // выход
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // класс для обработки сообщений и ролучении сведений о приложении
        }

        private void StartGame()
        {
            wins = 0;//// Сброс количества побед
            losses = 0;//// Сброс количества поражений
            MessageBox.Show("Игра началась");
            CreateGameBoard();// загрузка доски
        }

        private void StartGameContinue()
        {
            wins = 0;
            losses = 0;
            MessageBox.Show("Игра началась");
            CreateGameBoard();// загрузка доски
        }

        private void CreateGameBoard() // создание игровой доски
        {
            GameBoard.Children.Clear(); // Очистка предыдущего игрового поля для новой игры
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    buttons[row, col] = new Button();
                    buttons[row, col].Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.Red; // Чередование цветов клеток
                    buttons[row, col].Margin = new Thickness(0);
                    buttons[row, col].Height = 50;
                    buttons[row, col].Width = 50;

                    if (row < 3 && (row + col) % 2 != 0) // Шашки первого игрока
                    {
                        buttons[row, col].Content = "B"; // B - для черных шашек
                        board[row, col] = new Checker { Player = Player.Player1 };
                    }
                    else if (row > 4 && (row + col) % 2 != 0) // Шашки второго игрока
                    {
                        buttons[row, col].Content = "W"; // W - для белых шашек
                        board[row, col] = new Checker { Player = Player.Player2 };
                    }
                    else
                    {
                        buttons[row, col].Content = null; // пустое поле
                        board[row, col] = new Checker();
                    }

                    buttons[row, col].Tag = new Tuple<int, int>(row, col); // Хранение координат кнопки
                    buttons[row, col].Click += Button_Click; // Обработчик кликов
                    GameBoard.Children.Add(buttons[row, col]); // Добавление кнопок на доску
                }
            }
        }

        //Обработчик события нажатия на кнопку (клетку доски).
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            var position = (Tuple<int, int>)clickedButton.Tag; //clickedButton.Tag — это свойство хранения данных
            //Результат  сохраняется в переменной position. Если в Tag действительно находился объект типа Tuple<int, int>, то position будет ссылкой на этот объект
            int row = position.Item1;
            int col = position.Item2;

            // Проверка на снятие выбора шашки
            if (selectedRow != null && selectedCol != null && selectedRow == row && selectedCol == col)
            {
                // Снятие выделения
                clickedButton.Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.CadetBlue; // Возврат исходного цвета
                                                                                                     //использует операцию остатка от деления. Оно определяет, четная или нечетная сумма координат строки и столбца.
                                                                                                     //  сумма четная, то фон кнопки устанавливается в Brushes.White.
                                                                                                     //  сумма нечетная, то фон устанавливается в Brushes.CadetBlue
                selectedRow = null;
                selectedCol = null;
                return; // Завершение метода
               // Если ранее ничего не выбрано(selectedRow == null), то это первый клик пользователя.
                //Проверяется, что на выбранной клетке есть шашка, принадлежащая текущему игроку.
                //Если условия выполнены, устанавливаются значения selectedRow и selectedCol, указывая, что шашка выбрана, и показывается сообщение о том, какая шашка выбрана.


      }

            if (selectedRow == null)
            {
                if (board[row, col].Player != Player.Empty && (board[row, col].Player == (isPlayer1Turn ? Player.Player1 : Player.Player2)))
                {
                    //board[row, col].Player != Player.Empty:  условие проверяет, что  действующий игрок на данной позиции в массиве board не пустой, значит ячейка уже занята игроком.
                    //(board[row, col].Player == (isPlayer1Turn ? Player.Player1 : Player.Player2))) оператор для определения, является ли текущий игрок тем игроком, который находится на позиции row, col.
                    //Указание, что ячейка выбрана:
                    selectedRow = row;
                    selectedCol = col;
                    clickedButton.Background = Brushes.Gray; // Подсветка выбранной шашки
                }
            }
            else
            {
                if (MakeMove((int)selectedRow, (int)selectedCol, row, col))
                {
                    clickedButton.Content = buttons[selectedRow.Value, selectedCol.Value].Content;
                    // строка копирует содержимое кнопки, которая находится на позиции [selectedRow.Value, selectedCol.Value] в  buttons, и присваивает его Content для  кнопки, на которую был произведен клик (clickedButton).
                    //  фигура,  находившаяся в выделенной ячейке(по координатам selectedRow и selectedCol), ljl;yf переместится на новую выбранную позицию 
                    buttons[selectedRow.Value, selectedCol.Value].Content = null; // содержимое предыдущей ячейки становится нулём для очистки
                    clickedButton.Background = (row + col) % 2 == 0 ? Brushes.White : Brushes.CadetBlue; // Вернуть исходный цвет
                    isPlayer1Turn = !isPlayer1Turn; // Смена хода
                    selectedRow = null;
                    selectedCol = null;

                    if (isAgainstComputer && !isPlayer1Turn)
                    {
                        ComputerMove();
                    }
                }
                else
                {
                    MessageBox.Show("Неверный ход!");
                }
            }
        }

        private void PlayerMove(int fromRow, int fromCol, int toRow, int toCol)//ход игрока
        {
            //   логика хода игрока
            if (isPlayer1Turn) // Проверка, что ходит первый игрок
            {
                MakeMove(fromRow, fromCol, toRow, toCol); // выполнение хода игрока
                isPlayer1Turn = false; // После хода игрока  ход компьютера

                // Проверка на наличие возможных ходов у компьютера
                if (isAgainstComputer)
                {
                    ComputerMove(); // Компьютер делает ход
                }
            }
        }

        private void ComputerMove()
        {
            Random rand = new Random(); // генерация случайных чисел
            var possibleMoves = new List<Tuple<int, int, int, int>>(); // создание коллекции для хранения возможных ходов компьютера

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col].Player == Player.Player2) // Проверка, принадлежит ли клетка компьютеру
                    {
                        AddPossibleMoves(row, col, possibleMoves); // Заполнение возможных ходов компьютера
                    }
                }
            }

            if (possibleMoves.Count > 0) // Если есть возможные ходы
            {
                // Компьютер случайным образом выбирает один из возможных ходов
                var move = possibleMoves[rand.Next(possibleMoves.Count)];
                MakeMove(move.Item1, move.Item2, move.Item3, move.Item4);
                isPlayer1Turn = true; // переключаемся на игрока

            }
            else
            {
                // Логика в случае, если у компьютера нет доступных ходов (например, окончание игры)
                EndGame();
            }
        }

        private void EndGame()
        {
            // Логика завершения игры
            MessageBox.Show("Игра окончена!");
        }

        private void AddPossibleMoves(int row, int col, List<Tuple<int, int, int, int>> possibleMoves)// добавление возможных ходов
        {
            // игрок, находящийся на позиции board[row, col], равен Player.Player2, то direction устанавливается в 1, движется  вниз, в ином случае- вверх (в зависимости от игрока)
            int direction = (board[row, col].Player == Player.Player2) ? 1 : -1;
            // Движение влево
            //метод CheckMove,  проверяет возможность выполнения хода из ячейки (row, col) в новую позицию, определяемую (row + direction, col - 1).
            // фигура будет перемещаться на одну строку вниз или вверх(в зависимости от значения direction) и на один столбец влево.
            CheckMove(row, col, row + direction, col - 1, possibleMoves);
            // Движение вправо
            //перемещает фигуру на одну строку вниз или вверх и на один столбец вправо.
            CheckMove(row, col, row + direction, col + 1, possibleMoves);
            // Возможность поедания
            CheckCaptureMoves(row, col, possibleMoves);
        }

        private void CheckMove(int fromRow, int fromCol, int toRow, int toCol, List<Tuple<int, int, int, int>> possibleMoves) // проверка является ли ход допустимым
        {
            // Условие для проверки, находится ли целевая позиция (toRow, toCol) на поле.
            if (toRow >= 0 && toRow < BoardSize && toCol >= 0 && toCol < BoardSize)
            {
                // Проверка, свободна ли ячейка (занимает ли ее игрок). Если на этой позиции находится игрок Player.Empty, значит место свободно.
                if (board[toRow, toCol].Player == Player.Empty)
                {
                    // Если ячейка свободна, то возможный ход записывается в список possibleMoves.
                    possibleMoves.Add(new Tuple<int, int, int, int>(fromRow, fromCol, toRow, toCol));
                }

                // Проверка на поедание - необходимо, чтобы перемещение происходило на две клетки диагонально.
                if (Math.Abs(toRow - fromRow) == 2 && Math.Abs(toCol - fromCol) == 2)
                {
                    // Вычисляется строка и столбец позиции фигуры, которая может быть съедена.
                    int capturedX = fromRow + (toRow - fromRow) / 2;
                    int capturedY = fromCol + (toCol - fromCol) / 2;

                    // Проверка, находится ли на позиции, возможной для поедания, фигура соперника (не пуста и не своего игрока).
                    if (capturedX >= 0 && capturedX < BoardSize && capturedY >= 0 && capturedY < BoardSize &&
                     board[capturedX, capturedY].Player != Player.Empty &&
                     board[capturedX, capturedY].Player != (isPlayer1Turn ? Player.Player1 : Player.Player2))
                    {
                        // Если поедание возможно, то этот ход добавляется в список possibleMoves.
                        possibleMoves.Add(new Tuple<int, int, int, int>(fromRow, fromCol, toRow, toCol));
                    }
                }
            }
        }
        private void CheckCaptureMoves(int row, int col, List<Tuple<int, int, int, int>> possibleMoves) //проверят возможные для съедания шашки
        {
            int[] directions = { -1, 1 }; //массив целых чисел, представляющий направления, в которых может двигаться дамка.
                                          //-1 указывает движение вверх по доске. дамка перемещается от игрока 2 к игроку 1).
                                          //            1 указывает движение вниз по доске. дамка перемещается от игрока 1 к игроку 2).
                                          // позволяет проверять ходы в обе стороны.
            foreach (var dir in directions)//перебирает каждое значение из массива directions dir-переменые от -1 до 1
            {
                // Левое поедание
                //row + dir * 2 — это  строка для захвата, которая рассчитывается, добавляя 2 к текущей строке  dir. Если dir равен -1, то это будет означать row - 2; если dir равен 1, то это будет row + 2.
                //col - 2 — это  столбец для захвата, который всегда сдвигается на 2 влево.
                //possibleMoves — это список, в который будут добавляться возможные ходы.
                CheckCapture(row, col, row + dir * 2, col - 2, dir, possibleMoves);

                // Правое поедание
                CheckCapture(row, col, row + dir * 2, col + 2, dir, possibleMoves);

            }
        }
        private void CheckCapture(int fromRow, int fromCol, int toRow, int toCol, int direction, List<Tuple<int, int, int, int>> possibleMoves)//проверка можно ли съесть фигуру противника (
        {
            // Проверка на границы для целевой клетки
            if (toRow >= 0 && toRow < BoardSize && toCol >= 0 && toCol < BoardSize)
            //проверка, находится ли целевая клетка (toRow, toCol) в пределах доски
            //Если toRow меньше 0 или больше или равно BoardSize, тогда ход выходит за границы доски, и выполнение метода продолжено не будет.
            {
                // Проверка на границы для клетки, которую предполагается "съесть"

                int midRow = fromRow + direction;// вычисление координаты строки для средней клетки, которую предполагается "съесть".
                                                 // добавление значения direction к текущей строке fromRow.  direction может быть 1 для хода вниз и -1 для хода вверх 
                int midCol = fromCol + direction;

                // координаты клетки, которая потенциально будет "съедена" при выполнении хода. direction указывает, насколько
                // здесь должно быть смещение от начальной клетки для получения средней клетки, которая будет проверяться ,
                //если фигурка двигается на 2 клетки, direction может быть 1 или -1, чтобы  рассчитать координаты.


                if (midRow >= 0 && midRow < BoardSize && midCol >= 0 && midCol < BoardSize)//Проверка, находится ли клетка midRow, midCol  также в пределах доски.
                //Это предотвращает обращение к несуществующему индексу (были "вылеты")
                {
                    if (board[toRow, toCol].Player == Player.Empty &&//, проверка, можно ли сделать ход.  является ли целевая клетка (toRow, toCol) пустой (Player.Empty), чтобы туда можно было переместить шашку.
                                  board[midRow, midCol].Player != Player.Empty && //занята ли средней клетка фигурой. Если она не пустая, значит, там находится шашка, которую можно съесть.
                                  board[midRow, midCol].Player != (isPlayer1Turn ? Player.Player1 : Player.Player2))
                    //проверка шашки, которая потенциально будет съедена, принадлежит сопернику. вопросительный оператор для выбора игрока в зависимости от текущего хода (isPlayer1Turn).
                    {
                        possibleMoves.Add(new Tuple<int, int, int, int>(fromRow, fromCol, toRow, toCol));
                    }
                    // все предыдущие проверки прошли успешно, ход добавляется в список возможных ходов (possibleMoves). начальная позиция (fromRow, fromCol) и конечная позиция (toRow, toCol).
                    //означает, что данное перемещение — допустимое.
                }
            }
        }
        //метод для обычного движения шашек
               public bool MakeMove(int fromX, int fromY, int toX, int toY) ////Этот метод выполняет перемещение шашки. Он принимает координаты начальной и конечной позиции, получает шашку с координатами fromX и fromY.
        {
            // Получение шашки из исходной позиции
            Checker checker = board[fromX, fromY];

            // Проверка допустимости хода
            if (checker.Player == Player.Empty || !IsValidMove(checker, fromX, fromY, toX, toY))
            {
                return false;
            }

            // Перемещение шашки
            board[toX, toY] = checker;//Очистка старой клетки
            board[fromX, fromY] = new Checker();//перенесение шашки на новую клетку

            // Повышение в дамки
            if (toX == 0 || toX == 7)
            {
                checker.Status = CheckerStatus.King; //Проверяет, достигла ли шашка противоположного края и превращается в дамку, если это так.
            }

            // Обновление содержимого кнопки

            UpdateButtonContent(toX, toY, checker);
            CaptureCheckers(checker, fromX, fromY, toX, toY); //захват шашек противника, если это необходимо, и возвращает true, сигнализируя, что ход выполнен успешно.



            return true;
        }
        //замена шашки на дамку (изменение обозначнеия
        private void UpdateButtonContent(int x, int y, Checker checker)
        {
            if (checker.Player == Player.Player1)
            {
                // Если шашка белая и  дамка, - WQ
                buttons[x, y].Content = checker.Status == CheckerStatus.King ? "WQ" : "W";
            }
            else if (checker.Player == Player.Player2)
            {
                // Если шашка черная и  дамка, - BQ,
                buttons[x, y].Content = checker.Status == CheckerStatus.King ? "BQ" : "B";
            }
            else
            {
                // Если шашка отсутствует- пустое поле
                buttons[x, y].Content = null;
            }
        }

        //проверка является ли ход допустимым
        private bool IsValidMove(Checker checker, int fromX, int fromY, int toX, int toY)
        {
            if (checker.Status == CheckerStatus.Regular)
            // проверка, является ли  шашка обычной, проверяет допустимость хода для обычной шашки.
            {
                //шашка принадлежит первому игроку (Player1), и она пытается двигаться вверх по доске (по оси Y), что недопустимо, возвращает false 

                if (checker.Player == Player.Player1 && toX < fromX) return false;

                //аналогично 
                if (checker.Player == Player.Player2 && toX > fromX) return false;
                //Проверяет, что шашка перемещается на одну клетку по диагонали. если нет-false
                if (Math.Abs(toX - fromX) != 1 || Math.Abs(toY - fromY) != 1) return false;
                return board[toX, toY].Player == Player.Empty; // проверка на свободное место
            }
            else
            {
                // Движение королевы
                //перемещение происходит по диагонали на любое количество клеток. если нет- false.
                if (Math.Abs(toX - fromX) != Math.Abs(toY - fromY)) return false;

                //цикл, который проходит по всем клеткам между начальной и конечной позициями. Используется (toX - fromX) / Math.Abs(toX - fromX) для определения направления -вверх или вниз п.
                // i != toX -  цикл завершится, когда достигнет конечной ячейки.
                for (int i = fromX + (toX - fromX) / Math.Abs(toX - fromX); i != toX; i += (toX - fromX) / Math.Abs(toX - fromX))
                {
                    //попадает на клетку, в которой есть шашка ( не пустая)- false.
                    if (board[i, fromY + (toY - fromY) / Math.Abs(toY - fromY)].Player != Player.Empty)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            wins = 0;
            losses = 0;
            CreateGameBoard();
            MessageBox.Show("Игра началась");
        }


        private void CheckForWin()
        {
            // Проверка на выигрыш
            bool player1HasCheckers = false;
            bool player2HasCheckers = false;

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (board[row, col].Player == Player.Player1)
                        player1HasCheckers = true;
                    else if (board[row, col].Player == Player.Player2)
                        player2HasCheckers = true;
                }
            }

            if (!player1HasCheckers)
            {
                MessageBox.Show("Игрок 2 выиграл!");
                Application.Current.Shutdown();
            }
            else if (!player2HasCheckers)
            {
                MessageBox.Show("Игрок 1 выиграл!");
                Application.Current.Shutdown();
            }
        }
        private void ShowStatsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowStatistics();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Хотите сохранить текущую игру?", "Сохранить игру", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveGameState();
                MessageBox.Show("Состояние игры сохранено.");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
        // метод движения с возможностью поедания
        private void CaptureCheckers(Checker checker, int fromX, int fromY, int toX, int toY)
        {
            //Проверка, что шашка является обычной
            //цикл проходит по всем клеткам между начальной и конечной позициями (fromX и toX).  используется Math.Min и Math.Max, чтобы  обрабатывать движение в любую сторону.

            if (checker.Status == CheckerStatus.Regular &&
        Math.Abs(toX - fromX) == 2 && Math.Abs(toY - fromY) == 2)
            //Math.Abs возвращает абсолютное значение ( без знака), что позволяет проверить разницу между исходной и конечной позицией (по X) на равенство 2.
            //Math.Abs(toX - fromX) == 2: проверяет, перемещается ли шашка на два ряда вниз или вверх по доске.
            //Math.Abs(toY - fromY) == 2)проверяет разницу по горизонтали (по Y) на равенство 2.
            {
                int capturedX = fromX + (toX - fromX) / 2; //Координаты захваченной шашки
                // вычисляет координату X захваченной шашки.
                //  находит среднюю строку между изначальным (fromX) и конечным (toX) положением шашки.
                // Разница toX - fromX делится на 2, чтобы получить  строку, которая находится между ними.
                int capturedY = fromY + (toY - fromY) / 2;
                //тоже самое для Y
                board[capturedX, capturedY].Player = Player.Empty; // удаление шашки
            }

            //Проверка, что шашка является дамой
            //цикл проходит по всем клеткам между начальной и конечной позициями (fromX и toX).  используется Math.Min и Math.Max, чтобы  обрабатывать движение в любую сторону.
            else if (checker.Status == CheckerStatus.King)
            {
                //цикл проходит по всем строкам между начальной (fromX) и целевой (toX) позицией.
                //Math.Min(fromX, toX) + 1 устанавливает начальную точку цикла на первую строку, которая проходит через поедаемую шашку,
                // Math.Max(fromX, toX) указывает на конечную строку. 
                for (int i = Math.Min(fromX, toX) + 1; i < Math.Max(fromX, toX); i++)
                {
                    //определяет координату Y для промежуточного положения.
                    int j = fromY + (toY - fromY) / Math.Abs(toY - fromY);
                    if (board[i, j].Player != Player.Empty)//есть ли в текущей промежуточной клетке шашка. Если в ней есть шашка (не пусто), то:
                    {
                        board[i, j].Player = Player.Empty;//удаление
                    }
                }
            }
        }
        private void SaveStatistics(int wins, int losses)
        {
            using (StreamWriter sw = new StreamWriter("statistics.txt", true))
            {
                sw.WriteLine($"Победы: {wins}, Поражения: {losses}, Дата: {DateTime.Now}");
            }
        }

        private void ShowStatistics()
        {
            if (File.Exists("statistics.txt"))
            {
                string stats = File.ReadAllText("statistics.txt");
                MessageBox.Show(stats, "Статистика");
            }
            else
            {
                MessageBox.Show("Нет доступных данных о статистике.", "Статистика");
            }
        }

        private void SaveGameState()
        {
            using (StreamWriter sw = new StreamWriter("gamestate.txt"))
            {
                // Сохранение состояния доски
                for (int row = 0; row < BoardSize; row++)
                {
                    for (int col = 0; col < BoardSize; col++)
                    {
                        sw.Write(board[row, col].Player + ",");
                        //Object.ToString(),выведет на печать список всех элементов в DataRow, разделённых запятыми.
                    }
                    sw.WriteLine();
                }
                // Сохранение текущего игрока
                sw.WriteLine(isPlayer1Turn); // Сохранение состояния переменной isPlayer1Turn
            }
        }
        private void LoadGameState()
        {
            if (File.Exists("gamestate.txt")) // Проверка существования файла
            {
                using (StreamReader sr = new StreamReader("gamestate.txt")) // Объект StreamReader для чтения файла
                {
                    // Чтение состояния доски
                    for (int row = 0; row < BoardSize; row++)
                    {
                        string[] values = sr.ReadLine().Split(',');
                        for (int col = 0; col < BoardSize; col++)
                        {
                            board[row, col] = new Checker
                            {
                                Player = Enum.TryParse(values[col], out Player player) ? player : Player.Empty
                            };
                        }
                    }

                    // Загрузка состояния текущего игрока
                    string isPlayer1TurnValue = sr.ReadLine();
                    isPlayer1Turn = bool.TryParse(isPlayer1TurnValue, out bool isTurn) && isTurn;
                }
            }
        }


        //Перечисление, которое определяет возможные состояния для шашек: пустая клетка и шашки для первого и второго игроков.
        public enum Player
        {
            Empty,
            Player1,
            Player2
        }
        //Перечисление, определяющее состояние шашки: является ли она обычной или дамкой.
        public enum CheckerStatus
        {
            Regular,
            King
        }
        //представляет каждую шашку на доске. У нее есть свойства для хранения игрока и статуса. 

        public class Checker
        {
            public Player Player { get; set; }
            public CheckerStatus Status { get; set; }

            public Checker()
            {
                Player = Player.Empty;
                Status = CheckerStatus.Regular;
            }
        }
    }
}