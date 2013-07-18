namespace upikapik
{
    partial class Form1
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
            this.listPlay = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lytMonitor = new System.Windows.Forms.TableLayoutPanel();
            this.barProgress = new System.Windows.Forms.ProgressBar();
            this.barSeek = new System.Windows.Forms.TrackBar();
            this.lytMain.SuspendLayout();
            this.lytFlow.SuspendLayout();
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
            this.lytMain.Controls.Add(this.label1, 0, 3);
            this.lytMain.Controls.Add(this.lytMonitor, 0, 0);
            this.lytMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytMain.Location = new System.Drawing.Point(0, 0);
            this.lytMain.Name = "lytMain";
            this.lytMain.RowCount = 4;
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.lytMain.Size = new System.Drawing.Size(292, 291);
            this.lytMain.TabIndex = 0;
            // 
            // lytFlow
            // 
            this.lytFlow.Controls.Add(this.btnPlay);
            this.lytFlow.Controls.Add(this.btnOpen);
            this.lytFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytFlow.Location = new System.Drawing.Point(3, 60);
            this.lytFlow.MaximumSize = new System.Drawing.Size(0, 40);
            this.lytFlow.Name = "lytFlow";
            this.lytFlow.Size = new System.Drawing.Size(286, 39);
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
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(84, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            // 
            // listPlay
            // 
            this.listPlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listPlay.FormattingEnabled = true;
            this.listPlay.Location = new System.Drawing.Point(3, 105);
            this.listPlay.Name = "listPlay";
            this.listPlay.Size = new System.Drawing.Size(286, 163);
            this.listPlay.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 271);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Not Playing";
            // 
            // lytMonitor
            // 
            this.lytMonitor.ColumnCount = 1;
            this.lytMonitor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.Controls.Add(this.barProgress, 0, 1);
            this.lytMonitor.Controls.Add(this.barSeek, 0, 0);
            this.lytMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytMonitor.Location = new System.Drawing.Point(3, 3);
            this.lytMonitor.MaximumSize = new System.Drawing.Size(0, 80);
            this.lytMonitor.Name = "lytMonitor";
            this.lytMonitor.RowCount = 2;
            this.lytMonitor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytMonitor.Size = new System.Drawing.Size(286, 51);
            this.lytMonitor.TabIndex = 3;
            // 
            // barProgress
            // 
            this.barProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barProgress.Location = new System.Drawing.Point(3, 28);
            this.barProgress.Name = "barProgress";
            this.barProgress.Size = new System.Drawing.Size(280, 20);
            this.barProgress.TabIndex = 0;
            // 
            // barSeek
            // 
            this.barSeek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barSeek.Location = new System.Drawing.Point(3, 3);
            this.barSeek.Name = "barSeek";
            this.barSeek.Size = new System.Drawing.Size(280, 19);
            this.barSeek.TabIndex = 1;
            this.barSeek.TickStyle = System.Windows.Forms.TickStyle.None;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 291);
            this.Controls.Add(this.lytMain);
            this.Name = "Form1";
            this.Text = "upikApik";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.lytMain.ResumeLayout(false);
            this.lytMain.PerformLayout();
            this.lytFlow.ResumeLayout(false);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel lytMonitor;
        private System.Windows.Forms.ProgressBar barProgress;
        private System.Windows.Forms.TrackBar barSeek;
    }
}

