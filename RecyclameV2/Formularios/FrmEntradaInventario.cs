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
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.IO;
using RecyclameV2.Formularios;
using System.Data.SqlClient;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.XtraGrid.Views.Grid;
using System.Runtime.InteropServices;
using Log = RecyclameV2.Clases.Log;

namespace RecyclameV2.Formularios
{
    public partial class FrmEntradaInventario : MetroForm
    {
        const int CARGAR_FATURA_TAB = 0;
        const int ENTRADA_INVENTARIO_TAB = 1;
        const int ENTRADA_TIPO_MOVIMIENTOS_TAB = 2;
        const int ENTRADA_PRODUCTOS_TAB = 3;
        const int AGREGAR_PRODUCTO_TAB = 4;
        const int ENTRADA_MANUAL_INVENTARIO_TAB = 5;
        const int LISTADO_PRODUCTOS_TAB = 6;
        const int AUDITORIA_TAB = 7;
        const int ETIQUETAS_TAB = 8;
        const int LISTADO_ENTRADAS_TAB = 9;

        CFDS _cfds = null;
        TIPO_FACTURA _tipoFactura;
        Tipo_Movimiento _tipoMovimiento = null;
        Productos _producto = null;
        double _dImporteFlete = 0;
        bool _bFlete = false;
        bool _ignorarValueChange = false;
        int times = 0;
        private double Total = 0;
        private double SubTotal = 0;
        short _copies = 0;

        private PrintingSystem printingSystem1 = new PrintingSystem();
        long _idProducto = 0;
        public FrmEntradaInventario()
        {
            InitializeComponent();
            _tipoFactura = TIPO_FACTURA.ENTRADA;
            tabEntradas.SelectedIndex = 0;
        }

        private void tabEntradas_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearTabPage(tabEntradas.SelectedTab.Name);
        }
        private void clearTabPage(string name)
        {
            times = 0;
            switch (name)
            {
                case "tabCargaFactura":
                    Limpiar();
                    CargarTipoMovimiento();
                    CargarSucursales();
                    CargarEmpresas();
                    break;
                case "tabEntradaInventario":
                    Limpiar2();
                    CargarProvedor();
                    CargarTipoMovimiento2();
                    CargarSucursales2();
                    CargarEmpresa2();
                    break;
                case "tabProducto":
                    _producto = null;
                    Limpiar5();
                    break;
                case "tabListadoProductos":
                    if (txtBuscarProducto.Text.Length > 0)
                    {
                        txtBuscarProducto.Text = string.Empty;
                    }
                    else
                    {
                        buscarProductos(txtBuscarProducto.Text);
                    }
                    break;
                case "tabListadoEntradas":
                    fechaInicioListado.DateTime = DateTime.Now;
                    fechaFinListado.DateTime = DateTime.Now;
                    buscarListadoEntradas();
                    break;
            }
        }

