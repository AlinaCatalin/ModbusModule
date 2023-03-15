using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.Versioning;

namespace ModbusLib
{
    [SupportedOSPlatform("windows")]
    public class KoberSerial
    {
        /// <summary>
        /// Reference to the serial object. It will be preconfigurated
        /// </summary>
        private readonly SerialPort _serial;

        /// <summary>
        /// Buffer for reading and completing data
        /// </summary>
        private readonly byte[] _builder;

        /// <summary>
        /// 
        /// </summary>
        private bool _mutex;

        /// <summary>
        /// Constructor motherfucker
        /// </summary>
        /// <param name="port">SerialPort object </param>
        public KoberSerial(SerialPort port)
        {
            _builder = new byte[0];
            _mutex = false;
            _serial = port;
            _serial.DataReceived += (sender, args) =>
            {
                var local = new byte[_serial.BytesToRead];
                _serial.Read(local, 0, local.Length);

                //Debug.Write("\n");
                _mutex = false;
            };

            _serial.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Write(byte[] message)
        {
            if (!_mutex)
            {
                _serial.Write(message, 0, message.Length);
                _mutex = true;
            }
        }

        /// <summary>
        /// Getter
        /// </summary>
        /// <returns>Message from serial</returns>
        public string GetMessage()
        {
            return _builder.ToString();
        }
    }
}
