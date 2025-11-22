# HTTPS Configuration for DevHabit API

This guide explains how to enable HTTPS for the DevHabit API when running in Docker containers.

## Quick Setup

1. **Generate Development Certificate:**
   ```bash
   ./generate-dev-cert.sh
   ```

2. **Trust the Certificate (macOS):**
   ```bash
   sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain ./.certificates/aspnetapp.crt
   ```

3. **Rebuild and Start Containers:**
   ```bash
   docker-compose down
   docker-compose up --build
   ```

4. **Test HTTPS Endpoint:**
   ```bash
   curl https://localhost:9001/habits
   # OR visit in browser: https://localhost:9001/habits
   ```

## What's Configured

### Files Modified:
- **`appsettings.Docker.json`**: Added Kestrel HTTPS endpoint configuration
- **`docker-compose.yml`**: Added certificate volume mount and HTTPS environment variables
- **`.gitignore`**: Added certificate files to prevent committing secrets
- **`generate-dev-cert.sh`**: Script to create self-signed development certificates

### Certificate Details:
- **Password**: `devpassword` (for development only)
- **Valid for**: 365 days
- **Includes**: localhost, *.localhost, 127.0.0.1, 0.0.0.0 SANs

## Security Notes

⚠️ **Important**: The generated certificates are for development only!

- Self-signed certificates will show browser warnings
- Certificate password is hardcoded for development convenience
- For production, use proper certificates from a trusted CA

## Troubleshooting

### Certificate Errors
If you get certificate errors:
1. Ensure the certificate is trusted in your system keychain (see step 2 above)
2. Use `--insecure` flag with curl for testing: `curl --insecure https://localhost:9001/habits`

### Container Issues
If HTTPS port doesn't respond:
1. Check container logs: `docker logs devhabit.api`
2. Verify certificate file exists: `ls -la .certificates/`
3. Ensure volume is mounted: `docker inspect devhabit.api`

### Browser Warnings
Modern browsers will warn about self-signed certificates. You can:
1. Click "Advanced" → "Proceed to localhost" (not recommended for production)
2. Trust the certificate in your system keychain (recommended)
3. Use HTTP for development: `http://localhost:9000/habits`

## Production Considerations

For production deployment:
1. Replace self-signed certificate with CA-issued certificate
2. Update certificate password (use secrets management)
3. Configure proper certificate renewal
4. Remove development certificate generation script
5. Use HTTPS redirect middleware

## Alternative: Development Certificate from ASP.NET Core

You can also use ASP.NET Core's built-in development certificate:

```bash
# Generate ASP.NET Core dev cert
dotnet dev-certs https -ep .certificates/aspnetapp.pfx -p devpassword --trust

# Update docker-compose.yml to use this certificate
# (The current configuration already supports this)
```