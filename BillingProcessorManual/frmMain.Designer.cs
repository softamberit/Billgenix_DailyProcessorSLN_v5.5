namespace BillingProcessorManual
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnBillingProcessor = new System.Windows.Forms.Button();
            this.btnRequesttoTempStop = new System.Windows.Forms.Button();
            this.btnDiscontinueToDeviceColl = new System.Windows.Forms.Button();
            this.btnInactiveToDeviceColl = new System.Windows.Forms.Button();
            this.btnPackageChange = new System.Windows.Forms.Button();
            this.btnShifting = new System.Windows.Forms.Button();
            this.btnTempStopToDeviceColl = new System.Windows.Forms.Button();
            this.btnHandOverPending = new System.Windows.Forms.Button();
            this.btnSMSRemainder = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblStat = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(1124, 679);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 28);
            this.button2.TabIndex = 8;
            this.button2.Text = "Exit";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.button3.ForeColor = System.Drawing.Color.DarkRed;
            this.button3.Location = new System.Drawing.Point(1056, 82);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(136, 28);
            this.button3.TabIndex = 9;
            this.button3.Text = "STOP";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Enabled = false;
            this.progressBar1.Location = new System.Drawing.Point(1056, 46);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(136, 28);
            this.progressBar1.TabIndex = 15;
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 20;
            this.listBox1.Location = new System.Drawing.Point(19, 385);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(1200, 204);
            this.listBox1.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(15, 361);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Scheduler log";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(204, 62);
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(415, 9);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(553, 39);
            this.label8.TabIndex = 22;
            this.label8.Text = "BILLING PROCESSOR MANUAL";
            // 
            // btnBillingProcessor
            // 
            this.btnBillingProcessor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnBillingProcessor.ForeColor = System.Drawing.Color.DarkRed;
            this.btnBillingProcessor.Location = new System.Drawing.Point(41, 123);
            this.btnBillingProcessor.Margin = new System.Windows.Forms.Padding(4);
            this.btnBillingProcessor.Name = "btnBillingProcessor";
            this.btnBillingProcessor.Size = new System.Drawing.Size(235, 74);
            this.btnBillingProcessor.TabIndex = 23;
            this.btnBillingProcessor.Text = "BILLING PROCESSOR";
            this.btnBillingProcessor.UseVisualStyleBackColor = true;
            this.btnBillingProcessor.Click += new System.EventHandler(this.btnBillingProcessor_Click);
            // 
            // btnRequesttoTempStop
            // 
            this.btnRequesttoTempStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnRequesttoTempStop.ForeColor = System.Drawing.Color.DarkRed;
            this.btnRequesttoTempStop.Location = new System.Drawing.Point(292, 123);
            this.btnRequesttoTempStop.Margin = new System.Windows.Forms.Padding(4);
            this.btnRequesttoTempStop.Name = "btnRequesttoTempStop";
            this.btnRequesttoTempStop.Size = new System.Drawing.Size(341, 74);
            this.btnRequesttoTempStop.TabIndex = 24;
            this.btnRequesttoTempStop.Text = "REQUEST TO TEMPORARYSTOP";
            this.btnRequesttoTempStop.UseVisualStyleBackColor = true;
            this.btnRequesttoTempStop.Click += new System.EventHandler(this.btnRequesttoTempStop_Click);
            // 
            // btnDiscontinueToDeviceColl
            // 
            this.btnDiscontinueToDeviceColl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnDiscontinueToDeviceColl.ForeColor = System.Drawing.Color.DarkRed;
            this.btnDiscontinueToDeviceColl.Location = new System.Drawing.Point(292, 204);
            this.btnDiscontinueToDeviceColl.Margin = new System.Windows.Forms.Padding(4);
            this.btnDiscontinueToDeviceColl.Name = "btnDiscontinueToDeviceColl";
            this.btnDiscontinueToDeviceColl.Size = new System.Drawing.Size(341, 59);
            this.btnDiscontinueToDeviceColl.TabIndex = 25;
            this.btnDiscontinueToDeviceColl.Text = "DISCONTINUE TO DEVICECOLL";
            this.btnDiscontinueToDeviceColl.UseVisualStyleBackColor = true;
            this.btnDiscontinueToDeviceColl.Click += new System.EventHandler(this.btnDiscontinueToDeviceColl_Click);
            // 
            // btnInactiveToDeviceColl
            // 
            this.btnInactiveToDeviceColl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnInactiveToDeviceColl.ForeColor = System.Drawing.Color.DarkRed;
            this.btnInactiveToDeviceColl.Location = new System.Drawing.Point(659, 123);
            this.btnInactiveToDeviceColl.Margin = new System.Windows.Forms.Padding(4);
            this.btnInactiveToDeviceColl.Name = "btnInactiveToDeviceColl";
            this.btnInactiveToDeviceColl.Size = new System.Drawing.Size(283, 75);
            this.btnInactiveToDeviceColl.TabIndex = 26;
            this.btnInactiveToDeviceColl.Text = "INACTIVE TO DEVICECOLL";
            this.btnInactiveToDeviceColl.UseVisualStyleBackColor = true;
            this.btnInactiveToDeviceColl.Click += new System.EventHandler(this.btnInactiveToDeviceColl_Click);
            // 
            // btnPackageChange
            // 
            this.btnPackageChange.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnPackageChange.ForeColor = System.Drawing.Color.DarkRed;
            this.btnPackageChange.Location = new System.Drawing.Point(41, 204);
            this.btnPackageChange.Margin = new System.Windows.Forms.Padding(4);
            this.btnPackageChange.Name = "btnPackageChange";
            this.btnPackageChange.Size = new System.Drawing.Size(235, 59);
            this.btnPackageChange.TabIndex = 27;
            this.btnPackageChange.Text = "PACKAGE CHANGE";
            this.btnPackageChange.UseVisualStyleBackColor = true;
            this.btnPackageChange.Click += new System.EventHandler(this.btnPackageChange_Click);
            // 
            // btnShifting
            // 
            this.btnShifting.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnShifting.ForeColor = System.Drawing.Color.DarkRed;
            this.btnShifting.Location = new System.Drawing.Point(963, 123);
            this.btnShifting.Margin = new System.Windows.Forms.Padding(4);
            this.btnShifting.Name = "btnShifting";
            this.btnShifting.Size = new System.Drawing.Size(229, 74);
            this.btnShifting.TabIndex = 29;
            this.btnShifting.Text = "SHIFTING";
            this.btnShifting.UseVisualStyleBackColor = true;
            this.btnShifting.Click += new System.EventHandler(this.btnShifting_Click);
            // 
            // btnTempStopToDeviceColl
            // 
            this.btnTempStopToDeviceColl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnTempStopToDeviceColl.ForeColor = System.Drawing.Color.DarkRed;
            this.btnTempStopToDeviceColl.Location = new System.Drawing.Point(659, 206);
            this.btnTempStopToDeviceColl.Margin = new System.Windows.Forms.Padding(4);
            this.btnTempStopToDeviceColl.Name = "btnTempStopToDeviceColl";
            this.btnTempStopToDeviceColl.Size = new System.Drawing.Size(283, 58);
            this.btnTempStopToDeviceColl.TabIndex = 30;
            this.btnTempStopToDeviceColl.Text = "TEMP STOP TO DEVICECOLL";
            this.btnTempStopToDeviceColl.UseVisualStyleBackColor = true;
            this.btnTempStopToDeviceColl.Click += new System.EventHandler(this.btnTempStopToDeviceColl_Click);
            // 
            // btnHandOverPending
            // 
            this.btnHandOverPending.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnHandOverPending.ForeColor = System.Drawing.Color.DarkRed;
            this.btnHandOverPending.Location = new System.Drawing.Point(963, 206);
            this.btnHandOverPending.Margin = new System.Windows.Forms.Padding(4);
            this.btnHandOverPending.Name = "btnHandOverPending";
            this.btnHandOverPending.Size = new System.Drawing.Size(229, 58);
            this.btnHandOverPending.TabIndex = 31;
            this.btnHandOverPending.Text = "HANDOVER PENDING";
            this.btnHandOverPending.UseVisualStyleBackColor = true;
            this.btnHandOverPending.Click += new System.EventHandler(this.btnHandOverPending_Click);
            // 
            // btnSMSRemainder
            // 
            this.btnSMSRemainder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnSMSRemainder.ForeColor = System.Drawing.Color.DarkRed;
            this.btnSMSRemainder.Location = new System.Drawing.Point(32, 284);
            this.btnSMSRemainder.Margin = new System.Windows.Forms.Padding(4);
            this.btnSMSRemainder.Name = "btnSMSRemainder";
            this.btnSMSRemainder.Size = new System.Drawing.Size(229, 54);
            this.btnSMSRemainder.TabIndex = 32;
            this.btnSMSRemainder.Text = "SMS & EMAIL REMAINDER-4";
            this.btnSMSRemainder.UseVisualStyleBackColor = true;
            this.btnSMSRemainder.Click += new System.EventHandler(this.btnSMSRemainder_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.button1.ForeColor = System.Drawing.Color.DarkRed;
            this.button1.Location = new System.Drawing.Point(292, 284);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(341, 54);
            this.button1.TabIndex = 33;
            this.button1.Text = "SMS REMAINDER 2";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblStat
            // 
            this.lblStat.AutoSize = true;
            this.lblStat.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStat.ForeColor = System.Drawing.Color.Red;
            this.lblStat.Location = new System.Drawing.Point(652, 299);
            this.lblStat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStat.Name = "lblStat";
            this.lblStat.Size = new System.Drawing.Size(113, 39);
            this.lblStat.TabIndex = 34;
            this.lblStat.Text = "status";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1232, 721);
            this.Controls.Add(this.lblStat);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSMSRemainder);
            this.Controls.Add(this.btnHandOverPending);
            this.Controls.Add(this.btnTempStopToDeviceColl);
            this.Controls.Add(this.btnShifting);
            this.Controls.Add(this.btnPackageChange);
            this.Controls.Add(this.btnInactiveToDeviceColl);
            this.Controls.Add(this.btnDiscontinueToDeviceColl);
            this.Controls.Add(this.btnRequesttoTempStop);
            this.Controls.Add(this.btnBillingProcessor);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.Text = "BILLING PROCESSOR";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnBillingProcessor;
        private System.Windows.Forms.Button btnRequesttoTempStop;
        private System.Windows.Forms.Button btnDiscontinueToDeviceColl;
        private System.Windows.Forms.Button btnInactiveToDeviceColl;
        private System.Windows.Forms.Button btnPackageChange;
        private System.Windows.Forms.Button btnShifting;
        private System.Windows.Forms.Button btnTempStopToDeviceColl;
        private System.Windows.Forms.Button btnHandOverPending;
        private System.Windows.Forms.Button btnSMSRemainder;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblStat;
    }
}

