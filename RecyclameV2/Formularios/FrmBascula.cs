using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using Microsoft.PointOfService;
using RecyclameV2.Clases;

namespace RecyclameV2.Formularios
{
    public partial class FrmBascula : MetroForm
    {
        bool _formPago = false;
        bool cancelIt = false;
        System.Windows.Forms.Timer t = null;
        System.Windows.Forms.Timer tBascula = null;
        private PosExplorer explorer;
        public static Scanner activeScanner;
        public static Scale activeScale;
        private DeviceInfo selectedScale;
        private DeviceInfo selectedScanner;
        public readonly static Dictionary<long, Empleado> dicEmpleados = new Dictionary<long, Empleado>();
        public readonly static Dictionary<long, Cliente> dicClientes = new Dictionary<long, Cliente>();
        public bool _bInit = false;
        public static Empleado _Empleado = new Empleado();
        public bool asignarPeso = false;
        public bool existeProducto = true;
        public static string LOG_FILE_PATH = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + @"\Recyclame.txt";
        public static string LOG_DEBUG_PATH = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + @"\Log_Recyclame.txt";
        readonly ASCIIEncoding encoder = new ASCIIEncoding();
        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        readonly byte[] inBuffer = new byte[] { 80 };
        public delegate void InvokeDelegate(double peso);
        long _clienteId = 0;
        double subTotal = 0;
        public static readonly Dictionary<long, List<VentaDetalle>> dicVentas = new Dictionary<long, List<VentaDetalle>>();
        long _lastProductoBasculaId = 0;
        bool asignoPeso = false;
        bool bascula = false;
        decimal CINCO_GRAMOS = .005M;
        bool incrementar = false;
        string _strProducto = string.Empty;
        bool _timer = false;
        bool _permiso = false;
        long _IdAutorizacion = -1;
        bool _fromScanner = false;
        bool _hayBascula = false;
        decimal pesoEtiqueta = 0;
        SerialPort port = null;
        public delegate void InvokeDelegateActualizar(object obj, Productos producto, double cantidad, bool existe);
        public delegate void InvokeDelegateActualizarMontos();
        public delegate void InvokeDelegateMostrarVentanas(string code);
        System.Windows.Forms.Timer tEliminar = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer tLabel = new System.Windows.Forms.Timer();
        public enum DataGridViewColumnas
        {
            CANTIDAD = 0,
            DESCRIPCION = 1,
            PRECIO = 2,
            IMPORTE = 3
        }
        public void Mostrar()
        {
            ShowWindow(this.Handle, 1);

            SetForegroundWindow(this.Handle);
        }

        public void actualizarDiccionario(VentaDetalle detalle)
        {
            if (dicVentas.ContainsKey(detalle.IdDatosFiscales))
            {
                List<VentaDetalle> lstProductos = dicVentas[detalle.IdDatosFiscales];
                int length = lstProductos.Count;
                for (int i = 0; i < length; i++)
                {
                    if (lstProductos[i].Id_Producto == detalle.Id_Producto)
                    {
                        lstProductos[i] = detalle;
                        break;
                    }
                }
            }
        }

        public FrmBascula()
        {
            InitializeComponent();
            _bInit = true;
            txtControl.Focus();
            lblArticulos.Text = "0";
        }

        void t_Tick(object sender, EventArgs e)
        {
            t.Stop();
            lblFecha.Text = DateTime.Now.ToString("dddd, dd MMMM, yyyy h:mm tt");
            habilitarDatalogic();
            /*if (!bascula)
            {
                if (port != null && port.IsOpen)
                {
                    leerPeso();
                }
                else
                {
                    decimal peso = obtenerPesoBascula();
                    lblBascula.Text = peso.ToString("N3", NumberFormatInfo.InvariantInfo) + " kg";
                }
            }*/
            t.Start();
        }

