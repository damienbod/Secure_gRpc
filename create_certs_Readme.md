These certs are generated via openssl according to https://stackoverflow.com/questions/377@echo off
set OPENSSL_CONF=c:\OpenSSL-Win64\bin\openssl.cfg   

echo Generate CA key:
openssl genrsa -passout pass:1111 -des3 -out ca1.key 4096

echo Generate CA certificate:
openssl req -passin pass:1111 -new -x509 -days 365 -key ca1.key -out ca1.crt -subj  "/C=US/ST=CA/L=Cupertino/O=damienbod1/OU=YourApp/CN=MyRootCA"

echo Generate server key:
openssl genrsa -passout pass:1111 -des3 -out server1.key 4096

echo Generate server signing request:
openssl req -passin pass:1111 -new -key server1.key -out server1.csr -subj  "/C=US/ST=CA/L=Cupertino/O=damienbod1/OU=YourApp/CN=%COMPUTERNAME%"

echo Self-sign server certificate:
openssl x509 -req -passin pass:1111 -days 365 -in server1.csr -CA ca1.crt -CAkey ca1.key -set_serial 01 -out server1.crt

echo Remove passphrase from server key:
openssl rsa -passin pass:1111 -in server1.key -out server1.key

echo Generate client key
openssl genrsa -passout pass:1111 -des3 -out client1.key 4096

echo Generate client signing request:
openssl req -passin pass:1111 -new -key client1.key -out client1.csr -subj  "/C=US/ST=CA/L=Cupertino/O=damienbod1/OU=YourApp/CN=%CLIENT-COMPUTERNAME%"

echo Self-sign client certificate:
openssl x509 -passin pass:1111 -req -days 365 -in client1.csr -CA ca1.crt -CAkey ca1.key -set_serial 01 -out client1.crt

echo Remove passphrase from client key:
openssl rsa -passin pass:1111 -in client1.key -out client1.key

openssl pkcs12 -export -in server1.crt -inkey server1.key -out server1.pfx

openssl pkcs12 -export -in client1.crt -inkey client1.key -out client1.pfx