using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using RecyclameV2.Utils;
using RecyclameV2.Clases;

namespace RecyclameV2
{
    public partial class frmCliente : MetroFramework.Forms.MetroForm
    {
        Cliente _cliente = null;
        public frmCliente(Cliente cliente)
        {
            
            InitializeComponent();
            _cliente = cliente;
            CargarDatosCliente(_cliente);
        }

        public Cliente obtenerCliente()
        {
            return _cliente;
        }

        private void frmCliente_Load(object sender, EventArgs e)
        {

        }
        private void CargarDatosCliente(Cliente cliente)
        {
            txtNombre.Text = cliente.Nombre;
            txtApellidoP.Text = cliente.ApellidoPaterno;
            txtApellidoM.Text = cliente.ApellidoMaterno;
            txtRFC.Text = cliente.RFC;
            txtRazonSocial.Text = cliente.Razon_Social;
            txtComentario.Text = cliente.Comentario;
            if (cliente.Telefono != 0)
            {
                txtTelefono.Text = cliente.Telefono.ToString();
            }
            else
            {
                txtTelefono.Text = "";
            }
            txtMail.Text = cliente.Email;
            txtCalle.Text = cliente.Calle;
            txtLocalidad.Text = cliente.Localidad;
            txtCiudad.Text = cliente.Ciudad;
            txtColonia.Text = cliente.Colonia;
            txtNumExt.Text = cliente.NumExt;
            txtNumInt.Text = cliente.NumInt;
            txtCodigoPostal.Text = cliente.Codigo_Postal;
            txtPais.Text = cliente.Pais;
            txtEstado.Text = cliente.Estado;
            txtDiasCredito.Numero = Convert.ToDouble(cliente.Dias_de_Credito);
            txtCredito.Numero = cliente.Monto_Credito;
            txtCuentaContable.Text = cliente.Cuenta_Contable;

            cliente.Activo = true;
        }

        private void btnGrabar_Click(object sender, EventArgs e)
        {
            try
            {
                Cliente cliente = obtieneDatosCliente();
                if (esClienteValido(cliente))
                {
                    if (cliente.Localidad == null || cliente.Localidad.Length == 0)
                    {
                        cliente.Localidad = cliente.Ciudad;
                    }
                    if (cliente.Grabar())
                    {
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                        Close();
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "El cliente ha sido actualizado correctamente", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "Error al Intentar actualizar al cliente.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, string.Format("Error al Intentar actualizar al cliente. Detalle:{0}", ex.Message),
                    this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Cliente obtieneDatosCliente()
        {
            _cliente.Nombre = txtNombre.Text;
            _cliente.ApellidoPaterno = txtApellidoP.Text;
            _cliente.ApellidoMaterno = txtApellidoM.Text;
            _cliente.RFC = txtRFC.Text;
            _cliente.Razon_Social = txtRazonSocial.Text;
            _cliente.Cuenta_Contable = txtCuentaContable.Text;
            long telefono = 0;
            long.TryParse(txtTelefono.Text.Replace(" ", "").Replace("+", "").Replace("-", "").Replace("(", "").Replace(")", "").Trim(), out telefono);
            _cliente.Telefono = telefono;
            _cliente.Email = txtMail.Text;
            _cliente.Calle = txtCalle.Text;
            _cliente.Localidad = txtLocalidad.Text;
            _cliente.Ciudad = txtCiudad.Text;
            _cliente.Colonia = txtColonia.Text;
            _cliente.NumExt = txtNumExt.Text;
            _cliente.NumInt = txtNumInt.Text;
            _cliente.Codigo_Postal = txtCodigoPostal.Text;
            _cliente.Pais = txtPais.Text;
            _cliente.Estado = txtEstado.Text;
            _cliente.Comentario = txtComentario.Text;
            _cliente.Dias_de_Credito = (int)txtDiasCredito.Numero;
            _cliente.Activo = true;
            _cliente.Monto_Credito = txtCredito.Numero;
            return _cliente;
        }
        private bool esClienteValido(Cliente cliente)
        {
            string strMensaje = string.Empty;
            bool bFocus = false;
            if (cliente.Nombre.Trim().Length == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El nombre del cliente es requerido.";

                if (!bFocus)
                {
                    txtNombre.Focus();
                    bFocus = true;
                }
            }

            if (cliente.RFC.Trim().Length == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El RFC del cliente es requerido.";

                if (!bFocus)
                {
                    txtRFC.Focus();
                    bFocus = true;
                }
            }

            //if (cliente.Email.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- El correo electrónico del cliente es requerido.";

            //    if (!bFocus)
            //    { 
            //        txtEstado.Focus();
            //        bFocus = true;
            //    }
            //}

            ////if (cliente.Localidad.Trim().Length == 0)
            ////{
            ////    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            ////    strMensaje += "- La localidad del cliente es requerida.";

            ////    if (!bFocus)
            ////    {
            ////        txtLocalidad.Focus();
            ////        bFocus = true;
            ////    }
            ////}

            //if (cliente.Ciudad.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- La ciudad del cliente es requerida.";

            //    if (!bFocus)
            //    {
            //        txtCiudad.Focus();
            //        bFocus = true;
            //    }
            //}

            //if (cliente.Calle.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- La calle del cliente es requerida.";

            //    if (!bFocus)
            //    {
            //        txtCalle.Focus();
            //        bFocus = true;
            //    }
            //}
            //if (cliente.NumExt.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- El Número exterior del domicilio del cliente es requerido.";

            //    if (!bFocus)
            //    {
            //        txtNumExt.Focus();
            //        bFocus = true;
            //    }
            //}

            //if (cliente.Colonia.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- La colonia del cliente es requerida.";

            //    if (!bFocus)
            //    {
            //        txtColonia.Focus();
            //        bFocus = true;
            //    }
            //}
            //if (cliente.Codigo_Postal.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- El código postal del cliente es requerido.";

            //    if (!bFocus)
            //    {
            //        txtCodigoPostal.Focus();
            //        bFocus = true;
            //    }
            //}

            //if (cliente.Estado.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- El estado del cliente es requerido.";

            //    if (!bFocus)
            //    {
            //        txtEstado.Focus();
            //        bFocus = true;
            //    }
            //}

            //if (cliente.Pais.Trim().Length == 0)
            //{
            //    if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

            //    strMensaje += "- El país del cliente es requerido.";

            //    if (!bFocus)
            //    {
            //        txtPais.Focus();
            //        bFocus = true;
            //    }
            //}

            if (strMensaje != string.Empty)
            {
                strMensaje = "El cliente no puede ser actualizado debido a que: " + Environment.NewLine + Environment.NewLine + strMensaje;
                DevExpress.XtraEditors.XtraMessageBox.Show(this, strMensaje, Global.STR_NOMBRE_SISTEMA, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
            return true;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {

        }
    }
}
