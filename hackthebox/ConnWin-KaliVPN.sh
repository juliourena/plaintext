#!/bin/bash
# PlainText

# Configurar en Windows 
# route add 10.10.10.0 mask 255.255.255.0 TUIPdeKALI

# Aquí se está usando la interface tun0, en caso que tengas otra modificarla. 
iptables --table nat --append POSTROUTING --out-interface tun0 -j MASQUERADE
echo 1 > /proc/sys/net/ipv4/ip_forward
