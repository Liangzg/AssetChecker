namespace AssetChecker
{
    public interface IEditorPanel
    {
        void Initizalize();

        void OnGUI();
        
        void OnDestroy();
    }
}