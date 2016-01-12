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
        enum GameMode { OnePlayer, TwoPlayers };
        int currentGameMode;
        public MainWindow()
        {
            InitializeComponent();
            xColor = Colors.Red;
            oColor = Colors.Blue;
            fontSize = 28;
            //Button b = (Button)boardGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == 0 && Grid.GetColumn(e) == 0);
            createNewGame();
        }

        #region Chat
        private void messageSendButton_Click(object sender, RoutedEventArgs e)
        {
            ChatBlock newChatBlock = new ChatBlock();
            newChatBlock.nameLabel.Content = myName;
            newChatBlock.textLabel.Content = messageTextbox.Text;
            newChatBlock.timeLabel.Content = DateTime.Now.ToShortTimeString();
            chatPanel.Children.Add(newChatBlock);
            messageTextbox.Text = "";
        }

        private void nameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            myName = nameTextbox.Text;
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
        }

        void myGame_GameDidFinish(object sender, int result)
        {
            switch (result)
            {
                case GameBoard.X_WIN:
                    if (currentGameMode == (int)GameMode.TwoPlayers)
                        MessageBox.Show("Congratulations!\n    X won !!!");
                    else if (currentGameMode == (int)GameMode.OnePlayer)
                        MessageBox.Show("Congratulations\n     You won !!!");
                    break;
                case GameBoard.O_WIN:
                    if (currentGameMode == (int)GameMode.TwoPlayers)
                        MessageBox.Show("Congratulations!\n    O won !!!");
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
            Button b = (Button)boardGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
            OPlay(b);
        }
        private void newGameButton_Click(object sender, RoutedEventArgs e)
        {
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
                    Button myButton = new Button();
                    myButton.VerticalAlignment = VerticalAlignment.Stretch;
                    myButton.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Grid.SetRow(myButton, i);
                    Grid.SetColumn(myButton, j);
                    boardGrid.Children.Add(myButton);
                    myButton.Click += someButton_Click;
                }
            }
        }
        private void someButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int row = Grid.GetRow(clickedButton);
            int col = Grid.GetColumn(clickedButton);
            //string message = string.Format("Clicked at:\n  + Row: {0}\n  + Column: {1}", row.ToString(), col.ToString());
           //MessageBox.Show(message);
            UIPlayAt(row, col, clickedButton);
            //if (currentGameMode == (int)GameMode.OnePlayer)
            //{
            //    myGame.performAIMove();
            //}

        }

        private void UIPlayAt(int x, int y, Button clickedButton)
        {
            int result = myGame.PlayAt(x, y);
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
            clickedButton.Content = "X";
            clickedButton.Foreground = new SolidColorBrush(xColor);
            clickedButton.FontSize = fontSize;
        }

        private void OPlay (Button clickedButton)
        {
            clickedButton.Content = "O";
            clickedButton.Foreground = new SolidColorBrush(oColor);
            clickedButton.FontSize = fontSize;
        }
        #endregion

        private void gameModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            createNewGame();
        }

       
    }
}
