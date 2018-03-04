namespace FacePalm.Model {
    public interface IExportabe {
        string Id { get; }

        string ExportHeader { get; }

        string ExportData { get; }
    }
}