import serial


def main():
    try:
        port = "COM8"
        ser = serial.Serial(port, 115200, serial.EIGHTBITS, serial.PARITY_EVEN, serial.STOPBITS_ONE, timeout=0)
        
        print(f"started on {port}:")
        
        while True:
            buffer = ser.read(8)
            if len(buffer) > 0:
                print(f'read from serial: {buffer}')
                ser.write(buffer)
                print(f'wrote to serial')
                
    except KeyboardInterrupt:
        quit(0)


if __name__ == '__main__':
    main()
