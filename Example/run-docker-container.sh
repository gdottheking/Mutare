docker rm -f example
docker run --name example --add-host=host.docker.internal:host-gateway -p 5000:5000 -p5001:5001 -d my_example:latest