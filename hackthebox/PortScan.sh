#!/bin/bash

echo "Starting TCP unicornscan" 
masscan -p1-65535 --rate 500 -e tun0 $1 > masscan-tcp.all
echo

echo "**** Display Results****"
echo "****       TCP      ****"
cat masscan-tcp.all |grep tcp| cut -d '/' -f 1 | cut -d' ' -f 4|cut -d" " -f1|awk '{print}' ORS=','
echo

echo "Starting UDP unicornscan"
masscan -pU:1-65535 --rate 500 -e tun0 $1 > masscan-udp.all
echo

echo "****       UDP      ****"
cat masscan-udp.all |grep udp| cut -d '/' -f 1 | cut -d' ' -f 4|cut -d" " -f1|awk '{print}' ORS=','
echo