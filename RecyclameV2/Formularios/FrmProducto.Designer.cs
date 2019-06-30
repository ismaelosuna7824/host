namespace RecyclameV2.Formularios
{
    partial class FrmProducto
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
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnGrabar = new System.Windows.Forms.Button();
            this.labelControl32 = new MetroFramework.Controls.MetroLabel();
            this.labelControl31 = new MetroFramework.Controls.MetroLabel();
            this.labelControl24 = new MetroFramework.Controls.MetroLabel();
            this.txtDescripcion = new MetroFramework.Controls.MetroTextBox();
            this.labelControl21 = new MetroFramework.Controls.MetroLabel();
            this.labelControl22 = new MetroFramework.Controls.MetroLabel();
            this.txtUnidadMedidaProducto = new MetroFramework.Controls.MetroTextBox();
            this.txtCodigoBarrasProducto = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLimpiar.BackColor = System.Drawing.Color.SteelBlue;
            this.btnLimpiar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Trebuchet MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.ForeColor = System.Drawing.Color.White;
            this.btnLimpiar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLimpiar.Location = new System.Drawing.Point(268, 195);
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(2);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(101, 41);
            this.btnLimpiar.TabIndex = 211;
            this.btnLimpiar.Text = "Cerrar";
            this.btnLimpiar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLimpiar.UseVisualStyleBackColor = false;
            this.btnLimpiar.Click += new System.EventHandler(this.btnLimpiar_Click);
            // 
            // btnGrabar
            // 
            this.btnGrabar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGrabar.BackColor = System.Drawing.Color.SteelBlue;
            this.btnGrabar.FlatAppearance.BorderSize = 0;
            this.btnGrabar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnGrabar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGrabar.Font = new System.Drawing.Font("Trebuchet MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGrabar.ForeColor = System.Drawing.Color.White;
            this.btnGrabar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGrabar.Location = new System.Drawing.Point(163, 195);
            this.btnGrabar.Margin = new System.Windows.Forms.Padding(2);
            this.btnGrabar.Name = "btnGrabar";
            this.btnGrabar.Size = new System.Drawing.Size(101, 41);
            this.btnGrabar.TabIndex = 210;
            this.btnGrabar.Text = "Guardar";
            this.btnGrabar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnGrabar.UseVisualStyleBackColor = false;
            this.btnGrabar.Click += new System.EventHandler(this.btnGrabar_Click);
            // 
            // labelControl32
            // 
            this.labelControl32.AutoSize = true;
            this.labelControl32.Location = new System.Drawing.Point(22, 176);
            this.labelControl32.Name = "labelControl32";
            this.labelControl32.Size = new System.Drawing.Size(102, 19);
            this.labelControl32.TabIndex = 209;
            this.labelControl32.Text = "Precio Compra:";
            // 
            // labelControl31
            // 
            this.labelControl31.AutoSize = true;
            this.labelControl31.Location = new System.Drawing.Point(22, 147);
            this.labelControl31.Name = "labelControl31";
            this.labelControl31.Size = new System.Drawing.Size(85, 19);
            this.labelControl31.TabIndex = 208;
            this.labelControl31.Text = "Precio Venta:";
            // 
            // labelControl24
            // 
            this.labelControl24.AutoSize = true;
            this.labelControl24.Location = new System.Drawing.Point(22, 65);
            this.labelControl24.Name = "labelControl24";
            this.labelControl24.Size = new System.Drawing.Size(79, 19);
            this.labelControl24.TabIndex = 207;
            this.labelControl24.Text = "Descripción:";
            // 
            // txtDescripcion
            // 
            // 
            // 
            // 
            this.txtDescripcion.CustomButton.Image = null;
            this.txtDescripcion.CustomButton.Location = new System.Drawing.Point(203, 2);
            this.txtDescripcion.CustomButton.Name = "";
            this.txtDescripcion.CustomButton.Size = new System.Drawing.Size(15, 15);
            this.txtDescripcion.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.txtDescripcion.CustomButton.TabIndex = 1;
            this.txtDescripcion.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.txtDescripcion.CustomButton.UseSelectable = true;
            this.txtDescripcion.CustomButton.Visible = false;
            this.txtDescripcion.Lines = new string[0];
            this.txtDescripcion.Location = new System.Drawing.Point(148, 62);
            this.txtDescripcion.MaxLength = 32767;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.PasswordChar = '\0';
            this.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtDescripcion.SelectedText = "";
            this.txtDescripcion.SelectionLength = 0;
            this.txtDescripcion.SelectionStart = 0;
            this.txtDescripcion.ShortcutsEnabled = true;
            this.txtDescripcion.Size = new System.Drawing.Size(221, 20);
            this.txtDescripcion.TabIndex = 202;
            this.txtDescripcion.UseSelectable = true;
            this.txtDescripcion.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.txtDescripcion.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.txtDescripcion.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtDescripcion_KeyDown);
            // 
            // labelControl21
            // 
            this.labelControl21.AutoSize = true;
            this.labelControl21.Location = new System.Drawing.Point(22, 118);
            this.labelControl21.Name = "labelControl21";
            this.labelControl21.Size = new System.Drawing.Size(122, 19);
            this.labelControl21.TabIndex = 206;
            this.labelControl21.Text = "Unidad de Medida:";
            // 
            // labelControl22
            // 
            this.labelControl22.AutoSize = true;
            this.labelControl22.Location = new System.Drawing.Point(22, 92);
            this.labelControl22.Name = "labelControl22";
            this.labelControl22.Size = new System.Drawing.Size(116, 19);
            this.labelControl22.TabIndex = 205;
            this.labelControl22.Text = "Código de Barras:";
            // 
            // txtUnidadMedidaProducto
            // 
            // 
            // 
            // 
            this.txtUnidadMedidaProducto.CustomButton.Image = null;
            this.txtUnidadMedidaProducto.CustomButton.Location = new System.Drawing.Point(203, 2);
            this.txtUnidadMedidaProducto.CustomButton.Name = "";
            this.txtUnidadMedidaProducto.CustomButton.Size = new System.Drawing.Size(15, 15);
            this.txtUnidadMedidaProducto.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.txtUnidadMedidaProducto.CustomButton.TabIndex = 1;
            this.txtUnidadMedidaProducto.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.txtUnidadMedidaProducto.CustomButton.UseSelectable = true;
            this.txtUnidadMedidaProducto.CustomButton.Visible = false;
            this.txtUnidadMedidaProducto.Lines = new string[0];
            this.txtUnidadMedidaProducto.Location = new System.Drawing.Point(148, 115);
            this.txtUnidadMedidaProducto.MaxLength = 32767;
            this.txtUnidadMedidaProducto.Name = "txtUnidadMedidaProducto";
            this.txtUnidadMedidaProducto.PasswordChar = '\0';
            this.txtUnidadMedidaProducto.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtUnidadMedidaProducto.SelectedText = "";
            this.txtUnidadMedidaProducto.SelectionLength = 0;
            this.txtUnidadMedidaProducto.SelectionStart = 0;
            this.txtUnidadMedidaProducto.ShortcutsEnabled = true;
            this.txtUnidadMedidaProducto.Size = new System.Drawing.Size(221, 20);
            this.txtUnidadMedidaProducto.TabIndex = 204;
            this.txtUnidadMedidaProducto.UseSelectable = true;
            this.txtUnidadMedidaProducto.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.txtUnidadMedidaProducto.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // txtCodigoBarrasProducto
            // 
            // 
            // 
            // 
            this.txtCodigoBarrasProducto.CustomButton.Image = null;
            this.txtCodigoBarrasProducto.CustomButton.Location = new System.Drawing.Point(203, 2);
            this.txtCodigoBarrasProducto.CustomButton.Name = "";
            this.txtCodigoBarrasProducto.CustomButton.Size = new System.Drawing.Size(15, 15);
            this.txtCodigoBarrasProducto.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.txtCodigoBarrasProducto.CustomButton.TabIndex = 1;
            this.txtCodigoBarrasProducto.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.txtCodigoBarrasProducto.CustomButton.UseSelectable = true;
            this.txtCodigoBarrasProducto.CustomButton.Visible = false;
            this.txtCodigoBarrasProducto.Lines = new string[0];
            this.txtCodigoBarrasProducto.Location = new System.Drawing.Point(148, 89);
            this.txtCodigoBarrasProducto.MaxLength = 15;
            this.txtCodigoBarrasProducto.Name = "txtCodigoBarrasProducto";
            this.txtCodigoBarrasProducto.PasswordChar = '\0';
            this.txtCodigoBarrasProducto.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtCodigoBarrasProducto.SelectedText = "";
            this.txtCodigoBarrasProducto.SelectionLength = 0;
            this.txtCodigoBarrasProducto.SelectionStart = 0;
            this.txtCodigoBarrasProducto.ShortcutsEnabled = true;
            this.txtCodigoBarrasProducto.Size = new System.Drawing.Size(221, 20);
            this.txtCodigoBarrasProducto.TabIndex = 203;
            this.txtCodigoBarrasProducto.UseSelectable = true;
            this.txtCodigoBarrasProducto.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.txtCodigoBarrasProducto.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // FrmProducto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 248);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnGrabar);
            this.Controls.Add(this.labelControl32);
            this.Controls.Add(this.labelControl31);
            this.Controls.Add(this.labelControl24);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.labelControl21);
            this.Controls.Add(this.labelControl22);
            this.Controls.Add(this.txtUnidadMedidaProducto);
            this.Controls.Add(this.txtCodigoBarrasProducto);
            this.Name = "FrmProducto";
            this.Text = "Editar Producto";
            this.Load += new System.EventHandler(this.FrmProducto_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroLabel labelControl32;
        private TextBoxNumerico txtPrecioCompra;
        private MetroFramework.Controls.MetroLabel labelControl31;
        private TextBoxNumerico txtPrecioVenta;
        private MetroFramework.Controls.MetroLabel labelControl24;
        private MetroFramework.Controls.MetroTextBox txtDescripcion;
        private MetroFramework.Controls.MetroLabel labelControl21;
        private MetroFramework.Controls.MetroLabel labelControl22;
        private MetroFramework.Controls.MetroTextBox txtUnidadMedidaProducto;
        private MetroFramework.Controls.MetroTextBox txtCodigoBarrasProducto;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnGrabar;
    }
}