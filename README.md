# Overview
Simple Project to showcase how to create a Server Sent Event (SSE) flow using NET 8.

## Testing
To properly test the solution, I would suggest you create a Postman collection with the following route:

https://localhost:[port]/sse/connect/{userId}

Because neither Swagger nor .http files support SSE right now. Once you have postman, you can run the solution that will open Swagger with all the endpoints, with that, you can:

1. Connect an user to your system to simulate that a new client has accessed your website by calling: 
```
https://localhost:[port]/sse/connect/{userId}
```

2. You will see two types of messages comming from Postman:
   - User '{userId}' connected
   - <3: Keep alive
     
3. Now you can go to swagger and call the following endpoint to activate a simulated long task (10 secs): 
```
https://localhost:[port]/sse/task/{userId}
```

4. While the task is active, each second it will send a message to the user regarding the progress of the task like:
  - User '{userId}', attempt X / 10

Where "X" will increase by one each second. Once done, the task will resolve to a normal JSON object with an HTTP OK like:

```
{
  "message": "Task completed",
  "data": "User '{userId}' completed the task at: [DateTime.Now()]."
}
```

5. You can also test not only sending simple string messages but objects on itself by using the endpoint:
6. This endpoint makes a countdown message from 5 to 1 that looks like: Object sent to user '{userId}' in X...
7. Once it reaches 0, it both sends a last message with a structure like:
   ```
   {"Name":"Victor","Age":30,"Country":"Spain","IsActive":true}
   ```
   
   And returns  a normal JSON object with an HTTP OK like:

  ```
  {
    "message": "Task completed",
    "data": "User '{userId}' completed the task at: [DateTime.Now()]."
  }
  ```
