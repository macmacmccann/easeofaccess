import cv2
import mediapipe as mp
import urllib.request
import os
from gestures import GestureClassifier
from pipe_client import PipeClient

MODEL_PATH = "hand_landmarker.task"
MODEL_URL = "https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task"

def download_model():
    if not os.path.exists(MODEL_PATH):
        print("downloading hand landmarker model 8mb...")
        urllib.request.urlretrieve(MODEL_URL, MODEL_PATH)
        print("download succeeded.")

def main():
    download_model()

    options = mp.tasks.vision.HandLandmarkerOptions(
        base_options=mp.tasks.BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=mp.tasks.vision.RunningMode.IMAGE,
        num_hands=2,
    )

    cap = cv2.VideoCapture(0)
    classifier = GestureClassifier()
    pipe = PipeClient()
    pipe.connect()
    last_gesture = None

    with mp.tasks.vision.HandLandmarker.create_from_options(options) as detector:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
            result = detector.detect(mp_image)

            if result.hand_landmarks:
                h, w = frame.shape[:2]
                for i, hand in enumerate(result.hand_landmarks):
                    # draw landmarks
                    for lm in hand:
                        cx, cy = int(lm.x * w), int(lm.y * h)
                        cv2.circle(frame, (cx, cy), 5, (0, 255, 0), -1)

                    gesture = classifier.classify(hand, hand_index=i)
                    if gesture:
                        wrist = hand[0]
                        label_pos = (int(wrist.x * w), int(wrist.y * h) - 20)
                        cv2.putText(frame, gesture, label_pos,
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 200, 255), 2)
                        if gesture != last_gesture:
                            pipe.send(gesture)
                            last_gesture = gesture

            cv2.imshow("hand tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    cap.release()
    cv2.destroyAllWindows()
    pipe.close()

if __name__ == "__main__":
    main()
