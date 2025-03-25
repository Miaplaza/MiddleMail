# MiddleMail  [![Build](https://img.shields.io/github/workflow/status/Miaplaza/MiddleMail/CI%20with%20dependencies%20from%20helm%20chart?style=flat-square)](https://github.com/Miaplaza/MiddleMail/actions)


MiddleMail is an email sending middleware: It offers an API for transaction email delivery and activity logging and acts as a middleware for other delivery services.
Because all parts of MiddleMail are pluggable it can be used in different configurations for both production and staging applications: E.g. you can turn off actual delivery for a staging deployment but leave activity logging enabled to view recent email activity.

## Projects

| Project                               | Package     | Description |
|  -----------------------------------  |  -------- |  -----------------------------------------------------------  |
| `MiddleMail`                          | [![NuGet](https://img.shields.io/nuget/v/MiddleMail?style=flat-square)](https://www.nuget.org/packages/MiddleMail/) |  Core library and abstractions     |
| `MiddleMail.Model`                    | [![NuGet](https://img.shields.io/nuget/v/MiddleMail.Model?style=flat-square)](https://www.nuget.org/packages/MiddleMail.Model/) | Message model shared by most other projects                   |
| `MiddleMail.Server`                   | [![Docker](https://img.shields.io/docker/v/miaplaza/middlemail?color=blue&label=docker&sort=semver&style=flat-square)](https://hub.docker.com/r/miaplaza/middlemail) | Ready to use server application                               |
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

## Basic Local Setup
This setup assumes the use of Visual Studio Code as an editor. If not using 
VSCode, configure environment variables in whatever way is appropriate for your 
preferred launch mechanism. This setup also runs the dependencies in Docker.
You don't have to do this, but it sure is convenient.

1. Clone this repository.
1. Create a `launch.json` in which you configure the environment variables described above in the Configuration section. This `launch.json` should target the build of the `MiddleMail.Server` project, since that's the standalone process. You probably also need to create a `tasks.json` to run it, with a build task, but that's no different from the standard `launch.json` setup.
1. Run the `docker-compose.dev.yml` file to spin up dependency services. It mainly includes:
  - A RabbitMQ container as an email message queue.
  - An Elasticsearch container as a storage solution.
    - _Note that if you're not using Elasticsearch, you can edit the services found in `MiddleMail/Server/Program.cs` to replace `ElasticSearchStorage` in `services.AddSingleton<IStorage, ElasticSearchStorage>();` with `MemoryStorage`, an in-memory storage solution. You will also need to edit the dependencies in `MiddleMail.Server.csproj` to depend on `MiddleMail.Storage.Memory` instead of `MiddleMail.Storage.Elastic`._
  - A Redis container as a caching solution.
  - An smtp4dev container as an SMTP server for development and testing.
1. Run the MiddleMailServer process (with your `launch.json`).
1. Optional: Test your new MiddleMail instance using the `tools/EmailMessageGenerator` project to send test emails.


## Tools

| Project               | Description                    |
| --------------------- | ------------------------------ |
| EmailMessageGenerator | Generates test email messages  |
