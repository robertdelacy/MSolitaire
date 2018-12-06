using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;

namespace Solitaire
{
    public partial class MSolitaire : Form
    {
        int[] CurrentCardLocation = new int[2];
        int StackTopLocation = 12;

        int _xPos;
        int _yPos;
        int mouse_xPos;
        int mouse_yPos;
        bool tracking = false;
        int cardmove = -1;
        PictureBox MovingCard = new PictureBox();
        int[] MovingCardOrigin = new int[2];

        Deck deck = new Deck();
        string[,] table = new string[8, 8]; //[Suit then Face for each Stack,Position on Stack]
        string[,] play = new string[2, 2];
        string[,] discards = new string[2, 2];
        PictureBox[] TopStackControls = new PictureBox[4];
        int[] StackNumbers = { -1, -1, -1, -1 };

        string[, ,] TableHistory = new string[0, 8, 8]; //[n most recent tables, table(8,8)]
        string[, ,] DiscardHistory = new string[0, 2, 2]; //[n most recent discards, discard(2,2)]
        string[, ,] PlayHistory = new string[0, 2, 2]; //[n most recent plays, play(2,2)]
        int Undos = 0;
        string[,] lasttable = new string[8, 8];
        string[,] lastdiscards = new string[2, 2];
        string[,] lastplay = new string[2, 2];

        int[,] PotentialMergers = { { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 }, { -1, -1 } };

        bool Animate = false;
        PictureBox Animated = new PictureBox();
        int AnimatedOrigin = 0;
        int AnimatedDestination = 0;
        int AnimatedStack = -1;

        int timescalled = 0;

        #region Initial Methods

        public MSolitaire()
        {
            InitializeComponent();

            CurrentCardLocation[0] = CurrentCard.Left;
            CurrentCardLocation[1] = CurrentCard.Top;
            StackTopLocation = One_1.Top;

            DrawCard(true);
            CurrentCard.BringToFront();
            TopStackControls[0] = One_1;
            TopStackControls[1] = Two_1;
            TopStackControls[2] = Three_1;
            TopStackControls[3] = Four_1;
            UndoButton.Text = "Undo (" + Undos + ")";

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            PublicVariables.Settings = ReadFromFile("Settings", PublicVariables.Settings);

            string[] HighScores1D = new string[PublicVariables.HighScores.GetLength(0)];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                HighScores1D[i] = PublicVariables.HighScores[i, 0] + "\t" + PublicVariables.HighScores[i, 1] + ":" + PublicVariables.HighScores[i, 2] + ":" + PublicVariables.HighScores[i, 3];
            }

            HighScores1D = ReadFromFile("High Scores", HighScores1D);

            if (HighScores1D.GetLength(0) > 1)
            {
                string Recent = HighScores1D[HighScores1D.GetLength(0) - 1];
                Array.Resize<string>(ref HighScores1D, HighScores1D.GetLength(0) - 1);

                PublicVariables.HighScores = new string[HighScores1D.GetLength(0), 4];

                for (int i = 0; i < HighScores1D.GetLength(0); i++)
                {
                    string[] Separated = HighScores1D[i].Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    PublicVariables.HighScores[i, 0] = Separated[0];
                    PublicVariables.HighScores[i, 1] = Separated[1];
                    PublicVariables.HighScores[i, 2] = Separated[2];
                    PublicVariables.HighScores[i, 3] = Separated[3];
                }

                string[] Components = Recent.Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                PublicVariables.MostRecentScore[0] = Components[0];
                PublicVariables.MostRecentScore[1] = Components[1];
                PublicVariables.MostRecentScore[2] = Components[2];
                PublicVariables.MostRecentScore[3] = Components[3];
                PublicVariables.MostRecentScore[4] = Components[4];
            }
            else
            {
                PublicVariables.HighScores = new string[0, 4];
                PublicVariables.MostRecentScore = new string[5];
            }
        }

        private void MSolitaire_Paint(object sender, PaintEventArgs e)
        {
            Graphics Line = e.Graphics;
            Pen pen = new Pen(Color.White, 1);
            float[] dashValues = { 5, 5 };
            pen.DashPattern = dashValues;
            Line.DrawLine(pen, 12, 595, 484, 595);
        }

        #endregion

        #region GameScores

        private void NewGame()
        {
            PublicVariables.Points[0] = "0";
            PublicVariables.Points[1] = "0";
            PublicVariables.Points[2] = "0";
            cardmove = -1;
            deck = new Deck();

            table = new string[8, 8];
            play = new string[2, 2];
            discards = new string[2, 2];
            TopStackControls[0] = One_1;
            TopStackControls[1] = Two_1;
            TopStackControls[2] = Three_1;
            TopStackControls[3] = Four_1;
            StackNumbers[0] = -1;
            StackNumbers[1] = -1;
            StackNumbers[2] = -1;
            StackNumbers[3] = -1;

            TableHistory = new string[0, 8, 8];
            DiscardHistory = new string[0, 2, 2];
            PlayHistory = new string[0, 2, 2];
            Undos = 0;
            UndoButton.Text = "Undo (" + Undos + ")";
            lasttable = new string[8, 8];
            lastdiscards = new string[2, 2];
            lastplay = new string[2, 2];

            Animate = false;
            Animated = new PictureBox();
            AnimatedOrigin = 0;
            AnimatedDestination = 0;
            AnimatedStack = -1;

            DrawCard(true);
            DrawCard(true);
            Update(0);
            Update(1);
            Update(2);
            Update(3);
            Update(4);

            this.Enabled = true;
            timer.Start();
        }

