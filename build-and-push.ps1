# Set variables
$dockerUser = "burrnik"
$imageName = "podarchiver"
$tag = "latest"

# Build the image for ARM64
docker buildx build --platform linux/arm64 -t "${dockerUser}/${imageName}:${tag}" .

# Login to Docker Hub (interaktiv, sicherer als Passwort im Skript)
docker login

# Push the image to Docker Hub
docker push "${dockerUser}/${imageName}:${tag}"
