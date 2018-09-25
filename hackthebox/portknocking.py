#!/usr/bin/env python
# Source: http://f4l13n5n0w.github.io/blog/2015/06/21/vulnhub-knock-knock-1-dot-1/
# Modified by PlainText

from socket import *
from itertools import permutations
import time
import sys,os

ip = sys.argv[1]                        #target IP
ports = sys.argv[2]

def Knockports(ports):
        for port in ports:
                try:
                        print "[*] Knocking on port: ", port
                        s2 = socket(AF_INET, SOCK_STREAM)
                        s2.settimeout(0.1)                      # set timeout in 0.1s
                        s2.connect_ex((ip, port))
                        s2.close()
                except Exception, e:
                        print "[-] %s" % e

def main():
        # Array Ports "1 2 3" [1,2,3]
        r = eval(str(ports))
        print "received: ", r

        for comb in permutations(r):            # try all the possibility of 3-ports orders
                print "\n[*] Trying sequence %s" % str(comb)
                Knockports(comb)

                print "\n[*] Ejecutar nmap"
                command = "nmap -p " + sys.argv[3] + " " + sys.argv[1]
                print command
                os.system(command)

        print "[*] Done"


main()