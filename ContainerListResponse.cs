using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PEngineModule.Logs
{
    public class ContainerListResponse
    {
        [DataMember(Name = "Id", EmitDefaultValue = false)]
        public string ID { get; set; }

        [DataMember(Name = "Names", EmitDefaultValue = false)]
        public IList<string> Names { get; set; }

        [DataMember(Name = "Image", EmitDefaultValue = false)]
        public string Image { get; set; }

        [DataMember(Name = "ImageID", EmitDefaultValue = false)]
        public string ImageID { get; set; }

        [DataMember(Name = "State", EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(Name = "Status", EmitDefaultValue = false)]
        public string Status { get; set; }
    }
}