from imutils import paths
import numpy as np
import imutils
import cv2
import pickle
import requests
import re
from bs4 import BeautifulSoup
import urllib
from keras.models import load_model
from helpers import resize_to_fit
from imutils import paths


MODEL_FILENAME = "captcha_model.hdf5"
MODEL_LABELS_FILENAME = "model_labels.dat"
CAPTCHA_IMAGE_FOLDER = "generated_captcha_images"

# Load up the model labels (so we can translate model predictions to actual letters)
with open(MODEL_LABELS_FILENAME, "rb") as f:
    lb = pickle.load(f)

# Load the trained neural netw ork
model = load_model(MODEL_FILENAME)


url = "http://192.168.26.236/wordpress/index.php/test/"

s = requests.session()
r = s.get(url)



soup = BeautifulSoup(r.text, "lxml")

wpcf7_captcha_challenge_captcha = soup.findAll(attrs={"name" : "_wpcf7_captcha_challenge_captcha-1"})

print(wpcf7_captcha_challenge_captcha[0]["value"])

images_urls = soup.findAll("img")
image_url = ""

for url in images_urls:
    if "wpcf7" in url["src"]:
        print(url["src"])
        image_url = url["src"]
    else:
        None

resp = urllib.request.urlopen(image_url)
image = np.asarray(bytearray(resp.read()), dtype="uint8")
image = cv2.imdecode(image, cv2.IMREAD_COLOR)






#image = url_to_image(image_url)

# Load the image and convert it to grayscale
#image = cv2.imread(image_file)
image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)

# Add some extra padding around the image
image = cv2.copyMakeBorder(image, 20, 20, 20, 20, cv2.BORDER_REPLICATE)

# threshold the image (convert it to pure black and white)
thresh = cv2.threshold(image, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]

# find the contours (continuous blobs of pixels) the image
contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

# Hack for compatibility with different OpenCV versions
contours = contours[1] if imutils.is_cv3() else contours[0]

letter_image_regions = []

# Now we can loop through each of the four contours and extract the letter
# inside of each one
for contour in contours:
    # Get the rectangle that contains the contour
    (x, y, w, h) = cv2.boundingRect(contour)
    # Compare the width and height of the contour to detect letters that
    # are conjoined into one chunk
    if w / h > 1.25:
        # This contour is too wide to be a single letter!
        # Split it in half into two letter regions!
        half_width = int(w / 2)
        letter_image_regions.append((x, y, half_width, h))
        letter_image_regions.append((x + half_width, y, half_width, h))
    else:
        # This is a normal letter by itself
        letter_image_regions.append((x, y, w, h))


# Sort the detected letter images based on the x coordinate to make sure
# we are processing them from left-to-right so we match the right image
# with the right letter
letter_image_regions = sorted(letter_image_regions, key=lambda x: x[0])

# Create an output image and a list to hold our predicted letters
output = cv2.merge([image] * 3)
predictions = []

# loop over the lektters
for letter_bounding_box in letter_image_regions:
    # Grab the coordinates of the letter in the image
    x, y, w, h = letter_bounding_box
    # Extract the letter from the original image with a 2-pixel margin around the edge
    letter_image = image[y - 2:y + h + 2, x - 2:x + w + 2]
    # Re-size the letter image to 20x20 pixels to match training data
    letter_image = resize_to_fit(letter_image, 20, 20)
    # Turn the single image into a 4d list of images to make Keras happy
    letter_image = np.expand_dims(letter_image, axis=2)
    letter_image = np.expand_dims(letter_image, axis=0)
    # Ask the neural network to make a prediction
    prediction = model.predict(letter_image)
    # Convert the one-hot-encoded prediction back to a normal letter
    letter = lb.inverse_transform(prediction)[0]
    predictions.append(letter)
    # draw the prediction on the output image
    cv2.rectangle(output, (x - 2, y - 2), (x + w + 4, y + h + 4), (0, 255, 0), 1)
    cv2.putText(output, letter, (x - 5, y - 5), cv2.FONT_HERSHEY_SIMPLEX, 0.55, (0, 255, 0), 2)

# Print the captcha's text
captcha_text = "".join(predictions)
print("CAPTCHA text is: {}".format(captcha_text))

# Show the annotated image
#cv2.imshow("Output", output)
#cv2.waitKey()

post_url = "http://192.168.26.236/wordpress/index.php/wp-json/contact-form-7/v1/contact-forms/26/feedback"

data = {"_wpcf7":"26","_wpcf7_version":"5.1.4", "_wpcf7_locale":"en_US", "_wpcf7_unit_tag":"wpcf7-f26-p27-o1", "_wpcf7_container_post":"27","_wpcf7_captcha_challenge_captcha-1":wpcf7_captcha_challenge_captcha[0]["value"],"captcha-1":captcha_text.strip()}

import binascii
import os


def encode_multipart_formdata(fields):
    boundary = binascii.hexlify(os.urandom(16)).decode('ascii')
    boundary = "1092830918230981230472324234"
    #boundary = "1232224033827985033549011123"
    body = (
        "".join("-----------------------------%s\r\n"
                "Content-Disposition: form-data; name=\"%s\"\r\n"
                "\r\n"
                "%s\r\n" % (boundary, field, value)
                for field, value in fields.items()) +
        "-----------------------------%s--\r\n" % boundary
    )

    content_type = "multipart/form-data; boundary=---------------------------%s" % boundary

    return body, content_type

#data, header = encode_multipart_formdata(data)

#print("DATA: " + data)
#print("HEADER: " + header)

r = s.post(post_url, data=data)#, headers={"Content-Type":header})

if "Thank you for your message" in r.text:
    print("YOU GOT THIS :)")
else:
    print("YOU FAIL :(")
    
print(r.text)