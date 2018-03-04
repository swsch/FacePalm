using System.Collections.Generic;

namespace FacePalm.Model {
    public class Index<T>
        where T : INamedObject {
        private readonly Dictionary<string, T> _dict = new Dictionary<string, T>();

        public void Register(T namedObject) => _dict[namedObject.Id] = namedObject;

        public T ById(string id) => _dict[id];
    }
}