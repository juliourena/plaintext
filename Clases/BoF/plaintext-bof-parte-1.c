/*
Publicado por Julio Ureña (PlainText)
Twitter: @JulioUrena
Video: https://youtu.be/7KZ5LCFr6Sw

Install 32 bits libraries for 64 bit system
apt-get install gcc-multilib

Compilation Commands
#echo 0 > /proc/sys/kernel/randomize_va_space

gcc -g -fno-stack-protector -z execstack -o plaintext-bof-parte-1 plaintext-bof-parte-1.c -m32
sudo chown root plaintext-bof-parte-1
sudo chgrp root plaintext-bof-parte-1
sudo chmod +s plaintext-bof-parte-1
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
	setuid(0);	//Esto permitirá poner el setuid bit para ejecución como root desde otro usuario.
	vuln(argv[1]);
}