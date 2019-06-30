using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using RecyclameV2.Clases;
using RecyclameV2.Utils;

namespace RecyclameV2
{
    public partial class FrmInicioSesionUsuario : MetroForm
    {
        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        FormRecyclame recyclame = null;

        public FrmInicioSesionUsuario()
        {
            InitializeComponent();
            recyclame = new FormRecyclame();
        }

        private void txtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txtContraseña.Focus();
            }
        }

        private void txtContraseña_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAceptar_Click(null, null);
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (esUsuarioValido())
            {
                Empleado empleado = new Empleado();
                empleado.Usuario = txtUsuario.Text;
                empleado.Password = txtContraseña.Text;
                List<SqlParameter> parametros = new List<SqlParameter>();
                parametros.Add(new SqlParameter() { ParameterName = "@P_Usuario", Value = empleado.Usuario });
                parametros.Add(new SqlParameter() { ParameterName = "@P_Password", Value = empleado.Password });
                DataSet dataset = BaseDatos.ejecutarProcedimientoConsulta("Empleado_Inicio_Sesion_sp", parametros);
                if (dataset != null && dataset.Tables.Count > 0)
                {
                    if (dataset.Tables["Empleado_Inicio_Sesion_sp"].Rows != null && dataset.Tables["Empleado_Inicio_Sesion_sp"].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataset.Tables["Empleado_Inicio_Sesion_sp"].Rows)
                        {
                            empleado.Cargar(row);
                        }
                        if (empleado.Activo)
                        {
                            FormRecyclame._Empleado = empleado;
                            this.Hide();
                            recyclame.WindowState = FormWindowState.Maximized;
                            Properties.Settings.Default.NombreUsuario = empleado.Nombre.ToUpper();
                            recyclame.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show(this, "El usuario y contraseña son correctos.\r\nPero el usuario " + empleado.Usuario + " esta dado de baja.\r\nFavor de iniciar sesión con un usuario activo.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        empleado = null;
                        txtUsuario.Text = string.Empty;
                        txtContraseña.Text = string.Empty;
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "El usuario y/o contraseña son incorrectos. Intente nuevamente por favor.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtUsuario.Focus();
                    }
                }
                else
                {
                    empleado = null;
                    txtUsuario.Text = string.Empty;
                    txtContraseña.Text = string.Empty;
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "El usuario y/o contraseña son incorrectos. Intente nuevamente por favor.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUsuario.Focus();
                }
            }
        }

        public bool esUsuarioValido()
        {
            string mensaje = string.Empty;
            bool user = false;
            if (txtUsuario.Text.Trim().Length == 0)
            {
                mensaje = "No se ha introducido ningún usuario. Favor de agregar un nombre de usuario.";
                user = true;
            }
            if (txtContraseña.Text.Trim().Length == 0)
            {
                if (mensaje.Length > 0)
                {
                    mensaje += Environment.NewLine;
                }
                mensaje += "No se ha introducido ninguna contraseña. Favor de introducir una contraseña.";
            }
            if (mensaje.Length > 0)
            {
                MessageBox.Show(mensaje, Global.STR_NOMBRE_SISTEMA, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (user)
                {
                    txtUsuario.Focus();
                }
                else
                {
                    txtContraseña.Focus();
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void WndProc(ref Message m)
        {
            int nCodigoMensaje = Program.RegisterWindowMessage(Global.MENSAJES.EJECUTANDO);
            if (m.Msg == nCodigoMensaje)
            {
                if (this.Visible)
                {
                    ShowWindow(this.Handle, 1);

                    SetForegroundWindow(this.Handle);
                }
                else
                {
                    if (recyclame != null)
                    {
                        recyclame.Mostrar();
                    }
                }
            }
            else
            {
                try
                {
                    base.WndProc(ref m);
                }
                catch (Exception) { }
            }
        }

        private void FrmInicioSesionUsuario_Load(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                SetForegroundWindow(this.Handle);
            }
            this.Focus();
            bool hayEmpleadosRegistrados = false;
            DataRow row = BaseDatos.ejecutarProcedimientoConsultaDataRow("Empleado_Max_Consultar_sp", null);
            if (row != null)
            {
                hayEmpleadosRegistrados = (Convert.ToInt64(row["Id"]) > 0);
            }
            if (hayEmpleadosRegistrados)
            {
                if (!recyclame._bInit)
                {
                    this.Close();
                }
                txtUsuario.Focus();
            }
            else
            {

                this.Hide();
                try
                {
                    recyclame.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

                this.Close();
            }
        }
    }
}