        private void SaveScore()
        {
            string[,] Temp = new string[PublicVariables.HighScores.GetLength(0) + 1, 4];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                Temp[i, 0] = PublicVariables.HighScores[i, 0];
                Temp[i, 1] = PublicVariables.HighScores[i, 1];
                Temp[i, 2] = PublicVariables.HighScores[i, 2];
                Temp[i, 3] = PublicVariables.HighScores[i, 3];
            }
            Temp[PublicVariables.HighScores.GetLength(0), 0] = (DateTime.Now).ToString("dd/MM/yy");
            Temp[PublicVariables.HighScores.GetLength(0), 1] = PublicVariables.Points[0];
            Temp[PublicVariables.HighScores.GetLength(0), 2] = PublicVariables.Points[1];
            Temp[PublicVariables.HighScores.GetLength(0), 3] = ((int)Convert.ToDouble(PublicVariables.Points[2])).ToString();
            PublicVariables.HighScores = new string[Temp.GetLength(0), 4];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                PublicVariables.HighScores[i, 0] = Temp[i, 0];
                PublicVariables.HighScores[i, 1] = Temp[i, 1];
                PublicVariables.HighScores[i, 2] = Temp[i, 2];
                PublicVariables.HighScores[i, 3] = Temp[i, 3];
            }
            SortingHighScores();

            string[] HighScores1D = new string[PublicVariables.HighScores.GetLength(0) + 1];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                HighScores1D[i] = PublicVariables.HighScores[i, 0] + "\t" + PublicVariables.HighScores[i, 1] + ":" + PublicVariables.HighScores[i, 2] + ":" + PublicVariables.HighScores[i, 3];
            }
            HighScores1D[PublicVariables.HighScores.GetLength(0)] = PublicVariables.MostRecentScore[0] + ":" + PublicVariables.MostRecentScore[1] + "\t" + PublicVariables.MostRecentScore[2] + ":" + PublicVariables.MostRecentScore[3] + ":" + PublicVariables.MostRecentScore[4];

            HighScores1D = WriteToFile("High Scores", HighScores1D);

            string Recent = HighScores1D[HighScores1D.GetLength(0) - 1];
            Array.Resize<string>(ref HighScores1D, HighScores1D.GetLength(0) - 1);

            PublicVariables.HighScores = new string[HighScores1D.GetLength(0), 4];

            for (int i = 0; i < HighScores1D.GetLength(0); i++)
            {
                string[] Separated = HighScores1D[i].Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                PublicVariables.HighScores[i, 0] = Separated[0];
                PublicVariables.HighScores[i, 1] = Separated[1];
                PublicVariables.HighScores[i, 2] = Separated[2];
                PublicVariables.HighScores[i, 3] = Separated[3];
            }

            string[] Components = Recent.Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
            PublicVariables.MostRecentScore[0] = Components[0];
            PublicVariables.MostRecentScore[1] = Components[1];
            PublicVariables.MostRecentScore[2] = Components[2];
            PublicVariables.MostRecentScore[3] = Components[3];
            PublicVariables.MostRecentScore[4] = Components[4];
        }

        public void SortingHighScores()
        {
            string[,] Temp = new string[PublicVariables.HighScores.GetLength(0), 4];
            PublicVariables.MostRecentScore[1] = PublicVariables.HighScores[PublicVariables.HighScores.GetLength(0) - 1, 0];
            PublicVariables.MostRecentScore[2] = PublicVariables.HighScores[PublicVariables.HighScores.GetLength(0) - 1, 1];
            PublicVariables.MostRecentScore[3] = PublicVariables.HighScores[PublicVariables.HighScores.GetLength(0) - 1, 2];
            PublicVariables.MostRecentScore[4] = PublicVariables.HighScores[PublicVariables.HighScores.GetLength(0) - 1, 3];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                for (int j = 0; j < PublicVariables.HighScores.GetLength(0); j++)
                {
                    if (Temp[j, 1] == null)
                    {
                        Temp[j, 0] = PublicVariables.HighScores[i, 0];
                        Temp[j, 1] = PublicVariables.HighScores[i, 1];
                        Temp[j, 2] = PublicVariables.HighScores[i, 2];
                        Temp[j, 3] = PublicVariables.HighScores[i, 3];

                        if (i == PublicVariables.HighScores.GetLength(0) - 1)
                        {
                            PublicVariables.MostRecentScore[0] = (j + 1).ToString();
                        }
                        break;
                    }
                    else if (Convert.ToInt32(Temp[j, 1]) < Convert.ToInt32(PublicVariables.HighScores[i, 1]))
                    {
                        for (int k = PublicVariables.HighScores.GetLength(0) - 1; k > j; k--)
                        {
                            Temp[k, 0] = Temp[k - 1, 0];
                            Temp[k, 1] = Temp[k - 1, 1];
                            Temp[k, 2] = Temp[k - 1, 2];
                            Temp[k, 3] = Temp[k - 1, 3];
                        }
                        Temp[j, 0] = PublicVariables.HighScores[i, 0];
                        Temp[j, 1] = PublicVariables.HighScores[i, 1];
                        Temp[j, 2] = PublicVariables.HighScores[i, 2];
                        Temp[j, 3] = PublicVariables.HighScores[i, 3];


                        if (i == PublicVariables.HighScores.GetLength(0) - 1)
                        {
                            PublicVariables.MostRecentScore[0] = (j + 1).ToString();
                        }
                        break;
                    }
                    else if ((Temp[j, 1] == PublicVariables.HighScores[i, 1]) && (Convert.ToInt32(Temp[j, 2]) < Convert.ToInt32(PublicVariables.HighScores[i, 2])))
                    {
                        for (int k = PublicVariables.HighScores.GetLength(0) - 1; k > j; k--)
                        {
                            Temp[k, 0] = Temp[k - 1, 0];
                            Temp[k, 1] = Temp[k - 1, 1];
                            Temp[k, 2] = Temp[k - 1, 2];
                            Temp[k, 3] = Temp[k - 1, 3];
                        }
                        Temp[j, 0] = PublicVariables.HighScores[i, 0];
                        Temp[j, 1] = PublicVariables.HighScores[i, 1];
                        Temp[j, 2] = PublicVariables.HighScores[i, 2];
                        Temp[j, 3] = PublicVariables.HighScores[i, 3];


                        if (i == PublicVariables.HighScores.GetLength(0) - 1)
                        {
                            PublicVariables.MostRecentScore[0] = (j + 1).ToString();
                        }
                        break;
                    }
                    else if ((Convert.ToInt32(Temp[j, 1]) == Convert.ToInt32(PublicVariables.HighScores[i, 1])) && (Convert.ToInt32(Temp[j, 2]) == Convert.ToInt32(PublicVariables.HighScores[i, 2])) && (Convert.ToInt32(Temp[j, 3]) > Convert.ToInt32(PublicVariables.HighScores[i, 3])))
                    {
                        for (int k = PublicVariables.HighScores.GetLength(0) - 1; k > j; k--)
                        {
                            Temp[k, 0] = Temp[k - 1, 0];
                            Temp[k, 1] = Temp[k - 1, 1];
                            Temp[k, 2] = Temp[k - 1, 2];
                            Temp[k, 3] = Temp[k - 1, 3];
                        }
                        Temp[j, 0] = PublicVariables.HighScores[i, 0];
                        Temp[j, 1] = PublicVariables.HighScores[i, 1];
                        Temp[j, 2] = PublicVariables.HighScores[i, 2];
                        Temp[j, 3] = PublicVariables.HighScores[i, 3];


                        if (i == PublicVariables.HighScores.GetLength(0) - 1)
                        {
                            PublicVariables.MostRecentScore[1] = (j + 1).ToString();
                        }
                        break;
                    }
                }
            }
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                PublicVariables.HighScores[i, 0] = Temp[i, 0];
                PublicVariables.HighScores[i, 1] = Temp[i, 1];
                PublicVariables.HighScores[i, 2] = Temp[i, 2];
                PublicVariables.HighScores[i, 3] = Temp[i, 3];
            }
        }

        #endregion

        #region Read/Write

        public static string[] ReadFromFile(string FileName, string[] ArraytoWriteTo)
        {
            string directory = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\MSolitaire";
            System.IO.Directory.CreateDirectory(@directory);
            FileIOPermission permissions = new FileIOPermission(FileIOPermissionAccess.Read, directory);
            permissions.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, directory + "\\" + FileName + ".txt");

            try
            {
                permissions.Demand();
            }
            catch (System.Security.SecurityException s)
            {
                Console.WriteLine(s.Message);
            }
            try
            {
                ArraytoWriteTo = System.IO.File.ReadAllLines(@"" + directory + "\\" + FileName + ".txt");
            }
            catch
            {
                System.IO.File.WriteAllLines(@"" + directory + "\\" + FileName + ".txt", ArraytoWriteTo);
            }

            return ArraytoWriteTo;
        }

        public static string[] WriteToFile(string FileName, string[] ArraytoWriteFrom)
        {
            string directory = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\MSolitaire";
            System.IO.Directory.CreateDirectory(@directory);
            FileIOPermission permissions = new FileIOPermission(FileIOPermissionAccess.Write, directory);
            permissions.AddPathList(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, directory + "\\" + FileName + ".txt");

            try
            {
                permissions.Demand();
            }
            catch (System.Security.SecurityException s)
            {
                Console.WriteLine(s.Message);
            }
            try
            {
                System.IO.File.WriteAllLines(@"" + directory + "\\" + FileName + ".txt", ArraytoWriteFrom);
            }
            catch (System.Security.SecurityException s)
            {
                Console.WriteLine(s.Message);
            }

            return ArraytoWriteFrom;
        }

        #endregion

        #region Updating

        public void DrawCard(bool newcards)
        {
            Random rnd = new Random();
            int Current = rnd.Next(84);
            int Next = rnd.Next(84);
            if (newcards == true)
            {
                play[0, 0] = deck.cards[Current].suit;
                play[0, 1] = deck.cards[Current].face;
                while (play[0, 0] == "joker")
                {
                    play[0, 0] = deck.cards[Current].suit;
                    play[0, 1] = deck.cards[Current].face;
                }
            }
            else
            {
                play[0, 0] = play[1, 0];
                play[0, 1] = play[1, 1];
            }
            play[1, 0] = deck.cards[Next].suit;
            play[1, 1] = deck.cards[Next].face;
            ChangeCardImage(CurrentCard, play[0, 0], play[0, 1]);
            ChangeCardImage(NextCard, play[1, 0], play[1, 1]);

            CurrentCard.Left = CurrentCardLocation[0];
            CurrentCard.Top = CurrentCardLocation[1];
            CurrentCard.BringToFront();
        }

        public void Update(int carddrop)
        {
            ///0: Discard
            ///1-4: Corresponding table stack
            switch (carddrop)
            {
                case 0:
                    if (discards[1, 0] == null && discards[0, 0] != null)
                    {
                        ChangeCardImage(DiscardFirst, discards[0, 0], discards[0, 1]);
                        DiscardLast.Image = DiscardFirst.InitialImage;
                        DiscardLast.Visible = true;
                        DiscardLast.SendToBack();

                    }
                    else if (discards[0, 0] == null)
                    {
                        DiscardFirst.Image = DiscardFirst.InitialImage;
                        DiscardLast.Image = DiscardFirst.InitialImage;
                        DiscardLast.Visible = false;
                        DiscardLast.SendToBack();
                    }
                    else
                    {
                        ChangeCardImage(DiscardFirst, discards[0, 0], discards[0, 1]);
                        ChangeCardImage(DiscardLast, discards[1, 0], discards[1, 1]);
                        DiscardLast.Visible = true;
                        DiscardLast.SendToBack();
                    }
                    CurrentCard.BringToFront();
                    break;
                case 1:
                    UpdateStack("One", 0);
                    CurrentCard.BringToFront();
                    break;
                case 2:
                    UpdateStack("Two", 1);
                    CurrentCard.BringToFront();
                    break;
                case 3:
                    UpdateStack("Three", 2);
                    CurrentCard.BringToFront();
                    break;
                case 4:
                    UpdateStack("Four", 3);
                    CurrentCard.BringToFront();
                    break;
                case 5:
                    if (Undos > 0)
                    {
                        TableHistory = AddArrayElement(TableHistory, lasttable);
                        DiscardHistory = AddArrayElement(DiscardHistory, lastdiscards);
                        PlayHistory = AddArrayElement(PlayHistory, lastplay);
                    }
                    break;
            }

            if (CheckEnd() == true)
            {
                MessageBox.Show("Congratulations! Your score is " + PublicVariables.Points[0] + ":" + PublicVariables.Points[1] + ":" + PublicVariables.Points[2], "Congratulations!", MessageBoxButtons.OK);
                SaveScore();
                NewGame();
                highScoresToolStripMenuItem_Click(this, EventArgs.Empty);
            }
        }

        public void UpdateStack(string Stacknumber, int stacknumber)
        {
            PictureBox[] TablePositions = new PictureBox[8];
            for (int i = 0; i < 8; i++)
            {
                string Name = "" + Stacknumber + "_" + (i + 1) + "";
                TablePositions[i] = (PictureBox)Controls[Name];
            }
            for (int i = 0; i < 8; i++)
            {
                if (table[stacknumber * 2, i] == null)
                {
                    StackNumbers[stacknumber] = i - 1;
                    break;
                }
            }
            if (table[stacknumber * 2, 7] != null)
            {
                StackNumbers[stacknumber] = 7;
            }
            if (StackNumbers[stacknumber] == -1)
            {
                for (int i = 1; i < 8; i++)
                {
                    TablePositions[i].Visible = false;
                    TablePositions[i].Image = TablePositions[0].InitialImage;
                    TablePositions[i].Top = StackTopLocation + (i * 62);
                }

                TablePositions[0].Visible = true;
                TablePositions[0].Image = TablePositions[0].InitialImage;
                TablePositions[0].Top = StackTopLocation;
                TopStackControls[stacknumber] = TablePositions[0];
            }
            else
            {
                for (int i = 0; i <= StackNumbers[stacknumber]; i++)
                {
                    TablePositions[i].Visible = true;
                    ChangeCardImage(TablePositions[i], table[2 * stacknumber, i], table[2 * stacknumber + 1, i]);
                    TablePositions[i].Top = StackTopLocation + (i * 62);
                    TablePositions[i].BringToFront();
                }
                for (int i = StackNumbers[stacknumber] + 1; i < 8; i++)
                {
                    TablePositions[i].Visible = false;
                    TablePositions[i].Image = TablePositions[0].InitialImage;
                    TablePositions[i].Top = StackTopLocation + (i * 62);
                }
                TopStackControls[stacknumber] = TablePositions[StackNumbers[stacknumber]];
            }
        }

        public void Merge(int stacknumber, string playsuit)
        {
            if (StackNumbers[stacknumber] > 0)
            {
                string[] suits = new string[2];
                string[] face = new string[2];
                int[] SuitFaceIndexes = { (2 * stacknumber), (2 * stacknumber) + 1 };
                int[] StackPositions = { StackNumbers[stacknumber] - 1, StackNumbers[stacknumber] };
                if (playsuit == "")
                {
                    suits[0] = table[SuitFaceIndexes[0], StackPositions[0]];
                    suits[1] = table[SuitFaceIndexes[0], StackPositions[1]];
                    face[0] = table[SuitFaceIndexes[1], StackPositions[0]];
                    face[1] = table[SuitFaceIndexes[1], StackPositions[1]];
                }
                else
                {
                    suits[0] = table[SuitFaceIndexes[0], StackPositions[1]];
                    suits[1] = playsuit;
                    face[0] = table[SuitFaceIndexes[1], StackPositions[1]];
                    face[1] = table[SuitFaceIndexes[1], StackPositions[1]];
                }

                string Stacknumber = "";
                switch (stacknumber)
                {
                    case 0:
                        Stacknumber = "One_";
                        break;

                    case 1:
                        Stacknumber = "Two_";
                        break;

                    case 2:
                        Stacknumber = "Three_";
                        break;

                    case 3:
                        Stacknumber = "Four_";
                        break;
                }

                if (suits[1] == "joker")
                {
                    if ((TopStackControls[stacknumber].Top != Controls[Stacknumber + (StackNumbers[stacknumber])].Top) && playsuit == "")
                    {
                        Animate = true;
                        Animated = TopStackControls[stacknumber];
                        AnimatedOrigin = Animated.Top;
                        AnimatedDestination = Controls[Stacknumber + (StackNumbers[stacknumber])].Top;
                        AnimatedStack = stacknumber;
                    }
                    else if (Animate == false)
                    {
                        int newvalue = Convert.ToInt32(face[0]) + 1;
                        if (newvalue > Convert.ToInt32(PublicVariables.Points[1]))
                        {
                            PublicVariables.Points[1] = newvalue.ToString();
                        }
                        if (playsuit == "")
                        {
                            Animated.Top = AnimatedOrigin;
                            table[SuitFaceIndexes[0], StackPositions[1]] = null;
                            table[SuitFaceIndexes[1], StackPositions[1]] = null;
                            table[SuitFaceIndexes[1], StackPositions[0]] = (newvalue.ToString());
                        }
                        else
                        {
                            table[SuitFaceIndexes[1], StackPositions[1]] = (newvalue.ToString());
                        }
                        if (newvalue == 14)
                        {
                            discards = new string[2, 2];
                            Update(0);
                            for (int i = 0; i < 8; i++)
                            {
                                table[SuitFaceIndexes[0], i] = null;
                                table[SuitFaceIndexes[1], i] = null;
                            }
                            MessageBox.Show("Pair of Kings. Whole stack cleared! Discard pile also cleared. ", "Congratulations!");
                            int MergedKings = Convert.ToInt32(PublicVariables.Points[0]);
                            MergedKings++;
                            PublicVariables.Points[0] = MergedKings.ToString();
                            int MaxValue = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j <= StackNumbers[i]; j++)
                                {
                                    if (Convert.ToInt32(table[2 * i + 1, j]) > MaxValue)
                                    {
                                        MaxValue = Convert.ToInt32(table[2 * i + 1, j]);
                                    }
                                }
                            }
                            PublicVariables.Points[1] = MaxValue.ToString();
                        }
                        Update(stacknumber + 1);
                        Merge(stacknumber, "");
                    }
                }
                else if (face[0] == face[1])
                {
                    if ((TopStackControls[stacknumber].Top != Controls[Stacknumber + (StackNumbers[stacknumber])].Top) && playsuit == "")
                    {
                        Animate = true;
                        Animated = TopStackControls[stacknumber];
                        AnimatedOrigin = Animated.Top;
                        AnimatedDestination = Controls[Stacknumber + (StackNumbers[stacknumber])].Top;
                        AnimatedStack = stacknumber;
                    }
                    else if (Animate == false)
                    {
                        int newvalue = Convert.ToInt32(face[0]) + 1;
                        if (newvalue > Convert.ToInt32(PublicVariables.Points[1]))
                        {
                            PublicVariables.Points[1] = newvalue.ToString();
                        }
                        if (playsuit == "")
                        {
                            Animated.Top = AnimatedOrigin;
                            table[SuitFaceIndexes[0], StackPositions[1]] = null;
                            table[SuitFaceIndexes[1], StackPositions[1]] = null;
                            table[SuitFaceIndexes[1], StackPositions[0]] = ((Convert.ToInt32(face[0]) + 1).ToString());
                        }
                        else
                        {
                            table[SuitFaceIndexes[1], StackPositions[1]] = (newvalue.ToString());
                        }
                        if (newvalue == 14)
                        {
                            discards = new string[2, 2];
                            Update(0);
                            for (int i = 0; i < 8; i++)
                            {
                                table[SuitFaceIndexes[0], i] = null;
                                table[SuitFaceIndexes[1], i] = null;
                            }
                            MessageBox.Show("Pair of Kings. Whole stack cleared! Discard pile also cleared. ", "Congratulations!");
                            int MergedKings = Convert.ToInt32(PublicVariables.Points[0]);
                            MergedKings++;
                            PublicVariables.Points[0] = MergedKings.ToString();
                            int MaxValue = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j <= StackNumbers[i]; j++)
                                {
                                    if (Convert.ToInt32(table[2 * i + 1, j]) > MaxValue)
                                    {
                                        MaxValue = Convert.ToInt32(table[2 * i + 1, j]);
                                    }
                                }
                            }
                            PublicVariables.Points[1] = MaxValue.ToString();
                        }
                        else
                        {
                            int position = 0;
                            if (playsuit == "")
                            {
                                position = StackPositions[0];
                            }
                            else
                            {
                                position = StackPositions[1];
                            }
                            if ((suits[0] == "clubs" && (suits[1] == "spades" || suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "clubs" && (suits[0] == "spades" || suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "spades" && suits[1] == "spades"))
                            {
                                table[SuitFaceIndexes[0], position] = "hearts";
                            }
                            else if ((suits[0] == "spades" && (suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "spades" && (suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "hearts" && suits[1] == "hearts"))
                            {
                                table[SuitFaceIndexes[0], position] = "clubs";
                            }
                            else if ((suits[0] == "hearts" && suits[1] == "diamonds") || (suits[1] == "hearts" && suits[0] == "diamonds"))
                            {
                                table[SuitFaceIndexes[0], position] = "spades";
                            }
                            else if ((suits[0] == "clubs" && suits[1] == "clubs"))
                            {
                                table[SuitFaceIndexes[0], position] = "diamonds";
                            }

                            if (suits[1] == "spades" && suits[0] == "spades")
                            {
                                Undos++;
                                UndoButton.Text = "Undo (" + Undos + ")";
                                UndoButton.Enabled = true;
                                TableHistory = Resize3dArray(TableHistory, Undos);
                                DiscardHistory = Resize3dArray(DiscardHistory, Undos);
                                PlayHistory = Resize3dArray(PlayHistory, Undos);
                                MessageBox.Show("Pair of Spades. Extra Undo!", "Undo!");
                                Update(5);
                            }
                            if (suits[1] == "diamonds" && suits[0] == "diamonds")
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    table[(2 * stacknumber), i] = null;
                                    table[(2 * stacknumber) + 1, i] = null;
                                }
                                MessageBox.Show("Pair of Diamonds. Whole stack cleared!", "Bomb!");
                                int MaxValue = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    for (int j = 0; j <= StackNumbers[i]; j++)
                                    {
                                        if (Convert.ToInt32(table[2 * i + 1, j]) > MaxValue)
                                        {
                                            MaxValue = Convert.ToInt32(table[2 * i + 1, j]);
                                        }
                                    }
                                }
                                PublicVariables.Points[1] = MaxValue.ToString();
                            }
                        }
                        Update(stacknumber + 1);
                        Merge(stacknumber, "");
                    }
                }
            }
        }

        public void ChangeCardImage(PictureBox Slot, string Suit, string Face)
        {
            string face = Face;
            if (face == "1")
            {
                face = "A";
            }
            else if (face == "11")
            {
                face = "J";
            }
            else if (face == "12")
            {
                face = "Q";
            }
            else if (face == "13")
            {
                face = "K";
            }

            if (Suit == "joker")
            {
                Slot.Image = Image.FromFile(PublicVariables.Settings[2] + "/joker_small.png");
            }
            else
            {
                Slot.Image = Image.FromFile(PublicVariables.Settings[2] + "/" + Suit + "_" + face + "_small.png");
            }
        }

        public bool CheckEnd()
        {
            bool Check = false;

            if (StackNumbers[0] == 7 && StackNumbers[1] == 7 && StackNumbers[2] == 7 && StackNumbers[3] == 7)
            {
                if (play[0, 0] != "joker")
                {
                    if (play[0, 1] != table[7, 1])
                    {
                        if (play[0, 1] != table[7, 3])
                        {
                            if (play[0, 1] != table[7, 5])
                            {
                                if (play[0, 1] != table[7, 7])
                                {
                                    CheckPotentialMerges();
                                    for (int i = 0; i < 6; i++)
                                    {
                                        if (PotentialMergers[i, 0] != -1)
                                        {
                                            Check = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Check;
        }

        public void CheckPotentialMerges()
        {
            timescalled++;
            PotentialMergers[0,0] = -1;
            PotentialMergers[0,1] = -1;
            PotentialMergers[1,0] = -1;
            PotentialMergers[1,1] = -1;
            PotentialMergers[2,0] = -1;
            PotentialMergers[2, 1] = -1;
            PotentialMergers[3, 0] = -1;
            PotentialMergers[3, 1] = -1;
            PotentialMergers[4, 0] = -1;
            PotentialMergers[4, 1] = -1;
            PotentialMergers[5, 0] = -1;
            PotentialMergers[5, 1] = -1;

            if (Convert.ToInt32(discards[0, 1]) > 6)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (StackNumbers[j] > -1 && table[(2*j),StackNumbers[j]] == discards[0, 0] && table[(2 * j) + 1,StackNumbers[j]] == discards[0, 1])
                    {
                        PotentialMergers[j, 0] = 4;
                        PotentialMergers[j, 1] = 4;
                        PotentialMergers[4, 0] = j;
                        PotentialMergers[4, 1] = j;
                        break;
                    }
                }
            }
            if (Convert.ToInt32(discards[1, 1]) > 6)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (StackNumbers[j] > -1 && table[(2 * j), StackNumbers[j]] == discards[1, 0] && table[(2 * j) + 1, StackNumbers[j]] == discards[1, 1])
                    {
                        if (PotentialMergers[j,0] == -1 && PotentialMergers[5,0] == -1)
                        {
                            PotentialMergers[j, 0] = 5;
                            PotentialMergers[j, 1] = 5;
                            PotentialMergers[5, 0] = j;
                            PotentialMergers[5, 1] = j;
                            break;
                        }
                        else if (PotentialMergers[j, 0] != -1)
                        {
                            PotentialMergers[5, 0] = j;
                            PotentialMergers[5, 1] = PotentialMergers[j, 1];
                            PotentialMergers[j, 1] = 5;
                            PotentialMergers[PotentialMergers[j, 1], 0] = 5;
                        }
                        else
                        {
                            PotentialMergers[j, 0] = 5;
                            PotentialMergers[j, 1] = PotentialMergers[5, 1];
                            PotentialMergers[5, 1] = j;
                            PotentialMergers[PotentialMergers[j, 1], 0] = j;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (StackNumbers[i] > -1 && StackNumbers[j] > -1 && table[2*i,StackNumbers[i]] == table[2*j,StackNumbers[j]] && table[(2 * i) + 1,StackNumbers[i]] == table[(2 * j) + 1,StackNumbers[j]])
                    {
                        if (PotentialMergers[i, 0] == -1 && PotentialMergers[j, 0] == -1)
                        {
                            PotentialMergers[i, 0] = j;
                            PotentialMergers[i, 1] = j;
                            PotentialMergers[j, 0] = i;
                            PotentialMergers[j, 1] = i;
                        }
                        else if (PotentialMergers[i, 0] != -1 && PotentialMergers[j, 0] == -1)
                        {
                            PotentialMergers[j, 0] = i;
                            PotentialMergers[i, 1] = j;
                            PotentialMergers[PotentialMergers[i, 1], 0] = j;
                            PotentialMergers[j, 1] = PotentialMergers[i, 1];
                        }
                        else if (PotentialMergers[i,0] == -1 && PotentialMergers[j,0] != -1)
                        {
                            PotentialMergers[i, 0] = j;
                            PotentialMergers[j, 1] = i;
                            PotentialMergers[PotentialMergers[j, 1], 0] = i;
                            PotentialMergers[i, 1] = PotentialMergers[j, 1];
                        }
                        else if (PotentialMergers[i, 0] != -1 && PotentialMergers[j, 0] == -1)
                        {
                            PotentialMergers[i, 0] = j;
                            PotentialMergers[j, 1] = i;
                            PotentialMergers[PotentialMergers[j, 1], 0] = PotentialMergers[i, 0];
                            PotentialMergers[PotentialMergers[i, 0], 1] = PotentialMergers[j, 1];
                        }
                    }
                }
            }
            if (Convert.ToInt32(discards[0, 1]) > 6)
            {
                DiscardFirst.MouseDown += Generic_MouseDown;
                DiscardFirst.MouseMove += Generic_MouseMove;
                DiscardFirst.MouseUp += Generic_MouseUp;
            }
            else
            {
                DiscardFirst.MouseDown -= Generic_MouseDown;
                DiscardFirst.MouseMove -= Generic_MouseMove;
                DiscardFirst.MouseUp -= Generic_MouseUp;
            }
            if (Convert.ToInt32(discards[1, 1]) > 6)
            {
                DiscardLast.MouseDown += Generic_MouseDown;
                DiscardLast.MouseMove += Generic_MouseMove;
                DiscardLast.MouseUp += Generic_MouseUp;
            }
            else
            {
                DiscardLast.MouseDown -= Generic_MouseDown;
                DiscardLast.MouseMove -= Generic_MouseMove;
                DiscardLast.MouseUp -= Generic_MouseUp;
            }
            if (PotentialMergers[0, 0] > -1 && PotentialMergers[0, 0] < 4)
            {

                TopStackControls[0].MouseDown += Generic_MouseDown;
                TopStackControls[0].MouseMove += Generic_MouseMove;
                TopStackControls[0].MouseUp += Generic_MouseUp;
            }
            else
            {
                TopStackControls[0].MouseDown -= Generic_MouseDown;
                TopStackControls[0].MouseMove -= Generic_MouseMove;
                TopStackControls[0].MouseUp -= Generic_MouseUp;
            }
            if (PotentialMergers[1, 0] > -1 && PotentialMergers[1, 0] < 4)
            {
                TopStackControls[1].MouseDown += Generic_MouseDown;
                TopStackControls[1].MouseMove += Generic_MouseMove;
                TopStackControls[1].MouseUp += Generic_MouseUp;
            }
            else
            {
                TopStackControls[1].MouseDown -= Generic_MouseDown;
                TopStackControls[1].MouseMove -= Generic_MouseMove;
                TopStackControls[1].MouseUp -= Generic_MouseUp;
            }
            if (PotentialMergers[2, 0] > -1 && PotentialMergers[2, 0] < 4)
            {
                TopStackControls[2].MouseDown += Generic_MouseDown;
                TopStackControls[2].MouseMove += Generic_MouseMove;
                TopStackControls[2].MouseUp += Generic_MouseUp;
            }
            else
            {
                TopStackControls[2].MouseDown -= Generic_MouseDown;
                TopStackControls[2].MouseMove -= Generic_MouseMove;
                TopStackControls[2].MouseUp -= Generic_MouseUp;
            }
            if (PotentialMergers[3, 0] > -1 && PotentialMergers[3, 0] < 4)
            {
                TopStackControls[3].MouseDown += Generic_MouseDown;
                TopStackControls[3].MouseMove += Generic_MouseMove;
                TopStackControls[3].MouseUp += Generic_MouseUp;
            }
            else
            {
                TopStackControls[3].MouseDown -= Generic_MouseDown;
                TopStackControls[3].MouseMove -= Generic_MouseMove;
                TopStackControls[3].MouseUp -= Generic_MouseUp;
            }
        }

        #endregion

        #region 3D Array Manipulation

        public string[, ,] Resize3dArray(string[, ,] Array, int newsize)
        {
            string[, ,] Temp = new string[newsize, Array.GetLength(1), Array.GetLength(2)];

            for (int i = 0; i < Math.Min(newsize, Array.GetLength(0)); i++)
            {
                for (int j = 0; j < Array.GetLength(1); j++)
                {
                    for (int k = 0; k < Array.GetLength(2); k++)
                    {
                        Temp[i, j, k] = Array[i, j, k];
                    }
                }
            }

            return Temp;
        }

        public string[, ,] AddArrayElement(string[, ,] Array, string[,] NewElement)
        {
            for (int j = 0; j < Array.GetLength(1); j++)
            {
                for (int k = 0; k < Array.GetLength(2); k++)
                {
                    Array[(Array.GetLength(0) - 1), j, k] = NewElement[j, k];
                }
            }

            return Array;
        }

        public string[,] RemoveArrayElement(string[, ,] Array)
        {
            string[,] Element = new string[Array.GetLength(1), Array.GetLength(2)];

            for (int j = 0; j < Array.GetLength(1); j++)
            {
                for (int k = 0; k < Array.GetLength(2); k++)
                {
                    Element[j, k] = Array[(Array.GetLength(0) - 1), j, k];
                }
            }

            return Element;
        }

        #endregion

        #region Current Card Moving

        private void Generic_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                tracking = true;
                MovingCard = (PictureBox)sender;
                MovingCardOrigin[0] = MovingCard.Top;
                MovingCardOrigin[1] = MovingCard.Left;
                _xPos = e.X;
                _yPos = e.Y;
                MovingCard.BringToFront();
            }
        }

        private void Generic_MouseMove(object sender, MouseEventArgs e)
        {
            if (tracking == true)
            {
                mouse_xPos = e.X;
                mouse_yPos = e.Y;

                switch (cardmove)
                {
                    case 0:
                        if ((MovingCard == DiscardFirst && DiscardLast.Bounds.IntersectsWith(MovingCard.Bounds) == false) || (MovingCard != DiscardFirst && DiscardFirst.Bounds.IntersectsWith(MovingCard.Bounds) == false))
                        {
                            cardmove = -1;
                        }
                        break;
                    case 1:
                        if (MovingCard != TopStackControls[0] && TopStackControls[0].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                        {
                            cardmove = -1;
                        }
                        break;
                    case 2:
                        if (MovingCard != TopStackControls[1] && TopStackControls[1].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                        {
                            cardmove = -1;
                        }
                        break;
                    case 3:
                        if (MovingCard != TopStackControls[2] && TopStackControls[2].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                        {
                            cardmove = -1;
                        }
                        break;
                    case 4:
                        if (MovingCard != TopStackControls[3] && TopStackControls[3].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                        {
                            cardmove = -1;
                        }
                        break;
                    default:
                        if ((MovingCard == DiscardFirst && DiscardLast.Bounds.IntersectsWith(MovingCard.Bounds) == true) || (MovingCard != DiscardFirst && DiscardFirst.Bounds.IntersectsWith(MovingCard.Bounds) == true))
                        {
                            cardmove = 0;
                            DiscardFirst.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                            DiscardLast.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                        }
                        if (MovingCard != TopStackControls[0] && TopStackControls[0].Bounds.IntersectsWith(MovingCard.Bounds) == true)
                        {
                            cardmove = 1;
                            TopStackControls[0].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                        }
                        if (MovingCard != TopStackControls[1] && TopStackControls[1].Bounds.IntersectsWith(MovingCard.Bounds) == true)
                        {
                            cardmove = 2;
                            TopStackControls[1].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                        }
                        if (MovingCard != TopStackControls[2] && TopStackControls[2].Bounds.IntersectsWith(MovingCard.Bounds) == true)
                        {
                            cardmove = 3;
                            TopStackControls[2].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                        }
                        if (MovingCard != TopStackControls[3] && TopStackControls[3].Bounds.IntersectsWith(MovingCard.Bounds) == true)
                        {
                            cardmove = 4;
                            TopStackControls[3].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/red_border.png");
                        }
                        break;
                }
            }
        }

        private void CurrentCard_MouseUp(object sender, MouseEventArgs e)
        {
            tracking = false;

            switch (cardmove)
            {
                case 0:
                    DiscardFirst.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    DiscardLast.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    if ((discards[1, 0] == null && play[0, 0] != "joker") && (play[0,0] != discards[0,0] || play[0,1] != discards[0,1]))
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }
                        discards[1, 0] = discards[0, 0];
                        discards[1, 1] = discards[0, 1];
                        discards[0, 0] = play[0, 0];
                        discards[0, 1] = play[0, 1];
                        DrawCard(false);
                        Update(0);
                    }
                    else if (play[0, 0] == discards[0, 0] && play[0, 1] == discards[0, 1])
                    {
                        int newvalue = Convert.ToInt32(discards[0, 1]) + 1;
                        discards[0, 1] = newvalue.ToString();
                        string[] suits = { play[0, 0], discards[0, 0] };
                        if ((suits[0] == "clubs" && (suits[1] == "spades" || suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "clubs" && (suits[0] == "spades" || suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "spades" && suits[1] == "spades"))
                        {
                            discards[0, 0] = "hearts";
                        }
                        else if ((suits[0] == "spades" && (suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "spades" && (suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "hearts" && suits[1] == "hearts"))
                        {
                            discards[0, 0] = "clubs";
                        }
                        else if ((suits[0] == "hearts" && suits[1] == "diamonds") || (suits[1] == "hearts" && suits[0] == "diamonds"))
                        {
                            discards[0, 0] = "spades";
                        }
                        else if ((suits[0] == "clubs" && suits[1] == "clubs"))
                        {
                            discards[0, 0] = "diamonds";
                        }
                        if (suits[1] == "diamonds" && suits[0] == "diamonds")
                        {
                            MessageBox.Show("Pair of Diamonds. Discard slot cleared!", "Bomb!");
                            discards[0, 0] = discards[1, 0];
                            discards[0, 1] = discards[1, 1];
                            discards[1, 0] = null;
                            discards[1, 1] = null;
                        }
                        DrawCard(false);
                        Update(0);
                        if (discards[0, 0] == discards[1, 0] && discards[0, 1] == discards[1, 1])
                        {
                            Animate = true;
                            Animated = DiscardLast;
                            AnimatedOrigin = Animated.Top;
                            AnimatedDestination = DiscardFirst.Top;
                        }
                    }
                    else if (play[0, 0] == discards[1, 0] && play[0, 1] == discards[1, 1])
                    {
                        int newvalue = Convert.ToInt32(discards[1, 1]) + 1;
                        discards[1, 1] = newvalue.ToString();
                        string[] suits = { play[0, 0], discards[1, 0] };
                        if ((suits[0] == "clubs" && (suits[1] == "spades" || suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "clubs" && (suits[0] == "spades" || suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "spades" && suits[1] == "spades"))
                        {
                            discards[1, 0] = "hearts";
                        }
                        else if ((suits[0] == "spades" && (suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "spades" && (suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "hearts" && suits[1] == "hearts"))
                        {
                            discards[1, 0] = "clubs";
                        }
                        else if ((suits[0] == "hearts" && suits[1] == "diamonds") || (suits[1] == "hearts" && suits[0] == "diamonds"))
                        {
                            discards[1, 0] = "spades";
                        }
                        else if ((suits[0] == "clubs" && suits[1] == "clubs"))
                        {
                            discards[1, 0] = "diamonds";
                        }
                        if (suits[1] == "diamonds" && suits[0] == "diamonds")
                        {
                            MessageBox.Show("Pair of Diamonds. Discard slot cleared!", "Bomb!");
                            discards[1, 0] = null;
                            discards[1, 1] = null;
                        }
                        DrawCard(false);
                        Update(0);
                        if (discards[0, 0] == discards[1, 0] && discards[0, 1] == discards[1, 1])
                        {
                            Animate = true;
                            Animated = DiscardLast;
                            AnimatedOrigin = Animated.Top;
                            AnimatedDestination = DiscardFirst.Top;
                        }
                    }
                    else
                    {
                        goto default;
                    }
                    break;

                case 1:
                    TopStackControls[0].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    if (StackNumbers[0] < 7)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }
                        table[0, StackNumbers[0] + 1] = play[0, 0];
                        table[1, StackNumbers[0] + 1] = play[0, 1];
                        Update(1);
                        DrawCard(false);
                        Merge(0, "");
                        Update(1);
                    }
                    else if ((table[1, 7] == play[0, 1]) || play[0, 0] == "joker")
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }

                        DrawCard(false);
                        Merge(0, play[0, 0]);
                        Update(1);
                    }
                    else
                    {
                        goto default;
                    }
                    if (Convert.ToInt32(table[1, StackNumbers[0]]) > Convert.ToInt32(PublicVariables.Points[1]))
                    {
                        PublicVariables.Points[1] = table[1, StackNumbers[0]];
                    }
                    break;

                case 2:
                    TopStackControls[1].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    if (StackNumbers[1] < 7)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }
                        table[2, StackNumbers[1] + 1] = play[0, 0];
                        table[3, StackNumbers[1] + 1] = play[0, 1];
                        Update(2);
                        DrawCard(false);
                        Merge(1, "");
                        Update(2);
                    }
                    else if ((table[3, 7] == play[0, 1]) || play[0, 0] == "joker")
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }

                        DrawCard(false);
                        Merge(1, play[0, 0]);
                        Update(2);
                    }
                    else
                    {
                        goto default;
                    }
                    if (Convert.ToInt32(table[3, StackNumbers[1]]) > Convert.ToInt32(PublicVariables.Points[1]))
                    {
                        PublicVariables.Points[1] = table[3, StackNumbers[1]];
                    }
                    break;

                case 3:
                    TopStackControls[2].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    if (StackNumbers[2] < 7)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }
                        table[4, StackNumbers[2] + 1] = play[0, 0];
                        table[5, StackNumbers[2] + 1] = play[0, 1];
                        Update(3);
                        DrawCard(false);
                        Merge(2, "");
                        Update(3);
                    }
                    else if ((table[5, 7] == play[0, 1]) || play[0, 0] == "joker")
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }

                        DrawCard(false);
                        Merge(2, play[0, 0]);
                        Update(3);
                    }
                    else
                    {
                        goto default;
                    }
                    if (Convert.ToInt32(table[5, StackNumbers[2]]) > Convert.ToInt32(PublicVariables.Points[1]))
                    {
                        PublicVariables.Points[1] = table[5, StackNumbers[2]];
                    }
                    break;

                case 4:
                    TopStackControls[3].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    if (StackNumbers[3] < 7)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }
                        table[6, StackNumbers[3] + 1] = play[0, 0];
                        table[7, StackNumbers[3] + 1] = play[0, 1];
                        Update(4);
                        DrawCard(false);
                        Merge(3, "");
                        Update(4);
                    }
                    else if ((table[7, 7] == play[0, 1]) || play[0, 0] == "joker")
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                lasttable[i, j] = table[i, j];
                            }
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                lastdiscards[i, j] = discards[i, j];
                                lastplay[i, j] = play[i, j];
                            }
                        }
                        if (Undos > 0)
                        {
                            TableHistory = AddArrayElement(TableHistory, table);
                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                            PlayHistory = AddArrayElement(PlayHistory, play);
                        }

                        DrawCard(false);
                        Merge(3, play[0, 0]);
                        Update(4);
                    }
                    else
                    {
                        goto default;
                    }
                    if (Convert.ToInt32(table[7, StackNumbers[3]]) > Convert.ToInt32(PublicVariables.Points[1]))
                    {
                        PublicVariables.Points[1] = table[7, StackNumbers[3]];
                    }
                    break;

                default:
                    CurrentCard.Left = CurrentCardLocation[0];
                    CurrentCard.Top = CurrentCardLocation[1];
                    break;
            }

            CurrentCard.BringToFront();
            CheckPotentialMerges();
        }

        private void Generic_MouseUp(object sender, MouseEventArgs e)
        {
            if (tracking == true)
            {
                tracking = false;
                timescalled = 0;
                CheckPotentialMerges();
                bool moved = false;
                int MovingCardID = -1;
                string[] card = new string[2];
                if (MovingCard == DiscardFirst)
                {
                    MovingCardID = 4;
                    card[0] = discards[0, 0];
                    card[1] = discards[0, 1];
                    DiscardFirst.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    DiscardLast.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                else if (MovingCard == DiscardLast)
                {
                    MovingCardID = 5;
                    card[0] = discards[1, 0];
                    card[1] = discards[1, 1];
                    DiscardFirst.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    DiscardLast.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                else if (MovingCard == TopStackControls[0])
                {
                    MovingCardID = 0;
                    card[0] = table[0, StackNumbers[0]];
                    card[1] = table[1, StackNumbers[0]];
                }
                else if (MovingCard == TopStackControls[1])
                {
                    MovingCardID = 1;
                    card[0] = table[2, StackNumbers[1]];
                    card[1] = table[3, StackNumbers[1]];
                }
                else if (MovingCard == TopStackControls[2])
                {
                    MovingCardID = 2;
                    card[0] = table[4, StackNumbers[2]];
                    card[1] = table[5, StackNumbers[2]];
                }
                else if (MovingCard == TopStackControls[3])
                {
                    MovingCardID = 3;
                    card[0] = table[6, StackNumbers[3]];
                    card[1] = table[7, StackNumbers[3]];
                }

                if (MovingCardID > -1)
                {
                    switch (cardmove)
                    {
                        case 1:
                            TopStackControls[0].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                            if (PotentialMergers[MovingCardID, 0] != -1 && PotentialMergers[0, 0] != -1)
                            {
                                bool Check = false;
                                int ID = MovingCardID;
                                while (true)
                                {
                                    ID = PotentialMergers[ID, 0];
                                    if (ID == 0)
                                    {
                                        Check = true;
                                        break;
                                    }
                                    if (ID == MovingCardID)
                                    {
                                        break;
                                    }
                                }
                                if (Check == true)
                                {
                                    if (StackNumbers[0] < 7)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }
                                        table[0, StackNumbers[0] + 1] = card[0];
                                        table[1, StackNumbers[0] + 1] = card[1];
                                        Update(1);
                                        Merge(0, "");
                                        Update(1);
                                        moved = true;
                                    }
                                    else if ((table[1, 7] == card[1]) || card[0] == "joker")
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }

                                        Merge(0, card[0]);
                                        Update(1);
                                        moved = true;
                                    }
                                    if (Convert.ToInt32(table[1, StackNumbers[0]]) > Convert.ToInt32(PublicVariables.Points[1]))
                                    {
                                        PublicVariables.Points[1] = table[1, StackNumbers[0]];
                                    }
                                }
                            }
                            break;

                        case 2:
                            TopStackControls[1].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                            if (PotentialMergers[MovingCardID, 0] != -1 && PotentialMergers[1, 0] != -1)
                            {
                                bool Check = false;
                                int ID = MovingCardID;
                                while (true)
                                {
                                    ID = PotentialMergers[ID, 0];
                                    if (ID == 1)
                                    {
                                        Check = true;
                                        break;
                                    }
                                    if (ID == MovingCardID)
                                    {
                                        break;
                                    }
                                }
                                if (Check == true)
                                {
                                    if (StackNumbers[1] < 7)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }
                                        table[2, StackNumbers[1] + 1] = card[0];
                                        table[3, StackNumbers[1] + 1] = card[1];
                                        Update(2);
                                        Merge(1, "");
                                        Update(2);
                                        moved = true;
                                    }
                                    else if ((table[3, 7] == card[1]) || card[0] == "joker")
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }

                                        Merge(1, card[0]);
                                        Update(2);
                                        moved = true;
                                    }
                                    if (Convert.ToInt32(table[3, StackNumbers[1]]) > Convert.ToInt32(PublicVariables.Points[1]))
                                    {
                                        PublicVariables.Points[1] = table[3, StackNumbers[1]];
                                    }
                                }
                            }
                            break;

                        case 3:
                            TopStackControls[2].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                            if (PotentialMergers[MovingCardID, 0] != -1 && PotentialMergers[2, 0] != -1)
                            {
                                bool Check = false;
                                int ID = MovingCardID;
                                while (true)
                                {
                                    ID = PotentialMergers[ID, 0];
                                    if (ID == 2)
                                    {
                                        Check = true;
                                        break;
                                    }
                                    if (ID == MovingCardID)
                                    {
                                        break;
                                    }
                                }
                                if (Check == true)
                                {
                                    if (StackNumbers[2] < 7)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }
                                        table[4, StackNumbers[2] + 1] = card[0];
                                        table[5, StackNumbers[2] + 1] = card[1];
                                        Update(3);
                                        Merge(2, "");
                                        Update(3);
                                        moved = true;
                                    }
                                    else if ((table[5, 7] == card[1]) || card[0] == "joker")
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }

                                        Merge(2, card[0]);
                                        Update(3);
                                        moved = true;
                                    }
                                    if (Convert.ToInt32(table[5, StackNumbers[2]]) > Convert.ToInt32(PublicVariables.Points[1]))
                                    {
                                        PublicVariables.Points[1] = table[5, StackNumbers[2]];
                                    }
                                }
                            }
                            break;

                        case 4:
                            TopStackControls[3].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                            if (PotentialMergers[MovingCardID, 0] != -1 && PotentialMergers[3, 0] != -1)
                            {
                                bool Check = false;
                                int ID = MovingCardID;
                                while (true)
                                {
                                    ID = PotentialMergers[ID, 0];
                                    if (ID == 3)
                                    {
                                        Check = true;
                                        break;
                                    }
                                    if (ID == MovingCardID)
                                    {
                                        break;
                                    }
                                }
                                if (Check == true)
                                {
                                    if (StackNumbers[3] < 7)
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }
                                        table[6, StackNumbers[3] + 1] = card[0];
                                        table[7, StackNumbers[3] + 1] = card[1];
                                        Update(4);
                                        Merge(3, "");
                                        Update(4);
                                        moved = true;
                                    }
                                    else if ((table[7, 7] == card[1]) || card[0] == "joker")
                                    {
                                        for (int i = 0; i < 8; i++)
                                        {
                                            for (int j = 0; j < 8; j++)
                                            {
                                                lasttable[i, j] = table[i, j];
                                            }
                                        }
                                        for (int i = 0; i < 2; i++)
                                        {
                                            for (int j = 0; j < 2; j++)
                                            {
                                                lastdiscards[i, j] = discards[i, j];
                                                lastplay[i, j] = play[i, j];
                                            }
                                        }
                                        if (Undos > 0)
                                        {
                                            TableHistory = AddArrayElement(TableHistory, table);
                                            DiscardHistory = AddArrayElement(DiscardHistory, discards);
                                            PlayHistory = AddArrayElement(PlayHistory, play);
                                        }

                                        Merge(3, card[0]);
                                        Update(4);
                                        moved = true;
                                    }
                                    if (Convert.ToInt32(table[7, StackNumbers[3]]) > Convert.ToInt32(PublicVariables.Points[1]))
                                    {
                                        PublicVariables.Points[1] = table[7, StackNumbers[3]];
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("NO SENDER");
                }
                MovingCard.Top = MovingCardOrigin[0];
                MovingCard.Left = MovingCardOrigin[1];
                CurrentCard.BringToFront();
                if (moved == true)
                {
                    switch (MovingCardID)
                    {
                        case 0:
                            table[0, StackNumbers[0]] = null;
                            table[1, StackNumbers[0]] = null;
                            Update(1);
                            break;
                        case 1:
                            table[2, StackNumbers[1]] = null;
                            table[3, StackNumbers[1]] = null;
                            Update(2);
                            break;
                        case 2:
                            table[4, StackNumbers[2]] = null;
                            table[5, StackNumbers[2]] = null;
                            Update(3);
                            break;
                        case 3:
                            table[6, StackNumbers[3]] = null;
                            table[7, StackNumbers[3]] = null;
                            Update(4);
                            break;
                        case 4:
                            discards[0, 0] = null;
                            discards[0, 1] = null;
                            Update(0);
                            break;
                        case 5:
                            table[0, 0] = table[1, 0];
                            table[0, 1] = table[1, 1];
                            table[1, 0] = null;
                            table[1, 1] = null;
                            Update(0);
                            break;
                    }
                }
                CheckPotentialMerges();
                MessageBox.Show(timescalled.ToString());
            }
        }

        #endregion

        #region Timers

        private void timer_move_Tick(object sender, EventArgs e)
        {
            if (tracking == true)
            {
                MovingCard.Top = mouse_yPos + MovingCard.Top - _yPos;
                MovingCard.Left = mouse_xPos + MovingCard.Left - _xPos;
            }

            if (Animate == true)
            {
                double AnimationVelocity = 3.0;
                int Yincrement = (AnimatedDestination - AnimatedOrigin) / Math.Abs(AnimatedDestination - AnimatedOrigin);
                double Ychange = Animated.Top + (AnimationVelocity * Yincrement);

                if (Animated.Top - AnimatedDestination > AnimationVelocity)
                {
                    Animated.Top = (int)Ychange;
                }
                else
                {
                    Animated.Top = AnimatedDestination;
                    Animate = false;
                    Animated.Visible = false;
                    if (Animated != DiscardLast)
                    {
                        Merge(AnimatedStack, "");
                        CheckPotentialMerges();
                    }
                    else
                    {
                        DiscardLast.Top = AnimatedOrigin;
                        int newvalue = Convert.ToInt32(discards[0, 1]) + 1;
                        discards[0, 1] = newvalue.ToString();
                        string[] suits = { discards[0, 0], discards[1, 0] };
                        if ((suits[0] == "clubs" && (suits[1] == "spades" || suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "clubs" && (suits[0] == "spades" || suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "spades" && suits[1] == "spades"))
                        {
                            discards[0, 0] = "hearts";
                        }
                        else if ((suits[0] == "spades" && (suits[1] == "diamonds" || suits[1] == "hearts")) || (suits[1] == "spades" && (suits[0] == "diamonds" || suits[0] == "hearts")) || (suits[0] == "hearts" && suits[1] == "hearts"))
                        {
                            discards[0, 0] = "clubs";
                        }
                        else if ((suits[0] == "hearts" && suits[1] == "diamonds") || (suits[1] == "hearts" && suits[0] == "diamonds"))
                        {
                            discards[0, 0] = "spades";
                        }
                        else if ((suits[0] == "clubs" && suits[1] == "clubs"))
                        {
                            discards[0, 0] = "diamonds";
                        }
                        discards[1, 0] = null;
                        discards[1, 1] = null;
                        if (suits[1] == "diamonds" && suits[0] == "diamonds")
                        {
                            MessageBox.Show("Pair of Diamonds. Discard slot cleared!", "Bomb!");
                            discards[0, 0] = discards[1, 0];
                            discards[0, 1] = discards[1, 1];
                            discards[1, 0] = null;
                            discards[1, 1] = null;
                        }
                        Update(0);
                        CheckPotentialMerges();
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (tracking == true)
            {
                if ((MovingCard == DiscardFirst && DiscardLast.Bounds.IntersectsWith(MovingCard.Bounds) == false) || (MovingCard != DiscardFirst && DiscardFirst.Bounds.IntersectsWith(MovingCard.Bounds) == false))
                {
                    DiscardFirst.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                    DiscardLast.BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                if (TopStackControls[0].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                {
                    TopStackControls[0].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                if (TopStackControls[1].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                {
                    TopStackControls[1].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                if (TopStackControls[2].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                {
                    TopStackControls[2].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
                if (TopStackControls[3].Bounds.IntersectsWith(MovingCard.Bounds) == false)
                {
                    TopStackControls[3].BackgroundImage = Image.FromFile("D:/Documents/Programming/MSolitaire/ui/normal_border.png");
                }
            }

            double Time = Convert.ToDouble(PublicVariables.Points[2]);
            if (this.Focused == true)
            {
                Time = Time + 0.1;
            }
            PublicVariables.Points[2] = Time.ToString();
            string face = PublicVariables.Points[1];
            if (face == "1")
            {
                face = "A";
            }
            else if (face == "11")

            {
                face = "J";
            }
            else if (face == "12")
            {
                face = "Q";
            }
            else if (face == "13")
            {
                face = "K";
            }
            Label_Points.Text = PublicVariables.Points[0] + ":" + face + ":" + (int)Time;
        }

        #endregion

        private void UndoButton_Click(object sender, EventArgs e)
        {
            if (Undos > 0)
            {
                Undos--;
                table = RemoveArrayElement(TableHistory);
                discards = RemoveArrayElement(DiscardHistory);
                play = RemoveArrayElement(PlayHistory);
                TableHistory = Resize3dArray(TableHistory, Undos);
                DiscardHistory = Resize3dArray(DiscardHistory, Undos);
                PlayHistory = Resize3dArray(PlayHistory, Undos);
                ChangeCardImage(CurrentCard, play[0, 0], play[0, 1]);
                ChangeCardImage(NextCard, play[1, 0], play[1, 1]);
                Update(0);
                Update(1);
                Update(2);
                Update(3);
                Update(4);
                UndoButton.Text = "Undo (" + Undos + ")";
            }
            if (Undos == 0)
            {
                UndoButton.Enabled = false;
            }
        }

        #region ToolStripMenu Actions

        private void helpToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form Help = new Help();

            Help.Show();
        }

        private void highScoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string[] HighScores1D = new string[PublicVariables.HighScores.GetLength(0)+1];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                HighScores1D[i] = PublicVariables.HighScores[i, 0] + "\t" + PublicVariables.HighScores[i, 1] + ":" + PublicVariables.HighScores[i, 2] + ":" + PublicVariables.HighScores[i, 3];
            }
            HighScores1D[PublicVariables.HighScores.GetLength(0)] = PublicVariables.MostRecentScore[0] + ":" + PublicVariables.MostRecentScore[1] + "\t" + PublicVariables.MostRecentScore[2] + ":" + PublicVariables.MostRecentScore[3] + ":" + PublicVariables.MostRecentScore[4];

            HighScores1D = ReadFromFile("High Scores", HighScores1D);

            if (HighScores1D.GetLength(0) > 1)
            {
                string Recent = HighScores1D[HighScores1D.GetLength(0) - 1];
                Array.Resize<string>(ref HighScores1D, HighScores1D.GetLength(0) - 1);

                PublicVariables.HighScores = new string[HighScores1D.GetLength(0), 4];

                for (int i = 0; i < HighScores1D.GetLength(0); i++)
                {
                    string[] Separated = HighScores1D[i].Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                    PublicVariables.HighScores[i, 0] = Separated[0];
                    PublicVariables.HighScores[i, 1] = Separated[1];
                    PublicVariables.HighScores[i, 2] = Separated[2];
                    PublicVariables.HighScores[i, 3] = Separated[3];
                }
                string[] Components = Recent.Split(PublicVariables.separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                PublicVariables.MostRecentScore[0] = Components[0];
                PublicVariables.MostRecentScore[1] = Components[1];
                PublicVariables.MostRecentScore[2] = Components[2];
                PublicVariables.MostRecentScore[3] = Components[3];
                PublicVariables.MostRecentScore[4] = Components[4];
            }
            else
            {
                PublicVariables.HighScores = new string[0, 4];
                PublicVariables.MostRecentScore = new string[5];
            }

            Form HighScore = new HighScores();

            HighScore.Show();
        }

        private void endGameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MSolitaire.ActiveForm.Enabled = false;
            timer.Stop();
            if (MessageBox.Show("Are you sure you want to exit the current game?", "Exit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SaveScore();
                NewGame();
                highScoresToolStripMenuItem_Click(sender, e);
            }
            this.Enabled = true;
            timer.Start();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MSolitaire.ActiveForm.Enabled = false;
            timer.Stop();
            DialogResult DialogSaveScore = MessageBox.Show("Do you want to save your score?", "Save?", MessageBoxButtons.YesNoCancel);
            if (DialogSaveScore == DialogResult.Yes && MessageBox.Show("Are you sure you want to start a new game?", "New Game?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SaveScore();
                NewGame();
                highScoresToolStripMenuItem_Click(sender, e);
            }
            else if (DialogSaveScore == DialogResult.No && MessageBox.Show("Are you sure you want to start a new game?", "New Game?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                NewGame();
            }
            this.Enabled = true;
            timer.Start();
        }

#endregion

      }

    public class PublicVariables
    {
        public static string[] Settings = { "D:/Documents/Programming/MSolitaire/bkgnd", "D:/Documents/Programming/MSolitaire/back", "D:/Documents/Programming/MSolitaire/decks/stock", "D:/Documents/Programming/MSolitaire/ui" };

        public static string[,] HighScores = { { "Robert", "1","10","300" } };
        public static string[] MostRecentScore = { "1", "Robert", "1", "10", "300" };
        public static string[] separatingChars = { "\t", ":" };

        public static string[] Points = { "0", "0", "0" };
    }
}
