namespace upikapik
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lytMain = new System.Windows.Forms.TableLayoutPanel();
            this.lytFlow = new System.Windows.Forms.FlowLayoutPanel();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.barVol = new System.Windows.Forms.TrackBar();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.checkShuffle = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.listPlay = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lytMonitor = new System.Windows.Forms.TableLayoutPanel();
            this.barProgress = new System.Windows.Forms.ProgressBar();
            this.barSeek = new System.Windows.Forms.TrackBar();
            this.lytMain.SuspendLayout();
            this.lytFlow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barVol)).BeginInit();
            this.lytMonitor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barSeek)).BeginInit();
            this.SuspendLayout();
            // 
            // lytMain
            // 
            this.lytMain.ColumnCount = 1;
            this.lytMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytMain.Controls.Add(this.lytFlow, 0, 1);
            this.lytMain.Controls.Add(this.listPlay, 0, 2);
            this.lytMain.Controls.Add(this.lblStatus, 0, 3);
            this.lytMain.Controls.Add(this.lytMonitor, 0, 0);
            this.lytMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytMain.Location = new System.Drawing.Point(0, 0);
            this.lytMain.Name = "lytMain";
            this.lytMain.RowCount = 4;
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.lytMain.Size = new System.Drawing.Size(498, 291);
            this.lytMain.TabIndex = 0;
            // 
            // lytFlow
            // 
            this.lytFlow.Controls.Add(this.btnPlay);
            this.lytFlow.Controls.Add(this.btnOpen);
            this.lytFlow.Controls.Add(this.barVol);
            this.lytFlow.Controls.Add(this.btnRefresh);
            this.lytFlow.Controls.Add(this.checkShuffle);
            this.lytFlow.Controls.Add(this.btnConnect);
            this.lytFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytFlow.Location = new System.Drawing.Point(3, 70);
            this.lytFlow.MaximumSize = new System.Drawing.Size(0, 40);
            this.lytFlow.Name = "lytFlow";
            this.lytFlow.Size = new System.Drawing.Size(492, 29);
            this.lytFlow.TabIndex = 0;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(3, 3);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.Text = "Play/Pause";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(84, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Add";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // barVol
            // 
            this.barVol.Location = new System.Drawing.Point(165, 3);
            this.barVol.Name = "barVol";
            this.barVol.Size = new System.Drawing.Size(104, 45);
            this.barVol.TabIndex = 2;
            this.barVol.Value = 10;
            this.barVol.Scroll += new System.EventHandler(this.barVol_Scroll);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(275, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(29, 23);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "><";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // checkShuffle
            // 
            this.checkShuffle.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkShuffle.AutoSize = true;
            this.checkShuffle.Location = new System.Drawing.Point(310, 3);
            this.checkShuffle.Name = "checkShuffle";
            this.checkShuffle.Size = new System.Drawing.Size(50, 23);
            this.checkShuffle.TabIndex = 3;
            this.checkShuffle.Text = "Shuffle";
            this.checkShuffle.UseVisualStyleBackColor = true;
            this.checkShuffle.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Enabled = false;
            this.btnConnect.Location = new System.Drawing.Point(366, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(56, 23);
            this.btnConnect.TabIndex = 4;
            this.btnConnect.Text = "connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // listPlay
            // 
            this.listPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPlay.FormattingEnabled = true;
            this.listPlay.Location = new System.Drawing.Point(3, 105);
            this.listPlay.Name = "listPlay";
            this.listPlay.Size = new System.Drawing.Size(492, 163);
            this.listPlay.TabIndex = 1;
            this.listPlay.SelectedIndexChanged += new System.EventHandler(this.listPlay_SelectedIndexChanged);
            this.listPlay.DoubleClick += new System.EventHandler(this.listPlay_DoubleClick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Location = new System.Drawing.Point(3, 271);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(492, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Not Playing";
            // 
            // lytMonitor
            // 
            this.lytMonitor.ColumnCount = 1;
            this.lytMonitor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.Controls.Add(this.barProgress, 0, 1);
            this.lytMonitor.Controls.Add(this.barSeek, 0, 0);
            this.lytMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytMonitor.Location = new System.Drawing.Point(3, 3);
            this.lytMonitor.MaximumSize = new System.Drawing.Size(0, 100);
            this.lytMonitor.Name = "lytMonitor";
            this.lytMonitor.RowCount = 2;
            this.lytMonitor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.Size = new System.Drawing.Size(492, 61);
            this.lytMonitor.TabIndex = 3;
            // 
            // barProgress
            // 
            this.barProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barProgress.Location = new System.Drawing.Point(3, 33);
            this.barProgress.Name = "barProgress";
            this.barProgress.Size = new System.Drawing.Size(486, 25);
            this.barProgress.TabIndex = 0;
            // 
            // barSeek
            // 
            this.barSeek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barSeek.Location = new System.Drawing.Point(3, 3);
            this.barSeek.Name = "barSeek";
            this.barSeek.Size = new System.Drawing.Size(486, 24);
            this.barSeek.SmallChange = 6;
            this.barSeek.TabIndex = 1;
            this.barSeek.TickStyle = System.Windows.Forms.TickStyle.None;
            this.barSeek.Scroll += new System.EventHandler(this.barSeek_Scroll);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 291);
            this.Controls.Add(this.lytMain);
            this.Name = "mainForm";
            this.Text = "UpikApik";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.lytMain.ResumeLayout(false);
            this.lytMain.PerformLayout();
            this.lytFlow.ResumeLayout(false);
            this.lytFlow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barVol)).EndInit();
            this.lytMonitor.ResumeLayout(false);
            this.lytMonitor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barSeek)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel lytMain;
        private System.Windows.Forms.FlowLayoutPanel lytFlow;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.ListBox listPlay;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel lytMonitor;
        private System.Windows.Forms.ProgressBar barProgress;
        private System.Windows.Forms.TrackBar barSeek;
        private System.Windows.Forms.TrackBar barVol;
        private System.Windows.Forms.CheckBox checkShuffle;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnRefresh;
    }
}

