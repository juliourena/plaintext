#!/usr/bin/env python3

import argparse
import sqlite3

def main(dbpath, host, cookie):
	conn = sqlite3.connect(dbpath)
	cursor = conn.cursor()

	if (host == ""):
		query = "SELECT * FROM moz_cookies"
	elif (cookie != ""):
		query = "SELECT * FROM moz_cookies WHERE host LIKE '%{}%' AND name = '{}'".format(host, cookie)
	else:
		query = "SELECT * FROM moz_cookies WHERE host LIKE '%{}%'".format(host)

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
	parser.add_argument("--dbpath", "-d", required=True, help="the path to the sqlite cookies database")
	parser.add_argument("--host", "-o", required=False, help="the host for the cookie", default="")
	parser.add_argument("--cookie", "-c", required=False, help="the name of the cookie", default="")
	args = parser.parse_args()
	main(args.dbpath, args.host, args.cookie)
