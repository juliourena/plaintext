#!/usr/bin/env python3
# Sample Script to extract cookies offile from FireFox sqlite database 
# Created by PlainText 

import argparse
import sqlite3

def main(dbpath, host, cookie):
	conn = sqlite3.connect(dbpath)
	cursor = conn.cursor()

	if (host == "" and cookie == ""):
		query = "SELECT * FROM moz_cookies"
	elif (host != "" and cookie == ""):
		query = "SELECT * FROM moz_cookies WHERE host LIKE '%{}%'".format(host)
	elif (host == "" and cookie != ""):
		query = "SELECT * FROM moz_cookies WHERE name LIKE '%{}%'".format(cookie)
	elif (host != "" and cookie != ""):
		query = "SELECT * FROM moz_cookies WHERE name LIKE '%{}%' AND host LIKE '%{}%'".format(cookie, host)

	cursor.execute(query)
	records = cursor.fetchall()
	rowCount = len(records) 

	if (rowCount > 0):
		for row in records:
			print(row)
	else:
		print("[-] No cookie found with the selected options.")

	conn.close()

if __name__ == "__main__":
	parser = argparse.ArgumentParser()
	parser.add_argument("--dbpath", "-d", required=True, help="The path to the sqlite cookies database")
	parser.add_argument("--host", "-o", required=False, help="The host for the cookie", default="")
	parser.add_argument("--cookie", "-c", required=False, help="The name of the cookie", default="")
	args = parser.parse_args()
	main(args.dbpath, args.host, args.cookie)
