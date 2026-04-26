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
    # detects left/right/up/down swipe by watching wrist position over a short history.

    def __init__(self, history_len=12, threshold=0.15):
        self.history_x = deque(maxlen=history_len)
        self.history_y = deque(maxlen=history_len)
        self.threshold = threshold  # fraction of frame dimension

    def update(self, hand):
        self.history_x.append(hand[WRIST].x)
        self.history_y.append(hand[WRIST].y)

        if len(self.history_x) < self.history_x.maxlen:
            return None

        dx = self.history_x[-1] - self.history_x[0]
        dy = self.history_y[-1] - self.history_y[0]

        # Only fire when one axis clearly dominates the other
        if abs(dx) < self.threshold and abs(dy) < self.threshold:
            return None

        if abs(dx) >= abs(dy):
            # Horizontal swipe dominates
            # x=0 left edge, x=1 right edge (mirrored frame)
            # moving hand left  → x decreases → delta negative
            # moving hand right → x increases → delta positive
            if dx < -self.threshold:
                self.history_x.clear(); self.history_y.clear()
                return "swipe_left"
            if dx > self.threshold:
                self.history_x.clear(); self.history_y.clear()
                return "swipe_right"
        else:
            # vertical swipe dominates
            # y=0 top edge y=1 bottom edge
            # moving hand up   → y decreases → delta negative
            # moving hand down → y increases → delta positive
            if dy < -self.threshold:
                self.history_x.clear(); self.history_y.clear()
                return "swipe_up"
            if dy > self.threshold:
                self.history_x.clear(); self.history_y.clear()
                return "swipe_down"
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
