namespace HDPVRRecoder_W
{
    partial class FormMain
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
            this.treeViewFiles = new System.Windows.Forms.TreeView();
            this.labelStation = new System.Windows.Forms.Label();
            this.textBoxStation = new System.Windows.Forms.TextBox();
            this.textBoxChannel = new System.Windows.Forms.TextBox();
            this.labelChannel = new System.Windows.Forms.Label();
            this.textBoxTitle = new System.Windows.Forms.TextBox();
            this.labelTitle = new System.Windows.Forms.Label();
            this.textBoxDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textBoxStartTime = new System.Windows.Forms.TextBox();
            this.labelStartTime = new System.Windows.Forms.Label();
            this.textBoxEndTime = new System.Windows.Forms.TextBox();
            this.labelEndTime = new System.Windows.Forms.Label();
            this.textBoxDuration = new System.Windows.Forms.TextBox();
            this.labelDuration = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonSleep = new System.Windows.Forms.Button();
            this.buttonQuit = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.checkBoxAutoSleep = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonStartRoku = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxrokuRoot = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonStopRokuTVStreaming = new System.Windows.Forms.Button();
            this.buttonRokuTVStreaming = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeViewFiles
            // 
            this.treeViewFiles.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeViewFiles.Location = new System.Drawing.Point(0, 0);
            this.treeViewFiles.Name = "treeViewFiles";
            this.treeViewFiles.Size = new System.Drawing.Size(253, 530);
            this.treeViewFiles.TabIndex = 0;
            this.treeViewFiles.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewFiles_NodeMouseClick);
            // 
            // labelStation
            // 
            this.labelStation.AutoSize = true;
            this.labelStation.Location = new System.Drawing.Point(278, 29);
            this.labelStation.Name = "labelStation";
            this.labelStation.Size = new System.Drawing.Size(67, 13);
            this.labelStation.TabIndex = 1;
            this.labelStation.Text = "电视台呼号";
            // 
            // textBoxStation
            // 
            this.textBoxStation.Location = new System.Drawing.Point(351, 26);
            this.textBoxStation.Name = "textBoxStation";
            this.textBoxStation.Size = new System.Drawing.Size(432, 20);
            this.textBoxStation.TabIndex = 2;
            // 
            // textBoxChannel
            // 
            this.textBoxChannel.Location = new System.Drawing.Point(351, 60);
            this.textBoxChannel.Name = "textBoxChannel";
            this.textBoxChannel.Size = new System.Drawing.Size(432, 20);
            this.textBoxChannel.TabIndex = 4;
            // 
            // labelChannel
            // 
            this.labelChannel.AutoSize = true;
            this.labelChannel.Location = new System.Drawing.Point(302, 63);
            this.labelChannel.Name = "labelChannel";
            this.labelChannel.Size = new System.Drawing.Size(43, 13);
            this.labelChannel.TabIndex = 3;
            this.labelChannel.Text = "频道号";
            // 
            // textBoxTitle
            // 
            this.textBoxTitle.Location = new System.Drawing.Point(351, 94);
            this.textBoxTitle.Name = "textBoxTitle";
            this.textBoxTitle.Size = new System.Drawing.Size(432, 20);
            this.textBoxTitle.TabIndex = 6;
            // 
            // labelTitle
            // 
            this.labelTitle.AutoSize = true;
            this.labelTitle.Location = new System.Drawing.Point(290, 97);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(55, 13);
            this.labelTitle.TabIndex = 5;
            this.labelTitle.Text = "节目名称";
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Location = new System.Drawing.Point(351, 232);
            this.textBoxDescription.Multiline = true;
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(432, 135);
            this.textBoxDescription.TabIndex = 8;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(290, 235);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(55, 13);
            this.labelDescription.TabIndex = 7;
            this.labelDescription.Text = "节目描述";
            // 
            // textBoxStartTime
            // 
            this.textBoxStartTime.Location = new System.Drawing.Point(351, 129);
            this.textBoxStartTime.Name = "textBoxStartTime";
            this.textBoxStartTime.Size = new System.Drawing.Size(432, 20);
            this.textBoxStartTime.TabIndex = 12;
            // 
            // labelStartTime
            // 
            this.labelStartTime.AutoSize = true;
            this.labelStartTime.Location = new System.Drawing.Point(290, 132);
            this.labelStartTime.Name = "labelStartTime";
            this.labelStartTime.Size = new System.Drawing.Size(55, 13);
            this.labelStartTime.TabIndex = 11;
            this.labelStartTime.Text = "开始时间";
            // 
            // textBoxEndTime
            // 
            this.textBoxEndTime.Location = new System.Drawing.Point(351, 161);
            this.textBoxEndTime.Name = "textBoxEndTime";
            this.textBoxEndTime.Size = new System.Drawing.Size(432, 20);
            this.textBoxEndTime.TabIndex = 16;
            // 
            // labelEndTime
            // 
            this.labelEndTime.AutoSize = true;
            this.labelEndTime.Location = new System.Drawing.Point(290, 164);
            this.labelEndTime.Name = "labelEndTime";
            this.labelEndTime.Size = new System.Drawing.Size(55, 13);
            this.labelEndTime.TabIndex = 15;
            this.labelEndTime.Text = "截止时间";
            // 
            // textBoxDuration
            // 
            this.textBoxDuration.Location = new System.Drawing.Point(351, 196);
            this.textBoxDuration.Name = "textBoxDuration";
            this.textBoxDuration.Size = new System.Drawing.Size(432, 20);
            this.textBoxDuration.TabIndex = 18;
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(290, 199);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(55, 13);
            this.labelDuration.TabIndex = 17;
            this.labelDuration.Text = "节目长度";
            // 
            // buttonSave
            // 
            this.buttonSave.Enabled = false;
            this.buttonSave.Location = new System.Drawing.Point(295, 379);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 19;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            // 
            // buttonSleep
            // 
            this.buttonSleep.Location = new System.Drawing.Point(478, 379);
            this.buttonSleep.Name = "buttonSleep";
            this.buttonSleep.Size = new System.Drawing.Size(75, 23);
            this.buttonSleep.TabIndex = 20;
            this.buttonSleep.Text = "休眠";
            this.buttonSleep.UseVisualStyleBackColor = true;
            this.buttonSleep.Click += new System.EventHandler(this.buttonSleep_Click);
            // 
            // buttonQuit
            // 
            this.buttonQuit.Enabled = false;
            this.buttonQuit.Location = new System.Drawing.Point(397, 379);
            this.buttonQuit.Name = "buttonQuit";
            this.buttonQuit.Size = new System.Drawing.Size(75, 23);
            this.buttonQuit.TabIndex = 21;
            this.buttonQuit.Text = "关闭";
            this.buttonQuit.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(559, 379);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStop.TabIndex = 22;
            this.buttonStop.Text = "停止录制";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // checkBoxAutoSleep
            // 
            this.checkBoxAutoSleep.AutoSize = true;
            this.checkBoxAutoSleep.Location = new System.Drawing.Point(264, 347);
            this.checkBoxAutoSleep.Name = "checkBoxAutoSleep";
            this.checkBoxAutoSleep.Size = new System.Drawing.Size(74, 17);
            this.checkBoxAutoSleep.TabIndex = 23;
            this.checkBoxAutoSleep.Text = "自动休眠";
            this.checkBoxAutoSleep.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonStartRoku);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxrokuRoot);
            this.groupBox1.Location = new System.Drawing.Point(264, 408);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(527, 54);
            this.groupBox1.TabIndex = 26;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "RokuSettings";
            // 
            // buttonStartRoku
            // 
            this.buttonStartRoku.Location = new System.Drawing.Point(405, 20);
            this.buttonStartRoku.Name = "buttonStartRoku";
            this.buttonStartRoku.Size = new System.Drawing.Size(114, 23);
            this.buttonStartRoku.TabIndex = 27;
            this.buttonStartRoku.Text = "Start Roku Server";
            this.buttonStartRoku.UseVisualStyleBackColor = true;
            this.buttonStartRoku.Click += new System.EventHandler(this.buttonStartRoku_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Roku RootPath";
            // 
            // textBoxrokuRoot
            // 
            this.textBoxrokuRoot.Location = new System.Drawing.Point(101, 23);
            this.textBoxrokuRoot.Name = "textBoxrokuRoot";
            this.textBoxrokuRoot.Size = new System.Drawing.Size(283, 20);
            this.textBoxrokuRoot.TabIndex = 27;
            this.textBoxrokuRoot.Text = "e:\\";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonStopRokuTVStreaming);
            this.groupBox2.Controls.Add(this.buttonRokuTVStreaming);
            this.groupBox2.Location = new System.Drawing.Point(264, 468);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(527, 54);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "HDPVR Roku Streaming Settings";
            // 
            // buttonStopRokuTVStreaming
            // 
            this.buttonStopRokuTVStreaming.Location = new System.Drawing.Point(319, 19);
            this.buttonStopRokuTVStreaming.Name = "buttonStopRokuTVStreaming";
            this.buttonStopRokuTVStreaming.Size = new System.Drawing.Size(190, 23);
            this.buttonStopRokuTVStreaming.TabIndex = 28;
            this.buttonStopRokuTVStreaming.Text = "Stop Roku TV Streaming";
            this.buttonStopRokuTVStreaming.UseVisualStyleBackColor = true;
            this.buttonStopRokuTVStreaming.Click += new System.EventHandler(this.buttonStopRokuTVStreaming_Click);
            // 
            // buttonRokuTVStreaming
            // 
            this.buttonRokuTVStreaming.Location = new System.Drawing.Point(154, 19);
            this.buttonRokuTVStreaming.Name = "buttonRokuTVStreaming";
            this.buttonRokuTVStreaming.Size = new System.Drawing.Size(145, 23);
            this.buttonRokuTVStreaming.TabIndex = 27;
            this.buttonRokuTVStreaming.Text = "Start Roku TV Streaming";
            this.buttonRokuTVStreaming.UseVisualStyleBackColor = true;
            this.buttonRokuTVStreaming.Click += new System.EventHandler(this.buttonRokuTVStreaming_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 530);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxAutoSleep);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonQuit);
            this.Controls.Add(this.buttonSleep);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxDuration);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.textBoxEndTime);
            this.Controls.Add(this.labelEndTime);
            this.Controls.Add(this.textBoxStartTime);
            this.Controls.Add(this.labelStartTime);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.textBoxTitle);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.textBoxChannel);
            this.Controls.Add(this.labelChannel);
            this.Controls.Add(this.textBoxStation);
            this.Controls.Add(this.labelStation);
            this.Controls.Add(this.treeViewFiles);
            this.Name = "FormMain";
            this.Text = "HDPVR录制";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewFiles;
        private System.Windows.Forms.Label labelStation;
        private System.Windows.Forms.TextBox textBoxStation;
        private System.Windows.Forms.TextBox textBoxChannel;
        private System.Windows.Forms.Label labelChannel;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxStartTime;
        private System.Windows.Forms.Label labelStartTime;
        private System.Windows.Forms.TextBox textBoxEndTime;
        private System.Windows.Forms.Label labelEndTime;
        private System.Windows.Forms.TextBox textBoxDuration;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonSleep;
        private System.Windows.Forms.Button buttonQuit;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.CheckBox checkBoxAutoSleep;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonStartRoku;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxrokuRoot;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonStopRokuTVStreaming;
        private System.Windows.Forms.Button buttonRokuTVStreaming;
    }
}

