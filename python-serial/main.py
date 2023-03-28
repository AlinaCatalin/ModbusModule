import serial


def main():
    print("started on COM8:")
    try:
        ser = serial.Serial('COM8', 115200, serial.EIGHTBITS, serial.PARITY_EVEN, serial.STOPBITS_ONE, timeout=0)
        while True:
            buffer = ser.read(100)
            if len(buffer) > 0:
                print(f'read from serial: {buffer}')
                ser.write(b'cevacevaceva')
                print(f'wrote to serial')
    except KeyboardInterrupt:
        quit(0)


if __name__ == '__main__':
    main()
