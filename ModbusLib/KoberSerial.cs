using ModbusLib.Interface;

using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace ModbusLib {

    public class KoberSerial {
        /// <summary>
        /// Reference to the serial object. It will be preconfigurated
        /// </summary>
        private readonly SerialPort _serial;

        /// <summary>
        /// Buffer for reading and completing data
        /// </summary>
        private byte[] _builder;

        private readonly List<IObserver> observers = new();

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="port">SerialPort object </param>
        public KoberSerial(SerialPort port) {
            _builder = new byte[1024];
            _serial = port;
            _serial.DataReceived += (sender, args) => {
                var local = new byte[_serial.BytesToRead];
                _serial.Read(local, 0, local.Length);

                _builder = _builder.Concat(local).ToArray();
                observers.ForEach(item => {
                    item.Notify();
                });
                _builder = new byte[1024];
            };

            _serial.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Write(byte[] message) {
            _serial.Write(message, 0, message.Length);

        }

        /// <summary>
        /// Getter
        /// </summary>
        /// <returns>Message from serial</returns>
        public string GetMessage() {
            return _builder.ToString();
        }

        /// <summary>
        /// receive message
        /// </summary>
        /// <returns></returns>
        public byte[] GetRecv() {
            return _builder;
        }

        /// <summary>
        /// add observer
        /// </summary>
        /// <param name="observer"></param>
        public void AddObserver(IObserver observer) {
            observers.Add(observer);
        }
    }
}