# PageTrackerKata
A simple kata building a REST service that tracks page access and count

## API

API supports 2 methods:

### POST /

Accepts a payload with URL and VistorId and records the visit. Payload format:

```
{
	"Uri" : "<valid url>",
	"VisitorId" : "<vistor string id>"
}
```

Reponses:
* *204* - No content. Successfully registered. If visitor previously visited URI, visit is ignored.
* *400* - Bad Request. Payload is malformed.

### GET /counter/visitors/?uri=<uri to search>

Returns the number of visitors for the uri in the query string. Response payload is an scalar integer value.

Reponses:
* *200* - OK. Payload contains number of visitors to specified URI.
* *400* - Bad Request. Uri string is malformed or invalid.

## Running

Clone and build using Visual Studio F5 run or `dotnet run --project .\src\PageTracker.API\` from the repository root.

Once the server is running you can make requests to it using Postman, cUrl or other tool. 
Server listens on info: https://localhost:7027 and http://localhost:5027 .

