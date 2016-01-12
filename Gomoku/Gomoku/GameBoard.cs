using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



 

namespace Gomoku
{
    public class GameBoard
    {
        public const int NO_VALUE = 0;
        public const int X_PLAYER = 1;
        public const int O_PLAYER = 2;
        public const int X_WIN = 3;
        public const int O_WIN = 4;
        public const int TIE = 5;
        const int MULTIPLAYER_MODE = 6;
        const int AI_MODE = 7;
        int BOARDSIZE = Properties.Settings.Default.BoardSize;

        int currentPlayer;
        int moveCount = 0;
        bool isGameOver = false;

        int gameMode;
        public int[,] board { get; set; }
        public delegate void AIMovedEventHandler(object sender, int x, int y);
        public event AIMovedEventHandler AIMoved;
        public delegate void GameDidFinishEventHanlder(object sender, int result);
        public event GameDidFinishEventHanlder GameDidFinish;
        BackgroundWorker bw = new BackgroundWorker();

        public void setGameMode(int input)
        {
            if (input == 0) gameMode = AI_MODE;
            else gameMode = MULTIPLAYER_MODE;
        }

        public GameBoard()
        {
            board = new int[BOARDSIZE, BOARDSIZE];
            for (int i = 0; i < BOARDSIZE; ++i)
            {
                for (int j = 0; j < BOARDSIZE; ++j)
                    board[i,j] = NO_VALUE;
            }
            currentPlayer = X_PLAYER;
            gameMode = MULTIPLAYER_MODE;
            bw.DoWork += bw_DoWork;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] param = e.Argument as object[];
            int x, y;
            x = (int)param[0];
            y = (int)param[1];
            performAIMove(x, y);
        }

        public int PlayAt (int x, int y)
        {
                if (board[x, y] != NO_VALUE || isGameOver)
                {
                    return 0;
                }
            int rv = 0;
            if (gameMode == MULTIPLAYER_MODE ||
                (gameMode == AI_MODE && currentPlayer == X_PLAYER))
            {
                board[x, y] = currentPlayer;
                moveCount++;
                rv = performCheck(x, y);
                if (rv == X_WIN || rv == O_WIN || rv == TIE)
                {
                    rv = currentPlayer;
                    return rv;
                }
                rv = currentPlayer;
                SwitchPlayer();
            }
                    

                if (gameMode == AI_MODE && currentPlayer == O_PLAYER)
                {
                    object[] param = new object[] {x, y};
                    bw.RunWorkerAsync(param);
                    SwitchPlayer();
                    rv = currentPlayer;
                }
                return rv;
        }

        private void SwitchPlayer()
        {
            if (currentPlayer == X_PLAYER) currentPlayer = O_PLAYER;
            else currentPlayer = X_PLAYER;
        }

