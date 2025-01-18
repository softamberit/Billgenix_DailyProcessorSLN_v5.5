namespace CustomerEnableDisable
{
    partial class FrmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.button2 = new System.Windows.Forms.Button();
            this.CustomerOFF = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.CustomerON = new System.Windows.Forms.Button();
            this.btnCheckRouter = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.TextBox();
            this.lblStat = new System.Windows.Forms.Label();
            this.btnPullComment = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(1096, 587);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 28);
            this.button2.TabIndex = 8;
            this.button2.Text = "Exit";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.exitBtn_Click);
            // 
            // CustomerOFF
            // 
            this.CustomerOFF.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CustomerOFF.ForeColor = System.Drawing.Color.Red;
            this.CustomerOFF.Location = new System.Drawing.Point(223, 106);
            this.CustomerOFF.Margin = new System.Windows.Forms.Padding(4);
            this.CustomerOFF.Name = "CustomerOFF";
            this.CustomerOFF.Size = new System.Drawing.Size(145, 54);
            this.CustomerOFF.TabIndex = 9;
            this.CustomerOFF.Text = "OFF";
            this.CustomerOFF.UseVisualStyleBackColor = true;
            this.CustomerOFF.Click += new System.EventHandler(this.CustomerOFF_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Enabled = false;
            this.progressBar1.Location = new System.Drawing.Point(1016, 46);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(136, 27);
            this.progressBar1.TabIndex = 15;
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(17, 334);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(1172, 212);
            this.listBox1.TabIndex = 16;
            this.listBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBox1_KeyDown);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(416, 7);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(365, 39);
            this.label8.TabIndex = 23;
            this.label8.Text = "CUSTOMER ON OFF";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 310);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 20);
            this.label1.TabIndex = 24;
            this.label1.Text = "Scheduler log";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(12, 11);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(203, 62);
            this.pictureBox2.TabIndex = 29;
            this.pictureBox2.TabStop = false;
            // 
            // CustomerON
            // 
            this.CustomerON.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.CustomerON.ForeColor = System.Drawing.Color.Green;
            this.CustomerON.Location = new System.Drawing.Point(17, 106);
            this.CustomerON.Margin = new System.Windows.Forms.Padding(4);
            this.CustomerON.Name = "CustomerON";
            this.CustomerON.Size = new System.Drawing.Size(139, 54);
            this.CustomerON.TabIndex = 30;
            this.CustomerON.Text = "ON";
            this.CustomerON.UseVisualStyleBackColor = true;
            this.CustomerON.Click += new System.EventHandler(this.CustomerON_Click);
            // 
            // btnCheckRouter
            // 
            this.btnCheckRouter.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnCheckRouter.ForeColor = System.Drawing.Color.Red;
            this.btnCheckRouter.Location = new System.Drawing.Point(423, 70);
            this.btnCheckRouter.Margin = new System.Windows.Forms.Padding(4);
            this.btnCheckRouter.Name = "btnCheckRouter";
            this.btnCheckRouter.Size = new System.Drawing.Size(495, 54);
            this.btnCheckRouter.TabIndex = 31;
            this.btnCheckRouter.Text = "CHEK ROUTER CONNECTION";
            this.btnCheckRouter.UseVisualStyleBackColor = true;
            this.btnCheckRouter.Click += new System.EventHandler(this.btnCheckRouter_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(20, 218);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4);
            this.lblStatus.Multiline = true;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1092, 88);
            this.lblStatus.TabIndex = 32;
            // 
            // lblStat
            // 
            this.lblStat.AutoSize = true;
            this.lblStat.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStat.ForeColor = System.Drawing.Color.Red;
            this.lblStat.Location = new System.Drawing.Point(10, 575);
            this.lblStat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStat.Name = "lblStat";
            this.lblStat.Size = new System.Drawing.Size(151, 39);
            this.lblStat.TabIndex = 33;
            this.lblStat.Text = "ON OFF";
            // 
            // btnPullComment
            // 
            this.btnPullComment.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnPullComment.ForeColor = System.Drawing.Color.Red;
            this.btnPullComment.Location = new System.Drawing.Point(423, 156);
            this.btnPullComment.Margin = new System.Windows.Forms.Padding(4);
            this.btnPullComment.Name = "btnPullComment";
            this.btnPullComment.Size = new System.Drawing.Size(495, 54);
            this.btnPullComment.TabIndex = 34;
            this.btnPullComment.Text = "PULL COMMENT FROM MK";
            this.btnPullComment.UseVisualStyleBackColor = true;
            this.btnPullComment.Click += new System.EventHandler(this.btnPullComment_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1204, 629);
            this.Controls.Add(this.btnPullComment);
            this.Controls.Add(this.lblStat);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCheckRouter);
            this.Controls.Add(this.CustomerON);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.CustomerOFF);
            this.Controls.Add(this.button2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmMain";
            this.Text = "COLLECTION PROCESSOR";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button CustomerOFF;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button CustomerON;
        private System.Windows.Forms.Button btnCheckRouter;
        private System.Windows.Forms.TextBox lblStatus;
        private System.Windows.Forms.Label lblStat;
        private System.Windows.Forms.Button btnPullComment;
    }
}

