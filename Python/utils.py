import cv2
import numpy
import numpy as np


def preprocess(img):
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    blur = cv2.GaussianBlur(gray, (5, 5), 1)
    return cv2.adaptiveThreshold(blur, 255, 1, 1, 11, 2)


def biggest_contour(contours):
    biggest = numpy.array([])
    max_area = 0
    for i in contours:
        area = cv2.contourArea(i)
        if area > 30:
            peri = cv2.arcLength(i, True)
            approx = cv2.approxPolyDP(i, 0.02 * peri, True)
            if area > max_area and len(approx) == 4:
                biggest = approx
                max_area = area
    return biggest, max_area


def reorder(points):
    points = points.reshape((4, 2))
    result = numpy.zeros((4, 1, 2), dtype=numpy.int32)
    add = points.sum(1)
    result[0] = points[numpy.argmin(add)]
    result[3] = points[numpy.argmax(add)]
    diff = numpy.diff(points, axis=1)
    result[1] = points[np.argmin(diff)]
    result[2] = points[np.argmax(diff)]
    return result


def split_boxes(img):
    rows = numpy.vsplit(img, 9)
    boxes = []
    for r in rows:
        cols = np.hsplit(r, 9)
        for box in cols:
            boxes.append(box)
    return boxes


def predict(model, number):
    img = cv2.resize(number, (28, 28))  # For cv2.imshow: dimensions should be 28x28
    img = img.reshape(1, 28, 28, 1)

    return numpy.argmax(model.predict(img))