        #region CheckWin
        private int performCheck(int x, int y)
        {
            int check = CheckWin();
            if (check == X_WIN || check == O_WIN)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { GameDidFinish(this, check); }));
                isGameOver = true;
                return check;

            }
            if (moveCount == (BOARDSIZE * BOARDSIZE))
            {
                GameDidFinish(this, TIE);
                isGameOver = true;
                return TIE;
            }
            return 0;
        }
        

        private int CheckWin()
        {
            int result;
            result = UpDownBruteForceCheck();
            if (result == X_PLAYER) return X_WIN;
            if (result == O_PLAYER) return O_WIN;

            result = SlopeCheck();
            if (result == X_PLAYER) return X_WIN;
            if (result == O_PLAYER) return O_WIN;

            int count = 0;
            for (int i = 0; i < BOARDSIZE; ++i)
                for (int j = 0; j < BOARDSIZE; ++j)
                    if (board[i, j] != NO_VALUE)
                        count++;
            if (count == BOARDSIZE * BOARDSIZE) return TIE;
            return 0;

        }

        private int LineCheck(List<int> list)
        {
            if (list.Count < 5) return 0;
            for (int i = 0; i < list.Count - 4; ++i)
            {
                if (list[i] != NO_VALUE)
                    if (list[i] == list[i + 1]
                        && list[i] == list[i + 2]
                        && list[i] == list[i + 3]
                        && list[i] == list[i + 4])
                        return list[i];
            }
            return NO_VALUE;


        }

        private int UpDownBruteForceCheck()
        {
            List<int> vertical = new List<int>();
            List<int> horizontal = new List<int>();
            for (int i = 0; i < BOARDSIZE; ++i)
            {

                for (int j = 0; j < BOARDSIZE; ++j)
                {
                    horizontal.Add(board[i, j]);
                    vertical.Add(board[j, i]);
                }


                int result;
                result = LineCheck(vertical);
                vertical.Clear();
                if (result == X_PLAYER || result == O_PLAYER)
                    return result;
                result = LineCheck(horizontal);
                horizontal.Clear();
                if (result == X_PLAYER || result == O_PLAYER)
                    return result;
            }
            return 0;
        }

        private int SlopeCheck()
        {
            List<int> list = new List<int>();
            for (int i = 0; i < BOARDSIZE; ++i)
            {
                for (int j = 0; j < BOARDSIZE; ++j)
                {
                    if (i == 0)
                    {
                        int x1, x2, y1, y2;
                        int result;
                        x1 = x2 = i;
                        y1 = y2 = j;
                        while (x1 < BOARDSIZE && y1 < BOARDSIZE)
                        {
                            list.Add(board[x1++, y1++]);
                        }
                        result = LineCheck(list);
                        list.Clear();
                        if (result == X_PLAYER || result == O_PLAYER)
                            return result;



                        while (x2 < BOARDSIZE && y2 >= 0)
                        {
                            list.Add(board[x2++, y2--]);
                        }
                        result = LineCheck(list);
                        list.Clear();
                        if (result == X_PLAYER || result == O_PLAYER)
                            return result;
                    }

                    if (j == 0)
                    {
                        int x, y;
                        x = i;
                        y = j;
                        while (x < BOARDSIZE && y < BOARDSIZE)
                        {
                            list.Add(board[x++, y++]);
                        }
                        int result = LineCheck(list);
                        list.Clear();
                        if (result == X_PLAYER || result == O_PLAYER)
                            return result;
                    }


                    if (j == BOARDSIZE - 1)
                    {
                        int x, y;
                        x = i;
                        y = j;
                        while (x < BOARDSIZE && y < BOARDSIZE)
                        {
                            list.Add(board[x++, y--]);
                        }
                        int result = LineCheck(list);
                        list.Clear();
                        if (result == X_PLAYER || result == O_PLAYER)
                            return result;
                    }
                }
            }
            return 0;
        }
     

        #endregion


        #region AI
        private void performAIMove(int x, int y)
        {
            Random random = new Random();
            int a, b;
            int flag1, flag2;
            int count = 0;
            while (true)
            {
                count++;
                a = b = flag1 = flag2 = 0;
                a = random.Next(2);
                b = random.Next(2);
                flag1 = random.Next(2);
                flag2 = random.Next(2);
                if (flag1 == 1)
                    a = x + a;
                else
                    a = x - a;
                if (flag2 == 1)
                    b = y + b;
                else
                    b = y - b;

                if (a >= 0 && b >= 0 && a < BOARDSIZE && b < BOARDSIZE
                    && board[a, b] == NO_VALUE)
                {
                    board[a, b] = O_PLAYER;
                    moveCount++;
                    Application.Current.Dispatcher.Invoke(new Action(() => { this.AIMoved(this, a, b); }));
                    performCheck(a, b);
                    break;
                }


                if (count >= 20)
                {
                    a = random.Next(11);
                    b = random.Next(11);
                    if (a >= 0 && b >= 0 && a < BOARDSIZE && b < BOARDSIZE
                   && board[a, b] == NO_VALUE)
                    {
                        board[a, b] = O_PLAYER;
                        moveCount++;
                        Application.Current.Dispatcher.Invoke(new Action(() => { this.AIMoved(this, a, b); }));
                        performCheck(a, b);
                        break;
                    }
                }
                
                if (count > 200)
                {
                    for (int i = 0; i < BOARDSIZE; ++i)
                    {
                        for (int j = 0; j < BOARDSIZE; ++j)
                        {
                            if (board[i, j] == NO_VALUE)
                            {
                                board[a, b] = O_PLAYER;
                                moveCount++;
                                Application.Current.Dispatcher.Invoke(new Action(() => { this.AIMoved(this, a, b);}));
                                performCheck(a, b);
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion

       
    }
}
