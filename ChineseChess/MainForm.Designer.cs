
namespace ChineseChess
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.GameMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NewGameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GameMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.SwitchBoardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.AIOffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIEasyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIMediumMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIHardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AIMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.AISideBlackMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AISideRedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainStatus = new System.Windows.Forms.StatusStrip();
            this.StatusCampLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusGameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MoveListBox = new System.Windows.Forms.ListBox();
            this.MoveListLabel = new System.Windows.Forms.Label();
            this.Game = new ChineseChess.CGame();
            this.MainMenu.SuspendLayout();
            this.MainStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // MainMenu
            //
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.GameMenu,
                this.ViewMenu,
                this.AIMenu,
                this.HelpMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(660, 25);
            this.MainMenu.TabIndex = 0;
            //
            // GameMenu
            //
            this.GameMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.NewGameMenuItem,
                this.UndoMenuItem,
                this.GameMenuSeparator,
                this.ExitMenuItem});
            this.GameMenu.Name = "GameMenu";
            this.GameMenu.Text = "对局(&G)";
            //
            // NewGameMenuItem
            //
            this.NewGameMenuItem.Name = "NewGameMenuItem";
            this.NewGameMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.NewGameMenuItem.Text = "新对局(&N)";
            this.NewGameMenuItem.Click += new System.EventHandler(this.NewGameMenuItem_Click);
            //
            // UndoMenuItem
            //
            this.UndoMenuItem.Name = "UndoMenuItem";
            this.UndoMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.UndoMenuItem.Text = "悔棋(&U)";
            this.UndoMenuItem.Click += new System.EventHandler(this.UndoMenuItem_Click);
            //
            // GameMenuSeparator
            //
            this.GameMenuSeparator.Name = "GameMenuSeparator";
            //
            // ExitMenuItem
            //
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Text = "退出(&X)";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            //
            // ViewMenu
            //
            this.ViewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.SwitchBoardMenuItem});
            this.ViewMenu.Name = "ViewMenu";
            this.ViewMenu.Text = "视图(&V)";
            //
            // SwitchBoardMenuItem
            //
            this.SwitchBoardMenuItem.Name = "SwitchBoardMenuItem";
            this.SwitchBoardMenuItem.Text = "切换棋盘外观(&B)";
            this.SwitchBoardMenuItem.Click += new System.EventHandler(this.SwitchBoardMenuItem_Click);
            //
            // AIMenu
            //
            this.AIMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.AIOffMenuItem,
                this.AIEasyMenuItem,
                this.AIMediumMenuItem,
                this.AIHardMenuItem,
                this.AIMenuSeparator,
                this.AISideBlackMenuItem,
                this.AISideRedMenuItem});
            this.AIMenu.Name = "AIMenu";
            this.AIMenu.Text = "AI(&A)";
            //
            // AIOffMenuItem
            //
            this.AIOffMenuItem.Name = "AIOffMenuItem";
            this.AIOffMenuItem.Text = "关闭(&O)";
            this.AIOffMenuItem.Click += new System.EventHandler(this.AIOffMenuItem_Click);
            //
            // AIEasyMenuItem
            //
            this.AIEasyMenuItem.Name = "AIEasyMenuItem";
            this.AIEasyMenuItem.Text = "简单(&E)";
            this.AIEasyMenuItem.Click += new System.EventHandler(this.AIEasyMenuItem_Click);
            //
            // AIMediumMenuItem
            //
            this.AIMediumMenuItem.Name = "AIMediumMenuItem";
            this.AIMediumMenuItem.Text = "中等(&M)";
            this.AIMediumMenuItem.Click += new System.EventHandler(this.AIMediumMenuItem_Click);
            //
            // AIHardMenuItem
            //
            this.AIHardMenuItem.Name = "AIHardMenuItem";
            this.AIHardMenuItem.Text = "困难(&H)";
            this.AIHardMenuItem.Click += new System.EventHandler(this.AIHardMenuItem_Click);
            //
            // AIMenuSeparator
            //
            this.AIMenuSeparator.Name = "AIMenuSeparator";
            //
            // AISideBlackMenuItem
            //
            this.AISideBlackMenuItem.Name = "AISideBlackMenuItem";
            this.AISideBlackMenuItem.Text = "AI 执黑(&B)";
            this.AISideBlackMenuItem.Click += new System.EventHandler(this.AISideBlackMenuItem_Click);
            //
            // AISideRedMenuItem
            //
            this.AISideRedMenuItem.Name = "AISideRedMenuItem";
            this.AISideRedMenuItem.Text = "AI 执红(&R)";
            this.AISideRedMenuItem.Click += new System.EventHandler(this.AISideRedMenuItem_Click);
            //
            // HelpMenu
            //
            this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.AboutMenuItem});
            this.HelpMenu.Name = "HelpMenu";
            this.HelpMenu.Text = "帮助(&H)";
            //
            // AboutMenuItem
            //
            this.AboutMenuItem.Name = "AboutMenuItem";
            this.AboutMenuItem.Text = "关于(&A)";
            this.AboutMenuItem.Click += new System.EventHandler(this.AboutMenuItem_Click);
            //
            // MainStatus
            //
            this.MainStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.StatusCampLabel,
                this.StatusSpring,
                this.StatusGameLabel});
            this.MainStatus.Location = new System.Drawing.Point(0, 559);
            this.MainStatus.Name = "MainStatus";
            this.MainStatus.Size = new System.Drawing.Size(660, 22);
            this.MainStatus.TabIndex = 1;
            //
            // StatusCampLabel
            //
            this.StatusCampLabel.Name = "StatusCampLabel";
            this.StatusCampLabel.Text = "轮到 红方 走子";
            //
            // StatusSpring
            //
            this.StatusSpring.Name = "StatusSpring";
            this.StatusSpring.Spring = true;
            //
            // StatusGameLabel
            //
            this.StatusGameLabel.Name = "StatusGameLabel";
            this.StatusGameLabel.Text = "已走 0 步";
            //
            // MoveListLabel
            //
            this.MoveListLabel.AutoSize = true;
            this.MoveListLabel.Location = new System.Drawing.Point(469, 30);
            this.MoveListLabel.Name = "MoveListLabel";
            this.MoveListLabel.Size = new System.Drawing.Size(56, 17);
            this.MoveListLabel.Text = "走子记录";
            //
            // MoveListBox
            //
            this.MoveListBox.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.MoveListBox.FormattingEnabled = true;
            this.MoveListBox.IntegralHeight = false;
            this.MoveListBox.ItemHeight = 16;
            this.MoveListBox.Location = new System.Drawing.Point(469, 50);
            this.MoveListBox.Name = "MoveListBox";
            this.MoveListBox.Size = new System.Drawing.Size(180, 500);
            this.MoveListBox.TabIndex = 2;
            this.MoveListBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            //
            // Game
            //
            this.Game.Location = new System.Drawing.Point(5, 28);
            this.Game.MaximumSize = new System.Drawing.Size(458, 530);
            this.Game.MinimumSize = new System.Drawing.Size(458, 530);
            this.Game.Name = "Game";
            this.Game.Size = new System.Drawing.Size(458, 530);
            this.Game.TabIndex = 3;
            this.Game.Text = "ChineseChessGame";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 581);
            this.Controls.Add(this.Game);
            this.Controls.Add(this.MoveListBox);
            this.Controls.Add(this.MoveListLabel);
            this.Controls.Add(this.MainStatus);
            this.Controls.Add(this.MainMenu);
            this.MainMenuStrip = this.MainMenu;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "中国象棋";
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.MainStatus.ResumeLayout(false);
            this.MainStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private CGame Game;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem GameMenu;
        private System.Windows.Forms.ToolStripMenuItem NewGameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem UndoMenuItem;
        private System.Windows.Forms.ToolStripSeparator GameMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.ToolStripMenuItem SwitchBoardMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AIMenu;
        private System.Windows.Forms.ToolStripMenuItem AIOffMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AIEasyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AIMediumMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AIHardMenuItem;
        private System.Windows.Forms.ToolStripSeparator AIMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem AISideBlackMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AISideRedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.ToolStripMenuItem AboutMenuItem;
        private System.Windows.Forms.StatusStrip MainStatus;
        private System.Windows.Forms.ToolStripStatusLabel StatusCampLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusSpring;
        private System.Windows.Forms.ToolStripStatusLabel StatusGameLabel;
        private System.Windows.Forms.ListBox MoveListBox;
        private System.Windows.Forms.Label MoveListLabel;
    }
}
