#!/bin/bash

# Create certificate directory
mkdir -p ./.certificates

# Generate a self-signed certificate for development
openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes \
  -keyout ./.certificates/aspnetapp.key \
  -out ./.certificates/aspnetapp.crt \
  -subj "/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,DNS:*.localhost,IP:127.0.0.1,IP:0.0.0.0"

# Convert to PFX format for ASP.NET Core
openssl pkcs12 -export -out ./.certificates/aspnetapp.pfx \
  -inkey ./.certificates/aspnetapp.key \
  -in ./.certificates/aspnetapp.crt \
  -password pass:devpassword

echo "Development certificate generated successfully!"
echo "Certificate files created in ./.certificates/"
echo "- aspnetapp.pfx (for ASP.NET Core)"
echo "- aspnetapp.crt (public certificate)"
echo "- aspnetapp.key (private key)"
echo ""
echo "To trust the certificate on macOS:"
echo "sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain ./.certificates/aspnetapp.crt"