        #region CargarFacturas        
        private void txtArchivoXML_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            CargarArchivo("XML (*.xml)|*.xml");
        }

        private void txtArchivoPDF_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            CargarArchivo("PDF (*.pdf)|*.pdf");
        }

        private void txtArchivoIMG_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            CargarArchivo("Imágenes (*.gif)(*.jpg)(*.jpeg)(*.bmp)(*.wmf)(*.png)|*.gif;*.jpg;*.jpeg;*.bmp;*.wmf;*.png");
        }

        private void CargarArchivo(string strFiltro)
        {
            try
            {
                Herramientas.MostrarWaitDialog("CARGANDO ARCHIVO...", "ESPERE");

                OpenFileDialog openfile = new OpenFileDialog();
                openfile.Filter = strFiltro;
                if (openfile.ShowDialog() == DialogResult.OK)
                {
                    TIPO_ARCHIVO tipoArchivo = visorArchivoFactura.Cargar(openfile.FileName);
                    switch (tipoArchivo)
                    {
                        case TIPO_ARCHIVO.XML:
                            _cfds = Herramientas.CargarXMLFactura(openfile.FileName, _tipoFactura);
                            CFDS auxCFDS = new CFDS();
                            auxCFDS.Tipo_Id = _cfds.Tipo_Id;
                            auxCFDS.Folio_Fiscal = _cfds.Folio_Fiscal;
                            txtArchivoXML.Text = visorArchivoFactura.XMLRutaArchivo;
                            if (auxCFDS.Cargar().Result)
                            {
                                if (auxCFDS.Estatus == "PROCESADO")
                                {
                                    DevExpress.XtraEditors.XtraMessageBox.Show("La factura ya existe y tiene el estatus de PROCESADO y no se permiten modificaciones. Por favor seleccione otra factura.");
                                    Limpiar();
                                    return;
                                }

                                DevExpress.XtraEditors.XtraMessageBox.Show("La factura ya existe. Se cargará la información existente.");
                                _cfds = auxCFDS;
                                //txtImporteFlete.Text = _cfds.Flete.ToString("C");

                                foreach (CFDS_Archivo archivo in _cfds.Archivos)
                                {
                                    if (archivo.Tipo_Archivo_Id != (int)TIPO_ARCHIVO.XML)
                                    {
                                        visorArchivoFactura.Cargar(archivo.Nombre_Archivo, archivo.Archivo);
                                    }
                                }
                            }

                            if (_cfds.Tipo_Movimiento_Id > 0)
                                cboTipoMovimiento.EditValue = (System.Decimal)_cfds.Tipo_Movimiento_Id;

                            if (_cfds.IdSucursal > -1)
                                cboSucursales.EditValue = (System.Decimal)_cfds.IdSucursal;

                            //if (_cfds.IdDatosFiscales > -1)
                            //{
                            //    cboEmpresas.EditValue = (System.Decimal)_cfds.IdDatosFiscales;
                            //}

                            gridProducto.DataSource = null;
                            gridProducto.DataSource = _cfds.Productos;
                            RefrescarCantidades(-1);

                            break;
                        case TIPO_ARCHIVO.PDF:
                            txtArchivoPDF.Text = visorArchivoFactura.PDFRutaArchivo;
                            break;
                        case TIPO_ARCHIVO.IMAGEN:
                            txtArchivoIMG.Text = visorArchivoFactura.IMGRutaArchivo;
                            break;
                    }

                    if (tipoArchivo == TIPO_ARCHIVO.NO_SOPORTADO)
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("El tipo de archivo no es soportado.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information); ;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Herramientas.CerrarWaitDialog();
            }
        }

        private void btnGrabar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cfds == null)
                    return;

                CFDS auxCFDS = new CFDS();
                auxCFDS.Tipo_Id = _cfds.Tipo_Id;
                auxCFDS.Folio_Fiscal = _cfds.Folio_Fiscal;
                if (auxCFDS.Cargar().Result)
                {
                    if (auxCFDS.Estatus == "PROCESADO")
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show("La factura ya existe y tiene el estatus de PROCESADO y no se permiten modificaciones. Por favor seleccione otra factura.");
                        Limpiar();
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(txtArchivoXML.Text) && !string.IsNullOrWhiteSpace(txtArchivoXML.Text))
                {
                    FileInfo info = new FileInfo(txtArchivoXML.Text);
                    CFDS_Archivo archivo = null;

                    archivo = _cfds.Archivos.ToList().Find(a => a.Tipo_Archivo_Id == (int)TIPO_ARCHIVO.XML);

                    if (archivo == null)
                        archivo = new CFDS_Archivo();

                    archivo.Archivo = visorArchivoFactura.XMLArchivo64;
                    archivo.Nombre_Archivo = info.Name;
                    archivo.Tipo_Archivo_Id = (int)TIPO_ARCHIVO.XML;

                    _cfds.Archivos.Add(archivo);
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione el archivo XML con la información de la factura.", this.ProductName, MessageBoxButtons.OK);
                    txtArchivoXML.Focus();
                    return;
                }

                if (cboTipoMovimiento.EditValue == null || Convert.ToInt64(cboTipoMovimiento.EditValue) == 0)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione el tipo de movimiento de la factura.", this.ProductName, MessageBoxButtons.OK);
                    cboTipoMovimiento.Focus();
                    return;
                }

                if (!string.IsNullOrEmpty(txtArchivoPDF.Text) && !string.IsNullOrWhiteSpace(txtArchivoPDF.Text))
                {
                    FileInfo info = new FileInfo(txtArchivoPDF.Text);
                    CFDS_Archivo archivo = null;

                    archivo = _cfds.Archivos.ToList().Find(a => a.Tipo_Archivo_Id == (int)TIPO_ARCHIVO.PDF);

                    if (archivo == null)
                        archivo = new CFDS_Archivo();

                    archivo.Archivo = visorArchivoFactura.PDFArchivo64;
                    archivo.Nombre_Archivo = info.Name;
                    archivo.Tipo_Archivo_Id = (int)TIPO_ARCHIVO.PDF;

                    _cfds.Archivos.Add(archivo);
                }

                if (!string.IsNullOrEmpty(txtArchivoIMG.Text) && !string.IsNullOrWhiteSpace(txtArchivoIMG.Text))
                {
                    FileInfo info = new FileInfo(txtArchivoIMG.Text);
                    CFDS_Archivo archivo = null;

                    archivo = _cfds.Archivos.ToList().Find(a => a.Tipo_Archivo_Id == (int)TIPO_ARCHIVO.IMAGEN);

                    if (archivo == null)
                        archivo = new CFDS_Archivo();

                    archivo.Archivo = visorArchivoFactura.IMGArchivo64;
                    archivo.Nombre_Archivo = info.Name;
                    archivo.Tipo_Archivo_Id = (int)TIPO_ARCHIVO.IMAGEN;

                    _cfds.Archivos.Add(archivo);
                }

                Provedor provedor = new Provedor();
                provedor.RFC = _cfds.RFC_Emisor;
                if (!provedor.Cargar().Result)
                {
                    provedor.Nombre = _cfds.Nombre_Emisor;
                    provedor.Grabar();
                    //OnRaiseActualizarCatalogoEvent(CLASE.Provedor);
                }

                if (provedor.Provedor_Id > 0)
                {
                    foreach (var producto in _cfds.Productos)
                    {
                        if (producto.Producto_Id > 0)
                        {
                            if (producto.Numero_Identificacion.Trim().Length > 0)
                            {
                                Diccionario diccionario = new Diccionario();
                                diccionario.Producto_Id = producto.Producto_Id;
                                diccionario.Provedor_Id = provedor.Provedor_Id;
                                diccionario.Valor = producto.Numero_Identificacion;
                                diccionario.Grabar();
                            }

                            if (producto.Descripcion.Trim().Length > 0)
                            {
                                Diccionario diccionario = new Diccionario();
                                diccionario.Producto_Id = producto.Producto_Id;
                                diccionario.Provedor_Id = provedor.Provedor_Id;
                                diccionario.Valor = producto.Descripcion;
                                diccionario.Grabar();
                            }

                            Productos prod = new Productos();
                            prod.Producto_Id = producto.Producto_Id;
                            if (prod.Cargar().Result)
                            {
                                if (prod.Codigo_de_Barras == "" && producto.Numero_Identificacion.Length >= 13 && producto.Numero_Identificacion.Length <= 15)
                                {
                                    prod.Codigo_de_Barras = producto.Numero_Identificacion;
                                    prod.Grabar();
                                }
                            }
                        }
                    }
                }

                _cfds.Tipo_Movimiento_Id = Convert.ToInt64(cboTipoMovimiento.EditValue ?? 0);
                _cfds.IdSucursal = cboSucursales.EditValue != null ? Convert.ToInt64(cboSucursales.EditValue) : -1;
                _cfds.IdDatosFiscales = 0;// cboEmpresas.EditValue != null ? Convert.ToInt32(cboEmpresas.EditValue) : -1;

                if (_cfds.Grabar())
                {
                    //if (cboSucursales.EditValue == null)
                    //{
                    //    cboSucursales.EditValue = "Matriz";
                    //}
                    if (cboSucursales.EditValue != null)
                    {
                        Movimientos movimiento = new Movimientos()
                        {
                            CFDS_Id = _cfds.CFDS_Id,
                            Fecha_Movimiento = DateTime.Now,
                            Tipo_Movimiento_Id = _cfds.Tipo_Movimiento_Id,
                            Sucursal = cboSucursales.EditValue.ToString(),
                            Flete = Convert.ToDouble(_dImporteFlete)
                        };

                        foreach (CFDS_Producto producto in _cfds.Productos)
                        {
                            Movimiento_Detalle detalle = new Movimiento_Detalle()
                            {
                                Movimiento_Id = movimiento.Movimiento_Id,
                                Producto_Id = producto.Producto_Id,
                                Cantidad = producto.Cantidad
                            };
                            movimiento.Detalles.Add(detalle);
                        }

                        if (movimiento.Grabar())
                        {

                            DevExpress.XtraEditors.XtraMessageBox.Show(this, "La información se guardó correctamente.", this.ProductName, MessageBoxButtons.OK);
                            Limpiar();
                        }
                        else { DevExpress.XtraEditors.XtraMessageBox.Show(this, "Error al guardar la informacion, Favor de intentar de nuevo.", this.ProductName, MessageBoxButtons.OK); }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void Limpiar()
        {
            visorArchivoFactura.Limpiar();
            txtArchivoXML.Text = "";
            txtArchivoPDF.Text = "";
            txtArchivoIMG.Text = "";
            cboTipoMovimiento.EditValue = null;
            cboSucursales.EditValue = null;
            //cboEmpresas.EditValue = null;
            //txtImporteFlete.Text = "";
            //btnFlete.Enabled = true;
            List<string> listColumnasOcultar = new List<string>() { "Agregar", "CampoId", "CampoBusqueda", "Diferencia_Costo", "isCheckedCantidad_Empaque", "isCheckedCodigo_Producto", "isCheckedProducto", "isCheckedUltimo_Costo", "isCheckedUbicaciones", "TipoClase", "Numero_Serie", "ValorUnitarioOriginal", "Producto" };
            List<string> listColumnasEditar = new List<string> { "Cantidad_Factura", "Valor_Unitario" };

            if (_tipoFactura == TIPO_FACTURA.SALIDA)
            {
                listColumnasOcultar.Add("Cantidad_Empaque");
                listColumnasOcultar.Add("Cantidad");
            }
            else
            {
                listColumnasEditar.Add("Cantidad_Empaque");
            }

            gridProducto.DataSource = null;
            gridProducto.DataSource = new BindingList<CFDS_Producto>();
            Herramientas.GridViewEditarColumnas(gridViewProducto, true, true, false, listColumnasOcultar, listColumnasEditar, new List<string> { "Cantidad", "Cantidad_Empaque", "Cantidad_Factura" });

            _cfds = null;

            if (_tipoFactura == TIPO_FACTURA.ENTRADA)
            {
                DevExpress.XtraGrid.StyleFormatCondition sfcIgual5Porc = new DevExpress.XtraGrid.StyleFormatCondition();
                sfcIgual5Porc.Appearance.BackColor = System.Drawing.Color.Yellow;
                sfcIgual5Porc.Appearance.ForeColor = System.Drawing.Color.Black;
                sfcIgual5Porc.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
                sfcIgual5Porc.Appearance.Options.UseBackColor = true;
                sfcIgual5Porc.Appearance.Options.UseForeColor = true;
                sfcIgual5Porc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Default;
                sfcIgual5Porc.Appearance.TextOptions.HotkeyPrefix = DevExpress.Utils.HKeyPrefix.Default;
                sfcIgual5Porc.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.Default;
                sfcIgual5Porc.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Default;
                sfcIgual5Porc.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Default;
                sfcIgual5Porc.ApplyToRow = true;
                sfcIgual5Porc.Column = gridViewProducto.Columns["Diferencia_Costo"];
                sfcIgual5Porc.Condition = DevExpress.XtraGrid.FormatConditionEnum.Expression;
                sfcIgual5Porc.Expression = "Abs([Diferencia_Costo]) > 0 and Abs([Diferencia_Costo]) <= 5";

                DevExpress.XtraGrid.StyleFormatCondition sfcMayor5Porc = new DevExpress.XtraGrid.StyleFormatCondition();
                sfcMayor5Porc.Appearance.BackColor = System.Drawing.Color.Orange;
                sfcMayor5Porc.Appearance.ForeColor = System.Drawing.Color.Black;
                sfcMayor5Porc.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
                sfcMayor5Porc.Appearance.Options.UseBackColor = true;
                sfcMayor5Porc.Appearance.Options.UseForeColor = true;
                sfcMayor5Porc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Default;
                sfcMayor5Porc.Appearance.TextOptions.HotkeyPrefix = DevExpress.Utils.HKeyPrefix.Default;
                sfcMayor5Porc.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.Default;
                sfcMayor5Porc.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Default;
                sfcMayor5Porc.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Default;
                sfcMayor5Porc.ApplyToRow = true;
                sfcMayor5Porc.Column = gridViewProducto.Columns["Diferencia_Costo"];
                sfcMayor5Porc.Condition = DevExpress.XtraGrid.FormatConditionEnum.Expression;
                sfcMayor5Porc.Expression = "Abs([Diferencia_Costo]) > 5";

                gridViewProducto.FormatConditions.AddRange(new DevExpress.XtraGrid.StyleFormatCondition[] {
                    sfcIgual5Porc,
                    sfcMayor5Porc
                });
            }

            txtArchivoXML.Focus();
        }
        private void FrmEntradaInventario_Load(object sender, EventArgs e)
        {
            Limpiar();

            CargarTipoMovimiento();
            CargarSucursales();
            CargarProvedor();
            CargarTipoMovimiento2();
            CargarSucursales2();
            CargarEmpresas();
            CargarEmpresa2();
        }

        private void KeyEvent(object sender, KeyEventArgs e)
        {
            if (tabEntradas.SelectedIndex == CARGAR_FATURA_TAB)
            {
                switch (e.KeyCode)
                {
                    case Keys.F3:
                        BuscarProducto();
                        break;
                    case Keys.F5:
                        btnGrabar_Click(null, null);
                        break;
                    case Keys.F7:
                        btnLimpiar_Click(null, null);
                        break;
                }
            }
            else if (tabEntradas.SelectedIndex == ENTRADA_INVENTARIO_TAB)
            {
                switch (e.KeyCode)
                {
                    case Keys.F3:
                        BuscarProducto2();
                        break;
                    case Keys.F4:
                        txtFolioFiscal_ButtonClick(null, null);
                        break;
                    case Keys.F5:
                        btnGrabar2_Click(null, null);
                        break;
                    case Keys.F7:
                        btnLimpiar2_Click(null, null);
                        break;
                }
            }
            else if (tabEntradas.SelectedIndex == AGREGAR_PRODUCTO_TAB)
            {
                switch (e.KeyCode)
                {
                    case Keys.F5:
                        btnGrabar5_Click(null, null);
                        break;
                    case Keys.F7:
                        btnLimpiar5_Click(null, null);
                        break;
                }
            }
            else if (tabEntradas.SelectedIndex == LISTADO_PRODUCTOS_TAB)
            {
                if (e.KeyCode == Keys.F4)
                {
                    if (gridView1.SelectedRowsCount > 0)
                    {
                        int rowHandle = gridView1.GetSelectedRows()[0];
                        editarProducto(gridView1.FocusedRowHandle);
                    }
                }
                else if (e.KeyCode == Keys.F2)
                {
                    if (gridView1.SelectedRowsCount > 0)
                    {
                        int rowHandle = gridView1.GetSelectedRows()[0];
                        eliminarProducto(rowHandle);
                    }
                }
                else if (e.KeyCode == Keys.F6)
                {
                    btnExportarListadoProductos_Click(null, null);
                }
            }
            else if (string.Compare(tabEntradas.SelectedTab.Name, "tabListadoEntradas", true) == 0)
            {
                if (e.KeyCode == Keys.F3)
                {
                    buscarListadoEntradas();
                }
                if (e.KeyCode == Keys.F6)
                {
                    btnExportarPDF_Click(null, null);
                }
                //else if (e.KeyCode == Keys.F2)
                //{
                //    if (gridViewListadoEntradas.SelectedRowsCount > 0)
                //    {
                //        int rowHandle = gridViewListadoEntradas.GetSelectedRows()[0];
                //        eliminarListadoOrdenEntrada(rowHandle);
                //    }
                //}
            }
        }
        private void BuscarProducto()
        {
            if (gridViewProducto.SelectedRowsCount > 0)
            {
                int rowHandle = gridViewProducto.GetSelectedRows()[0];
                BuscarProducto(rowHandle);
            }
        }

        private void BuscarProducto(int rowHandle)
        {
            this.Cursor = Cursors.WaitCursor;
            CFDS_Producto producto = (CFDS_Producto)gridViewProducto.GetRow(rowHandle);
            if (producto != null)
            {
                Productos productos = new Productos();
                using (FrmBusqueda busqueda = new FrmBusqueda(productos.Listado())
                {
                    Width = 800,
                    Text = "Productos",
                    AjustarColumnas = true,
                    ColumnasOcultar = new List<string> { "IdProducto", "CampoId", "CampoBusqueda", "IdLinea1", "IdLinea2", "IdLinea3", "Status", "Serie" }
                })
                {
                    if (busqueda.ShowDialog() == DialogResult.OK)
                    {
                        if (busqueda.FilaDatos != null && productos.Cargar((DataRowView)busqueda.FilaDatos))
                        {
                            producto.ClearProducto();
                            producto.Producto_Id = productos.Producto_Id;
                            if (producto.Diferencia_Costo > 0) { }

                            gridProducto.RefreshDataSource();
                        }
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void gridViewProducto_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Point pt = gridViewProducto.GridControl.PointToClient(Control.MousePosition);
                GridHitInfo info = gridViewProducto.CalcHitInfo(pt);

                if (info.InRow || info.InRowCell)
                {
                    BuscarProducto(info.RowHandle);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //public override void ActualizarCatalogo(CLASE clase)
        //{
        //    if (clase == CLASE.Tipo_Movimiento)
        //    {
        //        CargarTipoMovimiento();
        //    }
        //}

        private void CargarTipoMovimiento()
        {
            object value = cboTipoMovimiento.EditValue;

            Tipo_Movimiento tipoMovimiento = new Tipo_Movimiento();
            Herramientas.LlenarCombo(cboTipoMovimiento, tipoMovimiento.Listado(true, _tipoFactura.ToString().Substring(0, 1)), tipoMovimiento.CampoId, tipoMovimiento.CampoBusqueda);

            cboTipoMovimiento.EditValue = value;
        }

        private void CargarSucursales()
        {
            /*object value = cboSucursales.EditValue;

            Sucursales sucursales = new Sucursales();
            Herramientas.LlenarCombo(cboSucursales, sucursales.Listado(), sucursales.CampoId, sucursales.CampoBusqueda);

            cboSucursales.EditValue = value;*/
        }

        private void CargarEmpresas()
        {
            //object value = cboEmpresas.EditValue;
            //DatosFacturacion empresa = new DatosFacturacion();
            //Herramientas.LlenarCombo(cboEmpresas, empresa.Listado(), empresa.CampoId, empresa.CampoBusqueda);
            //cboEmpresas.EditValue = value;
        }

        private void gridViewProducto_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            RefrescarCantidades(e.RowHandle);
        }

        private void RefrescarCantidades(int index)
        {
            if (_cfds == null)
                return;
            if (index < 0)
            {
                foreach (var producto in _cfds.Productos)
                {
                    producto.Cantidad = (producto.Cantidad_Empaque <= 0 ? 1 : producto.Cantidad_Empaque) * producto.Cantidad_Factura;
                }
                gridProducto.RefreshDataSource();
            }
            else
            {
                if (gridViewProducto.FocusedRowHandle != index)
                {
                    gridViewProducto.SelectRow(index);
                    gridViewProducto.FocusedRowHandle = index;
                }
                object r = gridViewProducto.GetRow(gridViewProducto.FocusedRowHandle);
                ((CFDS_Producto)r).Cantidad = (((CFDS_Producto)r).Cantidad_Empaque <= 0 ? 1 : ((CFDS_Producto)r).Cantidad_Empaque) * ((CFDS_Producto)r).Cantidad_Factura;
                ((CFDS_Producto)r).Valor_Unitario = ((CFDS_Producto)r).ValorUnitarioOriginal / ((CFDS_Producto)r).Cantidad_Empaque;
                gridViewProducto.RefreshRow(gridViewProducto.FocusedRowHandle);
            }
        }

        private void btnFlete_Click(object sender, EventArgs e)
        {
            string cellValue = "";
            if (DevExpress.XtraEditors.XtraMessageBox.Show("El Flete viene dentro de esta Factura?", "Confirmation", MessageBoxButtons.YesNo) !=
                  DialogResult.Yes)
            {
                if (_cfds != null)
                {
                    //double dImporteFlete = Convert.ToDouble(txtImporteFlete.Text);
                    _dImporteFlete = 0;// Global.StringToDouble(txtImporteFlete.Text);
                    double dSubTotal = _cfds.SubTotal - _cfds.Descuento;
                    double dFActor = _dImporteFlete / dSubTotal;

                    int nTotalRow = gridViewProducto.DataRowCount;
                    double dFlete = 0;// Convert.ToDouble(txtImporteFlete.Text);
                    double dTotal = dFlete / nTotalRow;

                    for (int i = 0; i < gridViewProducto.DataRowCount; i++)
                    {
                        string strCampo = _cfds.CampoId;
                        double dImporte = Convert.ToDouble(gridViewProducto.GetRowCellValue(i, "Importe"));
                        double dValorUnitario = _cfds.Productos[i].Valor_Unitario;
                        double dCantidad = Convert.ToDouble(gridViewProducto.GetRowCellValue(i, "Cantidad"));
                        double dCostoNuevo = 0;
                        dCostoNuevo = dValorUnitario + ((dFActor * dImporte) / dCantidad);
                        dCostoNuevo = Math.Round(dCostoNuevo, 2);
                        _cfds.Productos[i].Valor_Unitario = dCostoNuevo;
                        _cfds.Productos[i].ValorUnitarioOriginal = dCostoNuevo;
                    }
                    gridProducto.RefreshDataSource();
                }
            }
            else
            {
                if (_cfds != null && gridViewProducto.SelectedRowsCount > 0)
                {
                    int rowHandle = gridViewProducto.GetSelectedRows()[0];
                    cellValue = gridViewProducto.GetRowCellValue(rowHandle, "Importe").ToString();

                    //double dImporteFlete = Convert.ToDouble(cellValue);
                    _dImporteFlete = Convert.ToDouble(cellValue);
                    double dSubTotal = _cfds.SubTotal - _cfds.Descuento;
                    double dFActor = _dImporteFlete / dSubTotal;

                    //aocegueda
                    int nTotalRow = gridViewProducto.DataRowCount - 1;
                    double dFlete = Convert.ToDouble(gridViewProducto.GetRowCellValue(rowHandle, "Importe"));
                    double dTotal = dFlete / nTotalRow;

                    gridViewProducto.DeleteRow(rowHandle);

                    for (int i = 0; i < gridViewProducto.DataRowCount; i++)
                    {
                        string strCampo = _cfds.CampoId;
                        double dImporte = Convert.ToDouble(gridViewProducto.GetRowCellValue(i, "Importe"));
                        double dValorUnitario = _cfds.Productos[i].Valor_Unitario;
                        double dCantidad = Convert.ToDouble(gridViewProducto.GetRowCellValue(i, "Cantidad"));
                        double dCostoNuevo = 0;
                        dCostoNuevo = dValorUnitario + ((dFActor * dImporte) / dCantidad);
                        dCostoNuevo = Math.Round(dCostoNuevo, 2);
                        _cfds.Productos[i].Valor_Unitario = dCostoNuevo;
                        _cfds.Productos[i].ValorUnitarioOriginal = dCostoNuevo;
                    }
                }
            }
            gridProducto.RefreshDataSource();
            // btnFlete.Enabled = false;
            _bFlete = true;
            //if (_cfds != null)
            //{
            //    _cfds.Flete = Global.StringToDouble(txtImporteFlete.Text);
            //    txtImporteFlete.Text = _cfds.Flete.ToString("C");
            //}
        }

        private void gridViewProducto_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            foreach (int i in gridViewProducto.GetSelectedRows())
            {
                DataRow row = gridViewProducto.GetDataRow(i);
                DevExpress.XtraEditors.XtraMessageBox.Show(row[0].ToString());
                DevExpress.XtraEditors.XtraMessageBox.Show(row[1].ToString());
                DevExpress.XtraEditors.XtraMessageBox.Show(row[2].ToString());
                DevExpress.XtraEditors.XtraMessageBox.Show(row[3].ToString());
            }
        }

        private void gridProducto_Click(object sender, EventArgs e)
        {
            if (gridViewProducto.SelectedRowsCount > 0)
            {
                string cellValue = "";
                int rowHandle = gridViewProducto.GetSelectedRows()[0];
                cellValue = gridViewProducto.GetRowCellValue(rowHandle, "Importe").ToString();
                //txtImporteFlete.Text = cellValue;
            }
        }

        #endregion CargarFacturas

        #region EntradaInventario
        private void Limpiar2()
        {
            cboProvedor.EditValue = null;
            cboTipoMovimiento2.EditValue = null;
            txtFolioFiscal.Text = "";
            deFechaFactura.EditValue = null;
            txtMonto.EditValue = 0.00;
            cboSucursales2.EditValue = null;
            //cboEmpresa2.EditValue = null;
            List<string> listColumnasOcultar = null;
            if (Global.AgregarNumeroSerie)
            {
                listColumnasOcultar = new List<string>() { "CampoId", "CampoBusqueda", "Diferencia_Costo", "isCheckedCantidad_Empaque", "isCheckedCodigo_Producto", "isCheckedProducto", "isCheckedUltimo_Costo", "isCheckedUbicaciones", "TipoClase", "ValorUnitarioOriginal", "Producto" };
            }
            else
            {
                listColumnasOcultar = new List<string>() { "CampoId", "CampoBusqueda", "Diferencia_Costo", "isCheckedCantidad_Empaque", "isCheckedCodigo_Producto", "isCheckedProducto", "isCheckedUltimo_Costo", "isCheckedUbicaciones", "TipoClase", "Numero_Serie", "ValorUnitarioOriginal", "Producto" };
            }
            List<string> listColumnasEditar = new List<string> { "Agregar", "Cantidad_Factura", "Valor_Unitario", "Codigo_Producto", "Precio_Sugerido" };

            if (_tipoFactura == TIPO_FACTURA.SALIDA)
            {
                listColumnasOcultar.Add("Cantidad_Empaque");
                listColumnasOcultar.Add("Cantidad");
            }
            else
            {
                listColumnasEditar.Add("Cantidad_Empaque");
            }

            gridProducto2.DataSource = null;
            gridProducto2.DataSource = new BindingList<CFDS_Producto>();
            Herramientas.GridViewEditarColumnas(gridViewProducto2, true, true, false, listColumnasOcultar, listColumnasEditar, new List<string> { "Cantidad", "Cantidad_Empaque", "Cantidad_Factura", "Codigo_Producto" });

            _cfds = new CFDS();

            if (_tipoFactura == TIPO_FACTURA.ENTRADA)
            {
                DevExpress.XtraGrid.StyleFormatCondition sfcIgual5Porc = new DevExpress.XtraGrid.StyleFormatCondition();
                sfcIgual5Porc.Appearance.BackColor = System.Drawing.Color.Yellow;
                sfcIgual5Porc.Appearance.ForeColor = System.Drawing.Color.Black;
                sfcIgual5Porc.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
                sfcIgual5Porc.Appearance.Options.UseBackColor = true;
                sfcIgual5Porc.Appearance.Options.UseForeColor = true;
                sfcIgual5Porc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Default;
                sfcIgual5Porc.Appearance.TextOptions.HotkeyPrefix = DevExpress.Utils.HKeyPrefix.Default;
                sfcIgual5Porc.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.Default;
                sfcIgual5Porc.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Default;
                sfcIgual5Porc.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Default;
                sfcIgual5Porc.ApplyToRow = true;
                sfcIgual5Porc.Column = gridViewProducto2.Columns["Diferencia_Costo"];
                sfcIgual5Porc.Condition = DevExpress.XtraGrid.FormatConditionEnum.Expression;
                sfcIgual5Porc.Expression = "Abs([Diferencia_Costo]) > 0 and Abs([Diferencia_Costo]) <= 5";

                DevExpress.XtraGrid.StyleFormatCondition sfcMayor5Porc = new DevExpress.XtraGrid.StyleFormatCondition();
                sfcMayor5Porc.Appearance.BackColor = System.Drawing.Color.Orange;
                sfcMayor5Porc.Appearance.ForeColor = System.Drawing.Color.Black;
                sfcMayor5Porc.Appearance.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Horizontal;
                sfcMayor5Porc.Appearance.Options.UseBackColor = true;
                sfcMayor5Porc.Appearance.Options.UseForeColor = true;
                sfcMayor5Porc.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Default;
                sfcMayor5Porc.Appearance.TextOptions.HotkeyPrefix = DevExpress.Utils.HKeyPrefix.Default;
                sfcMayor5Porc.Appearance.TextOptions.Trimming = DevExpress.Utils.Trimming.Default;
                sfcMayor5Porc.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Default;
                sfcMayor5Porc.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Default;
                sfcMayor5Porc.ApplyToRow = true;
                sfcMayor5Porc.Column = gridViewProducto2.Columns["Diferencia_Costo"];
                sfcMayor5Porc.Condition = DevExpress.XtraGrid.FormatConditionEnum.Expression;
                sfcMayor5Porc.Expression = "Abs([Diferencia_Costo]) > 5";

                gridViewProducto2.FormatConditions.AddRange(new DevExpress.XtraGrid.StyleFormatCondition[] {
                    sfcIgual5Porc,
                    sfcMayor5Porc
                });
            }
            cboProvedor.Focus();
        }

        private void CargarProvedor()
        {
            object value = cboProvedor.EditValue;
            _ignorarValueChange = true;
            Provedor provedor = new Provedor();
            Herramientas.LlenarCombo(cboProvedor, provedor.Listado(), "RFC", "Nombre");

            cboProvedor.EditValue = value;
            _ignorarValueChange = false;
        }

        private void CargarTipoMovimiento2()
        {
            object value = cboTipoMovimiento2.EditValue;

            Tipo_Movimiento tipoMovimiento = new Tipo_Movimiento();
            Herramientas.LlenarCombo(cboTipoMovimiento2, tipoMovimiento.Listado(true, _tipoFactura.ToString().Substring(0, 1)), tipoMovimiento.CampoId, tipoMovimiento.CampoBusqueda);

            cboTipoMovimiento2.EditValue = value;
        }

        private void CargarSucursales2()
        {
            /*object value = cboSucursales2.EditValue;

            Sucursales sucursales = new Sucursales();
            Herramientas.LlenarCombo(cboSucursales2, sucursales.Listado(), sucursales.CampoId, sucursales.CampoBusqueda);

            cboSucursales2.EditValue = value;*/
        }
        private void CargarEmpresa2()
        {
            //object value = cboEmpresa2.EditValue;
            //DatosFacturacion empresas = new DatosFacturacion();
            //Herramientas.LlenarCombo(cboEmpresa2, empresas.Listado(), empresas.CampoId, empresas.CampoBusqueda);
            //cboEmpresa2.EditValue = value;
        }

        private void gridViewProducto2_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Point pt = gridViewProducto2.GridControl.PointToClient(Control.MousePosition);
                GridHitInfo info = gridViewProducto2.CalcHitInfo(pt);

                if (info.InRow || info.InRowCell)
                {
                    BuscarProducto2(info.RowHandle);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarProducto2()
        {
            if (gridViewProducto2.SelectedRowsCount > 0)
            {
                int rowHandle = gridViewProducto2.GetSelectedRows()[0];
                BuscarProducto2(rowHandle);
            }
        }

        private void BuscarProducto2(int rowHandle)
        {
            CFDS_Producto producto = (CFDS_Producto)gridViewProducto2.GetRow(rowHandle);
            if (producto != null)
            {
                Productos productos = new Productos();
                using (FrmBusqueda busqueda = new FrmBusqueda(productos.Listado())
                {
                    Width = 800,
                    Text = "Productos",
                    AjustarColumnas = true,
                    ColumnasOcultar = new List<string> { "IdProducto", "CampoId", "CampoBusqueda", "IdLinea1", "IdLinea2", "IdLinea3", "Status", "Serie" }
                })
                {
                    if (busqueda.ShowDialog() == DialogResult.OK)
                    {
                        if (busqueda.FilaDatos != null && productos.Cargar((DataRowView)busqueda.FilaDatos))
                        {
                            producto.ClearProducto();
                            producto.Producto_Id = productos.Producto_Id;
                            if (producto.Diferencia_Costo > 0) { }

                            gridProducto2.RefreshDataSource();
                        }
                    }
                }

            }
        }

        private void btnGrabar2_Click(object sender, EventArgs e)
        {
            try
            {
                if (_cfds == null)
                    return;

                if (cboTipoMovimiento2.EditValue == null || Convert.ToInt64(cboTipoMovimiento2.EditValue) == 0)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione el tipo de movimiento de la factura.", this.ProductName, MessageBoxButtons.OK);
                    cboTipoMovimiento2.Focus();
                    return;
                }

                Provedor provedor = new Provedor();
                provedor.RFC = _cfds.RFC_Emisor;
                if (!provedor.Cargar().Result)
                {
                    provedor.Nombre = _cfds.Nombre_Emisor;
                    provedor.Grabar();
                    //OnRaiseActualizarCatalogoEvent(CLASE.Provedor);
                }

                if (cboSucursales2.EditValue == null || Convert.ToString(cboSucursales2.EditValue) == "-1")
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione la sucursal para ingresar la factura.", this.ProductName, MessageBoxButtons.OK);
                    cboSucursales2.Focus();
                    return;
                }

                //if (cboEmpresa2.EditValue == null || Convert.ToString(cboEmpresa2.EditValue) == "-1")
                //{
                //    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione la empresa.", this.ProductName, MessageBoxButtons.OK);
                //    cboEmpresa2.Focus();
                //    return;
                //}

                if (provedor.Provedor_Id > 0)
                {
                    int length = gridViewProducto2.RowCount;
                    bool oneTime = true;
                    for (int i = 0; i < length; i++)
                    {
                        //foreach (var producto in gridViewProducto2.GetRow//_cfds.Productos)
                        {
                            CFDS_Producto producto = (CFDS_Producto)gridViewProducto2.GetRow(i);
                            if (producto.Agregar)
                            {
                                if (producto.Producto_Id > 0)
                                {
                                    if (producto.Numero_Identificacion.Trim().Length > 0)
                                    {
                                        Diccionario diccionario = new Diccionario();
                                        diccionario.Producto_Id = producto.Producto_Id;
                                        diccionario.Provedor_Id = provedor.Provedor_Id;
                                        diccionario.Valor = producto.Numero_Identificacion;
                                        diccionario.Grabar();
                                    }

                                    if (producto.Descripcion.Trim().Length > 0)
                                    {
                                        Diccionario diccionario = new Diccionario();
                                        diccionario.Producto_Id = producto.Producto_Id;
                                        diccionario.Provedor_Id = provedor.Provedor_Id;
                                        diccionario.Valor = producto.Descripcion;
                                        diccionario.Grabar();
                                    }

                                    if (producto.Codigo_Producto.Trim().Length > 0)
                                    {
                                        Diccionario diccionario = new Diccionario();
                                        diccionario.Producto_Id = producto.Producto_Id;
                                        diccionario.Provedor_Id = provedor.Provedor_Id;
                                        diccionario.Valor = producto.Codigo_Producto;
                                        diccionario.Grabar();
                                    }

                                    Productos prod = new Productos();
                                    prod.Producto_Id = producto.Producto_Id;
                                    prod.Unidad_de_Medida = producto.Unidad;
                                    prod.FolioFiscal = _cfds.Folio_Fiscal;
                                    if (prod.Cargar().Result)
                                    {
                                        prod.lstSeries = producto.lstSeries;
                                        if (prod.Codigo_de_Barras.Length == 0 && producto.Numero_Identificacion.Length > 0)
                                        {
                                            prod.Codigo_Producto = producto.Codigo_Producto;
                                            prod.Codigo_de_Barras = producto.Numero_Identificacion;
                                            prod.Detalle.Codigo_de_Barras = prod.Codigo_de_Barras;
                                        }
                                    }
                                    prod.Detalle.Costo_Proveedor = producto.Valor_Unitario;
                                    if (producto.Precio_Sugerido > 0)
                                    {
                                        prod.Detalle.Precio_General = producto.Precio_Sugerido;
                                    }
                                    else
                                    {
                                        prod.Detalle.Precio_General = Math.Round(prod.Detalle.Costo_Proveedor * 2);
                                    }
                                    prod.Detalle.Cantidad = producto.Cantidad;
                                    prod.IdDatosFiscales = 0;//Convert.ToInt32( cboEmpresa2.EditValue);

                                    //DialogResult result = MessageBox.Show("Desea imprimir las etiquetas de los productos?", "Imprimir Etiquetas", MessageBoxButtons.YesNoCancel);
                                    //if (result == DialogResult.Yes)
                                    //{

                                    //    //Etiqueta aqui
                                    //    string input = prod.Descripcion;
                                    //    string strPrecio = "$" + prod.Detalle.Precio_General;
                                    //    string strLinea1 = "";//"MESA DE CENTRO- SET DE 2";
                                    //    string strLinea2 = "";//"PIEZAS / CUBIERTA DE CRI";
                                    //    string strLinea3 = "";//"STAL";
                                    //    string strCodigoBarra = prod.Codigo_de_Barras;

                                    //    if (input.Length >= 50)
                                    //    {
                                    //        strLinea1 = input.Substring(0, 24);
                                    //        strLinea2 = input.Substring(25, 24);
                                    //        strLinea3 = input.Substring(50, input.Length - 50);
                                    //    }
                                    //    else if (input.Length <= 50)
                                    //    {
                                    //        if (input.Length <= 25)
                                    //        {
                                    //            strLinea1 = input.Substring(0, input.Length);
                                    //        }
                                    //        else
                                    //        {
                                    //            strLinea1 = input.Substring(0, 24);
                                    //            strLinea2 = input.Substring(25, input.Length - 25);
                                    //        }
                                    //    }

                                    //    for (int n = 1; n <= prod.Detalle.Cantidad; n++)
                                    //    {
                                    //        string crlf = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
                                    //        string command =
                                    //            "I8,A,001" + crlf +
                                    //            "" + crlf +
                                    //            "" + crlf +
                                    //            "Q203,024" + crlf +
                                    //            "q831" + crlf +
                                    //            "rN" + crlf +
                                    //            "S3" + crlf +
                                    //            "D7" + crlf +
                                    //            "ZT" + crlf +
                                    //            "JF" + crlf +
                                    //            "OC1,S" + crlf +
                                    //            "R212,0" + crlf +
                                    //            "f100" + crlf +
                                    //            "N" + crlf +
                                    //            "B9,81,0,1,2,6,25,B," + Convert.ToChar(34).ToString() + strCodigoBarra.Trim() + Convert.ToChar(34).ToString() + crlf +
                                    //            "A6,136,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea1.Trim() + Convert.ToChar(34).ToString() + crlf +
                                    //            "A6,158,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea2.Trim() + Convert.ToChar(34).ToString() + crlf +
                                    //            "A6,180,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea3.Trim() + Convert.ToChar(34).ToString() + crlf +
                                    //            "A239,87,0,1,2,2,N," + Convert.ToChar(34).ToString() + prod.Detalle.Precio_General.ToString("C0") + Convert.ToChar(34).ToString() + crlf +
                                    //            "A9,38,0,2,2,2,R," + Convert.ToChar(34).ToString() + "SPAZIO MAZATLAN" + Convert.ToChar(34).ToString() + crlf +
                                    //            "P1" + crlf;

                                    //        Byte[] buffer = new byte[command.Length];
                                    //        try
                                    //        {
                                    //            buffer = System.Text.Encoding.ASCII.GetBytes(command);
                                    //        }
                                    //        catch (Exception ex)
                                    //        {
                                    //            string message = ex.Message;
                                    //        }

                                    //        // Initialize unmanged memory to hold the array.
                                    //        int size = Marshal.SizeOf(buffer[0]) * buffer.Length;
                                    //        IntPtr pnt = Marshal.AllocHGlobal(size);
                                    //        try
                                    //        {
                                    //            // Copy the array to unmanaged memory.
                                    //            Marshal.Copy(buffer, 0, pnt, buffer.Length);
                                    //        }
                                    //        finally
                                    //        {
                                    //            // Free the unmanaged memory.
                                    //            Marshal.FreeHGlobal(pnt);
                                    //        }

                                    //        if (RawPrinterHelper.SendBytesToPrinter("ZDesigner GC420t (EPL)", pnt, buffer.Length))
                                    //        {

                                    //        }
                                    //        else { MessageBox.Show("Error al imprimir etiqueta"); }
                                    //    }
                                    //}

                                    //prod.Detalle.Grabar();
                                    prod.Grabar();
                                    //if (prod.lstSeries.Count > 0)
                                    //{
                                    //    Producto_Serie serie = null;
                                    //    foreach (string s in prod.lstSeries)
                                    //    {
                                    //        serie = new Producto_Serie(prod.Producto_Id);
                                    //        serie.Fecha_de_Entrada = DateTime.Now;
                                    //        serie.Numero_de_Serie = s;
                                    //        serie.Producto = prod.Descripcion;
                                    //        serie.Status = "VIGENTE";
                                    //        serie.Folio_Fiscal = prod.FolioFiscal;
                                    //        serie.Grabar();
                                    //        serie = null;
                                    //    }
                                    //}
                                }
                                else
                                {
                                    if (times == 0)
                                    {
                                        times++;
                                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor relacione los productos de la factura con los productos locales.", this.ProductName, MessageBoxButtons.OK);
                                        gridProducto2.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        if (oneTime)
                                        {
                                            oneTime = false;
                                            string mensaje = string.Empty;
                                            if (length == 1)
                                            {
                                                mensaje = "El producto de la factura que no esta relacionado con algún producto local.\r\nSe guardará como un producto local nuevo.";
                                            }
                                            else
                                            {
                                                mensaje = "Hay productos de la factura que no estan relacionados con algún producto local.\r\nSe guardarán como un producto local nuevo.";
                                            }
                                            DevExpress.XtraEditors.XtraMessageBox.Show(this, mensaje, this.ProductName, MessageBoxButtons.OK);
                                        }
                                        if (producto.Agregar)
                                        {
                                            Productos prod = new Productos();
                                            prod.Activo = true;
                                            if (producto.Codigo_Producto != null)
                                            {
                                                prod.Codigo_Producto = producto.Codigo_Producto;
                                            }
                                            prod.Producto_Id = producto.Producto_Id;
                                            prod.Descripcion = producto.Descripcion;
                                            prod.Unidad_de_Medida = producto.Unidad;
                                            prod.FolioFiscal = _cfds.Folio_Fiscal;

                                            if (prod.Cargar().Result)
                                            {
                                                prod.lstSeries = producto.lstSeries;
                                                if (prod.Codigo_de_Barras.Length == 0 && producto.Numero_Identificacion.Length > 0)
                                                {
                                                    prod.Codigo_Producto = producto.Codigo_Producto;
                                                    prod.Codigo_de_Barras = producto.Numero_Identificacion;
                                                    prod.Detalle.Codigo_de_Barras = prod.Codigo_de_Barras;
                                                }
                                            }

                                            prod.FolioFiscal = _cfds.Folio_Fiscal;
                                            prod.lstSeries = producto.lstSeries;
                                            prod.Unidad_de_Medida = producto.Unidad;
                                            prod.Ultimo_Costo = producto.Valor_Unitario;
                                            prod.Descripcion = producto.Descripcion;
                                            prod.Unidad_de_Medida = producto.Unidad;
                                            prod.Cantidad_Empaque = producto.Cantidad_Empaque;
                                            prod.Detalle.Cantidad = producto.Cantidad;
                                            prod.Detalle.Costo_Proveedor = producto.Valor_Unitario;
                                            if (producto.Precio_Sugerido > 0)
                                            {
                                                prod.Detalle.Precio_General = producto.Precio_Sugerido;
                                            }
                                            else
                                            {
                                                prod.Detalle.Precio_General = Math.Round(prod.Detalle.Costo_Proveedor * 2);
                                            }
                                            prod.Detalle.Proveedor_Id = provedor.Provedor_Id;
                                            //if (prod.Codigo_de_Barras.Length == 0 && producto.Numero_Identificacion.Length > 0)
                                            //{
                                            //    prod.Codigo_de_Barras = producto.Numero_Identificacion;
                                            //    prod.Detalle.Codigo_de_Barras = prod.Codigo_de_Barras;
                                            //    if (prod.Grabar())
                                            //    {
                                            //        producto.Producto_Id = prod.Producto_Id;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    if (prod.Grabar())
                                            //    {
                                            //        producto.Producto_Id = prod.Producto_Id;
                                            //    }
                                            //}
                                            if (prod.Grabar())
                                            {
                                                producto.Producto_Id = prod.Producto_Id;
                                            }
                                            if (producto.Numero_Identificacion.Trim().Length > 0)
                                            {
                                                Diccionario diccionario = new Diccionario();
                                                diccionario.Producto_Id = producto.Producto_Id;
                                                diccionario.Provedor_Id = provedor.Provedor_Id;
                                                diccionario.Valor = producto.Numero_Identificacion;
                                                diccionario.Grabar();
                                            }

                                            if (producto.Descripcion.Trim().Length > 0)
                                            {
                                                Diccionario diccionario = new Diccionario();
                                                diccionario.Producto_Id = producto.Producto_Id;
                                                diccionario.Provedor_Id = provedor.Provedor_Id;
                                                diccionario.Valor = producto.Descripcion;
                                                diccionario.Grabar();
                                            }

                                            if (producto.Codigo_Producto.Trim().Length > 0)
                                            {
                                                Diccionario diccionario = new Diccionario();
                                                diccionario.Producto_Id = producto.Producto_Id;
                                                diccionario.Provedor_Id = provedor.Provedor_Id;
                                                diccionario.Valor = producto.Codigo_Producto;
                                                diccionario.Grabar();
                                            }

                                            //DialogResult result = MessageBox.Show("Desea imprimir las etiquetas de los productos?", "Imprimir Etiquetas", MessageBoxButtons.YesNoCancel);
                                            //if (result == DialogResult.Yes)
                                            //{
                                            //    //Etiqueta aqui
                                            //    string input = prod.Descripcion;
                                            //    string strPrecio = "$" + prod.Detalle.Precio_General;
                                            //    string strLinea1 = "";//"MESA DE CENTRO- SET DE 2";
                                            //    string strLinea2 = "";//"PIEZAS / CUBIERTA DE CRI";
                                            //    string strLinea3 = "";//"STAL";
                                            //    string strCodigoBarra = prod.Codigo_de_Barras;

                                            //    if (input.Length >= 50)
                                            //    {
                                            //        strLinea1 = input.Substring(0, 24);
                                            //        strLinea2 = input.Substring(25, 24);
                                            //        strLinea3 = input.Substring(50, input.Length - 50);
                                            //    }
                                            //    else if (input.Length <= 50)
                                            //    {
                                            //        if (input.Length <= 25)
                                            //        {
                                            //            strLinea1 = input.Substring(0, input.Length);
                                            //        }
                                            //        else
                                            //        {
                                            //            strLinea1 = input.Substring(0, 24);
                                            //            strLinea2 = input.Substring(25, input.Length - 25);
                                            //        }
                                            //    }

                                            //    for (int n = 1; n <= prod.Detalle.Cantidad; n++)
                                            //    {
                                            //        string crlf = Convert.ToChar(13).ToString() + Convert.ToChar(10).ToString();
                                            //        string command =
                                            //            "I8,A,001" + crlf +
                                            //            "" + crlf +
                                            //            "" + crlf +
                                            //            "Q203,024" + crlf +
                                            //            "q831" + crlf +
                                            //            "rN" + crlf +
                                            //            "S3" + crlf +
                                            //            "D7" + crlf +
                                            //            "ZT" + crlf +
                                            //            "JF" + crlf +
                                            //            "OC1,S" + crlf +
                                            //            "R212,0" + crlf +
                                            //            "f100" + crlf +
                                            //            "N" + crlf +
                                            //            "B9,81,0,1,2,6,25,B," + Convert.ToChar(34).ToString() + strCodigoBarra.Trim() + Convert.ToChar(34).ToString() + crlf +
                                            //            "A6,136,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea1.Trim() + Convert.ToChar(34).ToString() + crlf +
                                            //            "A6,158,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea2.Trim() + Convert.ToChar(34).ToString() + crlf +
                                            //            "A6,180,0,4,1,1,N," + Convert.ToChar(34).ToString() + strLinea3.Trim() + Convert.ToChar(34).ToString() + crlf +
                                            //            "A239,87,0,1,2,2,N," + Convert.ToChar(34).ToString() + prod.Detalle.Precio_General.ToString("C0") + Convert.ToChar(34).ToString() + crlf +
                                            //            "A9,38,0,2,2,2,R," + Convert.ToChar(34).ToString() + "SPAZIO MAZATLAN" + Convert.ToChar(34).ToString() + crlf +
                                            //            "P1" + crlf;

                                            //        Byte[] buffer = new byte[command.Length];
                                            //        try
                                            //        {
                                            //            buffer = System.Text.Encoding.ASCII.GetBytes(command);
                                            //        }
                                            //        catch (Exception ex)
                                            //        {
                                            //            string message = ex.Message;
                                            //        }

                                            //        // Initialize unmanged memory to hold the array.
                                            //        int size = Marshal.SizeOf(buffer[0]) * buffer.Length;
                                            //        IntPtr pnt = Marshal.AllocHGlobal(size);
                                            //        try
                                            //        {
                                            //            // Copy the array to unmanaged memory.
                                            //            Marshal.Copy(buffer, 0, pnt, buffer.Length);
                                            //        }
                                            //        finally
                                            //        {
                                            //            // Free the unmanaged memory.
                                            //            Marshal.FreeHGlobal(pnt);
                                            //        }

                                            //        if (RawPrinterHelper.SendBytesToPrinter("ZDesigner GC420t (EPL)", pnt, buffer.Length))
                                            //        {

                                            //        }
                                            //        else { MessageBox.Show("Error al imprimir etiqueta"); }
                                            //    }
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                _cfds.Tipo_Movimiento_Id = Convert.ToInt64(cboTipoMovimiento2.EditValue ?? 0);
                _cfds.IdSucursal = Convert.ToInt64(cboSucursales2.EditValue ?? 0);
                _cfds.IdDatosFiscales = 0;// Convert.ToInt32(cboEmpresa2.EditValue ?? 0);

                _cfds.Estatus = "PROCESADO";

                if (_cfds.Grabar())
                {
                    Movimientos movimiento = new Movimientos();
                    movimiento.Tipo_Movimiento_Id = _cfds.Tipo_Movimiento_Id;
                    movimiento.Cargar_Mov_CFDI(Convert.ToInt32(_cfds.CFDS_Id), movimiento.Tipo_Movimiento_Id);

                    Movimiento_Detalle detalle = new Movimiento_Detalle();
                    int nMovDetalles = movimiento.Detalles.Count();
                    int[] nDetalleId = new int[nMovDetalles];
                    for (int i = 0; i < nMovDetalles; i++)
                    {
                        nDetalleId[i] = Convert.ToInt32(movimiento.Detalles[0].Movimiento_Detalle_Id);
                        movimiento.Detalles.Remove(movimiento.Detalles[0]);
                    }
                    //int n = 0;
                    //foreach (CFDS_Producto producto in _cfds.Productos)
                    int length = gridViewProducto2.RowCount;
                    for (int i = 0; i < length; i++)
                    {
                        //foreach (var producto in gridViewProducto2.GetRow//_cfds.Productos)
                        CFDS_Producto producto = (CFDS_Producto)gridViewProducto2.GetRow(i);
                        try
                        {
                            detalle.Movimiento_Detalle_Id = nDetalleId[i];
                            detalle.Movimiento_Id = movimiento.Movimiento_Id;
                            detalle.Producto_Id = producto.Producto_Id;
                            detalle.Cantidad = producto.Cantidad;
                            detalle.GrabarDetalles(i);
                            //n++;                            
                        }
                        catch { }
                    }

                    if (movimiento.GrabarMaestro(_cfds.Flete))
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "La información se guardó correctamente.", this.ProductName, MessageBoxButtons.OK);
                        Limpiar2();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar2_Click(object sender, EventArgs e)
        {
            Limpiar2();
        }

        private void txtFolioFiscal_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            try
            {
                string rfcProveder = (cboProvedor.EditValue ?? "").ToString();
                if (string.IsNullOrEmpty(rfcProveder))
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "Por favor seleccione un provedor de la lista para acotar la búsqueda de facturas.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cboProvedor.Focus();
                    return;
                }

                CFDS cfds = new CFDS();

                if (_tipoFactura == TIPO_FACTURA.ENTRADA)
                    cfds.RFC_Emisor = rfcProveder;
                else
                    cfds.RFC_Receptor = rfcProveder;
                cfds.Tipo_Id = (int)_tipoFactura;
                cfds.Estatus = "procesado";
                using (FrmBusqueda busqueda = new FrmBusqueda(cfds.CargarFacturasSeleccionar())
                {
                    Width = 1000,
                    Text = "Facturas",
                    AjustarColumnas = true,
                    ColumnasOcultar = new List<string> { "CampoId", "CampoBusqueda", "Sello", "Certificado", "Folio_Fiscal", "IdSucursal" },
                    ColumnasNoMoneda = new List<string> { "Tasa" }
                })
                {
                    if (busqueda.ShowDialog() == DialogResult.OK)
                    {
                        if (busqueda.FilaDatos != null && _cfds.Cargar((DataRowView)busqueda.FilaDatos))
                        {
                            if (_cfds.Estatus == "PROCESADO")
                            {
                                DevExpress.XtraEditors.XtraMessageBox.Show("La factura ya existe y tiene el estatus de PROCESADO y no se permiten modificaciones. Por favor seleccione otra factura.");
                                Limpiar2();
                                return;
                            }

                            txtFolioFiscal.Text = _cfds.Folio_Fiscal;
                            cboTipoMovimiento2.EditValue = (System.Decimal)_cfds.Tipo_Movimiento_Id;
                            deFechaFactura.DateTime = _cfds.Fecha;
                            txtMonto.EditValue = _cfds.Total;
                            //cboSucursales2.EditValue = cboSucursales2.Properties.get GetKeyValueByDisplayText("USD");
                            cboSucursales2.EditValue = (System.Decimal)Convert.ToInt64(_cfds.IdSucursal);
                            //cboEmpresa2.EditValue = (System.Decimal)Convert.ToInt32(_cfds.IdDatosFiscales);

                            gridProducto2.DataSource = null;

                            gridProducto2.DataSource = _cfds.Productos;
                            List<string> listColumnasOcultar = new List<string>() { "Agregar", "CampoId", "CampoBusqueda", "Diferencia_Costo", "isCheckedCantidad_Empaque", "isCheckedCodigo_Producto", "isCheckedProducto", "isCheckedUltimo_Costo", "isCheckedUbicaciones", "TipoClase", "ValorUnitarioOriginal", "Ubicaciones", "Descuento_Porciento", "Precio_Sugerido", "Impuesto_Monto", "Impuesto_Tasa", "Descuento_Monto" };
                            List<string> listColumnasEditar = new List<string> { "Cantidad_Factura", "Valor_Unitario", "Codigo_Producto" };
                            if (_tipoFactura == TIPO_FACTURA.SALIDA)
                            {
                                listColumnasOcultar.Add("Cantidad_Empaque");
                                listColumnasOcultar.Add("Cantidad");
                            }
                            else
                            {
                                listColumnasEditar.Add("Cantidad_Empaque");
                            }
                            Herramientas.GridViewEditarColumnas(gridViewProducto2, true, true, false, listColumnasOcultar, listColumnasEditar, new List<string> { "Cantidad", "Cantidad_Empaque", "Cantidad_Factura", "Codigo_Producto" });

                            gridViewProducto2.BestFitColumns();
                            RefrescarCantidades2(-1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, ex.Message);
                DevExpress.XtraEditors.XtraMessageBox.Show(this, ex.Message, this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboProvedor_EditValueChanged(object sender, EventArgs e)
        {
            if (!_ignorarValueChange && sender is DevExpress.XtraEditors.LookUpEdit && ((DevExpress.XtraEditors.LookUpEdit)sender).ItemIndex < 0)
            {
                Limpiar2();
            }
        }

        private void gridViewProducto2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            RefrescarCantidades2(e.RowHandle);
        }

        private void gridViewProducto2_RowCellClick(object sender, DevExpress.XtraGrid.Views.Grid.RowCellClickEventArgs e)
        {
            if (Global.AgregarNumeroSerie)
            {
                if (string.Compare(e.Column.Name, "colNumero_Serie") == 0)
                {
                    if (e.RowHandle > -1)
                    {
                        if (gridViewProducto2.FocusedRowHandle != e.RowHandle)
                        {
                            gridViewProducto2.SelectRow(e.RowHandle);
                            gridViewProducto2.FocusedRowHandle = e.RowHandle;
                        }
                        object r = gridViewProducto2.GetRow(gridViewProducto2.FocusedRowHandle);
                        long total = (long)(((CFDS_Producto)r).Cantidad);
                        /*if (((CFDS_Producto)r).lstSeries.Count < total)
                        {
                            FrmNumeroSerie serie = new FrmNumeroSerie((CFDS_Producto)r);
                            if (serie.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                ((CFDS_Producto)r).Numero_Serie = "ASIGNADO";
                                ((CFDS_Producto)r).lstSeries = serie.getSeries();
                                gridViewProducto2.RefreshRow(gridViewProducto2.FocusedRowHandle);
                            }
                        }
                        else
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show(this, "Ya se han agregado todos los Números de Serie del produto " + ((CFDS_Producto)r).Descripcion + ".", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }*/
                    }
                }
            }
        }
        private void RefrescarCantidades2(int index)
        {
            if (_cfds == null)
                return;
            if (index < 0)
            {
                foreach (var producto in _cfds.Productos)
                {
                    producto.Cantidad = (producto.Cantidad_Empaque <= 0 ? 1 : producto.Cantidad_Empaque) * producto.Cantidad_Factura;
                }

                gridProducto2.RefreshDataSource();
            }
            else
            {
                if (gridViewProducto2.FocusedRowHandle != index)
                {
                    gridViewProducto2.SelectRow(index);
                    gridViewProducto2.FocusedRowHandle = index;
                }
                object r = gridViewProducto2.GetRow(gridViewProducto2.FocusedRowHandle);
                ((CFDS_Producto)r).Cantidad = (((CFDS_Producto)r).Cantidad_Empaque <= 0 ? 1 : ((CFDS_Producto)r).Cantidad_Empaque) * ((CFDS_Producto)r).Cantidad_Factura;
                ((CFDS_Producto)r).Valor_Unitario = ((CFDS_Producto)r).ValorUnitarioOriginal / ((CFDS_Producto)r).Cantidad_Empaque;
                gridViewProducto2.RefreshRow(gridViewProducto2.FocusedRowHandle);
            }
        }
        #endregion EntradaInventario


        #region ListadoProductos
        private void btnEditarProducto_Click(object sender, EventArgs e)
        {
            if (gridView1.SelectedRowsCount > 0)
            {
                int rowHandle = gridView1.GetSelectedRows()[0];
                editarProducto(rowHandle);
            }
        }

        private bool editarProducto(int rowHandle)
        {
            ProductoListado producto = (ProductoListado)gridView1.GetRow(rowHandle);
            if (producto != null)
            {
                if (producto.Activo)
                {
                    FrmProducto prod = new FrmProducto(producto);
                    if (prod.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        producto = prod.ObtenerProductoEditado();
                        gridView1.RefreshRow(rowHandle);
                        return true;
                    }
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "No se puede editar el producto porque ha sido dado de baja.\r\nfavor de seleccionar un producto que esté activo.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            return false;
        }
        private void btnEliminarProducto_Click(object sender, EventArgs e)
        {
            if (gridView1.SelectedRowsCount > 0)
            {
                int rowHandle = gridView1.GetSelectedRows()[0];
                eliminarProducto(rowHandle);
            }
        }

        private bool eliminarProducto(int rowHandle)
        {
            ProductoListado producto = (ProductoListado)gridView1.GetRow(rowHandle);
            if (producto != null)
            {
                if (producto.Activo)
                {
                    if (DevExpress.XtraEditors.XtraMessageBox.Show(this, "¿Estás seguro de eliminar el producto " + producto.Descripcion + "?", this.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (producto.Borrar())
                        {
                            //producto.Activo = false;
                            //producto.Estado = "Eliminado";
                            gridView1.DeleteRow(rowHandle);
                            //gridView1.RefreshData();
                            return true;
                        }
                    }
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(this, "El producto " + producto.Descripcion + " ya ha sido dado de baja.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return false;
        }
        private void txtBuscarProducto_TextChanged(object sender, EventArgs e)
        {
            buscarProductos(txtBuscarProducto.Text);
        }
        private void buscarProductos(string search)
        {
            try
            {
                gridProductos.DataSource = null;
                List<SqlParameter> parametros = new List<SqlParameter>();
                if (txtBuscarProducto.Text.Length > 0)
                {
                    parametros.Add(new SqlParameter() { ParameterName = "@P_Texto", Value = txtBuscarProducto.Text.ToLower() });
                }
                else
                {
                    parametros.Add(new SqlParameter() { ParameterName = "@P_Texto", Value = string.Empty });
                }
                parametros.Add(new SqlParameter() { ParameterName = "@P_Status", Value = -1 });
                gridProductos.DataSource = Global.CargarListaGrid(BaseDatos.ejecutarProcedimientoConsultaDataTable("Productos_Consultar_Grid_sp", parametros), "producto");
                gridView1.BestFitColumns();
                List<string> listColumnasOcultar = new List<string>() { "Id", "TipoClase", "CampoId", "CampoBusqueda", "Producto_Id", "Activo", "IdLinea1", "IdLinea2", "IdLinea3", "TieneNumeroSerie", "Proveedor_Id", "Estado", "QueryGrabar", "QueryGrabarCodigo", "QueryConsultar", "QueryBorrar", "Cantidad_Minima", "Cantidad_Maxima", "Precio_Mayoreo", "Cantidad_Mayoreo", "IEPS", "Color", "Cantidad_Empaque", "IVA", "Departamento", "Marca", "Modelo", "Codigo_Producto" };
                Herramientas.GridViewEditarColumnas(gridView1, true, true, false, listColumnasOcultar, new List<string>(), new List<string>() { "Cantidad_Empaque", "Existencia" });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BUSCAR Productos EXCEPTION: " + e.ToString());
            }
        }

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (gridView1.SelectedRowsCount > 0)
                {
                    int rowHandle = gridView1.GetSelectedRows()[0];
                    editarProducto(rowHandle);
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (gridView1.SelectedRowsCount > 0)
                {
                    int rowHandle = gridView1.GetSelectedRows()[0];
                    eliminarProducto(rowHandle);
                }
            }
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            int index = Global.RowIndexClicked(gridView1);
            if (index > -1)
            {
                editarProducto(index);
            }
        }

        #endregion ListadoProductos

        private void btnExportarListadoProductos_Click(object sender, EventArgs e)
        {
            Global.ExportGridTodocument(gridProductos, "pdf");
        }

        private void buscarListadoEntradas()
        {
            try
            {
                gridControl1.DataSource = null;
                List<SqlParameter> parametros = new List<SqlParameter>();
                parametros.Add(new SqlParameter() { ParameterName = "@P_FechaInicio", Value = Convert.ToDateTime(fechaInicioListado.EditValue).ToString("yyyy/MM/dd") });
                parametros.Add(new SqlParameter() { ParameterName = "@P_FechaFin", Value = Convert.ToDateTime(fechaFinListado.EditValue).ToString("yyyy/MM/dd") });
                DataTable tableCFD = BaseDatos.ejecutarProcedimientoConsultaDataTable("Listado_Orden_Entrada_Consultar_sp", parametros);
                parametros.Clear();
                parametros.Add(new SqlParameter() { ParameterName = "@P_FechaInicio", Value = Convert.ToDateTime(fechaInicioListado.EditValue).ToString("yyyy/MM/dd") });
                parametros.Add(new SqlParameter() { ParameterName = "@P_FechaFin", Value = Convert.ToDateTime(fechaFinListado.EditValue).ToString("yyyy/MM/dd") });
                DataTable tableProductos = BaseDatos.ejecutarProcedimientoConsultaDataTable("CFDS_Producto_Detalle_Consultar_sp", parametros);
                DataSet ds = new DataSet();
                ds.Tables.Add(tableCFD);
                ds.Tables.Add(tableProductos);
                ds.Relations.Add("Productos", tableCFD.Columns[0], tableProductos.Columns[1], false);
                gridControl1.DataSource = tableCFD;
                //gridControl1.DataSource = Global.CargarListaGrid(BaseDatos.ejecutarProcedimientoConsultaDataTable("Listado_Orden_Entrada_Consultar_sp", parametros), "ordenentrada");                
                gridView2.BestFitColumns();
                List<string> listColumnasOcultar = new List<string>() { "CFDS_Id", "TipoClase", "CampoId", "CampoBusqueda", "RFC_Receptor", "Nombre_Receptor", "Serie", "Folio", "Numero_Aprobacion", "Sello", "Certificado", "Tipo_Comprobante", "SubTotal", "Descuento", "Impuesto", "Tasa", "Tipo_Id", "Tipo", "Comentario", "IdSucursal", "Flete", "Moneda", "TipoCambio", "IdDatosFiscales" };
                Herramientas.GridViewEditarColumnas(gridView2, true, true, false, listColumnasOcultar, new List<string>(), new List<string>());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BUSCAR entradas EXCEPTION: " + e.ToString());
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            buscarListadoEntradas();
        }

        private void btnExportarPDF_Click(object sender, EventArgs e)
        {
            if (gridControl1.DataSource != null)
            {
                string sFiltro = "Documento PDF (*.pdf)|*.pdf";
                using (SaveFileDialog Dir = new SaveFileDialog() { Filter = sFiltro })
                {
                    if (Dir.ShowDialog() == DialogResult.OK)
                    {
                        gridView2.OptionsBehavior.AutoExpandAllGroups = true;
                        gridControl1.ExportToPdf(Dir.FileNames[0]);
                    }
                }
            }
            else
            {
                DevExpress.XtraEditors.XtraMessageBox.Show("No hay ningún registro a exportar.");
            }
        }

        private void gridControl1_ViewRegistered(object sender, DevExpress.XtraGrid.ViewOperationEventArgs e)
        {
            GridView view = (e.View as GridView);

            if (!view.IsDetailView)
            {
                return;
            }

            List<string> listColumnasOcultar2 = new List<string>() { "CFDS_Producto_Id", "Agregar", "TipoClase", "CampoId", "CampoBusqueda", "CFDS_Id", "Numero_Identificacion", "isCheckedCantidad_Empaque", "ValorUnitarioOriginal", "Numero_Serie", "Producto_Id", "isCheckedCodigo_Producto", "Producto", "Ultimo_Costo", "Diferencia_Costo", "isCheckedUbicaciones", "Ubicaciones", "Descuento_Porciento", "Descuento_Monto", "Impuesto_Tasa", "Impuesto_Monto" };
            List<string> listColumnasNoMoneda = new List<string>() { "Cantidad", "Cantidad_Empaque", "Cantidad_Factura" };
            Herramientas.GridViewEditarColumnas(view, true, true, false, listColumnasOcultar2, new List<string>() { "Cantidad_Empaque", "Cantidad_Factura" }, listColumnasNoMoneda);
            view.ViewCaption = "Listado de Productos";
        }


        private void btnExportarFacturasExcel_Click(object sender, EventArgs e)
        {
            Global.ExportGridTodocument(gridControl1, "excel");
        }

        private void Limpiar5()
        {
            txtDescripcion.Text = string.Empty;
            txtCodigoBarrasProducto.Text = string.Empty;
            txtUnidadMedidaProducto.Text = string.Empty;
            txtPrecioVenta.Numero = 0;
            txtPrecioCompra.Numero = 0;
            txtDescripcion.Focus();
            _producto = null;
        }
        private void btnGrabar5_Click(object sender, EventArgs e)
        {
            try
            {
                _producto = obtieneProductoDeControles();
                if (esProductoValido(_producto))
                {
                    if (_producto.Grabar())
                    {
                        Limpiar5();
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "El producto ha sido agregado correctamente", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DevExpress.XtraEditors.XtraMessageBox.Show(this, "Error al Intentar agregar el producto.", this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(this, string.Format("Error al Intentar agregar el producto. Detalle:{0}", ex.Message),
                    this.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Productos obtieneProductoDeControles()
        {
            Productos producto = new Productos();
            producto.Descripcion = txtDescripcion.Text;
            producto.Codigo_Producto = txtCodigoBarrasProducto.Text;
            producto.Codigo_de_Barras = txtCodigoBarrasProducto.Text;
            producto.Unidad_de_Medida = txtUnidadMedidaProducto.Text;
            producto.Activo = true;
            producto.Detalle.Codigo_de_Barras = producto.Codigo_de_Barras;
            producto.Detalle.Color = "";// ColorProducto.Text;
            producto.Detalle.Costo_Proveedor = producto.Ultimo_Costo;
            producto.Detalle.IEPS = 0;
            producto.Detalle.IVA = 16;

            producto.Detalle.Precio_General = txtPrecioVenta.Numero;
            producto.Detalle.Precio_Mayoreo = 0;
            producto.Detalle.Precio_Compra = txtPrecioCompra.Numero;
            producto.Detalle.Cantidad_Minima = 0;
            producto.Detalle.Cantidad_Maxima = 0;
            producto.Detalle.Cantidad_Mayoreo = 0;
            return producto;
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

                strMensaje += "- El precio de venta del producto es requerido.";

                if (!bFocus)
                {
                    txtPrecioVenta.Focus();
                    bFocus = true;
                }
            }

            if (producto.Detalle.Precio_Compra == 0)
            {
                if (strMensaje != string.Empty) { strMensaje += Environment.NewLine; }

                strMensaje += "- El precio de compra del producto es requerido.";

                if (!bFocus)
                {
                    txtPrecioCompra.Focus();
                    bFocus = true;
                }
            }
            if (strMensaje != string.Empty)
            {
                strMensaje = "El producto no puede ser agregado debido a que: " + Environment.NewLine + Environment.NewLine + strMensaje;
                DevExpress.XtraEditors.XtraMessageBox.Show(this, strMensaje, Global.STR_NOMBRE_SISTEMA, MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
            return true;
        }
        private void btnLimpiar5_Click(object sender, EventArgs e)
        {

        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            Global.moveFocusToNextControl(e.KeyCode);
        }
    }
}