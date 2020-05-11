# MiddleMail

An API for transaction email

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

`SMTP:Server`: The SMTP server

`SMTP:Port`: Port of the SMTP server

`SMTP:Username` Username for the SMTP connection

`SMTP:Password` Password for the SMTP connection

### MimeMessage

`MimeMessage:MessageIdDomainPart`: Domain part of the message id as in `<foo@domain.part>`

### ExponentialRetryDelay

`ExponentialRetryDelay:Multiplicator`: `f(x) = 2^x * multiplicator`

### ElasticSearchStorage

`ElasticSearchStorage:Uri`: ElasticSearch URI

`ElasticSearchStorage:Index`: Index name

### RabbitMQMessageSource

`RabbitMQMessageSource:ConnectionString`: Connectionstring to rabbitmq

`RabbitMQMessageSource:SubscriptionId`: Uniqq subscription id per middle mail instance


## Tools

| Project               | Description                   |
| --------------------- | ----------------------------- |
| EmailMessageGenerator | Generate test email messages  |