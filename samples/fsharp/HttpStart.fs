// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VSSample

open System
open System.Net.Http
open System.Net.Http.Headers
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Azure.WebJobs.Host
open FSharp.Control.Tasks

module HttpStart = 

  [<FunctionName("HttpStart")>]
  let Run([<HttpTrigger(AuthorizationLevel.Function, "post", Route = "orchestrators/{functionName}")>] req: HttpRequestMessage,
          [<OrchestrationClient>] starter: DurableOrchestrationClient,
          functionName: string,
          log: TraceWriter) =
    task {
      let! eventData =  req.Content.ReadAsAsync<Object>()
      let! instanceId = starter.StartNewAsync(functionName, eventData)

      log.Info(sprintf "Started orchestration with ID = '{%s}'." instanceId)

      let res = starter.CreateCheckStatusResponse(req, instanceId)
      res.Headers.RetryAfter <- RetryConditionHeaderValue(TimeSpan.FromSeconds(10.0))
      return res
    }