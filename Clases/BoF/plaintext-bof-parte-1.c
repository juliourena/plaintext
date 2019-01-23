//vuln.c
/*
Publicado por Julio Ureña (PlainText)
Twitter: @JulioUrena
Video: https://youtu.be/7KZ5LCFr6Sw

Install 32 bits libraries for 64 bit system
apt-get install gcc-multilib

Compilation Commands
#echo 0 > /proc/sys/kernel/randomize_va_space

$gcc -g -fno-stack-protector -z execstack -o vuln vuln.c -m32
sudo chown root vuln2
sudo chgrp root vuln2
sudo chmod +s vuln2
*/

#include <stdio.h>
#include <string.h>

int vuln(char *str)
{
	char buf[64];
    strcpy(buf,str);
   	printf("Input:%s\n",buf);
    return 0;
}
int main(int argc, char* argv[]) 
{
	setuid(0);
	vuln(argv[1]);
}