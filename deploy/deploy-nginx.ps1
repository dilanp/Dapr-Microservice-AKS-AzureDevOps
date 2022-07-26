# Add the ingress-nginx repository.
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

# Use Helm to deploy an NGINX ingress controller.
helm install nginx-ingress ingress-nginx/ingress-nginx `
    --namespace default `
    --set controller.replicaCount=2 `
    --set controller.service.annotations."service\.beta.kubernetes\.io/azure-dns-label-name"="dapringress"

# Verify
kubectl --namespace default get svc -o wide -w nginx-ingress-ingress-nginx-controller

# Create ingress
#kubectl apply -f ./deploy/ingress-nginx.yaml