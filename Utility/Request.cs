using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.ComponentModel.DataAnnotations;

namespace DynamoDbAPI.Utility
{
    public class Request
    {
        [Required]
        public string Action { get; set; }
        public UpdateItem? Update { get; set; }
        public PutItem? Add { get; set; }
        public DeleteItemRequest? Delete { get; set; }
        public ScanRequest? Scan { get; set; }
        public QueryRequest? Query { get; set; }
    }

    public class PutItem
    {
        [Required]
        public string TableName { get; set; }
        public List<Dictionary<string,AttributeValue>> Item { get; set; }
    }

    public class UpdateItem
    {
        [Required]
        public string TableName { get; set; }
        [Required]
        public Dictionary<string,AttributeValue> TableKey { get; set; }
        public Dictionary<string,AttributeValueUpdate> Item { get; set; }
    }
}
