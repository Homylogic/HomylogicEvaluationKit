ECHO # Starting generate SSL certicate for homylogic

IF NOT EXIST homylogic.pem "C:\Program Files\OpenSSL\bin\openssl.exe" req -x509 -config homylogic.cnf -newkey rsa:4096 -sha256 -nodes -out homylogic.pem -outform PEM -keyout homylogic.key


ECHO # Generating CRT

IF NOT EXIST homylogic.crt "C:\Program Files\OpenSSL\bin\openssl.exe" x509 -outform der -in homylogic.pem -out homylogic.crt


ECHO # Exporting PFX

IF NOT EXIST homylogic.pfx "C:\Program Files\OpenSSL\bin\openssl.exe" pkcs12 -export -out homylogic.pfx -inkey homylogic.key -in homylogic.pem
                                                              

PAUSE


REM *** VYPLNIT ***
REM Country Name (2 letter code) [AU]:SK
REM State or Province Name (full name) [Some-State]:Slovakia
REM Locality Name (eg, city) []:Ruzomberok
REM Organization Name (eg, company) [Internet Widgits Pty Ltd]:Exalogic
REM Organizational Unit Name (eg, section) []:Homylogic
REM Common Name (e.g. server FQDN or YOUR name) []:homylogic.go
REM Email Address []:homylogic@homylogic.com
REM 
REM Password: homysmartlogic