using RecyclameV2.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecyclameV2
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        /// 

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [STAThread]
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormRecyclame());*/
            bool bMutexCreado = false;
            Mutex mutex = new Mutex(true, Global.STR_NOMBRE_SISTEMA, out bMutexCreado);
            if (bMutexCreado)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                if (Global.InicioLector)
                {
                    //Application.Run(new FrmInicioSesion());
                }
                else
                {
                    Application.Run(new FrmInicioSesionUsuario());
                }

                mutex.ReleaseMutex();
            }
            else
            {
                int nMensaje = RegisterWindowMessage(Global.MENSAJES.EJECUTANDO);
                PostMessage((IntPtr)0xffff, nMensaje, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
