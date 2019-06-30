using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RecyclameV2.Clases;

namespace RecyclameV2.Utils
{
    public partial class VisorArchivo : DevExpress.XtraEditors.XtraUserControl
    {
        private DevExpress.XtraTab.XtraTabControl tabControl;
        private DevExpress.XtraTab.XtraTabPage tabPagePDF;
        private WebBrowser pdfViewer;
        private DevExpress.XtraTab.XtraTabPage tabPageXML;
        private WebBrowser xmlViewer;
        private DevExpress.XtraTab.XtraTabPage tabPageImagen;
        private DevExpress.XtraEditors.PictureEdit imageViewer;

        public string PDFRutaArchivo { get; set; }
        public string PDFNombreArchivo { get; set; }
        public string PDFArchivo64 { get; set; }

        public string XMLRutaArchivo { get; set; }
        public string XMLNombreArchivo { get; set; }
        public string XMLArchivo64 { get; set; }

        public string IMGRutaArchivo { get; set; }
        public string IMGNombreArchivo { get; set; }
        public string IMGArchivo64 { get; set; }

        public VisorArchivo()
        {
            InitializeComponent();
            Limpiar();
        }

        public TIPO_ARCHIVO Cargar(string nombreArchivo, string Archivo64)
        {
            TIPO_ARCHIVO resultado = TIPO_ARCHIVO.NO_SOPORTADO;
            string rutaArchivo = nombreArchivo;
            byte[] _fileData = Convert.FromBase64String(Archivo64);
         
            File.WriteAllBytes(rutaArchivo, _fileData);
            resultado = Cargar(rutaArchivo);

            return resultado;
        }

        public TIPO_ARCHIVO Cargar(string rutaArchivo)
        {
            TIPO_ARCHIVO resultado = TIPO_ARCHIVO.NO_SOPORTADO;
            string extension = "";

            string Archivo64 = "";
            FileInfo info = new FileInfo(rutaArchivo);
            extension = info.Extension;
            if (info.Length < (Int32.MaxValue / 2))
            {
                byte[] _fileData = new byte[info.Length];
                using (FileStream stream = info.OpenRead())
                {
                    stream.Read(_fileData, 0, (int)info.Length);
                    Archivo64 = Convert.ToBase64String(_fileData);
                    stream.Close();
                }
            }
            else
                throw new OverflowException("El tamaño del archivo excede el permitido.");


            try
            {
                tabControl.SelectedTabPage = tabPageImagen;
                Image newImage = Image.FromFile(info.FullName);
                imageViewer.Image = newImage;
                IMGArchivo64 = Archivo64;
                IMGRutaArchivo = rutaArchivo;
                IMGNombreArchivo = Path.GetFileName(rutaArchivo);
                resultado = TIPO_ARCHIVO.IMAGEN;
            }
            catch (OutOfMemoryException)
            {
                if (extension.ToUpper() == ".PDF")
                {
                    tabControl.SelectedTabPage = tabPagePDF;
                    //pdfViewer.LoadDocument(rutaArchivo);
                    pdfViewer.Navigate(info.FullName);
                    PDFArchivo64 = Archivo64;
                    PDFRutaArchivo = rutaArchivo;
                    PDFNombreArchivo = Path.GetFileName(rutaArchivo);
                    resultado = TIPO_ARCHIVO.PDF;
                }
                else if (extension.ToUpper() == ".XML")
                {                    
                    tabControl.SelectedTabPage = tabPageXML;
                    xmlViewer.Navigate(info.FullName);
                    XMLArchivo64 = Archivo64;
                    XMLRutaArchivo = rutaArchivo;
                    XMLNombreArchivo = Path.GetFileName(rutaArchivo);
                    resultado = TIPO_ARCHIVO.XML;
                }
                else
                {
                    Limpiar();
                }
            }

            return resultado;
        }

        public void Limpiar()
        {
            PDFArchivo64 = "";
            PDFNombreArchivo = "";
            PDFRutaArchivo = "";
            XMLArchivo64 = "";
            XMLNombreArchivo = "";
            XMLRutaArchivo = "";
            IMGArchivo64 = "";
            IMGNombreArchivo = "";
            IMGRutaArchivo = "";
            //pdfViewer.DocumentFilePath = null;
            pdfViewer.Navigate("about:blank");
            xmlViewer.Navigate("about:blank");
            imageViewer.Image = null;
        }

