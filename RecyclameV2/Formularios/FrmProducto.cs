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
using RecyclameV2.Clases;
using RecyclameV2.Utils;

namespace RecyclameV2.Formularios
{
    public partial class FrmProducto : MetroForm
    {
        Productos _producto = null;
        ProductoListado productoListado = null;

        public FrmProducto(ProductoListado producto)
        {
            InitializeComponent();
            productoListado = producto;
            cargarProducto(producto);
        }

        private void cargarProducto(ProductoListado productoListado)
        {
            _producto = new Productos();
            _producto.Activo = true;
            _producto.Codigo_de_Barras = productoListado.Codigo_de_Barras;
            _producto.Codigo_Producto = productoListado.Codigo_Producto;
            _producto.Codigo_de_Barras = productoListado.Codigo_de_Barras;
            _producto.Descripcion = productoListado.Descripcion;
            _producto.IdLinea1 = productoListado.IdLinea1;
            _producto.IdLinea2 = productoListado.IdLinea2;
            _producto.IdLinea3 = productoListado.IdLinea3;
            _producto.TieneNumeroSerie = productoListado.TieneNumeroSerie;
            _producto.Ultimo_Costo = productoListado.Ultimo_Costo;
            _producto.Unidad_de_Medida = productoListado.Unidad_de_Medida;
            _producto.Producto_Id = productoListado.Producto_Id;
            _producto.Detalle.Cantidad = productoListado.Existencia;
            _producto.Detalle.Cantidad_Maxima = productoListado.Cantidad_Maxima;
            _producto.Detalle.Cantidad_Mayoreo = productoListado.Cantidad_Mayoreo;
            _producto.Detalle.Cantidad_Minima = productoListado.Cantidad_Minima;
            //_producto.Detalle.Codigo_de_Barras = productoListado.Codigo_de_Barras;
            _producto.Detalle.Color = productoListado.Color;
            _producto.Detalle.Costo_Proveedor = productoListado.Costo_Proveedor;
            _producto.Detalle.IEPS = productoListado.IEPS;
            _producto.Detalle.IVA = productoListado.IVA;
            _producto.Detalle.Marca = productoListado.Marca;
            _producto.Detalle.Precio_General = productoListado.Precio_General;
            _producto.Detalle.Precio_Mayoreo = productoListado.Precio_Mayoreo;
            _producto.Detalle.Precio_Compra = productoListado.Precio_Compra;
            _producto.Detalle.Producto_Id = productoListado.Producto_Id;
            _producto.Detalle.Proveedor_Id = productoListado.Proveedor_Id;
            _producto.Detalle.setQueryGrabar("Producto_Detalle_Editar_sp");
            CargarProducto();
        }
        public Productos ObtenerProducto()
        {
            return _producto;
        }
        public ProductoListado ObtenerProductoEditado()
        {
            productoListado.Codigo_de_Barras = _producto.Codigo_de_Barras;
            productoListado.Codigo_Producto = _producto.Codigo_Producto;
            productoListado.Codigo_de_Barras = _producto.Codigo_de_Barras;
            productoListado.Descripcion = _producto.Descripcion;
            productoListado.IdLinea1 = _producto.IdLinea1;
            productoListado.IdLinea2 = _producto.IdLinea2;
            productoListado.IdLinea3 = _producto.IdLinea3;
            productoListado.TieneNumeroSerie = _producto.TieneNumeroSerie;
            productoListado.Ultimo_Costo = _producto.Ultimo_Costo;
            productoListado.Unidad_de_Medida = _producto.Unidad_de_Medida;
            productoListado.Producto_Id = _producto.Producto_Id;
            productoListado.Existencia = _producto.Detalle.Cantidad;
            productoListado.Cantidad_Maxima = _producto.Detalle.Cantidad_Maxima;
            productoListado.Cantidad_Mayoreo = _producto.Detalle.Cantidad_Mayoreo;
            productoListado.Cantidad_Minima = _producto.Detalle.Cantidad_Minima;
            //productoListado.Codigo_de_Barras = _producto.Detalle.Codigo_de_Barras;
            productoListado.Color = _producto.Detalle.Color;
            productoListado.Costo_Proveedor = _producto.Detalle.Costo_Proveedor;
            productoListado.IEPS = _producto.Detalle.IEPS;
            productoListado.IVA = _producto.Detalle.IVA;
            productoListado.Marca = _producto.Detalle.Marca;
            productoListado.Precio_General = _producto.Detalle.Precio_General;
            productoListado.Precio_Mayoreo = _producto.Detalle.Precio_Mayoreo;
            productoListado.Precio_Compra = _producto.Detalle.Precio_Compra;
            productoListado.Producto_Id = _producto.Detalle.Producto_Id;
            productoListado.Proveedor_Id = _producto.Detalle.Proveedor_Id;
            productoListado.Departamento = "";
            productoListado.Modelo = "";
            productoListado.Marca = "";
            return productoListado;
        }

