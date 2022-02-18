# OpenTelemetry Web Job example in .Net 6.

This simple sample will show you how to use open telemetry in .Net 6.
The output will go the console as well as to an OTEL exporter.

If you would like to see the output in ZipKin and Prometeus, use the following docker file.
Drop the following three files to a folder and run `docker-compose  up`


## docker-compose.yaml
```
version: "1"
services:
  # back-ends
  zipkin-all-in-one:
    image: openzipkin/zipkin:latest
    ports:
      - "9411:9411"

  prometheus:
    container_name: prometheus
    image: prom/prometheus:latest
    volumes:
      - ./prometheus.yaml:/etc/prometheus/prometheus.yml
    ports:
      - "9090:9090"

  # OpenTelemetry Collector
  otel-collector:
    image: otel/opentelemetry-collector:latest
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
      - ./output:/etc/output:rw # Store the logs
    ports:
      - "8888:8888"   # Prometheus metrics exposed by the collector
      - "8889:8889"   # Prometheus exporter metrics
      - "4317:4317"   # OTLP gRPC receiver
    depends_on:
      - zipkin-all-in-one
```
**Logs** Will be in `output` folder

## otel-collector-config.yaml
```
# Configure receivers
# We only need otlp protocol on grpc, but you can use http, zipkin, jaeger, aws, etc.
# https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/receiver
receivers:
  otlp:
    protocols:
      grpc:

# Configure exporters
exporters:
  # Export prometheus endpoint
  prometheus:
    endpoint: "0.0.0.0:8889"

  # log to the console
  logging:

  # Export to zipkin
  zipkin:
    endpoint: "http://zipkin-all-in-one:9411/api/v2/spans"
    format: proto

  # Export to a file
  file:
    path: /etc/output/logs.json

# Configure processors (batch, sampling, filtering, hashing sensitive data, etc.)
# https://opentelemetry.io/docs/collector/configuration/#processors
processors:
  batch:

# Configure pipelines. Pipeline defines a path the data follows in the Collector
# starting from reception, then further processing or modification and finally
# exiting the Collector via exporters.
# https://opentelemetry.io/docs/collector/configuration/#service
# https://github.com/open-telemetry/opentelemetry-collector/blob/main/docs/design.md#pipelines
service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, zipkin]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [logging, prometheus]
    logs:
      receivers: [otlp]
      processors: []
      exporters: [logging, file]
```

## prometheus.yaml
```
scrape_configs:
- job_name: 'otel-collector'
  scrape_interval: 10s
  static_configs:
  - targets: ['otel-collector:8889']
  - targets: ['otel-collector:8888']
```

