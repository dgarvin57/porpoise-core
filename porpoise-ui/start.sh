#!/bin/sh
set -e

# Use Railway's PORT or default to 80
PORT=${PORT:-80}

# Replace PORT placeholder in nginx config
sed "s/\${PORT}/$PORT/g" /etc/nginx/templates/default.conf.template > /etc/nginx/conf.d/default.conf

echo "Starting nginx on port $PORT"

# Start nginx
exec nginx -g 'daemon off;'
