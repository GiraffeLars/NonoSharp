namespace GUI
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelBoard = new Panel();
            panelVerHints = new Panel();
            panelHorHints = new Panel();
            SuspendLayout();
            // 
            // panelBoard
            // 
            panelBoard.Location = new Point(166, 118);
            panelBoard.Name = "panelBoard";
            panelBoard.Size = new Size(700, 700);
            panelBoard.TabIndex = 0;
            panelBoard.Paint += panelBoard_Paint;
            panelBoard.MouseClick += panelBoard_MouseClick;
            // 
            // panelVerHints
            // 
            panelVerHints.Location = new Point(166, 12);
            panelVerHints.Name = "panelVerHints";
            panelVerHints.Size = new Size(700, 100);
            panelVerHints.TabIndex = 0;
            panelVerHints.Paint += panelVerHints_Paint;
            // 
            // panelHorHints
            // 
            panelHorHints.Location = new Point(12, 118);
            panelHorHints.Name = "panelHorHints";
            panelHorHints.Size = new Size(148, 700);
            panelHorHints.TabIndex = 0;
            panelHorHints.Paint += panelHorHints_Paint;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1904, 1041);
            Controls.Add(panelBoard);
            Controls.Add(panelHorHints);
            Controls.Add(panelVerHints);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Picross";
            WindowState = FormWindowState.Maximized;
            ResumeLayout(false);
        }

        #endregion

        private Panel panelBoard;
        private Panel panelVerHints;
        private Panel panelHorHints;
    }
}
