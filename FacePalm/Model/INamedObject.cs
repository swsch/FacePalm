using System.Dynamic;

namespace FacePalm.Model {
    public interface INamedObject {
        string Id { get; }
        string Description { get; }
    }
}