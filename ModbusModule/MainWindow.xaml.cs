using ModbusLib;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows;
using System.Timers;

namespace ModbusModule {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private static SerialPort serialPort;

        public MainWindow() {
            InitializeComponent();

            serialPort = new("COM1") {
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                Handshake = Handshake.None
            };

           // KoberSerial kober = new KoberSerial(serialPort);

            new Thread(new ThreadStart(() => {
                for (int i = 0; i < 1; i++) {
                    Thread.Sleep(100);
                    /*var message = ModbusMessage.WriteAOSignal(0x03, 0x00, 0x00);
                    kober.Write(message);*/

                    var msg = ModbusMessage.WriteDouble(0x03, 0x25, (float)123.21);
                   
                    Debug.WriteLine(msg);
                    //PrintByteArray();

                    //kober.GetMessage();
                }
            })).Start();

            //Debug.WriteLine(ModbusMessage.CreateCommand(03, 0x00, 0x05, 0xFF00));

            //Console.ReadKey(); 
        }
        public static void PrintByteArray(byte[] bytes) {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes) {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Debug.WriteLine(sb.ToString());
        }
    }
}
