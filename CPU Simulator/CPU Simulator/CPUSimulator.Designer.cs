namespace CPU_Simulator
{
    partial class MainForm
    {
        ///required designer variable
        private System.ComponentModel.IContainer components = null;

        ///clean up any resources being used
        ///disposing = true if managed resources should be disposed
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "MainForm";
            this.ResumeLayout(false);

}

        #endregion
    }
}

