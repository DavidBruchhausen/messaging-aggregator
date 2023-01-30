# Messaging Aggregator

## Introduction
.NET 6 application that exposes a REST HTTP endpoint which receives messages from an external client. These messages are sent to a RabbitMQ message queue, processed and sent in batches to an REST HTTP endpoint. These batches are sent with a minimum specified time interval between sends, which can be changed in the configuration.

## Some requirements
1. No authorization or authentication is needed for the service.
2. The order of batches should follow the order of arrival at the message service (FIFO).
3. A maximum batch size can be set, but it is not specified.
4. Batches must be sent every 10 seconds, at a minimum.
5. The application should run in a docker container.
6. The system should be designed to minimize data loss in case of failure.

## Design choices
A message queue service (RabbitMQ) was chosen to facilitate the asynchronous communication between the client and upstream aggregated service. The motivation fo this is:
1. Decoupling: By using a message queue, microservices can communicate asynchronously, reducing dependencies and allowing each service to evolve and scale independently.
2. Resilience: A message queue service provides a buffer for messages, allowing services to continue functioning even if one of them is unavailable.
3. Scalability: RabbitMQ can handle a large number of messages and can be easily scaled to meet growing demands.
4. Reliability: RabbitMQ is known for its reliability and is widely used in production environments.
5. Load balancing: A message queue can help distribute the load between microservices, improving performance and reducing the risk of overloading any one service.

The queue service can be scaled multiple ways:
1. Horizontal scaling: You can add more nodes to the cluster as demand grows, enabling you to scale out horizontally.
2. Load balancing: You can use a load balancer to distribute incoming messages across multiple nodes in the cluster.
3. Auto-scaling: RabbitMQ can be deployed on cloud platforms that support auto-scaling, allowing you to dynamically add or remove nodes based on demand.

The REST HTTP endpoint exposed by the applications publishes message to the queue. A retry mechanism was built into this flow. If the call to publish the message fails, it will be retried 3 times with 1 second until ultimately an error response is returned from the endpoint.

A background service was included to consume the messages form the queue and send them to the upstream service. This service sends message received from the queue every 10 sconds to the aggregation server. A maximum batch size is set in configuration. This should be tweaked to suit what the aggregated server can handle. A recommended starting value is 100. If this number is small compared to the number that could be sent from the queue after the given time interval, increase the batch size. It is not recommended to go past a size of 1000.

Similarly to the publishing of messages, sending batches to the aggregation server implements a retyr policy. The HTTP POST to the server will be attempted 3 times, with 1 a second interval. If this failes, the batches will not be discarded, but the servie will rather sleep for the time interval (10 seconds) before it tries to send batches to the aggregation server again.

## Areas for improvement
1. Instead of retries with fixed time intervals, backoff policies could be implemented when making calls ot he aggregated server.
2. If the service needed to scale dramatically to process up to millions of events per second, Apache Kafka could be used instead of RabbitMQ as it can handle higher throughput.

## Instructions
[Docker](https://www.docker.com/) needs to be installed. Navigate to the route directory of the project and run the following command:

```
docker compose up --build
```
This will run the docker-compose script in the repository. This script runs two containers:
1. A container that hosts a RabbitMQ queue.
2. A container that hosts the ASP.Net Core Web API, with the hosted service (consumer).

The API can then be accessed on [http://localhost:8080](http://localhost:8080/), or [Swagger](http://localhost:8080/swagger/index.html), and the RabbitMQ dashboard at [http://localhost:15672](http://localhost:15672/).