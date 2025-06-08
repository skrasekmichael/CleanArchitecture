docker run --name ca-prometheus -p 9092:9090 -v $PSScriptRoot/prometheus.yml:/etc/prometheus/prometheus.yml -d prom/prometheus
