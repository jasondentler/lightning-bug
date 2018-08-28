using Newtonsoft.Json;
using Polly.Caching;

namespace LightningBug.Polly.Caching
{
    public class JsonKeySerializer : ICacheItemSerializer<object, string>
    {
        public string Serialize(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        public object Deserialize(string objectToDeserialize)
        {
            return JsonConvert.DeserializeObject(objectToDeserialize);
        }
    }
}