        private void btnGrabar_Click(object sender, EventArgs e)
        {
            try
            {
                _producto = obtieneProductoDeControles();
                if (esProductoValido(_producto))
                {
                    if (_producto.Grabar())
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "El producto ha sido actualizado correctamente", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DialogResult = System.Windows.Forms.DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "Error al Intentar actualizar el producto.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, string.Format("Error al Intentar actualizar el producto. Detalle:{0}", ex.Message),
                    this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {

        }

        private decimal selectItemCombo(DataTable dataTable, string nombre, object value, string ColumnReturn)
        {
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row[nombre] is string)
                    {
                        if (string.Compare(Convert.ToString(value), Convert.ToString(row[nombre]), true) == 0)
                        {
                            return Convert.ToDecimal(row[ColumnReturn]);
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(value) == Convert.ToInt32(row[nombre]))
                        {
                            return Convert.ToDecimal(row[ColumnReturn]);
                        }
                    }
                }
            }
            return -1;
        }
        private void CargarProducto()
        {
            txtDescripcion.Text = _producto.Descripcion;
            txtCodigoBarrasProducto.Text = _producto.Codigo_de_Barras;
            txtUnidadMedidaProducto.Text = _producto.Unidad_de_Medida;
            txtPrecioVenta.Numero = _producto.Detalle.Precio_General;
            txtPrecioCompra.Numero = _producto.Detalle.Precio_Compra;
            txtDescripcion.Focus();
        }

        private void FrmProducto_Load(object sender, EventArgs e)
        {
            //Cargarcombos();
            txtDescripcion.Focus();
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            Global.moveFocusToNextControl(e.KeyCode);
        }

        private Productos obtieneProductoDeControles()
        {
            _producto.Descripcion = txtDescripcion.Text;
            _producto.Codigo_de_Barras = txtCodigoBarrasProducto.Text.Replace("'", "").Replace("\"", "");
            _producto.Unidad_de_Medida = txtUnidadMedidaProducto.Text;
            _producto.Activo = true;
            _producto.Detalle.Codigo_de_Barras = _producto.Codigo_de_Barras;
            _producto.Detalle.Color = "";
            _producto.Detalle.Costo_Proveedor = _producto.Ultimo_Costo;
            _producto.Detalle.IVA = 16;
            _producto.Detalle.Precio_General = txtPrecioVenta.Numero;
            _producto.Detalle.Precio_Compra = txtPrecioCompra.Numero;
            _producto.Detalle.Cantidad_Minima = 0;
            _producto.Detalle.Cantidad_Maxima = 0;
            _producto.Detalle.Cantidad_Mayoreo = 0;
            return _producto;
        }

        private bool esProductoValido(Productos producto)
        {
            string strMensaje = string.Empty;
            bool bFocus = false;
            if (producto.Descripcion.Trim().Length == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- La descripción del producto es requerida.";

                if (!bFocus)
                {
                    txtDescripcion.Focus();
                    bFocus = true;
                }
            }

            if (producto.Codigo_de_Barras.Trim().Length == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El código de barras del producto es requerido.";

                if (!bFocus)
                {
                    txtCodigoBarrasProducto.Focus();
                    bFocus = true;
                }
            }

            if (producto.Unidad_de_Medida.Trim().Length == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- La unidad de medida del producto es requerida.";

                if (!bFocus)
                {
                    txtUnidadMedidaProducto.Focus();
                    bFocus = true;
                }
            }

            if (producto.Detalle.Precio_General == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El precio venta del producto es requerido.";

                if (!bFocus)
                {
                    txtPrecioVenta.Focus();
                    bFocus = true;
                }
            }
            if (producto.Detalle.Precio_Compra == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El precio compra del producto es requerido.";

                if (!bFocus)
                {
                    txtPrecioCompra.Focus();
                    bFocus = true;
                }
            }
            if (strMensaje != string.Empty)
            {
                strMensaje = "El producto no puede ser actualizado debido a que: " + Environment.NewLine + Environment.NewLine + strMensaje;
                DevExpress.XtraEditors.XtraMessageBox.Show(this, strMensaje, Global.STR_NOMBRE_SISTEMA, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
            return true;
        }
    }
}
