import smtplib, click
from time import sleep

s = smtplib.SMTP('outlook.office365.com',587)

def AccessOffice(username, password):
    print (s.starttls())

    try:
        s.login(username.strip(),password.strip())
        return True
    except Exception as e: print(e)

def print_help(self, param, value):
	if value is False:
		return
	click.echo(self.get_help())
	self.exit()

@click.command()
@click.option('-u', '--username', 'username', help='Username to login into Office 365.', required=True)
@click.option('-p', '--password', 'password', help='Password to login into Office 365.', required=True)
@click.option('-h', '--help', 'help', help='Help', is_flag=True, callback=print_help, expose_value=False, is_eager=False)
@click.pass_context


def main(self, username, password):
    if not username and not password:
        print_help(self, None,  value=True)
        print("[-] For usage reference use --help")
        exit(0)

    message = """[+] Office 365 SMTP Connection test
[+] Publicado por Julio Ureña (PlainText)
[+] Blog: https://plaintext.do
"""
    print(message)

    if AccessOffice(username, password):
        print("[+] Logged in successfully to Office 365 via SMTP.")
    else:
        print("[-] Unable to login. Conditional Access Enabled or Password is wrong. Check logs: https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/SignIns")
        exit(0)

if __name__ == "__main__":
	main()