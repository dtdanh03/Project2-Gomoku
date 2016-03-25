using System;
using System.Collections.Generic;
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
using System.Windows.Resources;
using System.Windows.Shapes;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string myName;
        GameBoard myGame;
        Color xColor;
        Color oColor;
        int fontSize;
        bool autoFlag = true;
        NetworkProcess networkProcess;
        enum GameMode { OnePlayer, TwoPlayers, OnePlayerOnline, AIOnline};
        int currentGameMode;
        public MainWindow()
        {
            InitializeComponent();
            xColor = Colors.Red;
            oColor = Colors.Blue;
            fontSize = 28;
            nameTextbox.Text = "Player1";
            myName = "Player1";
            createNewGame();
          
        }

        void networkProcess_performMove(object sender, int player, int row, int col)
        {
            Button b = (Button)boardGrid.Children.Cast<UIElement>().Last(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
            UIPlayAt(row, col, b, player);
            if (currentGameMode == (int)GameMode.AIOnline)
            {
                if (autoFlag)
                {
                    myGame.getRandomMove(row, col);
                }
                autoFlag = !autoFlag;
                
            }
        }

        void networkProcess_messageComing(object sender, string message, string name)
        {
            newChat(message, name);
        }

        void networkProcess_firstAIMove(object sender)
        {
            myGame.getRandomMove(5, 5);
            autoFlag = false;
        }

        #region Chat
        private void messageSendButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (currentGameMode == (int)GameMode.OnePlayerOnline
                || currentGameMode == (int)GameMode.AIOnline)
            {
                networkProcess.ChatToServer(messageTextbox.Text);
            }
            else
            {
                newChat(messageTextbox.Text, myName);
            }
            messageTextbox.Text = "";
        }

        private void newChat(string message, string name)
        {
            ChatBlock newChatBlock = new ChatBlock();
            newChatBlock.nameLabel.Content = name;
            newChatBlock.textLabel.Content = message;
            newChatBlock.timeLabel.Content = DateTime.Now.ToShortTimeString();
            chatPanel.Items.Add(newChatBlock);
        }

        private void nameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            myName = nameTextbox.Text;
            if (currentGameMode == (int)GameMode.OnePlayerOnline
                || currentGameMode == (int)GameMode.AIOnline)
            {
                networkProcess.setNameToServer(myName);
            }
        }

        private void messageTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                messageSendButton_Click(this, new RoutedEventArgs());
            }
        }

        private void nameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                nameChangeButton_Click(this, new RoutedEventArgs());
            }
        }
        #endregion

        #region UIGamePlay

        private void createNewGame()
        {
            myGame = new GameBoard();
            myGame.AIMoved += myGame_AIMoved;
            myGame.GameDidFinish += myGame_GameDidFinish;
            CreateUIGameBoard();
            currentGameMode = gameModeComboBox.SelectedIndex;
            myGame.setGameMode(currentGameMode);
            autoFlag = true;

            if (currentGameMode == (int)GameMode.OnePlayerOnline
                || currentGameMode == (int)GameMode.AIOnline)
            {
                networkProcess = new NetworkProcess();
                networkProcess.messageComing += networkProcess_messageComing;
                networkProcess.performMove += networkProcess_performMove;
                networkProcess.firstAIMove += networkProcess_firstAIMove;
                networkProcess.Init();
            } 
        }
        void myGame_GameDidFinish(object sender, int result)
        {
            switch (result)
            {
                case GameBoard.X_WIN:
                    if (currentGameMode == (int)GameMode.TwoPlayers)
                        MessageBox.Show("Congratulations!\n    White won !!!");
                    else if (currentGameMode == (int)GameMode.OnePlayer)
                        MessageBox.Show("Congratulations\n     You won !!!");
                    break;
                case GameBoard.O_WIN:
                    if (currentGameMode == (int)GameMode.TwoPlayers)
                        MessageBox.Show("Congratulations!\n    Black won !!!");
                    else if (currentGameMode == (int)GameMode.OnePlayer)
                        MessageBox.Show("Computer won !!!");
                    break;
                case GameBoard.TIE:
                    MessageBox.Show("Game tie");
                    break;
            }
        }

        void myGame_AIMoved(object sender, int x, int y)
        {
            Button b = (Button)boardGrid.Children.Cast<UIElement>().Last(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);

            if (currentGameMode == (int)GameMode.OnePlayer)
            {
                OPlay(b);
            }
            else if (currentGameMode == (int)GameMode.AIOnline)
            {
                networkProcess.PlayAt(x, y);
            }
         
        }
        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (networkProcess != null)
                networkProcess.TurnOffOnline();
            createNewGame();
        }
        private void CreateUIGameBoard()
        {
            boardGrid.Children.Clear();
            int boardSize = Properties.Settings.Default.BoardSize;
            for (int i = 0; i < boardSize; ++i)
            {
                for (int j = 0; j < boardSize; ++j)
                {
                    Rectangle rect = new Rectangle();
                    if ((i + j) % 2 == 0)
                    {
                        rect.Fill = Brushes.Aqua;
                    } 
                    else
                    {
                        //rect.Fill = Brushes.CadetBlue;
                        //rect.Fill = Brushes.Brown;
                        rect.Fill = Brushes.Gray;
                    }
                    rect.Opacity = 0.4;
                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);
                    boardGrid.Children.Add(rect);
                    Button myButton = new Button();
                    myButton.VerticalAlignment = VerticalAlignment.Stretch;
                    myButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                    myButton.Background = Brushes.Transparent;
                    myButton.Focusable = false;
                    Grid.SetRow(myButton, i);
                    Grid.SetColumn(myButton, j);
                    boardGrid.Children.Add(myButton);
                    myButton.Click += someButton_Click;
                    myButton.Focusable = false;
                   
                }
            }
        }
        private void someButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int row = Grid.GetRow(clickedButton);
            int col = Grid.GetColumn(clickedButton);
            
            if (currentGameMode == (int)GameMode.OnePlayer
                || currentGameMode == (int)GameMode.TwoPlayers)
            {
                UIPlayAt(row, col, clickedButton, 0);
            }
            else if (currentGameMode == (int)GameMode.OnePlayerOnline)
            {
                networkProcess.PlayAt(row, col);
            }
        }

        private void UIPlayAt(int x, int y, Button clickedButton, int player)
        {
            int result = 0;
            if (currentGameMode == (int)GameMode.OnePlayer
                || currentGameMode == (int)GameMode.TwoPlayers)
            {
                result = myGame.PlayAt(x, y);
            }
            else if (currentGameMode == (int)GameMode.OnePlayerOnline
                || currentGameMode == (int)GameMode.AIOnline)
            {
                result = player + 1;
                myGame.PlayAtOnline(x, y, player);
            }
           
            switch (result)
            {
                case GameBoard.NO_VALUE: 
                    break;
                case GameBoard.X_PLAYER:
                    XPlay(clickedButton);
                    break;
                case GameBoard.O_PLAYER:
                    OPlay(clickedButton);
                    break; 
            }
        }

        private void XPlay (Button clickedButton)
        {
            //clickedButton.Content = "X";
            //clickedButton.Foreground = new SolidColorBrush(xColor);
            //clickedButton.FontSize = fontSize;
            ImageBrush br = new ImageBrush();

            br.ImageSource = GetBitmapSource(Properties.Resources.White);

            clickedButton.Background = br;
           
           
        }

        public BitmapSource GetBitmapSource(System.Drawing.Bitmap bitmap)
        {
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bitmapSource;
        }

        private void OPlay (Button clickedButton)
        {
            //clickedButton.Content = "O";
            //clickedButton.Foreground = new SolidColorBrush(oColor);
            //clickedButton.FontSize = fontSize;
            ImageBrush br = new ImageBrush();
            br.ImageSource = GetBitmapSource(Properties.Resources.Black);

            clickedButton.Background = br;
        }
        #endregion

        private void gameModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (networkProcess != null)
                networkProcess.TurnOffOnline();
            createNewGame();
        }

     
       
    }
}