        private bool asignarPesoProductoBascula(long productoid, decimal dPEso)
        {
            int length = gridViewVentas.RowCount;
            if (length > 0)
            {
                VentaDetalle p = null;
                foreach (DataGridViewRow row in gridViewVentas.Rows)
                {
                    p = (VentaDetalle)row.Tag;
                    if (p.Id_Producto == productoid)
                    {
                        p.AsignoPeso = true;
                        asignarPeso = false;
                        decimal diferencia = 0;
                        if (incrementar)
                        {
                            if (p.Cantidad > 0)
                            {
                                if (dPEso > 0)
                                {
                                    if (pesoEtiqueta >= dPEso)
                                    {
                                        diferencia = pesoEtiqueta - dPEso;
                                    }
                                    else
                                    {
                                        diferencia = dPEso - pesoEtiqueta;
                                    }
                                    p.Cantidad -= Convert.ToDouble(pesoEtiqueta);
                                    if (diferencia >= 0 && diferencia <= CINCO_GRAMOS)
                                    {
                                        p.Cantidad += Convert.ToDouble(pesoEtiqueta);
                                    }
                                    else
                                    {
                                        if (pesoEtiqueta >= dPEso)
                                        {
                                            p.Cantidad += Convert.ToDouble(pesoEtiqueta);
                                        }
                                        else
                                        {
                                            p.Cantidad += Convert.ToDouble(dPEso);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                p.Cantidad += Convert.ToDouble(dPEso);
                            }
                        }
                        else
                        {
                            if (p.Cantidad > 0)
                            {
                                if (dPEso > 0)
                                {
                                    if (Convert.ToDecimal(p.Cantidad) >= dPEso)
                                    {
                                        diferencia = Convert.ToDecimal(p.Cantidad) - dPEso;
                                    }
                                    else
                                    {
                                        diferencia = dPEso - Convert.ToDecimal(p.Cantidad);
                                    }
                                    if (diferencia >= 0 && diferencia <= CINCO_GRAMOS)
                                    {
                                        p.Cantidad = p.Cantidad;
                                    }
                                    else
                                    {
                                        if (pesoEtiqueta >= dPEso)
                                        {
                                            p.Cantidad = p.Cantidad;
                                        }
                                        else
                                        {
                                            p.Cantidad = Convert.ToDouble(dPEso);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                p.Cantidad = Convert.ToDouble(dPEso);
                            }
                        }
                        actualizarPrecios(ref p);
                        row.Cells[(int)DataGridViewColumnas.CANTIDAD].Value = p.Cantidad;//.ToString("");
                        row.Cells[(int)DataGridViewColumnas.PRECIO].Value = Global.DoubleToString(p.Precio_Venta);//.ToString("");
                        row.Cells[(int)DataGridViewColumnas.IMPORTE].Value = Global.DoubleToString(Convert.ToDouble(p.Importe));//.ToString("");
                        row.Tag = p;
                        RefrescarMontos();
                        pesoEtiqueta = 0;
                        p = null;
                        return true;
                    }
                    p = null;
                }
            }
            return false;
        }

        private void habilitarDatalogic()
        {
            if (selectedScanner != null)
            {
                if (activeScanner != null && !activeScanner.Claimed)
                {
                    releaseScanner();
                    try
                    {
                        activeScanner = (Scanner)explorer.CreateInstance(selectedScanner);
                        activeScanner.Open();
                        activeScanner.Claim(1000);
                        if (activeScanner.Claimed)
                        {
                            activeScanner.DeviceEnabled = true;
                            activeScanner.DataEvent += new DataEventHandler(activeScanner_DataEvent);
                            activeScanner.ErrorEvent += new DeviceErrorEventHandler(activeScanner_ErrorEvent);
                            activeScanner.DecodeData = true;
                            activeScanner.DataEventEnabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        RecyclameV2.Utils.Logger.addLogEntry("Hablitar scanner excepcion: " + ex.ToString());
                    }
                }
            }
            if (selectedScale != null)
            {
                if (activeScale != null && !activeScale.Claimed)
                {
                    releaseBascula();
                    try
                    {
                        activeScale = (Scale)explorer.CreateInstance(selectedScale);
                        activeScale.Open();
                        activeScale.Claim(1000);
                        _hayBascula = true;
                        if (activeScale.Claimed)
                        {
                            activeScale.DeviceEnabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        RecyclameV2.Utils.Logger.addLogEntry("hablitar bascula excepcion: " + ex.ToString());
                    }
                }
            }
        }

        void activeScanner_ErrorEvent(object sender, DeviceErrorEventArgs e)
        {
            try
            {
                // re-enable the data event for subsequent scans
                activeScanner.DataEventEnabled = true;
            }

            catch (PosControlException ex)
            {
                RecyclameV2.Utils.Logger.addLogEntry(LOG_FILE_PATH, "DATA_EVENT :" + ex.ToString());
            }
        }

        private void releaseScanner()
        {
            if (activeScanner != null)
            {
                try
                {
                    activeScanner.DeviceEnabled = false;
                    activeScanner.Release();
                    activeScanner.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    activeScanner = null;
                }
            }
        }
        private void releaseBascula()
        {
            if (activeScale != null)
            {
                try
                {
                    activeScale.DeviceEnabled = false;
                    activeScale.Release();
                    activeScale.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    activeScale = null;
                }
            }
            else
            {
                if (port != null && port.IsOpen)
                {
                    try
                    {
                        port.Close();
                        port.Dispose();
                        port = null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }
        private decimal obtenerPesoBascula()
        {
            try
            {
                if (activeScale != null && activeScale.Claimed && activeScale.DeviceEnabled)
                {
                    return activeScale.ReadWeight(5000);
                }
            }
            catch (Exception ex)
            {
                //POSCremeriaElFuerte.Utils.Logger.addLogEntry(LOG_FILE_PATH, "btnCalibrarBascula_Click :" + ex.ToString());
            }
            return 0;
        }

        async void activeScanner_DataEvent(object sender, DataEventArgs e)
        {
            activeScanner.DataEventEnabled = true;
            //Thread oThread = new Thread(new ParameterizedThreadStart(leerCodigo));
            //oThread.Start(encoder.GetString(activeScanner.ScanDataLabel));
            if (!_formPago)
            {
                if (!_timer)
                {
                    //pesoEtiqueta = 0;
                    if (existeProducto)
                    {
                        try
                        {
                            _lastProductoBasculaId = 0;
                            incrementar = false;
                            // Display the ASCII encoded label text
                            string code = encoder.GetString(activeScanner.ScanDataLabel);
                            if (code.StartsWith("20"))
                            {
                                if (_hayBascula)
                                {
                                    asignoPeso = false;
                                    bascula = true;
                                }
                                else
                                {
                                    asignoPeso = true;
                                    bascula = false;
                                    _permiso = false;
                                }
                                code = code.Remove(0, 2);
                                string peso = code.Remove(0, 5);
                                code = code.Substring(0, 5);
                                /*pesoEtiqueta = decimal.Round(Convert.ToDecimal(peso) / 10000, 3);
                                if (pesoEtiqueta > 15)
                                {
                                    _permiso = true;
                                }
                                else
                                {
                                    _permiso = false;
                                }*/
                            }
                            else
                            {
                                asignoPeso = true;
                                bascula = false;
                                _permiso = false;
                            }
                            if (txtControl.Text.IndexOf("*") != -1)
                            {
                                code = txtControl.Text + code;
                            }
                            double cangtidad = 0;
                            if (code.IndexOf("*") != -1)
                            {
                                cangtidad = obtenerCantidad(ref code);
                            }
                            txtControl.Focus();
                            txtControl.Text = code;
                            //Global.SendEnter();
                            if (MuestraProductos(cangtidad))
                            {
                                RefrescarMontos();
                            }
                            else
                            {
                                asignoPeso = true;
                                bascula = false;
                                _permiso = false;
                            }
                            // Display the encoding type
                            // re-enable the data event for subsequent scans                    
                        }
                        catch (PosControlException ex)
                        {
                            RecyclameV2.Utils.Logger.addLogEntry(LOG_FILE_PATH, "DATA_EVENT :" + ex.ToString());
                        }
                    }
                }
                else
                {
                    if (!_permiso)
                    {
                        if (_strProducto.Length > 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (_strProducto.Length > 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private double obtenerCantidad(ref string codigo)
        {
            double cantidad = 0;
            int start = codigo.IndexOf("*");
            if (start > -1)
            {
                string can = codigo.Substring(0, start).Replace("*", "").Trim();
                codigo = codigo.Remove(0, start).Replace("*", "").Trim();
                double.TryParse(can, out cantidad);
            }
            return cantidad;
        }

        public bool MuestraProductos(double totalCantidad)
        {
            try
            {
                Productos producto = new Productos();
                object objProducto = producto.buscarProductoVenta(txtControl.Text);
                if (objProducto != null)
                {
                    if (!existeProducto)
                    {
                        return false;
                    }
                    if (objProducto is DataTable)
                    {
                        existeProducto = true;
                        fillSeleccionarProducto((DataTable)objProducto, 0);
                        return false;
                    }
                    else if (objProducto is Productos)
                    {
                        existeProducto = true;
                        producto = (Productos)objProducto;
                        if (producto.Detalle.Existencia <= 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("El Producto se encuentra agotado en el Sistema.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtControl.Text = "";
                            txtControl.Focus();
                            if (producto.Detalle.Bascula)
                            {
                                asignoPeso = true;
                                bascula = false;
                                asignoPeso = true;
                                _timer = false;
                            }
                        }
                        else
                        {
                            _strProducto = producto.Descripcion;
                            incrementar = false;
                            bool bSobreVenta = false;
                            //BindingList<VentaDetalle> lista = (BindingList<VentaDetalle>)gridDetalleVenta.DataSource;
                            bool agregarVenta = true;
                            double cantidad = 0;
                            bool incrementarArticulo = true;
                            if (_hayBascula)
                            {
                                if (producto.Detalle.Bascula)
                                {
                                    bascula = true;
                                    asignoPeso = false;
                                    //pesoEtiqueta = 0;
                                }
                                else
                                {
                                    asignoPeso = true;
                                    bascula = false;
                                    asignoPeso = true;
                                }
                            }
                            else
                            {
                                asignoPeso = true;
                                bascula = false;
                                asignoPeso = true;
                            }
                            int length = gridViewVentas.RowCount;
                            //gridViewVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                            //gridViewVentas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                            if (length > 0)
                            {
                                VentaDetalle p = null;
                                foreach (DataGridViewRow Row in gridViewVentas.Rows)
                                {
                                    p = (VentaDetalle)Row.Tag;
                                    if (p.Id_Producto == producto.Producto_Id)
                                    {
                                        _lastProductoBasculaId = producto.Producto_Id;
                                        if (producto.Detalle.Bascula)
                                        {
                                            producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                                            cargarVentaGrid(producto, ref agregarVenta, totalCantidad);
                                            if (agregarVenta)
                                            {
                                                incrementarArticulo = false;
                                                if (_hayBascula)
                                                {
                                                    p.AsignoPeso = false;
                                                    _timer = true;
                                                    if (!_permiso)
                                                    {
                                                        tBascula.Start();
                                                    }
                                                }
                                                else
                                                {
                                                    _timer = false;
                                                }
                                            }
                                            else
                                            {
                                                _timer = false;
                                                txtControl.Text = "";
                                                txtControl.Focus();
                                                //gridViewVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                                //gridViewVentas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                                                return agregarVenta;
                                            }
                                        }
                                        else
                                        {
                                            _timer = false;
                                            cantidad = p.Cantidad;
                                            if (totalCantidad > 0)
                                            {
                                                cantidad += totalCantidad;
                                            }
                                            else
                                            {
                                                cantidad++;
                                            }
                                            if (producto.Detalle.Existencia < cantidad)
                                            {
                                                DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                                bSobreVenta = true;
                                                break;
                                            }
                                            else
                                            {
                                                p.Cantidad = cantidad;
                                                p.UltimaCantidad = p.Cantidad;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _lastProductoBasculaId = producto.Producto_Id;
                                if (producto.Detalle.Bascula)
                                {
                                    producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                                    agregarVenta = false;
                                    if (_hayBascula)
                                    {
                                        if (!_permiso)
                                        {
                                            tBascula.Start();
                                        }
                                        _timer = true;
                                    }
                                    else
                                    {
                                        _timer = false;
                                    }
                                }
                                else
                                {
                                    _timer = false;
                                    //producto.Detalle.Cantidad++;
                                }
                            }
                            if (!bSobreVenta)
                            {
                                bool mostrarMensaje = !agregarVenta;
                                AgregarVentaGrid(producto, mostrarMensaje, incrementarArticulo, totalCantidad);
                                RefrescarMontos();
                                txtControl.Text = "";
                                txtControl.Focus();
                                gridViewVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                gridViewVentas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                                return true;
                            }
                            else
                            {
                                gridViewVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                                gridViewVentas.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                                return false;
                            }

                        }
                    }
                    else
                    {
                        existeProducto = false;
                        DevExpress.XtraEditors.XtraMessageBox.Show("Artículo no encontrado.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        existeProducto = true;
                        return false;
                    }
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("El Producto no está definido en el catálogo.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtControl.Text = "";
                    txtControl.Focus();
                    return false;
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void fillSeleccionarProducto(DataTable table, double totalCantidad)
        {
            CFDS_Producto producto = new CFDS_Producto();
            Productos productos = new Productos();
            using (FrmBusqueda busqueda = new FrmBusqueda(table)
            {
                Width = 1300,
                Text = "Productos",
                AjustarColumnas = true,
                ColumnasOcultar = new List<string> { "IdProducto", "CampoId", "CampoBusqueda", "IdLinea1", "IdLinea2", "IdLinea3", "Status", "Serie", "IdDatosFiscales", "Ultimo_Costo", "CodigoProducto", "Cantidad_Empaque", "UnidadMedida" }
            })
            {
                if (busqueda.ShowDialog() == DialogResult.OK)
                {
                    if (busqueda.FilaDatos != null && productos.Cargar((DataRowView)busqueda.FilaDatos))
                    {
                        producto.ClearProducto();
                        producto.Producto_Id = productos.Producto_Id;
                        MuestraProductos(productos, totalCantidad);
                        //gridDetalleVenta.RefreshDataSource();
                        RefrescarMontos();
                    }
                }
            }
        }

        public void MuestraProductos(Productos producto, double totalCantidad)
        {
            try
            {
                //if (producto.Cargar().Result)
                //{    //Ojo revisar esta parte                    
                if (producto.Detalle.Existencia <= 0)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("El Producto se encuentra agotado en el Sistema.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtControl.Text = "";
                    txtControl.Focus();
                }
                else
                {
                    _strProducto = producto.Descripcion;
                    _lastProductoBasculaId = producto.Producto_Id;
                    bool agregarVenta = true;
                    if (producto.Detalle.Bascula)
                    {
                        //producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                    }
                    cargarVentaGrid(producto, ref agregarVenta, totalCantidad);
                    if (_hayBascula)
                    {
                        if (producto.Detalle.Bascula)
                        {
                            _timer = true;
                            if (!_permiso)
                            {
                                tBascula.Start();
                            }
                        }
                        else
                        {
                            _timer = false;
                        }
                    }
                    else
                    {
                        _timer = false;
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cargarVentaGrid(Productos producto, ref bool agregarVenta, double totalCantidad)
        {
            bool bSobreVenta = false;
            int length = gridViewVentas.RowCount;
            //BindingList<VentaDetalle> lista = (BindingList<VentaDetalle>)gridDetalleVenta.DataSource;
            bool incrementarArticulo = true;
            int index = -1;
            if (length > 0)
            {
                //int length = lista.Count;
                double nuevaCantidad = 0;
                int i = 0;
                foreach (DataGridViewRow row in gridViewVentas.Rows)
                {
                    VentaDetalle p = (VentaDetalle)row.Tag;
                    if (p.Id_Producto == producto.Producto_Id)
                    {
                        if (!p.Bascula)
                        {
                            if (totalCantidad > 0)
                            {
                                nuevaCantidad = p.Cantidad + totalCantidad;
                            }
                            else
                            {
                                nuevaCantidad = p.Cantidad + 1;
                            }
                        }
                        else
                        {
                            nuevaCantidad = p.Cantidad + producto.Detalle.Cantidad;
                        }
                        if (producto.Detalle.Existencia < nuevaCantidad)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            bSobreVenta = true;
                            agregarVenta = false;
                            break;
                        }
                        else
                        {
                            incrementarArticulo = false;
                            p.TotalArticulos++;
                            incrementar = true;
                            if (!p.Bascula)
                            {
                                p.Cantidad++;
                                p.AsignoPeso = true;
                            }
                            else
                            {
                                p.Cantidad += producto.Detalle.Cantidad;
                                p.AsignoPeso = false;
                            }
                            p.UltimaCantidad = p.Cantidad;
                            actualizarPrecios(ref p);
                            index = i;
                            row.Tag = p;
                        }
                    }
                    i++;
                }
            }
            if (!bSobreVenta)
            {
                AgregarVentaGrid(producto, true, incrementarArticulo, totalCantidad);
                //Herramientas.GridViewEditarColumnas(
                //    gridViewDetalleVenta,
                //    true,
                //    true,
                //    false,
                //    new List<string>() { "Id_Venta_Detalle", "Id_Venta", "Id_Producto", "CampoId", "Quien_Surte", "Id_Sucursal", "Surtido", "IEPS", "IVA", "CampoBusqueda", "TipoClase", "Precio_Promocion", "Precio_Mayoreo", "Precio_Original", "IEPSimporte", "IVAimporte", "IdDatosFiscales", "IdVentas", "UltimaCantidad", "Precio2", "Precio3", "Precio4", "Precio5", "Precio", "CantidadPrecio", "CantidadPrecio2", "CantidadPrecio3", "CantidadPrecio4", "CantidadPrecio5", "Bascula", "Descuento_PorCiento", "Descuento_Precio", "Existencia", "AsignoPeso", "TotalArticulos", "Activo", "Su_Ahorro" },
                //    new List<string> { "Cantidad"},
                //    new List<string> { "Cantidad", "Descuento_PorCiento" }
                //    );
                //Herramientas.GridViewSoloLecturaColumnas(gridViewDetalleVenta);
                RefrescarMontos();
            }
            if (index >= 0)
            {
                EnsureVisibleRow(gridViewVentas, index);
                //GridViewInfo viewInfo = gridViewDetalleVenta.GetViewInfo() as GridViewInfo;
                //if (viewInfo.VScrollBarPresence == ScrollBarPresence.Visible)
                //{
                //    gridViewDetalleVenta.MakeRowVisible(index);
                //}
                //gridViewDetalleVenta.SelectRow(index);
                //gridViewDetalleVenta.FocusedRowHandle = index;
            }
            txtControl.Text = "";
            txtControl.Focus();
        }

        private void actualizarPrecios(ref VentaDetalle p)
        {
            double iva = 0;
            double ieps = 0;
            if (p.IEPS > 0)
            {
                ieps = Convert.ToDouble(p.IEPS) / 100;
                ieps = 1 + ieps;
            }
            if (p.IVA > 0)
            {
                iva = Convert.ToDouble(p.IVA) / 100;
                iva = 1 + iva;
            }
            int porcentajeiva = -1;
            if (p.IVA > -1)
            {
                porcentajeiva = Convert.ToInt32(p.IVA);
            }
            int porcentajeieps = Convert.ToInt32(p.IEPS);
            double importe = 0;
            double importeiva = 0;
            double importeieps = 0;
            double cantidad = p.Cantidad;
            double precioseleccionado = 0;
            double normal = 0;
            if (_clienteId > 0)
            {
                precioseleccionado = p.Precio;
            }
            else
            {
                precioseleccionado = p.Precio;
            }
            normal = cantidad * p.Precio;
            importe = cantidad * precioseleccionado;
            p.Precio_Venta = precioseleccionado;
            if (iva > 0)
            {
                importeiva = (importe - (importe / iva));
                if (ieps != 0)
                {
                    double valorunitario = importe - importeiva;
                    importeieps = (valorunitario - (valorunitario / ieps));
                }
            }
            else if (ieps != 0)
            {
                importeieps = (importe - (importe / ieps));
            }
            p.IVA = porcentajeiva;
            p.IVAimporte = Global.StringToDouble(Global.DoubleToString(importeiva));
            p.IEPS = porcentajeieps;
            p.IEPSimporte = Global.StringToDouble(Global.DoubleToString(importeieps));
            p.Importe = Convert.ToDecimal(importe);
        }

        private static void EnsureVisibleRow(DataGridView view, int rowToShow)
        {
            if (rowToShow >= 0 && rowToShow < view.RowCount)
            {
                var countVisible = view.DisplayedRowCount(false);
                var firstVisible = view.FirstDisplayedScrollingRowIndex;
                if (rowToShow < firstVisible)
                {
                    view.FirstDisplayedScrollingRowIndex = rowToShow;
                }
                else if (rowToShow >= firstVisible + countVisible)
                {
                    view.FirstDisplayedScrollingRowIndex = rowToShow - countVisible + 1;
                }
            }
        }

        private void RefrescarMontos()
        {
            double subtotal = 0;
            double total = 0;
            double importeiva = 0;
            double importeieps = 0;
            int nArticulos = 0;
            double suahorro = 0;
            int length = gridViewVentas.RowCount;
            if (length > 0)
            {
                //int length = gridViewDetalleVenta.RowCount;
                //for (int i = 0; i < length; i++)
                foreach (DataGridViewRow row in gridViewVentas.Rows)
                {
                    //VentaDetalle detalle = (VentaDetalle)gridViewDetalleVenta.GetRow(i);
                    VentaDetalle detalle = (VentaDetalle)row.Tag;
                    subtotal += Convert.ToDouble(detalle.Importe);
                    importeiva += Convert.ToDouble(detalle.IVAimporte);
                    importeieps += Convert.ToDouble(detalle.IEPSimporte);
                    nArticulos += detalle.TotalArticulos;
                }
            }
            lblArticulos.Text = nArticulos.ToString();
            double importe = subtotal;
            subTotal = subtotal;
            total = subTotal;
            txtTotal.Text = Global.DoubleToString(total = subtotal);
        }

        private bool AgregarVentaGrid(Productos producto, bool mostrarMensaje, bool incrementarArticulo, double totalCantidad)
        {
            //BindingList<VentaDetalle> lista = (BindingList<VentaDetalle>)gridDetalleVenta.DataSource;
            VentaDetalle p = null;
            bool found = false;
            int index = -1;
            bool agrego = true;
            int length = gridViewVentas.RowCount;
            if (length > 0)
            {
                //int length = lista.Count;
                VentaDetalle v = null;
                int i = 0;
                foreach (DataGridViewRow Row in gridViewVentas.Rows)
                {
                    //v = lista.ElementAt(i);
                    v = (VentaDetalle)Row.Tag;
                    if (v.Id_Producto == producto.Producto_Id)
                    {
                        index = i;
                        if (producto.Detalle.Existencia < v.Cantidad)
                        {
                            if (mostrarMensaje)
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        else
                        {
                            if (v.Bascula)
                            {
                                v.AsignoPeso = false;
                            }
                            else
                            {
                                v.AsignoPeso = true;
                            }
                            if (incrementarArticulo)
                            {
                                v.TotalArticulos++;
                            }
                            actualizarPrecios(ref v);
                            incrementar = true;
                        }
                        agrego = false;
                        found = true;
                        break;
                    }
                    i++;
                }
                if (!found)
                {
                    incrementar = false;
                    p = new VentaDetalle();
                    p.Bascula = producto.Detalle.Bascula;
                    p.Precio_Original = producto.Detalle.Precio;
                    p.Id_Producto = producto.Producto_Id;
                    p.IdDatosFiscales = producto.IdDatosFiscales;
                    p.Descripcion = producto.Descripcion;
                    _lastProductoBasculaId = producto.Producto_Id;
                    if (producto.Detalle.Bascula)
                    {
                        producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                        p.Cantidad = producto.Detalle.Cantidad;
                        p.UltimaCantidad = p.Cantidad;
                        if (_hayBascula)
                        {
                            _timer = true;
                            if (!_permiso)
                            {
                                tBascula.Start();
                            }
                        }
                        else
                        {
                            _timer = false;
                        }
                    }
                    else
                    {
                        p.AsignoPeso = true;
                        _timer = false;
                        if (totalCantidad > 0)
                        {
                            if (producto.Detalle.Existencia < totalCantidad)
                            {
                                if (mostrarMensaje)
                                {
                                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                                return false;
                            }
                            else
                            {
                                p.Cantidad += totalCantidad;
                            }
                        }
                        else
                        {
                            p.Cantidad = 1;
                        }
                    }
                    p.TotalArticulos++;
                    p.Precio = producto.Detalle.Precio;
                    p.Existencia = Convert.ToInt32(producto.Detalle.Existencia);
                    p.IVA = producto.Detalle.IVA;
                    p.IEPS = producto.Detalle.IEPS;
                    p.UltimaCantidad = p.Cantidad;
                    actualizarPrecios(ref p);
                    int nRow = gridViewVentas.Rows.Add();
                    gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = p.Cantidad;
                    gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.DESCRIPCION].Value = p.Descripcion;
                    gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.PRECIO].Value = Global.DoubleToString(p.Precio_Venta);
                    gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.IMPORTE].Value = Global.DoubleToString(Convert.ToDouble(p.Importe));
                    gridViewVentas.Rows[nRow].Tag = p;
                    gridViewVentas.Rows[nRow].Selected = true;
                    index = nRow;
                    //addNewRowInGroupMode(gridViewDetalleVenta, p);
                    //lista.Add(p);
                    //index = lista.Count; 
                    //gridDetalleVenta.DataSource = lista;
                }
                else
                {
                    p = (VentaDetalle)gridViewVentas.Rows[i].Tag;
                    gridViewVentas.Rows[i].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = p.Cantidad;
                    gridViewVentas.Rows[i].Cells[(int)DataGridViewColumnas.DESCRIPCION].Value = p.Descripcion;
                    gridViewVentas.Rows[i].Cells[(int)DataGridViewColumnas.PRECIO].Value = Global.DoubleToString(p.Precio_Venta);
                    gridViewVentas.Rows[i].Cells[(int)DataGridViewColumnas.IMPORTE].Value = Global.DoubleToString(Convert.ToDouble(p.Importe));
                    gridViewVentas.Rows[i].Tag = p;
                    gridViewVentas.Rows[i].Selected = true;
                }
                //gridDetalleVenta.RefreshDataSource();
            }
            else
            {
                incrementar = false;
                //lista = new BindingList<VentaDetalle>();
                p = new VentaDetalle();
                p.Bascula = producto.Detalle.Bascula;
                p.Precio_Original = producto.Detalle.Precio;
                p.Descripcion = producto.Descripcion;
                /*if (producto.Detalle.Precio_Promocion > 0)
                {
                    p.Precio_Venta = producto.Detalle.Precio_Promocion;
                    p.Precio_Promocion = Convert.ToDecimal(producto.Detalle.Precio_Promocion);
                }
                else
                {
                    p.Precio_Venta = producto.Detalle.Precio;
                }*/
                //p.Precio = producto.Detalle.Precio;
                //p.CantidadPrecio = producto.Detalle.CantidadPrecio;
                p.Existencia = Convert.ToInt32(producto.Detalle.Existencia);
                p.Id_Producto = producto.Producto_Id;
                p.IdDatosFiscales = producto.IdDatosFiscales;
                p.IVA = producto.Detalle.IVA;
                p.IEPS = producto.Detalle.IEPS;
                _lastProductoBasculaId = producto.Producto_Id;
                p.TotalArticulos++;
                if (producto.Detalle.Bascula)
                {
                    producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                    p.Cantidad = producto.Detalle.Cantidad;
                    p.UltimaCantidad = p.Cantidad;
                    if (_hayBascula)
                    {
                        _timer = true;
                        if (!_permiso)
                        {
                            tBascula.Start();
                        }
                    }
                    else
                    {
                        _timer = false;
                    }
                }
                else
                {
                    p.AsignoPeso = true;
                    if (totalCantidad > 0)
                    {
                        if (producto.Detalle.Existencia < totalCantidad)
                        {
                            if (mostrarMensaje)
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                            return false;
                        }
                        else
                        {
                            p.Cantidad += totalCantidad;
                        }
                    }
                    else
                    {
                        p.Cantidad = 1;
                    }
                    _timer = false;
                }
                p.UltimaCantidad = p.Cantidad;
                actualizarPrecios(ref p);
                int nRow = gridViewVentas.Rows.Add();
                gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = p.Cantidad;
                gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.DESCRIPCION].Value = p.Descripcion;
                gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.PRECIO].Value = Global.DoubleToString(p.Precio_Venta);
                gridViewVentas.Rows[nRow].Cells[(int)DataGridViewColumnas.IMPORTE].Value = Global.DoubleToString(Convert.ToDouble(p.Importe));
                gridViewVentas.Rows[nRow].Tag = p;
                gridViewVentas.Rows[nRow].Selected = true;
                index = nRow;
            }
            List<VentaDetalle> lstVentas = null;
            if (!found)
            {
                if (!dicVentas.ContainsKey(producto.IdDatosFiscales))
                {
                    if (lstVentas == null)
                    {
                        lstVentas = new List<VentaDetalle>();
                    }
                    lstVentas.Add(p);
                    dicVentas.Add(producto.IdDatosFiscales, lstVentas);
                }
                else
                {
                    lstVentas = dicVentas[producto.IdDatosFiscales];
                    if (lstVentas != null)
                    {
                        lstVentas.Add(p);
                        dicVentas[producto.IdDatosFiscales] = lstVentas;
                    }
                }
            }
            if (index >= 0)
            {
                EnsureVisibleRow(gridViewVentas, index);
            }
            return agrego;
        }

        private void FrmBascula_Load(object sender, EventArgs e)
        {
            lblNombreCajera.Text = Properties.Settings.Default.NombreUsuario;
            lblNombreTienda.Text = Properties.Settings.Default.NombreTienda;
            lblNumeroCaja.Text = Properties.Settings.Default.NumeroCaja.ToString();
            string strIpLocal = Properties.Settings.Default.IpLocal;
            string[] Ip = strIpLocal.Split('.');
            int nBandera = 0;
            string strIpCorta = "";
            foreach (string word in Ip)
            {
                if (nBandera >= 2)
                {
                    if (nBandera == 3)
                        strIpCorta += word;
                    else
                        strIpCorta += word + ".";
                }
                nBandera++;
            }
            lblInfo.Text = Properties.Settings.Default.Version + " - Online: " + strIpCorta;
            try
            {
                explorer = new PosExplorer();
            }
            catch (Exception) { }
            if (explorer != null)
            {
                DeviceCollection scannerList = explorer.GetDevices(DeviceType.Scanner);
                DeviceCollection scaleList = explorer.GetDevices(DeviceType.Scale);
                foreach (DeviceInfo device in scannerList)
                {
                    if (string.Compare(device.ServiceObjectName, "qs6000", true) == 0)
                    {
                        selectedScanner = device;
                        break;
                    }
                }
                if (selectedScanner != null)
                {
                    try
                    {
                        activeScanner = (Scanner)explorer.CreateInstance(selectedScanner);
                        activeScanner.Open();
                        activeScanner.Claim(1000);
                        if (activeScanner.Claimed)
                        {
                            activeScanner.DeviceEnabled = true;
                            activeScanner.DataEvent += new DataEventHandler(activeScanner_DataEvent);
                            activeScanner.ErrorEvent += new DeviceErrorEventHandler(activeScanner_ErrorEvent);
                            activeScanner.DecodeData = true;
                            activeScanner.DataEventEnabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        RecyclameV2.Utils.Logger.addLogEntry("Hablitar scanner LOad, excepcion: " + ex.ToString());
                    }
                }
                foreach (DeviceInfo device in scaleList)
                {
                    if (string.Compare(device.ServiceObjectName, "rs232scale", true) == 0)
                    {
                        selectedScale = device;
                        break;
                    }
                }
            }
            if (selectedScale != null)
            {
                try
                {
                    activeScale = (Scale)explorer.CreateInstance(selectedScale);
                    activeScale.Open();
                    activeScale.Claim(1000);
                    _hayBascula = true;
                    if (activeScale.Claimed)
                    {
                        activeScale.DeviceEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("FAVOR DE VERIFICAR QUE NO QUE LA BASCULA NO ESTE OCUPADA");
                    RecyclameV2.Utils.Logger.addLogEntry("Hablitar bascula, Load excepcion: " + ex.ToString());
                }
            }
            else
            {
                string[] puertos = SerialPort.GetPortNames();
                if (puertos != null)
                {
                    foreach (string ports in puertos)
                    {
                        try
                        {
                            port = new SerialPort(ports);
                            port.BaudRate = 9600;
                            port.Parity = Parity.None;
                            port.StopBits = StopBits.One;
                            port.DataBits = 8;
                            port.WriteTimeout = 500;
                            port.Handshake = Handshake.None;
                            port.ReadTimeout = 4800;
                            port.Open();
                            port.Write(inBuffer, 0, inBuffer.Length);
                            System.Threading.Thread.Sleep(300);
                            string informacionBascula = port.ReadExisting().Replace("\r", "").Trim();
                            if (informacionBascula.Length > 0)
                            {
                                _hayBascula = true;
                                port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.LeerBascula);
                                port.ErrorReceived += new SerialErrorReceivedEventHandler(PuertoSerieBascula_ErrorReceived);
                                break;
                            }
                            else
                            {
                                port.Close();
                                port.Dispose();
                                port = null;
                            }
                        }
                        catch (Exception)
                        {
                            port = null;
                        }
                    }
                }
                else
                {
                    _hayBascula = false;
                }
            }
            t = new System.Windows.Forms.Timer();
            t.Enabled = true;
            t.Interval = 50;
            t.Tick += t_Tick;
            tBascula = new System.Windows.Forms.Timer();
            //tBascula.Enabled = true;
            tBascula.Interval = 250;
            tBascula.Tick += tBascula_Tick;
            tEliminar.Interval = 50;
            tEliminar.Tick += tEliminar_Tick;
            tLabel.Interval = 750;
            tLabel.Tick += tLabel_Tick;
            txtControl.Focus();
        }
        void PuertoSerieBascula_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

            switch (e.EventType)
            {
                case SerialError.Frame:
                    MessageBox.Show("Error de trama...");
                    break;
                case SerialError.Overrun:
                    MessageBox.Show("Saturación de buffer...");
                    break;
                case SerialError.RXOver:
                    MessageBox.Show("Desbordamiento de buffer de entrada");
                    break;
                case SerialError.RXParity:
                    MessageBox.Show("Error de paridad...");
                    break;
                case SerialError.TXFull:
                    MessageBox.Show("Buffer lleno...");
                    break;

            }
            throw new NotImplementedException();
        }

        private void LeerBascula(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string informacionBascula = port.ReadExisting().Replace("\r", "").Replace("kg", "").Trim();
            double peso = 0;
            if (informacionBascula.Length > 0)
            {
                double.TryParse(informacionBascula, out peso);
                if (_lastProductoBasculaId > 0 && !asignoPeso)
                {
                    if (peso > 0)
                    {
                        this.BeginInvoke(new InvokeDelegate(asignarPesoLabel), peso);
                    }
                }
                else
                {
                    this.BeginInvoke(new InvokeDelegate(asignarPesoLabelDefault), peso);
                }
            }
        }
        private void FrmBascula_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dicVentas.Count > 0)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("Debe finalizar venta antes de cerrar el sistema.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {
                releaseScanner();
                releaseBascula();
            }
        }
        private void asignarPesoLabelDefault(double peso)
        {
            lblBascula.Text = peso.ToString("N3", NumberFormatInfo.InvariantInfo) + " kg";
        }
        private void asignarPesoLabel(double peso)
        {
            tBascula.Stop();
            lblBascula.Text = peso.ToString("N3", NumberFormatInfo.InvariantInfo) + " kg";
            bascula = false;
            asignoPeso = true;
            if (asignarPesoProductoBascula(_lastProductoBasculaId, Convert.ToDecimal(peso)))
            {
                pesoEtiqueta = 0;
                _strProducto = string.Empty;
                _timer = false;
                _lastProductoBasculaId = 0;
                asignoPeso = true;
                tLabel.Start();
            }
        }
        void tLabel_Tick(object sender, EventArgs e)
        {
            tLabel.Stop();
            lblBascula.Text = "0.000 kg";
        }
        void tBascula_Tick(object sender, EventArgs e)
        {
            if (_lastProductoBasculaId > 0 && !asignoPeso)
            {
                if ((activeScale != null && activeScale.Claimed))
                {
                    decimal peso = obtenerPesoBascula();
                    lblBascula.Text = peso.ToString("N3", NumberFormatInfo.InvariantInfo) + " kg";
                    if (peso > 0)
                    {
                        tBascula.Stop();
                        bascula = false;
                        asignoPeso = true;
                        if (asignarPesoProductoBascula(_lastProductoBasculaId, peso))
                        {
                            pesoEtiqueta = 0;
                            _strProducto = string.Empty;
                            _timer = false;
                            _lastProductoBasculaId = 0;
                            asignoPeso = true;
                            tLabel.Start();
                        }
                    }
                }
                else if (port != null && port.IsOpen)
                {
                    leerPeso();
                }
                else
                {
                    tBascula.Stop();
                    bascula = false;
                    asignarPeso = true;
                    _timer = false;
                    _lastProductoBasculaId = 0;
                    _strProducto = string.Empty;
                }
            }
        }
        public void leerPeso()
        {
            try
            {
                if (port != null && port.IsOpen)
                {
                    port.Write(inBuffer, 0, inBuffer.Length);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                tBascula.Stop();
                if (_lastProductoBasculaId > 0 && !asignoPeso)
                {
                    lblBascula.Text = "0.000 kg";
                    bascula = false;
                    asignarPeso = true;
                    _timer = false;
                    _lastProductoBasculaId = 0;
                    _strProducto = string.Empty;
                }
            }
        }
        int indexDelete = -1;
        async void tEliminar_Tick(object sender, EventArgs e)
        {
            tEliminar.Stop();
            if (indexDelete > -1)
            {
                eliminarProducto(indexDelete);
            }
        }
        private void eliminarProducto(int index)
        {
            try
            {
                if (gridViewVentas.SelectedRows != null && gridViewVentas.SelectedRows.Count > 0)
                {
                    DataGridViewRow row = gridViewVentas.SelectedRows[0];
                    var rowview = (VentaDetalle)row.Tag;//(VentaDetalle)gridViewDetalleVenta.GetRow(index);
                    gridViewVentas.Rows.Remove(row);
                    //gridViewDetalleVenta.DeleteRow(index);
                    RefrescarMontos();
                    if (dicVentas.ContainsKey(rowview.IdDatosFiscales))
                    {
                        List<VentaDetalle> lst = dicVentas[rowview.IdDatosFiscales];
                        int length = lst.Count;
                        for (int i = 0; i < length; i++)
                        {
                            if (lst[i].Id_Producto == rowview.Id_Producto)
                            {
                                lst.RemoveAt(i);
                                break;
                            }
                        }
                        if (lst.Count > 0)
                        {
                            dicVentas[rowview.IdDatosFiscales] = lst;
                            if (!rowview.AsignoPeso)
                            {
                                _lastProductoBasculaId = 0;
                                _timer = false;
                                bascula = false;
                                asignarPeso = true;
                                tBascula.Stop();
                                _permiso = false;
                            }
                        }
                        else
                        {
                            _lastProductoBasculaId = 0;
                            _timer = false;
                            bascula = false;
                            asignarPeso = true;
                            tBascula.Stop();
                            _permiso = false;
                            _IdAutorizacion = -1;
                            dicVentas.Remove(rowview.IdDatosFiscales);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            txtControl.Focus();
        }
        private void leerCodigo(object obj)
        {
            string code = obj.ToString();
            if (!_formPago)
            {
                if (!_timer)
                {
                    pesoEtiqueta = 0;
                    if (existeProducto)
                    {
                        try
                        {
                            _lastProductoBasculaId = 0;
                            incrementar = false;
                            // Display the ASCII encoded label text
                            //string code = encoder.GetString(activeScanner.ScanDataLabel);
                            if (code.StartsWith("20"))
                            {
                                if (_hayBascula)
                                {
                                    asignoPeso = false;
                                    bascula = true;
                                    _timer = true;
                                }
                                else
                                {
                                    asignoPeso = true;
                                    bascula = false;
                                    _permiso = false;
                                    _timer = false;
                                }
                                code = code.Remove(0, 2);
                                string peso = code.Remove(0, 5);
                                code = code.Substring(0, 5);
                                pesoEtiqueta = decimal.Round(Convert.ToDecimal(peso) / 10000, 3);
                                if (pesoEtiqueta > 15)
                                {
                                    _permiso = true;
                                }
                                else
                                {
                                    _permiso = false;
                                }
                            }
                            else
                            {
                                asignoPeso = true;
                                bascula = false;
                                _permiso = false;
                                _timer = false;
                            }
                            if (txtControl.Text.IndexOf("*") != -1)
                            {
                                code = txtControl.Text + code;
                            }
                            double cangtidad = 0;
                            if (code.IndexOf("*") != -1)
                            {
                                cangtidad = obtenerCantidad(ref code);
                            }
                            txtControl.Focus();
                            //txtControl.Text = code;
                            //Global.SendEnter();
                            MuestraProductos(cangtidad, code);
                            //if (MuestraProductos(cangtidad, code))
                            //{
                            //    this.BeginInvoke(new InvokeDelegateActualizarMontos(RefrescarMontosThread));
                            //}
                            //else
                            //{
                            //    asignoPeso = true;
                            //    bascula = false;
                            //    _permiso = false;
                            //}
                            // Display the encoding type
                            // re-enable the data event for subsequent scans                    
                        }
                        catch (PosControlException ex)
                        {
                            RecyclameV2.Utils.Logger.addLogEntry(LOG_FILE_PATH, "DATA_EVENT :" + ex.ToString());
                        }
                    }
                }
                else
                {
                    if (!_permiso)
                    {
                        if (_strProducto.Length > 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (_strProducto.Length > 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }
        public bool MuestraProductos(double totalCantidad, string code)
        {
            try
            {
                Productos producto = new Productos();
                object objProducto = producto.buscarProductoVenta(code);
                if (objProducto != null)
                {
                    if (!existeProducto)
                    {
                        return false;
                    }
                    if (objProducto is DataTable)
                    {
                        existeProducto = false;
                        //fillSeleccionarProducto((DataTable)objProducto, 0);
                        return false;
                    }
                    else if (objProducto is Productos)
                    {
                        existeProducto = true;
                        this.BeginInvoke(new InvokeDelegateActualizar(muestraProductosThread), objProducto, producto, totalCantidad, true);
                    }
                    else
                    {
                        asignoPeso = true;
                        bascula = false;
                        _permiso = false;
                        _timer = false;
                        existeProducto = false;
                        DevExpress.XtraEditors.XtraMessageBox.Show("Artículo no encontrado.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        existeProducto = true;
                        return false;
                    }
                }
                else
                {
                    asignoPeso = true;
                    bascula = false;
                    _permiso = false;
                    _timer = false;
                    DevExpress.XtraEditors.XtraMessageBox.Show("El Producto no está definido en el catálogo.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtControl.Text = "";
                    txtControl.Focus();
                    return false;
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            asignoPeso = true;
            bascula = false;
            _permiso = false;
            _timer = false;
            return false;
        }
        private void muestraProductosThread(object objProducto, Productos producto, double totalCantidad, bool existeProducto)
        {
            try
            {
                if (objProducto != null)
                {
                    if (!existeProducto)
                    {
                        return;
                    }
                    if (objProducto is Productos)
                    {
                        producto = (Productos)objProducto;
                        if (producto.Detalle.Existencia <= 0)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("El Producto se encuentra agotado en el Sistema.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            txtControl.Text = "";
                            txtControl.Focus();
                            if (producto.Detalle.Bascula)
                            {
                                asignoPeso = true;
                                bascula = false;
                                asignoPeso = true;
                                _timer = false;
                            }
                        }
                        else
                        {
                            _strProducto = producto.Descripcion;
                            incrementar = false;
                            bool bSobreVenta = false;
                            //BindingList<VentaDetalle> lista = (BindingList<VentaDetalle>)gridDetalleVenta.DataSource;
                            bool agregarVenta = true;
                            double cantidad = 0;
                            bool incrementarArticulo = true;
                            if (_hayBascula)
                            {
                                if (producto.Detalle.Bascula)
                                {
                                    bascula = true;
                                    asignoPeso = false;
                                    //pesoEtiqueta = 0;
                                }
                                //else
                                //{
                                //    asignoPeso = true;
                                //    bascula = false;
                                //    asignoPeso = true;
                                //}
                            }
                            else
                            {
                                asignoPeso = true;
                                bascula = false;
                                asignoPeso = true;
                            }
                            int length = gridViewVentas.RowCount;
                            if (length > 0)
                            {
                                VentaDetalle p = null;
                                foreach (DataGridViewRow row in gridViewVentas.Rows)
                                {
                                    p = (VentaDetalle)row.Tag;
                                    if (p.Id_Producto == producto.Producto_Id)
                                    {
                                        _lastProductoBasculaId = producto.Producto_Id;
                                        if (producto.Detalle.Bascula)
                                        {
                                            producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                                            cargarVentaGrid(producto, ref agregarVenta, totalCantidad);
                                            if (agregarVenta)
                                            {
                                                incrementarArticulo = false;
                                                if (_hayBascula)
                                                {
                                                    p.AsignoPeso = false;
                                                    _timer = true;
                                                    if (!_permiso)
                                                    {
                                                        tBascula.Start();
                                                    }
                                                }
                                                else
                                                {
                                                    _timer = false;
                                                }
                                            }
                                            else
                                            {
                                                _timer = false;
                                                txtControl.Text = "";
                                                txtControl.Focus();
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            _timer = false;
                                            cantidad = p.Cantidad;
                                            if (totalCantidad > 0)
                                            {
                                                cantidad += totalCantidad;
                                            }
                                            else
                                            {
                                                cantidad++;
                                            }
                                            if (producto.Detalle.Existencia < cantidad)
                                            {
                                                DevExpress.XtraEditors.XtraMessageBox.Show(this, "No puedes vender mas articulos de los que tienes dado de alta en el Sistema", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                                bSobreVenta = true;
                                                break;
                                            }
                                            else
                                            {
                                                p.Cantidad = cantidad;
                                                p.UltimaCantidad = p.Cantidad;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _lastProductoBasculaId = producto.Producto_Id;
                                if (producto.Detalle.Bascula)
                                {
                                    producto.Detalle.Cantidad = Convert.ToDouble(pesoEtiqueta);
                                    agregarVenta = false;
                                    if (_hayBascula)
                                    {
                                        if (!_permiso)
                                        {
                                            tBascula.Start();
                                        }
                                        _timer = true;
                                    }
                                    else
                                    {
                                        _timer = false;
                                    }
                                }
                                else
                                {
                                    _timer = false;
                                    //producto.Detalle.Cantidad++;
                                }
                            }
                            if (!bSobreVenta)
                            {
                                bool mostrarMensaje = !agregarVenta;
                                AgregarVentaGrid(producto, mostrarMensaje, incrementarArticulo, totalCantidad);
                                //Herramientas.GridViewEditarColumnas(
                                //    gridViewDetalleVenta,
                                //    true,
                                //    true,
                                //    false,
                                //    new List<string>() { "Id_Venta_Detalle", "Id_Venta", "Id_Producto", "CampoId", "Quien_Surte", "Id_Sucursal", "Surtido", "IEPS", "IVA", "CampoBusqueda", "TipoClase", "Precio_Promocion", "Precio_Mayoreo", "Precio_Original", "IEPSimporte", "IVAimporte", "IdDatosFiscales", "IdVentas", "UltimaCantidad", "Precio2", "Precio3", "Precio4", "Precio5", "Precio", "CantidadPrecio", "CantidadPrecio2", "CantidadPrecio3", "CantidadPrecio4", "CantidadPrecio5", "Bascula", "Descuento_PorCiento", "Descuento_Precio", "Existencia", "AsignoPeso", "TotalArticulos", "Activo", "Su_Ahorro" },
                                //    new List<string> { "Cantidad" },
                                //    new List<string> { "Cantidad", "Descuento_PorCiento" }
                                //    );
                                //Herramientas.GridViewSoloLecturaColumnas(gridViewDetalleVenta);
                                RefrescarMontos();
                                txtControl.Text = "";
                                txtControl.Focus();
                                return;
                            }
                            else
                            {
                                RefrescarMontosThread();
                                //this.BeginInvoke(new InvokeDelegateActualizarMontos(RefrescarMontosThread));
                                return;
                            }

                        }
                    }
                    else
                    {
                        existeProducto = false;
                        DevExpress.XtraEditors.XtraMessageBox.Show("Artículo no encontrado.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        existeProducto = true;
                        return;
                    }
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("El Producto no está definido en el catálogo.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtControl.Text = "";
                    txtControl.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void RefrescarMontosThread()
        {
            double subtotal = 0;
            double total = 0;
            double importeiva = 0;
            double importeieps = 0;
            int nArticulos = 0;
            int length = gridViewVentas.RowCount;
            if (length > 0)
            {
                //int length = gridViewDetalleVenta.RowCount;
                //for (int i = 0; i < length; i++)
                foreach (DataGridViewRow row in gridViewVentas.Rows)
                {
                    //VentaDetalle detalle = (VentaDetalle)gridViewDetalleVenta.GetRow(i);
                    VentaDetalle detalle = (VentaDetalle)row.Tag;
                    subtotal += Convert.ToDouble(detalle.Importe);
                    importeiva += Convert.ToDouble(detalle.IVAimporte);
                    importeieps += Convert.ToDouble(detalle.IEPSimporte);
                    nArticulos += detalle.TotalArticulos;
                }
            }
            lblArticulos.Text = nArticulos.ToString();
            double importe = subtotal;
            subTotal = subtotal;
            total = subTotal;
            txtTotal.Text = Global.DoubleToString(total = subtotal);
        }
        public void Limpiar(bool needAutorizacion)
        {
            int length = gridViewVentas.RowCount;
            if (needAutorizacion && length > 0)
            {
                _lastProductoBasculaId = 0;
                _timer = false;
                bascula = false;
                asignarPeso = true;
                tBascula.Stop();
                _permiso = false;
                _IdAutorizacion = -1;
                dicVentas.Clear();
                _clienteId = 0;
                txtNombreCliente.Text = "PUBLICO GENERAL";
                txtControl.Text = "";
                subTotal = 0;
                txtTotal.Text = "0.00";
                lblBascula.Text = "0.000 kg";
                lblArticulos.Text = "0";
                if (length > 0)
                {
                    gridViewVentas.Rows.Clear();
                }
            }
            else
            {
                _lastProductoBasculaId = 0;
                _timer = false;
                bascula = false;
                asignarPeso = true;
                tBascula.Stop();
                _permiso = false;
                _IdAutorizacion = -1;
                dicVentas.Clear();
                _clienteId = 0;
                txtNombreCliente.Text = "PUBLICO GENERAL";
                txtControl.Text = "";
                subTotal = 0;
                txtTotal.Text = "0.00";
                lblBascula.Text = "0.000 kg";
                lblArticulos.Text = "0";
                if (length > 0)
                {
                    gridViewVentas.Rows.Clear();
                }
            }
            txtControl.Focus();
        }
        private void btnCalibraBascula_Click(object sender, EventArgs e)
        {
            Limpiar(true);
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (!_timer && !bascula)
            {
                List<SqlParameter> parametros = new List<SqlParameter>();
                parametros.Add(new SqlParameter() { ParameterName = "@P_Codigo_de_Barras", Value = string.Empty });
                DataSet dataset = BaseDatos.ejecutarProcedimientoConsulta("Producto_Busqueda_POS_sp", parametros);
                DataTable resultado = null;
                if (dataset != null && dataset.Tables.Count > 0)
                {
                    resultado = dataset.Tables["Producto_Busqueda_POS_sp"];
                }
                {
                    Productos productos = new Productos();
                    using (FrmBusqueda busqueda = new FrmBusqueda(resultado)//productos.Listado())
                    {
                        Width = 1300,
                        Text = "Productos",
                        AjustarColumnas = true,
                        ColumnasOcultar = new List<string> { "IdProducto", "CampoId", "UnidadMedida", "CantidadPrecio", "PrecioGeneral2", "CantidadPrecio2", "PrecioGeneral3", "CantidadPrecio3", "PrecioGeneral4", "CantidadPrecio4", "PrecioGeneral5", "CantidadPrecio5", "IVA", "IEPS", "PrecioMayoreo", "CantidadMayoreo", "Bascula", "Existencia" }
                    })
                    {
                        if (busqueda.ShowDialog() == DialogResult.OK)
                        {
                            double totalCantidad = 0;
                            string codigo = txtControl.Text.Trim();
                            productos.setFromVenta(true);
                            if (busqueda.FilaDatos != null && productos.Cargar((DataRowView)busqueda.FilaDatos))
                            {
                                if (_hayBascula)
                                {
                                    if (productos.Detalle.Bascula)
                                    {
                                        bascula = true;
                                        asignoPeso = false;
                                        pesoEtiqueta = 0;
                                    }
                                    else
                                    {
                                        asignoPeso = true;
                                        bascula = false;
                                        _permiso = false;
                                    }
                                }
                                else
                                {
                                    asignoPeso = true;
                                    bascula = false;
                                    _permiso = false;
                                }
                                if (codigo.Length > 0)
                                {
                                    totalCantidad = obtenerCantidad(ref codigo);
                                }
                                MuestraProductos(productos, totalCantidad);
                                RefrescarMontos();
                            }
                        }
                    }
                }
            }
            else
            {
                if (!_permiso)
                {
                    if (_strProducto.Length > 0)
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    if (_strProducto.Length > 0)
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            if (txtControl.Text.Length > 0)
            {
                txtControl.Text = string.Empty;
            }
            txtControl.Focus();
        }
        private void btnClientes_Click(object sender, EventArgs e)
        {
            seleccionarcliente();
        }
        private void btnCancelarVenta_Click(object sender, EventArgs e)
        {

        }
        private void btnEliminarProducto_Click(object sender, EventArgs e)
        {
            eliminarProducto();
        }
        private void btnCobrar_Click(object sender, EventArgs e)
        {
            if (dicVentas.Count == 0)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, "No se ha agregado ningún producto para vender.", this.ProductName, MessageBoxButtons.OK);
                return;
            }
            else
            {
                _formPago = true;
                /*try
                {
                    FrmFormaPago formaPago = new FrmFormaPago(this);
                    formaPago.ShowDialog();
                }
                catch (Exception) { }*/
                _formPago = false;
                return;
            }
        }
        private void txtControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
            }
            if (e.KeyCode == Keys.F2)
            {
            }
            else if (e.KeyCode == Keys.F3)
            {
                btnBuscar_Click(null, null);
            }
            else if (e.KeyCode == Keys.F5)
            {
                seleccionarcliente();
            }
            else if (e.KeyCode == Keys.F9)
            {
                Limpiar(true);
            }
            else if (e.KeyCode == Keys.F11)
            {
                eliminarProducto();
            }
            else if (e.KeyCode == Keys.F12)
            {
                btnCobrar_Click(null, null);
            }
            else if (e.KeyValue == 220)
            {
                if (_hayBascula)
                {
                    if (port != null && port.IsOpen)
                    {
                        leerPeso();
                    }
                    else
                    {
                        decimal peso = obtenerPesoBascula();
                        lblBascula.Text = peso.ToString("N3", NumberFormatInfo.InvariantInfo) + " kg";
                    }
                }
                else
                {
                    lblBascula.Text = "0.000 kg";
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (txtControl.Text.Trim().Length > 0)
                {
                    if (!_timer && !bascula)
                    {
                        if (!_hayBascula)
                        {
                            pesoEtiqueta = 0;
                        }
                        string codigo = txtControl.Text;
                        if (codigo.StartsWith("20"))
                        {
                            codigo = codigo.Remove(0, 2);
                            string peso = codigo.Remove(0, 5);
                            codigo = codigo.Substring(0, 5);
                            pesoEtiqueta = decimal.Round(Convert.ToDecimal(peso) / 10000, 3);
                            if (pesoEtiqueta > 15)
                            {
                                _permiso = true;
                            }
                            else
                            {
                                _permiso = false;
                            }
                            txtControl.Text = codigo;
                        }                        
                        double cantidad = 0;
                        if (codigo.IndexOf("*") != -1)
                        {
                            cantidad = obtenerCantidad(ref codigo);
                            txtControl.Text = codigo;
                        }
                        if (MuestraProductos(cantidad))
                        {
                            RefrescarMontos();
                        }
                        else
                        {
                            asignoPeso = true;
                            bascula = false;
                            _permiso = false;
                        }
                    }
                    else
                    {
                        if (!_permiso)
                        {
                            if (_strProducto.Length > 0)
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta el pese en la báscula el producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else
                        {
                            if (_strProducto.Length > 0)
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto " + _strProducto + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("No se pueden agregar más productos hasta que se autorize el peso del producto anterior.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }
        private void eliminarProducto()
        {
            try
            {
                //if (gridViewDetalleVenta.SelectedRowsCount > 0)
                if (gridViewVentas.SelectedRows != null && gridViewVentas.SelectedRows.Count > 0)
                {
                    DataGridViewRow row = gridViewVentas.SelectedRows[0];
                    var rowview = (VentaDetalle)row.Tag;
                    gridViewVentas.Rows.Remove(row);
                    //gridViewDetalleVenta.DeleteRow(rowHandle);
                    RefrescarMontos();
                    if (dicVentas.ContainsKey(rowview.IdDatosFiscales))
                    {
                        List<VentaDetalle> lst = dicVentas[rowview.IdDatosFiscales];
                        int length = lst.Count;
                        for (int i = 0; i < length; i++)
                        {
                            if (lst[i].Id_Producto == rowview.Id_Producto)
                            {
                                lst.RemoveAt(i);
                                break;
                            }
                        }
                        if (lst.Count > 0)
                        {
                            dicVentas[rowview.IdDatosFiscales] = lst;
                            if (!rowview.AsignoPeso)
                            {
                                _lastProductoBasculaId = 0;
                                _timer = false;
                                bascula = false;
                                asignarPeso = true;
                                tBascula.Stop();
                                _permiso = false;
                            }
                        }
                        else
                        {
                            _lastProductoBasculaId = 0;
                            _timer = false;
                            bascula = false;
                            asignarPeso = true;
                            tBascula.Stop();
                            _permiso = false;
                            _IdAutorizacion = -1;
                            dicVentas.Remove(rowview.IdDatosFiscales);
                        }
                    }
                }
                else
                {
                    txtControl.Focus();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            txtControl.Focus();
        }
        private void seleccionarcliente()
        {
            long id = -1;
            Cliente cliente = new Cliente();
            FrmBusqueda busqueda = null;
            if (FrmBascula.dicClientes.Count > 0)
            {
                busqueda = new FrmBusqueda(new List<Cliente>(FrmBascula.dicClientes.Values));
                busqueda.ColumnasOcultar = new List<string> { "Cliente_Id", "RFC", "Razon_Social", "Cuenta_Contable", "FechaAlta", "Localidad", "Ciudad", "Calle", "NumInt", "NumExt", "Colonia", "Codigo_Postal", "Estado", "Pais", "Domicilio", "Telefono", "Telefono2", "Telefono3", "Email", "Email2", "Email3", "Comentario", "Activo", "Dias_de_Credito", "Saldo", "Status", "Monto_Credito" };
            }
            else
            {
                busqueda = new FrmBusqueda(cliente.Listado());
                busqueda.ColumnasOcultar = new List<string> { "IdCliente", "RazonSocial", "Email1", "Email2", "Email3", "Telefono1", "Telefono2", "Telefono3", "Localidad", "Ciudad", "Calle", "NumInt", "NumExt", "Colonia", "CodigoPostal", "Estado", "Pais", "CuentaContable", "Comentario", "FechaAlta", "DiasCredito", "MontoCredito", "Status", "Saldo", "ClienteStatus", "Domicilio" };
            }
            busqueda.Width = 1300;
            busqueda.Text = "Clientes";
            busqueda.AjustarColumnas = true;
            busqueda.MaximizeBox = false;
            busqueda.MinimizeBox = false;
            if (busqueda.ShowDialog() == DialogResult.OK)
            {
                if (busqueda.FilaDatos != null && cliente.Cargar((DataRowView)busqueda.FilaDatos))
                {
                    id = cliente.Cliente_Id;
                    _clienteId = cliente.Cliente_Id;
                    txtNombreCliente.Text = cliente.Nombre + " " + cliente.ApellidoPaterno + " " + cliente.ApellidoMaterno;
                }
                else
                {
                    cliente = null;
                }
            }
            else
            {
                cliente = null;
            }
            if (id <= 0)
            {
                _clienteId = 0;
                txtNombreCliente.Text = "PUBLICO GENERAL";
            }
            txtControl.Focus();
        }
        private void txtControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 220)
            {
                txtControl.Text = string.Empty;
            }
        }
        public string getTotalVenta()
        {
            return txtTotal.Text;
        }
        public long obtenerClienteId()
        {
            return _clienteId;
        }
        public string obtenerNombreCliente()
        {
            if (_clienteId == 0)
            {
                return "PUBLICO GENERAL";
            }
            else
            {
                return txtNombreCliente.Text;
            }
        }
        private void addNewRowInGroupMode(DevExpress.XtraGrid.Views.Grid.GridView View, VentaDetalle detalle)
        {
            try
            {
                View.AddNewRow();
                int rowHandle = View.GetRowHandle(View.DataRowCount);
                if (View.IsNewItemRow(rowHandle))
                {
                    View.SetRowCellValue(rowHandle, View.Columns[0], detalle.Id_Venta_Detalle);
                    View.SetRowCellValue(rowHandle, View.Columns[1], detalle.Id_Venta);
                    View.SetRowCellValue(rowHandle, View.Columns[2], detalle.Id_Producto);
                    View.SetRowCellValue(rowHandle, View.Columns[3], detalle.Cantidad);
                    View.SetRowCellValue(rowHandle, View.Columns[4], detalle.Descripcion);
                    View.SetRowCellValue(rowHandle, View.Columns[5], detalle.Precio);
                    View.SetRowCellValue(rowHandle, View.Columns[10], detalle.Precio_Venta);
                    View.SetRowCellValue(rowHandle, View.Columns[11], detalle.Precio_Original);
                    View.SetRowCellValue(rowHandle, View.Columns[12], detalle.Quien_Surte);
                    View.SetRowCellValue(rowHandle, View.Columns[13], detalle.Id_Sucursal);
                    View.SetRowCellValue(rowHandle, View.Columns[14], detalle.Surtido);
                    View.SetRowCellValue(rowHandle, View.Columns[15], detalle.Precio_Mayoreo);
                    View.SetRowCellValue(rowHandle, View.Columns[16], detalle.IEPS);
                    View.SetRowCellValue(rowHandle, View.Columns[17], detalle.IVA);
                    View.SetRowCellValue(rowHandle, View.Columns[18], detalle.IEPSimporte);
                    View.SetRowCellValue(rowHandle, View.Columns[19], detalle.IVAimporte);
                    View.SetRowCellValue(rowHandle, View.Columns[23], detalle.Importe);
                    View.SetRowCellValue(rowHandle, View.Columns[24], detalle.Existencia);
                    View.SetRowCellValue(rowHandle, View.Columns[25], detalle.IdDatosFiscales);
                    View.SetRowCellValue(rowHandle, View.Columns[26], detalle.IdVentas);
                    View.SetRowCellValue(rowHandle, View.Columns[27], detalle.UltimaCantidad);
                    View.SetRowCellValue(rowHandle, View.Columns[28], detalle.CampoId);
                    View.SetRowCellValue(rowHandle, View.Columns[29], detalle.CampoBusqueda);
                    View.SetRowCellValue(rowHandle, View.Columns[35], detalle.Bascula);
                    View.SetRowCellValue(rowHandle, View.Columns[36], detalle.AsignoPeso);
                    View.SetRowCellValue(rowHandle, View.Columns[37], detalle.TotalArticulos);
                    View.SetRowCellValue(rowHandle, View.Columns[40], detalle.TipoClase);

                    View.UpdateCurrentRow();
                }
                View.FocusedRowHandle = rowHandle;
            }
            catch (Exception) { }
        }
        private void actualizarPrecios(VentaDetalle p)
        {
            double cantidad = p.Cantidad;
            double precioseleccionado = 0;
            if (_clienteId > 0)
            {
                precioseleccionado = p.Precio;
            }
            else
            {
                precioseleccionado = p.Precio;
            }
        }
        public void agregarDatosVenta(ref Venta v)
        {
            v.IdAutorizo = _IdAutorizacion;
            v.Id_Cliente = _clienteId;
            v.Id_Sucursal = Properties.Settings.Default.IdSucursal;
            v.Caja = Convert.ToInt32(lblNumeroCaja.Text);
            v.Fecha_Venta = Convert.ToDateTime(lblFecha.Text);
            v.Hora_Venta = v.Fecha_Venta;
            v.TotalArticulos = Convert.ToInt32(lblArticulos.Text);
            v.Id_Cotizacion = -1;
            v.Fecha_Tipo_Cambio = Global.MinDate;
            v.Id_Tipo_Cambio = -1;
            v.Comentario = "";
            v.Id_Factura = -1;
            v.Id_Metodo_pago = -1;
            v.Referencia = -1;
            v.Id_Tipo_Venta = -1;
            v.Debe = -1;
            v.Id_Promocion = -1;
        }
        private void gridViewDetalleVenta_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                eliminarProducto();
            }
        }
        private void lblInfo_Click(object sender, EventArgs e)
        {
            string strIpLocal = Properties.Settings.Default.IpLocal;
            string[] Ip = strIpLocal.Split('.');
            int nBandera = 0;
            string strIpCorta = "";
            foreach (string word in Ip)
            {
                if (nBandera >= 2)
                {
                    if (nBandera == 3)
                        strIpCorta += word;
                    else
                        strIpCorta += word + ".";
                }
                nBandera++;
            }
            lblInfo.Text = Properties.Settings.Default.Version + " - Online: " + strIpCorta;
            lblNombreTienda.Text = Properties.Settings.Default.NombreTienda;
            lblNumeroCaja.Text = Properties.Settings.Default.NumeroCaja.ToString();
        }

        private void gridViewVentas_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            cancelIt = true;
        }

        private void gridViewVentas_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (!cancelIt)
            {
                gridViewVentas.EndEdit();
                return;
            }
            indexDelete = -1;
                                
            if (e.ColumnIndex == 0)
            {
                double cantidad = 0;
                if (!double.TryParse(e.FormattedValue.ToString(), out cantidad))
                {
                    e.Cancel = true;
                    gridViewVentas.EndEdit();
                    VentaDetalle r = (VentaDetalle)gridViewVentas.Rows[e.RowIndex].Tag;
                    gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = r.Cantidad;
                    gridViewVentas.Rows[e.RowIndex].Tag = r;
                    //Neighbours.Rows[e.RowIndex].ErrorText = "error";
                }
                else
                {
                    VentaDetalle r = (VentaDetalle)gridViewVentas.Rows[e.RowIndex].Tag;
                    bool adelante = true;
                    if (!r.Bascula)
                    {
                        if (cantidad != (int)cantidad)
                        {
                            adelante = false;
                            e.Cancel = true;
                            gridViewVentas.EndEdit();
                            DevExpress.XtraEditors.XtraMessageBox.Show("No puedes agregar decimales a la cantidad puesto que el producto seleccionado no es de báscula.\r\nFavor de agregar una cantidad válida al producto seleccionado.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = r.Cantidad;
                            gridViewVentas.Rows[e.RowIndex].Tag = r;
                        }
                    }
                    if (adelante)
                    {
                        if (r.Cantidad != cantidad)
                        {
                            r.Cantidad = cantidad;
                            if (r.Cantidad <= r.Existencia)
                            {
                                if (r.Bascula && !r.AsignoPeso)
                                {
                                    if (_timer || bascula)
                                    {
                                        try
                                        {
                                            _permiso = false;
                                            bascula = false;
                                            _timer = false;
                                            tBascula.Stop();
                                            asignarPeso = true;
                                            _lastProductoBasculaId = 0;
                                        }
                                        catch (Exception) { }
                                        r.AsignoPeso = true;
                                    }
                                }
                                if (r.Cantidad > 0)
                                {
                                    if (!r.Bascula)
                                    {
                                        r.TotalArticulos = Convert.ToInt32(r.Cantidad);
                                    }
                                    r.Importe = Convert.ToDecimal(r.Precio_Venta * r.Cantidad);
                                    r.UltimaCantidad = r.Cantidad;
                                    actualizarPrecios(r);
                                    RefrescarMontos();
                                    e.Cancel = true;
                                    gridViewVentas.EndEdit();
                                    gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = r.Cantidad;
                                    gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.DESCRIPCION].Value = r.Descripcion;
                                    gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.PRECIO].Value = Global.DoubleToString(r.Precio_Venta);
                                    gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.IMPORTE].Value = Global.DoubleToString(Convert.ToDouble(r.Importe));
                                    gridViewVentas.Rows[e.RowIndex].Tag = r;
                                }
                                else
                                {
                                    e.Cancel = true;
                                    gridViewVentas.EndEdit();
                                    indexDelete = e.RowIndex;
                                }
                            }
                            else
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("No puedes vender mas articulos de los que tienes dado de alta en el Sistema.\r\nSolamente hay " + ((VentaDetalle)r).Existencia + " en existencia.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                e.Cancel = true;
                                gridViewVentas.EndEdit();
                                r.Cantidad = r.UltimaCantidad;
                                gridViewVentas.Rows[e.RowIndex].Cells[(int)DataGridViewColumnas.CANTIDAD].Value = r.Cantidad;
                                gridViewVentas.Rows[e.RowIndex].Tag = r;
                            }
                        }
                    }
                }
            }
            cancelIt = false;
            if (indexDelete > -1)
            {
                tEliminar.Start();
            }
            txtControl.Focus();
        }
    }
}
