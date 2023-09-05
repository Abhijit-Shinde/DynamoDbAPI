using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DynamoDbAPI.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DynamoDbAPI;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var dynamoDbClient = new AmazonDynamoDBClient();
        var errorMessage = String.Empty;

        if (!string.IsNullOrEmpty(request.Body))
        {
            LambdaLogger.Log($"RequestBody : {request.Body}");

            var parsedJson = JsonConvert.DeserializeObject<Request>(request.Body);

            if (!string.IsNullOrEmpty(parsedJson.Action))
            {
                return parsedJson.Action.ToUpper() switch
                {
                    "ADD" => await AddItems(dynamoDbClient, parsedJson.Add),
                    "UPDATE" => await UpdateItems(dynamoDbClient, parsedJson.Update),
                    "DELETE" => await DeleteItems(dynamoDbClient, parsedJson.Delete),
                    "SCAN" => await ScanItems(dynamoDbClient, parsedJson.Scan),
                    "QUERY" => await QueryItems(dynamoDbClient, parsedJson.Query)
                };
            }

            errorMessage = $"{HttpStatusCode.BadRequest} Action cannot be null or Empty";
            return errorMessage;
        }

        errorMessage = $"{HttpStatusCode.NotFound} Invalid Request Body";
        LambdaLogger.Log(errorMessage);

        return errorMessage;

    }

    private async Task<string> AddItems(AmazonDynamoDBClient dynamoDbClient, PutItem request)
    {
        try
        {
            foreach (var item in request.Item)
            {
                var res = await dynamoDbClient.PutItemAsync(new PutItemRequest
                {
                    TableName = request.TableName,
                    Item = item
                });
            }

            return HttpStatusCode.Created.ToString();
        }
        catch (AmazonDynamoDBException e)
        {
            return e.Message;
        }
    }
    private async Task<string> UpdateItems(AmazonDynamoDBClient dynamoDbClient, UpdateItem request)
    {
        try
        {
            var response = await dynamoDbClient.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = request.TableName,
                Key = request.TableKey,
                AttributeUpdates = request.Item              
            });

            return response.HttpStatusCode.ToString();
        }
        catch (AmazonDynamoDBException e)
        {
            return e.Message;
        }
    }

    private async Task<string> DeleteItems(AmazonDynamoDBClient dynamoDbClient, DeleteItemRequest request)
    {
        try
        {
            var response = await dynamoDbClient.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = request.TableName,
                Key = request.Key,
            });

            return response.HttpStatusCode.ToString();
        }
        catch (AmazonDynamoDBException e)
        {
            return e.Message;
        }
    }

    private async Task<string> QueryItems(AmazonDynamoDBClient dynamoDbClient, QueryRequest? request)
    {
        try
        {
            var response = await dynamoDbClient.QueryAsync(new QueryRequest
            {
                ExpressionAttributeValues = request.ExpressionAttributeValues,
                TableName = request.TableName,
                Select = Select.ALL_ATTRIBUTES,
                KeyConditionExpression = request.KeyConditionExpression
            });

            var jsonResult = new JObject();

            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                foreach (var keyValue in item)
                {
                    jsonResult.Add(keyValue.Key, keyValue.Value.S);
                }
            }

            string jsonString = jsonResult.ToString(Formatting.Indented);

            return jsonString;

        }catch (AmazonDynamoDBException e)
        {
            return e.Message;
        }
    }

    private async Task<string> ScanItems(AmazonDynamoDBClient dynamoDbClient, ScanRequest? request)
    {
        try
        {
            var response = await dynamoDbClient.ScanAsync(new ScanRequest
            {
                TableName = request.TableName,
                ProjectionExpression = request.ProjectionExpression,
                FilterExpression = request.FilterExpression,
                ExpressionAttributeValues = request.ExpressionAttributeValues,
            });

            var jsonResult = new JArray();

            foreach (Dictionary<string, AttributeValue> item in response.Items)
            {
                foreach (var keyValue in item)
                {
                    jsonResult.Add(keyValue.Value.S);
                }
            }

            string jsonString = jsonResult.ToString();

            return jsonString;

        }catch(AmazonDynamoDBException e)
        {
            return e.Message;
        }

    }

}
