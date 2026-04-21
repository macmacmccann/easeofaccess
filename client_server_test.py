import win32pipe, win32file

pipe = win32pipe.CreateNamedPipe(
    r'\\.\pipe\gestures',
    win32pipe.PIPE_ACCESS_INBOUND,
    win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
    1, 65536, 65536, 0, None
  )
print("simple server waiting for client (which is my other python file)...")
win32pipe.ConnectNamedPipe(pipe, None)
print("client connected!!! Reading gestures now :")
while True:
    _, data = win32file.ReadFile(pipe, 64)
    print(data.decode().strip())

