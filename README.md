# BrokerAdapter

Combines similar HTTP requests together to make a single call to backend. 

Use case:
1) An HTTP request with URL "/api/value/1" comes. The application understands there are no similar requests pending and publishes a message to a Message Queue to start request processing.
2) A similar HTTP request with URL "/api/value/1" comes. In this case no new messages are published to the Message Queue. The HTTP request synchronously waits for the response from the Message Queue, similar to the first one.
3) The response from the Message Queue comes. Both pending clients are released with the same response.

## Memory Queue implementation

For simplicity sake the Memory Queue is implemented using files and folders.

*Publish* - creates a file in the requests folder. File name is a hash of URL + HTTP verb.

*Retrieve* - polls a file with the same hash in the responses folder until it's created.
