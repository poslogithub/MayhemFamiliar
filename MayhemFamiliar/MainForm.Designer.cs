namespace MayhemFamiliar
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSpeaker = new System.Windows.Forms.TabPage();
            this.listBoxOpponentsVoices = new System.Windows.Forms.ListBox();
            this.groupBoxOpponentsSpeakMode = new System.Windows.Forms.GroupBox();
            this.radioButtonOpponentsSpeakModeOff = new System.Windows.Forms.RadioButton();
            this.radioButtonOpponentsSpeakModeThird = new System.Windows.Forms.RadioButton();
            this.radioButtonOpponentsSpeakModeOn = new System.Windows.Forms.RadioButton();
            this.groupBoxYourSpeakMode = new System.Windows.Forms.GroupBox();
            this.radioButtonYourSpeakModeOff = new System.Windows.Forms.RadioButton();
            this.radioButtonYourSpeakModeOn = new System.Windows.Forms.RadioButton();
            this.groupBoxOpponentsSynthesizer = new System.Windows.Forms.GroupBox();
            this.radioButtonOpponentsVoicevox = new System.Windows.Forms.RadioButton();
            this.radioButtonOpponentsSAPI = new System.Windows.Forms.RadioButton();
            this.buttonTestSpeech = new System.Windows.Forms.Button();
            this.listBoxYourVoices = new System.Windows.Forms.ListBox();
            this.groupBoxYourSynthesizer = new System.Windows.Forms.GroupBox();
            this.radioButtonYourSAPI = new System.Windows.Forms.RadioButton();
            this.radioButtonYourVoicevox = new System.Windows.Forms.RadioButton();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPageSpeaker.SuspendLayout();
            this.groupBoxOpponentsSpeakMode.SuspendLayout();
            this.groupBoxYourSpeakMode.SuspendLayout();
            this.groupBoxOpponentsSynthesizer.SuspendLayout();
            this.groupBoxYourSynthesizer.SuspendLayout();
            this.tabPageLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageSpeaker);
            this.tabControl1.Controls.Add(this.tabPageLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageSpeaker
            // 
            this.tabPageSpeaker.Controls.Add(this.listBoxOpponentsVoices);
            this.tabPageSpeaker.Controls.Add(this.groupBoxOpponentsSpeakMode);
            this.tabPageSpeaker.Controls.Add(this.groupBoxYourSpeakMode);
            this.tabPageSpeaker.Controls.Add(this.groupBoxOpponentsSynthesizer);
            this.tabPageSpeaker.Controls.Add(this.buttonTestSpeech);
            this.tabPageSpeaker.Controls.Add(this.listBoxYourVoices);
            this.tabPageSpeaker.Controls.Add(this.groupBoxYourSynthesizer);
            this.tabPageSpeaker.Location = new System.Drawing.Point(4, 22);
            this.tabPageSpeaker.Name = "tabPageSpeaker";
            this.tabPageSpeaker.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSpeaker.Size = new System.Drawing.Size(792, 424);
            this.tabPageSpeaker.TabIndex = 1;
            this.tabPageSpeaker.Text = "話者";
            this.tabPageSpeaker.UseVisualStyleBackColor = true;
            // 
            // listBoxOpponentsVoices
            // 
            this.listBoxOpponentsVoices.FormattingEnabled = true;
            this.listBoxOpponentsVoices.ItemHeight = 12;
            this.listBoxOpponentsVoices.Location = new System.Drawing.Point(209, 220);
            this.listBoxOpponentsVoices.Name = "listBoxOpponentsVoices";
            this.listBoxOpponentsVoices.Size = new System.Drawing.Size(197, 196);
            this.listBoxOpponentsVoices.TabIndex = 10;
            // 
            // groupBoxOpponentsSpeakMode
            // 
            this.groupBoxOpponentsSpeakMode.Controls.Add(this.radioButtonOpponentsSpeakModeOff);
            this.groupBoxOpponentsSpeakMode.Controls.Add(this.radioButtonOpponentsSpeakModeThird);
            this.groupBoxOpponentsSpeakMode.Controls.Add(this.radioButtonOpponentsSpeakModeOn);
            this.groupBoxOpponentsSpeakMode.Location = new System.Drawing.Point(209, 6);
            this.groupBoxOpponentsSpeakMode.Name = "groupBoxOpponentsSpeakMode";
            this.groupBoxOpponentsSpeakMode.Size = new System.Drawing.Size(200, 100);
            this.groupBoxOpponentsSpeakMode.TabIndex = 5;
            this.groupBoxOpponentsSpeakMode.TabStop = false;
            this.groupBoxOpponentsSpeakMode.Text = "対戦相手のアクション";
            // 
            // radioButtonOpponentsSpeakModeOff
            // 
            this.radioButtonOpponentsSpeakModeOff.AutoSize = true;
            this.radioButtonOpponentsSpeakModeOff.Location = new System.Drawing.Point(6, 62);
            this.radioButtonOpponentsSpeakModeOff.Name = "radioButtonOpponentsSpeakModeOff";
            this.radioButtonOpponentsSpeakModeOff.Size = new System.Drawing.Size(76, 16);
            this.radioButtonOpponentsSpeakModeOff.TabIndex = 9;
            this.radioButtonOpponentsSpeakModeOff.TabStop = true;
            this.radioButtonOpponentsSpeakModeOff.Text = "実況しない";
            this.radioButtonOpponentsSpeakModeOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonOpponentsSpeakModeThird
            // 
            this.radioButtonOpponentsSpeakModeThird.AutoSize = true;
            this.radioButtonOpponentsSpeakModeThird.Location = new System.Drawing.Point(6, 40);
            this.radioButtonOpponentsSpeakModeThird.Name = "radioButtonOpponentsSpeakModeThird";
            this.radioButtonOpponentsSpeakModeThird.Size = new System.Drawing.Size(112, 16);
            this.radioButtonOpponentsSpeakModeThird.TabIndex = 8;
            this.radioButtonOpponentsSpeakModeThird.TabStop = true;
            this.radioButtonOpponentsSpeakModeThird.Text = "三人称で実況する";
            this.radioButtonOpponentsSpeakModeThird.UseVisualStyleBackColor = true;
            // 
            // radioButtonOpponentsSpeakModeOn
            // 
            this.radioButtonOpponentsSpeakModeOn.AutoSize = true;
            this.radioButtonOpponentsSpeakModeOn.Location = new System.Drawing.Point(6, 18);
            this.radioButtonOpponentsSpeakModeOn.Name = "radioButtonOpponentsSpeakModeOn";
            this.radioButtonOpponentsSpeakModeOn.Size = new System.Drawing.Size(66, 16);
            this.radioButtonOpponentsSpeakModeOn.TabIndex = 7;
            this.radioButtonOpponentsSpeakModeOn.TabStop = true;
            this.radioButtonOpponentsSpeakModeOn.Text = "実況する";
            this.radioButtonOpponentsSpeakModeOn.UseVisualStyleBackColor = true;
            // 
            // groupBoxYourSpeakMode
            // 
            this.groupBoxYourSpeakMode.Controls.Add(this.radioButtonYourSpeakModeOff);
            this.groupBoxYourSpeakMode.Controls.Add(this.radioButtonYourSpeakModeOn);
            this.groupBoxYourSpeakMode.Location = new System.Drawing.Point(3, 6);
            this.groupBoxYourSpeakMode.Name = "groupBoxYourSpeakMode";
            this.groupBoxYourSpeakMode.Size = new System.Drawing.Size(200, 100);
            this.groupBoxYourSpeakMode.TabIndex = 4;
            this.groupBoxYourSpeakMode.TabStop = false;
            this.groupBoxYourSpeakMode.Text = "自分のアクション";
            // 
            // radioButtonYourSpeakModeOff
            // 
            this.radioButtonYourSpeakModeOff.AutoSize = true;
            this.radioButtonYourSpeakModeOff.Location = new System.Drawing.Point(6, 40);
            this.radioButtonYourSpeakModeOff.Name = "radioButtonYourSpeakModeOff";
            this.radioButtonYourSpeakModeOff.Size = new System.Drawing.Size(76, 16);
            this.radioButtonYourSpeakModeOff.TabIndex = 6;
            this.radioButtonYourSpeakModeOff.TabStop = true;
            this.radioButtonYourSpeakModeOff.Text = "実況しない";
            this.radioButtonYourSpeakModeOff.UseVisualStyleBackColor = true;
            // 
            // radioButtonYourSpeakModeOn
            // 
            this.radioButtonYourSpeakModeOn.AutoSize = true;
            this.radioButtonYourSpeakModeOn.Location = new System.Drawing.Point(6, 18);
            this.radioButtonYourSpeakModeOn.Name = "radioButtonYourSpeakModeOn";
            this.radioButtonYourSpeakModeOn.Size = new System.Drawing.Size(66, 16);
            this.radioButtonYourSpeakModeOn.TabIndex = 5;
            this.radioButtonYourSpeakModeOn.TabStop = true;
            this.radioButtonYourSpeakModeOn.Text = "実況する";
            this.radioButtonYourSpeakModeOn.UseVisualStyleBackColor = true;
            // 
            // groupBoxOpponentsSynthesizer
            // 
            this.groupBoxOpponentsSynthesizer.Controls.Add(this.radioButtonOpponentsVoicevox);
            this.groupBoxOpponentsSynthesizer.Controls.Add(this.radioButtonOpponentsSAPI);
            this.groupBoxOpponentsSynthesizer.Location = new System.Drawing.Point(209, 112);
            this.groupBoxOpponentsSynthesizer.Name = "groupBoxOpponentsSynthesizer";
            this.groupBoxOpponentsSynthesizer.Size = new System.Drawing.Size(200, 100);
            this.groupBoxOpponentsSynthesizer.TabIndex = 9;
            this.groupBoxOpponentsSynthesizer.TabStop = false;
            this.groupBoxOpponentsSynthesizer.Text = "対戦相手の音声合成ソフト";
            // 
            // radioButtonOpponentsVoicevox
            // 
            this.radioButtonOpponentsVoicevox.AutoSize = true;
            this.radioButtonOpponentsVoicevox.Location = new System.Drawing.Point(6, 40);
            this.radioButtonOpponentsVoicevox.Name = "radioButtonOpponentsVoicevox";
            this.radioButtonOpponentsVoicevox.Size = new System.Drawing.Size(80, 16);
            this.radioButtonOpponentsVoicevox.TabIndex = 8;
            this.radioButtonOpponentsVoicevox.TabStop = true;
            this.radioButtonOpponentsVoicevox.Text = "VOICEVOX";
            this.radioButtonOpponentsVoicevox.UseVisualStyleBackColor = true;
            // 
            // radioButtonOpponentsSAPI
            // 
            this.radioButtonOpponentsSAPI.AutoSize = true;
            this.radioButtonOpponentsSAPI.Location = new System.Drawing.Point(6, 18);
            this.radioButtonOpponentsSAPI.Name = "radioButtonOpponentsSAPI";
            this.radioButtonOpponentsSAPI.Size = new System.Drawing.Size(82, 16);
            this.radioButtonOpponentsSAPI.TabIndex = 7;
            this.radioButtonOpponentsSAPI.TabStop = true;
            this.radioButtonOpponentsSAPI.Text = "Speech API";
            this.radioButtonOpponentsSAPI.UseVisualStyleBackColor = true;
            // 
            // buttonTestSpeech
            // 
            this.buttonTestSpeech.Location = new System.Drawing.Point(671, 4);
            this.buttonTestSpeech.Name = "buttonTestSpeech";
            this.buttonTestSpeech.Size = new System.Drawing.Size(112, 24);
            this.buttonTestSpeech.TabIndex = 2;
            this.buttonTestSpeech.Text = "テスト発声";
            this.buttonTestSpeech.UseVisualStyleBackColor = true;
            // 
            // listBoxYourVoices
            // 
            this.listBoxYourVoices.FormattingEnabled = true;
            this.listBoxYourVoices.ItemHeight = 12;
            this.listBoxYourVoices.Location = new System.Drawing.Point(6, 220);
            this.listBoxYourVoices.Name = "listBoxYourVoices";
            this.listBoxYourVoices.Size = new System.Drawing.Size(197, 196);
            this.listBoxYourVoices.TabIndex = 1;
            // 
            // groupBoxYourSynthesizer
            // 
            this.groupBoxYourSynthesizer.Controls.Add(this.radioButtonYourSAPI);
            this.groupBoxYourSynthesizer.Controls.Add(this.radioButtonYourVoicevox);
            this.groupBoxYourSynthesizer.Location = new System.Drawing.Point(3, 112);
            this.groupBoxYourSynthesizer.Name = "groupBoxYourSynthesizer";
            this.groupBoxYourSynthesizer.Size = new System.Drawing.Size(200, 100);
            this.groupBoxYourSynthesizer.TabIndex = 6;
            this.groupBoxYourSynthesizer.TabStop = false;
            this.groupBoxYourSynthesizer.Text = "自分の音声合成ソフト";
            // 
            // radioButtonYourSAPI
            // 
            this.radioButtonYourSAPI.AutoSize = true;
            this.radioButtonYourSAPI.Location = new System.Drawing.Point(6, 18);
            this.radioButtonYourSAPI.Name = "radioButtonYourSAPI";
            this.radioButtonYourSAPI.Size = new System.Drawing.Size(82, 16);
            this.radioButtonYourSAPI.TabIndex = 0;
            this.radioButtonYourSAPI.TabStop = true;
            this.radioButtonYourSAPI.Text = "Speech API";
            this.radioButtonYourSAPI.UseVisualStyleBackColor = true;
            // 
            // radioButtonYourVoicevox
            // 
            this.radioButtonYourVoicevox.AutoSize = true;
            this.radioButtonYourVoicevox.Location = new System.Drawing.Point(6, 40);
            this.radioButtonYourVoicevox.Name = "radioButtonYourVoicevox";
            this.radioButtonYourVoicevox.Size = new System.Drawing.Size(80, 16);
            this.radioButtonYourVoicevox.TabIndex = 3;
            this.radioButtonYourVoicevox.TabStop = true;
            this.radioButtonYourVoicevox.Text = "VOICEVOX";
            this.radioButtonYourVoicevox.UseVisualStyleBackColor = true;
            // 
            // tabPageLog
            // 
            this.tabPageLog.Controls.Add(this.textBoxLog);
            this.tabPageLog.Location = new System.Drawing.Point(4, 22);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(792, 424);
            this.tabPageLog.TabIndex = 0;
            this.tabPageLog.Text = "ログ";
            this.tabPageLog.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(3, 3);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(786, 418);
            this.textBoxLog.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "MayhemFamiliar";
            this.tabControl1.ResumeLayout(false);
            this.tabPageSpeaker.ResumeLayout(false);
            this.groupBoxOpponentsSpeakMode.ResumeLayout(false);
            this.groupBoxOpponentsSpeakMode.PerformLayout();
            this.groupBoxYourSpeakMode.ResumeLayout(false);
            this.groupBoxYourSpeakMode.PerformLayout();
            this.groupBoxOpponentsSynthesizer.ResumeLayout(false);
            this.groupBoxOpponentsSynthesizer.PerformLayout();
            this.groupBoxYourSynthesizer.ResumeLayout(false);
            this.groupBoxYourSynthesizer.PerformLayout();
            this.tabPageLog.ResumeLayout(false);
            this.tabPageLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.TabPage tabPageSpeaker;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.RadioButton radioButtonYourSAPI;
        private System.Windows.Forms.ListBox listBoxYourVoices;
        private System.Windows.Forms.Button buttonTestSpeech;
        private System.Windows.Forms.RadioButton radioButtonYourVoicevox;
        private System.Windows.Forms.GroupBox groupBoxYourSpeakMode;
        private System.Windows.Forms.GroupBox groupBoxOpponentsSpeakMode;
        private System.Windows.Forms.RadioButton radioButtonOpponentsSpeakModeOff;
        private System.Windows.Forms.RadioButton radioButtonOpponentsSpeakModeThird;
        private System.Windows.Forms.RadioButton radioButtonOpponentsSpeakModeOn;
        private System.Windows.Forms.RadioButton radioButtonYourSpeakModeOff;
        private System.Windows.Forms.RadioButton radioButtonYourSpeakModeOn;
        private System.Windows.Forms.RadioButton radioButtonOpponentsVoicevox;
        private System.Windows.Forms.RadioButton radioButtonOpponentsSAPI;
        private System.Windows.Forms.GroupBox groupBoxOpponentsSynthesizer;
        private System.Windows.Forms.GroupBox groupBoxYourSynthesizer;
        private System.Windows.Forms.ListBox listBoxOpponentsVoices;
    }
}

