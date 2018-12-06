using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Solitaire;

namespace Solitaire
{
    public partial class HighScores : Form
    {
        string[] separatingChars = { ". " };
        string[] Separated = new string[2];

        public HighScores()
        {
            InitializeComponent();

            UpdateTables();
        }

        private void UpdateTables()
        {
            string[] HighScores1D = new string[PublicVariables.HighScores.GetLength(0)+1];
            for (int i = 0; i < PublicVariables.HighScores.GetLength(0); i++)
            {
                HighScores1D[i] = PublicVariables.HighScores[i, 0] + "\t" + PublicVariables.HighScores[i, 1] + ":" + PublicVariables.HighScores[i, 2] + ":" + PublicVariables.HighScores[i, 3];
            }

            HighScores1D[PublicVariables.HighScores.GetLength(0)] = PublicVariables.MostRecentScore[0] + ":" + PublicVariables.MostRecentScore[1] + "\t" + PublicVariables.MostRecentScore[2] + ":" + PublicVariables.MostRecentScore[3] + ":" + PublicVariables.MostRecentScore[4];

            HighScores1D = MSolitaire.WriteToFile("High Scores", HighScores1D);
            HighScores1D = MSolitaire.ReadFromFile("High Scores", HighScores1D);

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

            if (PublicVariables.MostRecentScore[2] != null)
            {
                HighScores_Recent.GetControlFromPosition(0, 1).Text = PublicVariables.MostRecentScore[0] + ". " + PublicVariables.MostRecentScore[1];
                HighScores_Recent.GetControlFromPosition(1, 1).Text = PublicVariables.MostRecentScore[2] + ":" + PublicVariables.MostRecentScore[3] + ":" + PublicVariables.MostRecentScore[4];
                RecentScore.Visible = true;
                RecentNameLabel.Visible = true;
                RecentScoreLabel.Visible = true;
                Recent_Score.Visible = true;
                Recent_NameLabel.Visible = true;
                Recent_NameLabel.Enabled = true;
            }
            else
            {
                RecentScore.Visible = false;
                RecentNameLabel.Visible = false;
                RecentScoreLabel.Visible = false;
                Recent_Score.Visible = false;
                Recent_NameLabel.Visible = false;
                Recent_NameLabel.Enabled = false;
            }

            int Value = PublicVariables.HighScores.GetLength(0);
            if (10 < Value)
            {
                Value = 10;
            }
            for (int i = 0; i < Value; i++)
            {
                Label[] Labels = { (Label)HighScores_Top10.GetControlFromPosition(0, i + 1), (Label)HighScores_Top10.GetControlFromPosition(1, i + 1) };
                Labels[0].Text = (i + 1).ToString() + ". " + PublicVariables.HighScores[i, 0];
                Labels[1].Text = PublicVariables.HighScores[i, 1] + ":" + PublicVariables.HighScores[i, 2] + ":" + PublicVariables.HighScores[i, 3];
            }
            for (int i = Value; i < 10; i++)
            {
                Label[] Labels = { (Label)HighScores_Top10.GetControlFromPosition(0, i + 1), (Label)HighScores_Top10.GetControlFromPosition(1, i + 1) };
                Labels[0].Text = (i + 1).ToString() + ". ";
                Labels[1].Text = "";
            }
        }

        private void Recent_NameLabel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TextBox Recent_NameText = new TextBox();
            Recent_NameText.ReadOnly = false;
            Separated = HighScores_Recent.GetControlFromPosition(0, 1).Text.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
            Recent_NameText.Anchor = System.Windows.Forms.AnchorStyles.None;
            Recent_NameText.BackColor = System.Drawing.Color.Black;
            Recent_NameText.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Recent_NameText.ForeColor = System.Drawing.Color.White;
            Recent_NameText.Location = new System.Drawing.Point(8, 59);
            Recent_NameText.MaxLength = 9;
            Recent_NameText.Name = "Recent_NameText";
            Recent_NameText.Size = new System.Drawing.Size(220, 38);
            Recent_NameText.TabIndex = 25;
            Recent_NameText.Text = Separated[1];
            Recent_NameText.KeyDown += Recent_NameText_KeyDown;
            this.HighScores_Recent.Controls.Remove(HighScores_Recent.GetControlFromPosition(0, 1));
            this.HighScores_Recent.Controls.Add(Recent_NameText, 0, 1);
            this.HighScores_Recent.ResumeLayout(false);
            this.HighScores_Recent.PerformLayout();
            HighScores_Recent.GetControlFromPosition(0, 1).Select();
            HighScores_Recent.GetControlFromPosition(0, 1).Focus();
        }

        private void Recent_NameText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Escape)
            {
                Separated[1] = HighScores_Recent.GetControlFromPosition(0, 1).Text;
                Label Recent_NameLabel = new Label();
                Recent_NameLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
                Recent_NameLabel.AutoSize = true;
                Recent_NameLabel.BackColor = System.Drawing.Color.Transparent;
                Recent_NameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                Recent_NameLabel.ForeColor = System.Drawing.Color.White;
                Recent_NameLabel.Location = new System.Drawing.Point(11, 62);
                Recent_NameLabel.Name = "Recent_NameLabel";
                Recent_NameLabel.Size = new System.Drawing.Size(214, 31);
                Recent_NameLabel.TabIndex = 11;
                Recent_NameLabel.Text = Separated[0] + ". " + Separated[1];
                Recent_NameLabel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Recent_NameLabel_MouseDoubleClick);
                PublicVariables.HighScores[Convert.ToInt32(Separated[0]) - 1, 0] = Separated[1];
                PublicVariables.MostRecentScore[1] = Separated[1];
                UpdateTables();
                this.HighScores_Recent.Controls.Remove(HighScores_Recent.GetControlFromPosition(0, 1));
                this.HighScores_Recent.Controls.Add(Recent_NameLabel, 0, 1);
                this.HighScores_Recent.ResumeLayout(false);
                this.HighScores_Recent.PerformLayout();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
