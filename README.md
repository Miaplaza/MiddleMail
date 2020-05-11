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


## Tools

| Project               | Description                   |
| --------------------- | ----------------------------- |
| EmailMessageGenerator | Generate test email messages  |