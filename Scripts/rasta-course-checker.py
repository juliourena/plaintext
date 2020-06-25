#!/usr/bin/python3 
'''
Script to check when ZeroPoint Security RedTeam Ops course is available 
If you are not using O365, change the smtp configuration
Don't forget to set your username and password :) 
Create a cronjob with this script, you may want to do some extra modifications. 
'''


import sys
import requests
import re
import json
from termcolor import colored
from bs4 import BeautifulSoup
import smtplib
import datetime
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

url = 'https://www.zeropointsecurity.co.uk/red-team-ops/store/course'
s = requests.session()
r = s.get(url)
soup = BeautifulSoup(r.text,"lxml")
info = soup("div", {"class": "product-variants"})[0]
dv = info.attrs['data-variants']
courses = json.loads(dv)

mailUser = ''
mailPass = ''
mailNotificationTo = ''

now = datetime.datetime.now()
print ("[+] Current Time: " + now.strftime("%Y-%m-%d %H:%M:%S") + "\n")

try:
	for course in courses:
		print("[+] Package information: ")
		print("[+] Name: " + course["attributes"]["Package"])
		print("[+] Price: " + course["priceMoney"]["currency"] + " " + course["priceMoney"]["value"])
		if (course["qtyInStock"] == "0"):
			print("[+] Available: ",colored('No :(','red'))
		else:
			print("[+] Available: ",colored('YES!!! run!! buy it!!','green'))

			msg = MIMEMultipart("alternative")
			msg['Subject'] = "Red Team Ops " + course["attributes"]["Package"] + " Available!!"
			msg['From'] = mailUser
			msg['To'] = mailNotificationTo
			text = """
Red Team Ops course is not available!! 
Purchase it here:https://www.zeropointsecurity.co.uk/red-team-ops/store/course
Hurry up!!!"""
			
			body = MIMEText(text, 'plain')
			msg.attach(body)

			mailserver = smtplib.SMTP('smtp.office365.com',587)
			mailserver.ehlo()
			mailserver.starttls()
			mailserver.login(mailUser, mailPass)
			mailserver.sendmail(mailUser, mailNotificationTo, msg.as_string())
			print(colored("[+] Mail Sent",'green'))

		print("")
except Exception as e:
	print(colored("[-] Error :",'red'), e)
