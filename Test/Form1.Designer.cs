namespace Test
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.placeHolderTextBox1 = new Bian.Controls.PlaceHolderTextBox();
            this.slideSwitch2 = new Bian.Controls.SlideSwitch();
            this.slideSwitch1 = new Bian.Controls.SlideSwitch();
            this.SuspendLayout();
            // 
            // placeHolderTextBox1
            // 
            this.placeHolderTextBox1.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.placeHolderTextBox1.Location = new System.Drawing.Point(135, 108);
            this.placeHolderTextBox1.Name = "placeHolderTextBox1";
            this.placeHolderTextBox1.PlaceholderText = "请输入密码：";
            this.placeHolderTextBox1.Size = new System.Drawing.Size(270, 34);
            this.placeHolderTextBox1.TabIndex = 4;
            // 
            // slideSwitch2
            // 
            this.slideSwitch2.Checked = true;
            this.slideSwitch2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.slideSwitch2.IsOpen = true;
            this.slideSwitch2.Location = new System.Drawing.Point(135, 301);
            this.slideSwitch2.Name = "slideSwitch2";
            this.slideSwitch2.ShowState = true;
            this.slideSwitch2.Size = new System.Drawing.Size(125, 47);
            this.slideSwitch2.TabIndex = 3;
            this.slideSwitch2.Text = "slideSwitch2";
            this.slideSwitch2.UseVisualStyleBackColor = true;
            // 
            // slideSwitch1
            // 
            this.slideSwitch1.IsOpen = false;
            this.slideSwitch1.Location = new System.Drawing.Point(280, 301);
            this.slideSwitch1.Name = "slideSwitch1";
            this.slideSwitch1.ShowState = true;
            this.slideSwitch1.Size = new System.Drawing.Size(125, 47);
            this.slideSwitch1.TabIndex = 2;
            this.slideSwitch1.Text = "slideSwitch1";
            this.slideSwitch1.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.placeHolderTextBox1);
            this.Controls.Add(this.slideSwitch2);
            this.Controls.Add(this.slideSwitch1);
            this.Name = "Form1";
            this.Text = "演示";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Bian.Controls.SlideSwitch slideSwitch1;
        private Bian.Controls.SlideSwitch slideSwitch2;
        private Bian.Controls.PlaceHolderTextBox placeHolderTextBox1;
    }
}

