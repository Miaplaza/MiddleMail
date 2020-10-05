# MiddleMail ![Build](https://img.shields.io/github/workflow/status/Miaplaza/MiddleMail/CI%20with%20dependencies%20from%20helm%20chart?style=flat-square)

MiddleMail is an email sending middleware: It offers an API for transaction email delivery and activity logging and acts as a middleware for other delivery services.
Because all parts of MiddleMail are pluggable it can be used in different configurations for both production and staging applications: E.g. you can turn off actual delivery for a staging deployment but leave activity logging enabled to view recent email activity.

## Projects

| Project                               | Package     | Description |
|  -----------------------------------  |  -------- |  -----------------------------------------------------------  |
| `MiddleMail`                          | [![NuGet](https://img.shields.io/nuget/v/MiddleMail?style=flat-square)](https://www.nuget.org/packages/MiddleMail/) |  Core library and abstractions     |
| `MiddleMail.Model`                    | [![NuGet](https://img.shields.io/nuget/v/MiddleMail.Model?style=flat-square)](https://www.nuget.org/packages/MiddleMail.Model/) | Message model shared by most other projects                   |
| `MiddleMail.Server`                   | [![Docker](https://img.shields.io/docker/v/miaplaza/middlemail?color=blue&label=docker&sort=semver&style=flat-square)] (https://hub.docker.com/r/miaplaza/middlemail)  | Ready to use server application                               |
| `MiddleMail.Client.RabbitMQ`          | [![NuGet](https://img.shields.io/nuget/v/MiddleMail.Client.RabbitMQ?style=flat-square)](https://www.nuget.org/packages/MiddleMail.Client.RabbitMQ/) | Client library that uses RabbitMQ as a backend                |
| `MiddleMail.MessageSource.RabbitMQ`   | -         | MessageSource implementation that uses RabbitMQ as a backend  |
| `MiddleMail.Delivery.Smtp`            | -         | Delivery implementation via SMTP                              |
| `MiddleMail.Storage.Memory`           | -         | Activity storage in-memory                                    |
| `MiddleMail.Storage.ElasticSearch`    | -         | Activity storage in ElasticSearch                             |

## Concepts

The three most important concepts in MiddleMail are **MessageSource**, **Storage** and **Delivery**. 

A `MiddleMail.IMessageSource` defines how MiddleMail consumes emails. Those emails then enter the processing pipeline. 

To persist information about email activity MiddleMail uses a `MiddleMail.IStorage`. 
Depending on the implementation this makes email activity easily searchable and debuggable. 

For actual delivery an `MiddleMail.IMailDeliverer` is used: `MiddleMail.Delivery.Smtp` implements such a deliverer for SMTP but it is easy to plugin your prefered Email Delivery SaaS or disable delivery for debugging and testing.

## Configuration

The following table lists the configurable parameters for MiddleMail. To pass them as environment variables on bash replace `:` with `__`.
  
| Parameter                           | Description                          |
| ----------------------------------- | ------------------------------------ |
| `MiddleMail:Delivery:Smtp:Server`   | Hostname of the upstream SMTP server |
| `MiddleMail:Delivery:Smtp:Port`.    | Port of the SMTP server              |
| `MiddleMail:Delivery:Smtp:Username` | Username for the SMTP connection     |
| `MiddleMail:Delivery:Smtp:Password` | Password for the SMTP connection     |
| `MiddleMail:Delivery:MimeMessage:MessageIdDomainPart` | Domain part of the message id as in `<random-message-id@domain.part>` used to construct the message id of a mime message.|
| `MiddleMail:ExponentialBackoff:Multiplicator` | `delay = 2^iteration * multiplicator` seconds delay after processing failure.   |
| `MiddleMail:Storage:ElasticSearch:Uri` | URI of the Elasticsearch instance |
| `MiddleMail:Storage:ElasticSearch:Index` | name of the Elasticsearch index we write messages to |
| `MiddleMail:MessageSource:RabbitMQ:ConnectionString` | Connectionstring to rabbitmq, as defined at https://github.com/EasyNetQ/EasyNetQ/wiki/Connecting-to-RabbitMQ. E.g. `host=localhost;prefetchcount=10` |
| `MiddleMail:MessageSource:RabbitMQ:SubscriptionId` | Unique subscription id for this instance of middle mail |
| `REDIS_CONFIGURATION` | Configuration used to connect to Redis, as defined at https://stackexchange.github.io/StackExchange.Redis/Configuration |
| `REDIS_INSTANCE_NAME` | The Redis instance name |
| `DISABLE_SMTP` | Do not actually send anything via SMTP. |

## Tools

| Project               | Description                    |
| --------------------- | ------------------------------ |
| EmailMessageGenerator | Generates test email messages  |