        private void InitializeComponent()
        {
            this.tabControl = new DevExpress.XtraTab.XtraTabControl();
            this.tabPagePDF = new DevExpress.XtraTab.XtraTabPage();
            this.pdfViewer = new System.Windows.Forms.WebBrowser();
            this.tabPageXML = new DevExpress.XtraTab.XtraTabPage();
            this.xmlViewer = new System.Windows.Forms.WebBrowser();
            this.tabPageImagen = new DevExpress.XtraTab.XtraTabPage();
            this.imageViewer = new DevExpress.XtraEditors.PictureEdit();
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabPagePDF.SuspendLayout();
            this.tabPageXML.SuspendLayout();
            this.tabPageImagen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageViewer.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.HeaderLocation = DevExpress.XtraTab.TabHeaderLocation.Right;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedTabPage = this.tabPagePDF;
            this.tabControl.Size = new System.Drawing.Size(523, 290);
            this.tabControl.TabIndex = 6;
            this.tabControl.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabPageXML,
            this.tabPagePDF,
            this.tabPageImagen});
            // 
            // tabPagePDF
            // 
            this.tabPagePDF.Controls.Add(this.pdfViewer);
            this.tabPagePDF.Margin = new System.Windows.Forms.Padding(0);
            this.tabPagePDF.Name = "tabPagePDF";
            this.tabPagePDF.Size = new System.Drawing.Size(494, 284);
            this.tabPagePDF.Text = "PDF";
            // 
            // pdfViewer
            // 
            this.pdfViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pdfViewer.Location = new System.Drawing.Point(0, 0);
            this.pdfViewer.Margin = new System.Windows.Forms.Padding(0);
            this.pdfViewer.MinimumSize = new System.Drawing.Size(20, 20);
            this.pdfViewer.Name = "pdfViewer";
            this.pdfViewer.Size = new System.Drawing.Size(494, 284);
            this.pdfViewer.TabIndex = 4;
            // 
            // tabPageXML
            // 
            this.tabPageXML.Controls.Add(this.xmlViewer);
            this.tabPageXML.Name = "tabPageXML";
            this.tabPageXML.Size = new System.Drawing.Size(494, 284);
            this.tabPageXML.Text = "XML";
            // 
            // xmlViewer
            // 
            this.xmlViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xmlViewer.Location = new System.Drawing.Point(0, 0);
            this.xmlViewer.Margin = new System.Windows.Forms.Padding(0);
            this.xmlViewer.MinimumSize = new System.Drawing.Size(20, 20);
            this.xmlViewer.Name = "xmlViewer";
            this.xmlViewer.Size = new System.Drawing.Size(494, 284);
            this.xmlViewer.TabIndex = 3;
            // 
            // tabPageImagen
            // 
            this.tabPageImagen.Controls.Add(this.imageViewer);
            this.tabPageImagen.Name = "tabPageImagen";
            this.tabPageImagen.Size = new System.Drawing.Size(494, 284);
            this.tabPageImagen.Text = "Imagen";
            // 
            // imageViewer
            // 
            this.imageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewer.Location = new System.Drawing.Point(0, 0);
            this.imageViewer.Name = "imageViewer";
            this.imageViewer.Properties.ShowScrollBars = true;
            this.imageViewer.Properties.ShowZoomSubMenu = DevExpress.Utils.DefaultBoolean.True;
            this.imageViewer.Size = new System.Drawing.Size(494, 284);
            this.imageViewer.TabIndex = 1;
            // 
            // VisorArchivo
            // 
            this.Controls.Add(this.tabControl);
            this.Name = "VisorArchivo";
            this.Size = new System.Drawing.Size(523, 290);
            ((System.ComponentModel.ISupportInitialize)(this.tabControl)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabPagePDF.ResumeLayout(false);
            this.tabPageXML.ResumeLayout(false);
            this.tabPageImagen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageViewer.Properties)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
