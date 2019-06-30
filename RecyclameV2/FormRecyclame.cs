using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using RecyclameV2.Clases;
using RecyclameV2.Formularios;

namespace RecyclameV2
{
    public partial class FormRecyclame : MetroForm
    {
        public bool _bInit = false;
        public static Empleado _Empleado = new Empleado();
        public static DataTable _dataTableMetodosPago = null;
        public static RequisitosFacturacion requisitosFacturacion = null;
        public static readonly Dictionary<long, RequisitosFacturacion> dicFacturacion = new Dictionary<long, RequisitosFacturacion>();
        public static readonly Dictionary<long, MetodoPago> dicMetodoPago = new Dictionary<long, MetodoPago>();
        public static DatosFacturacion _datosFacturacion = new DatosFacturacion();
        public static UbicacionFiscal _ubicacionFiscal = new UbicacionFiscal();

        public FormRecyclame()
        {
            InitializeComponent();
            _bInit = true;
            AseingarTipoMovimientos();
        }

        private void AseingarTipoMovimientos()
        {
            Tipo_Movimiento _tipoMovimiento = new Tipo_Movimiento();
            _tipoMovimiento.Tipo_Movimiento_Id = -1;
            _tipoMovimiento.Descripcion = "Entrada por CFDI";
            _tipoMovimiento.Clave = "ECF";
            _tipoMovimiento.EntradaSalida = "E";
            _tipoMovimiento.Activo = true;
            _tipoMovimiento.Grabar();
        }

        private void tileItemVenta_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmCompraVenta venta = new FrmCompraVenta();
            venta.ShowDialog();
        }

        private void tileItemProovedor_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmProveedores proveedores = new FrmProveedores();
            proveedores.ShowDialog();
        }

        private void tileItemCliente_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmClientes clientes = new FrmClientes();
            clientes.ShowDialog();
        }

        private void tileItemInventario_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmEntradaInventario inventario = new FrmEntradaInventario();
            inventario.ShowDialog();
        }

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public void Mostrar()
        {
            ShowWindow(this.Handle, 1);

            SetForegroundWindow(this.Handle);
        }

        private void tileItemReporte_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmReportes reportes = new FrmReportes();
            reportes.ShowDialog();
        }

        private void tileItemConfiguracion_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmConfiguracion configuracion = new FrmConfiguracion();
            configuracion.ShowDialog();
        }

        private void tileItemEmpleados_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmEmpleados empleados = new FrmEmpleados();
            empleados.ShowDialog();
        }

        private void tileItemBascula_ItemClick(object sender, DevExpress.XtraEditors.TileItemEventArgs e)
        {
            FrmBascula bascula = new FrmBascula();
            bascula.ShowDialog();
        }
    }
}
