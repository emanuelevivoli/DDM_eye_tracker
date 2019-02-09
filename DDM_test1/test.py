import sys
import time
MAX_ = 7
i = 0

while i < MAX_:
    input = sys.stdin.readline()
    app = str(input).split(" ")

    time.sleep(3)
    
    print("ACK "+str(app[0].split("\\")[-1])+" "+str(app[1].split("\\")[-1]))
    sys.stdout.flush()

    i = i+1

input = sys.stdin.readline()

time.sleep(3)
    
print("END")
sys.stdout.flush()