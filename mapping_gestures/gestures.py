from collections import deque

# mediapipe hand landmark indices
WRIST = 0
THUMB_TIP = 4
INDEX_TIP, INDEX_PIP = 8, 6
MIDDLE_TIP, MIDDLE_PIP = 12, 10
RING_TIP, RING_PIP = 16, 14
PINKY_TIP, PINKY_PIP = 20, 18


def fingers_extended(hand):
    # returns list of booleans [index, middle, ring, pinky] — True if extended.
    return [
        hand[INDEX_TIP].y  < hand[INDEX_PIP].y,
        hand[MIDDLE_TIP].y < hand[MIDDLE_PIP].y,
        hand[RING_TIP].y   < hand[RING_PIP].y,
        hand[PINKY_TIP].y  < hand[PINKY_PIP].y,
    ]


def classify_static(hand):
    # classify singleframe static gestures
    extended = fingers_extended(hand)
    count = sum(extended)

    if count == 4:
        return "open_hand"
    if count == 0:
        return "fist"
    if extended == [True, False, False, False]:
        return "pointing"
    if extended == [True, True, False, False]:
        return "peace"
    if extended == [False, False, False, True]:
        return "pinky"
    return None


class SwipeDetector:
    # detects left/right swipe by watching wrist x over a short history.

    def __init__(self, history_len=12, threshold=0.15):
        self.history = deque(maxlen=history_len)
        self.threshold = threshold  # fraction of frame width

    def update(self, hand):
        self.history.append(hand[WRIST].x)

        if len(self.history) < self.history.maxlen:
            return None

        delta = self.history[-1] - self.history[0]
        # In mediapipe x coords:
        #  0=left edge
        #  1=right edge of the mirrored frame.
        # moving hand left  → x decreases → delta negative
        # moving hand right → x increases → delta positive
        if delta < -self.threshold:
            self.history.clear()
            return "swipe_left"
        if delta > self.threshold:
            self.history.clear()
            return "swipe_right"
        return None


class GestureClassifier:
    # combines static + swipe detection per hand slot.

    def __init__(self):
        # support up to 2 hands each with its own swipe detector
        self.swipe = [SwipeDetector(), SwipeDetector()]

    def classify(self, hand, hand_index=0):
        # return the most specific gesture label for this hand, or None.
        swipe = self.swipe[hand_index].update(hand)
        if swipe:
            return swipe
        return classify_static(hand)
