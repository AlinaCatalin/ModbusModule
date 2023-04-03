
namespace ModbusLib.Interface {
    public interface IObserver {
        void Notify(byte[] message);
    }
}