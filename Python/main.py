import sys
import cv2
import numpy

import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'  # or any {'0', '1', '2'}
import tensorflow

import utils

if len(sys.argv) <= 1:
    print("_Need at least one argument : the image path")
    pass

img = cv2.imread(sys.argv[1])
threshold = utils.preprocess(img)
contours, hierarchy = cv2.findContours(threshold, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
biggest, max_area = utils.biggest_contour(contours)

if biggest.size == 0:
    print("_No valid area found")
    pass

biggest = utils.reorder(biggest)
pts1 = numpy.float32(biggest)
pts2 = numpy.float32([[0, 0], [450, 0], [0, 450], [450, 450]])
matrix = cv2.getPerspectiveTransform(pts1, pts2)
warp = cv2.warpPerspective(img, matrix, (450, 450))
warp = cv2.cvtColor(warp, cv2.COLOR_BGR2GRAY)

boxes = utils.split_boxes(warp)
model = tensorflow.keras.models.load_model("C:\\Users\\Zach\\Desktop\\Perso\\Numeristiq\\Python\\model.h5")  # TODO better model and not harcoded path

result = ''
for box in boxes:
    result += str(utils.predict(model, box))
print(result, end="")
