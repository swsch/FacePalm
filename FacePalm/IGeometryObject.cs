namespace FacePalm {
    internal interface IGeometryObject {
        string Id { get; set; }

        string Description { get; set; }

        bool IsVisible { get; set; }

        bool IsDefined { get; }
    }
}