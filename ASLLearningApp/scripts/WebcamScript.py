#import needed libraries
import cv2 #library for webcam
import mediapipe as mp #library that implemented mediapipe, what allows the program to recognize the gestures through our model
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import threading #library for threading
import win32pipe #libraries for interacting with the c# application
import win32file
import os
import time

class GestureRecognition:
    def main(self):
        BaseOptions = mp.tasks.BaseOptions #Creating variables for mediapipe tasks for use in creating settings for recognition
        GestureRecognizer = mp.tasks.vision.GestureRecognizer
        GestureRecognizerOptions = mp.tasks.vision.GestureRecognizerOptions
        VisionRunningMode = mp.tasks.vision.RunningMode
        current_dir = os.path.dirname(__file__) #setting current_dir to this file's current directory
        model_path = current_dir+'\\asl_gesture_recognizer.task' #getting the model path so the program knows where the model is for use

        self.lock = threading.Lock() #creating a threading lock so we can retrieve the gesture being recognized
        self.currentGesture = [] #creating an empty array 
        options = GestureRecognizerOptions( #setting options for gesture recognition 
            base_options=BaseOptions(model_asset_path=model_path),
            running_mode=VisionRunningMode.LIVE_STREAM,
            result_callback=self.__result_callback)
        recognizer = GestureRecognizer.create_from_options(options)

        video = cv2.VideoCapture(0) #creating a video object using cv2
        pipe = "NeedNewPipe" #setting string variable for checking
        while (True): #while loop to indefinitely run
            with open(current_dir+"\\MultithreadController.txt", "r") as file: #block to read MultithreadController
                line = file.readline() #reading the file line
                if line == "End": ##checking if the file has 'End' and breaking if so
                    break
            if (pipe == "NeedNewPipe"): #checking the data of pipe variable
                try:
                    pipe_name = r'\\.\pipe\GesturePipe' #creating a pipe to interact with the c# program
                    pipe = win32file.CreateFile(pipe_name, win32file.GENERIC_WRITE, 0, None, win32file.OPEN_EXISTING, 0, None)
                except: #exception when there is no pipe
                    print("No pipe")
            frameGrab, frame = video.read() #grabbing the frame of the webcam video. frameGrab is a boolean to see if a frame was grabbed
            if not frameGrab: #breaking if a frame was not grabbed
                break
            flipped_frame = cv2.flip(frame, 1) #getting the flipped frame for mediapipe
            timestamp = video.get(cv2.CAP_PROP_POS_MSEC) #getting timestamp of the frame received 
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=flipped_frame) #creating the frame in a format that mediapipe wants
            recognizer.recognize_async(mp_image, int(timestamp)) #running the recognizer 
            pipe = self.frame_gesture(frame, pipe) #setting the pipe as the gesture and confidence level
            try:
                #block to keep the webcam window always on top to stop from disappearing
                window_name = "Gesture Interpreter"
                cv2.namedWindow(window_name, cv2.WINDOW_NORMAL)
                cv2.setWindowProperty(window_name, cv2.WND_PROP_TOPMOST, 1)
            except:
                print("Some error occured with the window properties...")
            cv2.imshow("Gesture Interpreter", frame) #showing the webcam with a name and the frame received
            if cv2.waitKey(1) & 0xFF == ord('q'): #code for the user the webcam to stay open. User can press 'q' to manually end the webcam, if needed.
                break
        if pipe != "NeedNewPipe": #checking pipe to see if it should close
            pipe.close()
        video.release() #releasing the video
        # Free the IPC on the C# end
        try:
            pipe_name = r'\\.\pipe\GesturePipe'
            pipe = win32file.CreateFile(pipe_name, win32file.GENERIC_WRITE, 0, None, win32file.OPEN_EXISTING, 0, None)
            win32file.WriteFile(pipe, bytes("*", 'utf-8'))  # Send the recognized gesture
        except:
            print("No pipe")


    def frame_gesture(self, frame, pipe): #method to acquire and send the recognized gesture to c#
        self.lock.acquire() #acquiring the threading lock
        gestures = self.currentGesture #creating another array copy of currentGesture
        self.lock.release() #releasing the threading lock
        for name in gestures: #loop for each name
            if pipe != "NeedNewPipe": #if to check pipe status
                win32file.WriteFile(pipe, bytes(name[0], 'utf-8'))  # Send the recognized gesture
                pipe.close() #close the pipe
                pipe = "NeedNewPipe" #setting pipe 

            #code to add name and confidence level on the frame, taken away in final release but here for debugging purposes
            #cv2.putText(frame, name, (225, 400), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 5)
            #cv2.putText(frame, name, (225, 400), cv2.FONT_HERSHEY_SIMPLEX, 1, (255, 255, 255), 2)
        return pipe

    def __result_callback(self, result, output_image: mp.Image, timestamp_ms: int): #method to get the information on the gestures
        self.lock.acquire() #acquiring thread lock
        self.currentGesture = [] #creating blank array
        for hand in result.handedness: #for each hand in the results
            #print("Hand:") #print statement
            handName = hand[0].category_name #print the name of the hand, this will be either Left or Right
            #print(handName)
        for hand_gesture in result.gestures: #for each hand gesture in the result
            #print("Gestures:")
            gestureName = hand_gesture[0].category_name #gesture name is the new category name
            gesturePercent = str(format(f"{hand_gesture[0].score:.2%}")) # gesturePercent is the score for the hand_gesture
            #print(gestureName)
            # symbol = gestureName[0]
            self.currentGesture.append(gestureName + " " + gesturePercent) #appending the gesture recognized and the percent confidence into currentGesture
        self.lock.release() #releasing the lock
        # print('gesture recognition result: {}'.format(result))

def runScriptFromCS(): #method to run the script, used in C# program
    rec = GestureRecognition()
    rec.main()