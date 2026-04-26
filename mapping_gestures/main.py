import cv2
import mediapipe as mp
import urllib.request
import os
import time
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

    HOLD_SECONDS   = 0.45   # static gesture must be held this long before firing
    SWIPE_COOLDOWN = 1.5    # silence everything after a swipe fires (hand-drop window)

    stable_gesture = None
    stable_since   = 0.0
    hold_fired     = False
    cooldown_until = 0.0    # time.time() value after which gestures are accepted again

    with mp.tasks.vision.HandLandmarker.create_from_options(options) as detector:
        while cap.isOpened():
            ret, frame = cap.read()
            if not ret:
                break

            rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb)
            result = detector.detect(mp_image)

            gesture = None
            if result.hand_landmarks:
                h, w = frame.shape[:2]
                for i, hand in enumerate(result.hand_landmarks):
                    for lm in hand:
                        cx, cy = int(lm.x * w), int(lm.y * h)
                        cv2.circle(frame, (cx, cy), 5, (0, 255, 0), -1)

                    gesture = classifier.classify(hand, hand_index=i)
                    if gesture:
                        wrist = hand[0]
                        label_pos = (int(wrist.x * w), int(wrist.y * h) - 20)
                        cv2.putText(frame, gesture, label_pos,
                                    cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 200, 255), 2)

            now = time.time()
            h, w = frame.shape[:2]
            in_cooldown = now < cooldown_until

            if in_cooldown:
                # keep resetting hold state so no time pre-accumulates for the
                # shapes the hand passes through while being lowered after a swipe
                if gesture != stable_gesture:
                    stable_gesture = gesture
                stable_since = now
                hold_fired   = False
            elif gesture and gesture.startswith("swipe_"):
                pipe.send(gesture)
                cooldown_until = now + SWIPE_COOLDOWN
                stable_gesture = None
                hold_fired     = False
            else:
                if gesture != stable_gesture:
                    stable_gesture = gesture
                    stable_since   = now
                    hold_fired     = False
                elif gesture and not hold_fired and (now - stable_since) >= HOLD_SECONDS:
                    pipe.send(gesture)
                    hold_fired = True

            # visual feedback bar at the bottom of the camera window:
            #  green  = charging toward static-gesture fire
            #  blue   = swipe cooldown draining
            if in_cooldown:
                remaining = (cooldown_until - now) / SWIPE_COOLDOWN
                bar_w = int(w * remaining)
                cv2.rectangle(frame, (0, h - 8), (w, h),      (40, 40, 40),    -1)
                cv2.rectangle(frame, (0, h - 8), (bar_w, h),  (180, 100, 0),   -1)
            elif stable_gesture and not hold_fired:
                progress = min((now - stable_since) / HOLD_SECONDS, 1.0)
                bar_w = int(w * progress)
                cv2.rectangle(frame, (0, h - 8), (w, h),      (40, 40, 40),    -1)
                cv2.rectangle(frame, (0, h - 8), (bar_w, h),  (0, 200, 100),   -1)

            cv2.imshow("hand tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

    cap.release()
    cv2.destroyAllWindows()
    pipe.close()

if __name__ == "__main__":
    main()
