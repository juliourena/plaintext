#!/bin/bash
# Script usado para nest, permite descargar todos los archivos de todos los directorios compartidos
# Requiere smbmap y smbclient
# https://github.com/juliourena/plaintext/blob/master/hackthebox/smbgetall.sh

username=$1
password=$2
ip=$3

mkdir $username 
cd $username

for share in $(smbmap -u "$username" -p "$password" -H "$ip" | grep -i READ | cut -d$'\t' -f2)
do
	mkdir $share
	cd $share
	smbclient -U $username%$password //"$ip"/$share -c 'prompt OFF; recurse ON;mget *'
	cd ..
done
