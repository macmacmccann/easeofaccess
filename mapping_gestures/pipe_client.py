class PipeClient:
    
    # Writes strings to a Windows named pipe server

    def __init__(self, pipe_name=r'\\.\pipe\gestures'):
        self.pipe_name = pipe_name
        self._pipe = None

    def connect(self):
        # all once the server has created the pipe and is waiting.
        self._pipe = open(self.pipe_name, 'wb', buffering=0)
        print(f" connected to pipe: {self.pipe_name}")

    def send(self, message: str):
        if self._pipe is None:
            return
        self._pipe.write((message + '\n').encode())

    def close(self):
        if self._pipe:
            self._pipe.close()
            self._pipe = None
