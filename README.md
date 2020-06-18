# MiddleMail

An API for transactional email: TODO

## Projects

| Project                               | NuGet     | Description |
|  -----------------------------------  |  -------  |  -----------------------------------------------------------  |
| `MiddleMail`                          | -         | Core library and abstractions                                 |
| `MiddleMail.Model`                    | -         | Message model shared by most other projects                   |
| `MiddleMail.Server`                   | -         | Ready to use server application                               |
| `MiddleMail.Client.RabbitMQ`          | -         | Client library that uses RabbitMQ as a backend                |
| `MiddleMail.MessageSource.RabbitMQ`   | -         | MessageSource implementation that uses RabbitMQ as a backend  |
| `MiddleMail.Delivery.Smtp`            | -         | Delivery implementation via SMTP                              |
| `MiddleMail.Storage.Memory`           | -         | Activity storage in-memory                                    |
| `MiddleMail.Storage.ElasticSearch`    | -         | Activity storage in ElasticSearch                             |

## Configuration

Replace `:` with `__` on bash, etc.

### SMTP

Configuration for `MiddleMail.Delivery.Smtp` used to connect and deliver mails to an SMTP server.

`MiddleMail:Delivery:Smtp:Server`: Hostname of the upstream SMTP server

`MiddleMail:Delivery:Smtp:Port`: Port of the SMTP server

`MiddleMail:Delivery:Smtp:Username` Username for the SMTP connection

`MiddleMail:Delivery:Smtp:Password` Password for the SMTP connection

### MimeMessage

Configuration for `MiddleMail.Delivery.Smtp` used to construct the message id of a mime message.

`MiddleMail:Delivery:MimeMessage:MessageIdDomainPart`: Domain part of the message id as in `<random-message-id@domain.part>`

### ExponentialBackoff

Configuration for `MiddleMail.MessageSource.RabbitMQ` used to calculate delay after processing failure.

`MiddleMail:ExponentialBackoff:Multiplicator`: `delay = 2^iteration * multiplicator` seconds

### ElasticSearchStorage

Configuration for `MiddleMail.Storage.ElasticSearch` 

`MiddleMail:Storage:ElasticSearch:Uri`: URI of the Elasticsearch instance

`MiddleMail:Storage:ElasticSearch:Index`: name of the Elasticsearch index we write messages to

### RabbitMQMessageSource

`MiddleMail:MessageSource:RabbitMQ:ConnectionString`: Connectionstring to rabbitmq, as defined at https://github.com/EasyNetQ/EasyNetQ/wiki/Connecting-to-RabbitMQ. E.g. `host=localhost;prefetchcount=10`

`MiddleMail:MessageSource:RabbitMQ:SubscriptionId`: Unique subscription id for this instance of middle mail

### `MiddleMail.Server` configuration

`REDIS_CONFIGURATION`: Configuration used to connect to Redis, as defined at https://stackexchange.github.io/StackExchange.Redis/Configuration

`REDIS_INSTANCE_NAME`: The Redis instance name

`DISABLE_SMTP`: Do not actually send anything via SMTP.

## Tools

| Project               | Description                    |
| --------------------- | ------------------------------ |
| EmailMessageGenerator | Generates test email messages  |
